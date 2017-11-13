var hud = hud || {};

hud.plugins = hud.plugins || {};

// keep track of map objects belonging to different panels
var rtmap_maps = {};

// set of base map layers
var rtmap_baseLayers;

// set of overlays
var rtmap_overlayLayers;

// global settings
var rtmap_settings;

hud.plugins.rtmap = (function () {

    var markersi, markersr, markersd, georesLayer;

    var _initMap = function (panel) {
        L_PREFER_CANVAS = true;

        $.get(hud.getURL("RTM/GetSettings"), function (data) {
            rtmap_settings = data;

            var osmUrl = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
            var osmAttrib = "Map data © OpenStreetMap contributors";
            var osm = new L.TileLayer(osmUrl, { attribution: osmAttrib });
            var mbAttr = 'Map data &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors, ' +
                '<a href="http://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, ' +
                'Imagery © <a href="http://mapbox.com">Mapbox</a>',
                mbUrl = "https://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoibWFwYm94IiwiYSI6ImNpandmbXliNDBjZWd2M2x6bDk3c2ZtOTkifQ._QA7i5Mpkd_m30IGElHziw";

            //https://leaflet-extras.github.io/leaflet-providers/preview/

            var grayscale = L.tileLayer('https://stamen-tiles-{s}.a.ssl.fastly.net/toner-lite/{z}/{x}/{y}.{ext}', {
                attribution: 'Map tiles by <a href="http://stamen.com">Stamen Design</a>, <a href="http://creativecommons.org/licenses/by/3.0">CC BY 3.0</a> &mdash; Map data &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>',
                subdomains: 'abcd',
                minZoom: 0,
                maxZoom: 20,
                ext: 'png'
            });

            var barts = L.tileLayer.wms("http://" + rtmap_settings.mapServer + "/cgi-bin/mapserv?MAP=/maps/extent.map&crs=EPSG:27700", { layers: "Barts", format: "image/png", maxZoom: 22, minZoom: 0, continuousWorld: true, noWrap: true });

            var stations = L.tileLayer.wms("http://" + rtmap_settings.mapServer + "/cgi-bin/mapserv?MAP=/maps/extent.map&crs=EPSG:27700", { layers: "Stations", format: "image/png", transparent: true, maxZoom: 22, minZoom: 0, continuousWorld: true, noWrap: true });

            var carto_dark = L.tileLayer('https://cartodb-basemaps-{s}.global.ssl.fastly.net/dark_nolabels/{z}/{x}/{y}.png', {
                attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="http://cartodb.com/attributions">CartoDB</a>',
                subdomains: 'abcd',
                maxZoom: 19
            });

            var Esri_WorldImagery = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
                attribution: 'Tiles &copy; Esri &mdash; Source: Esri, i-cubed, USDA, USGS, AEX, GeoEye, Getmapping, Aerogrid, IGN, IGP, UPR-EGP, and the GIS User Community'
            });

            rtmap_baseLayers = {
                "OSM": osm,
                "Barts": barts,
                "Grayscale": grayscale,
                "Dark": carto_dark,
                "Satellite": Esri_WorldImagery,
                "None": null
            };
            var baseLayer = osm;

            rtmap_overlayLayers = {
                "Stations": stations
            };

            var mapdiv = 'map' + panel;

            var map = new L.Map(mapdiv, {
                center: new L.LatLng(rtmap_settings.latitude, rtmap_settings.longitude),
                zoom: rtmap_settings.zoom,
                layers: baseLayer,
                zoomControl: false,
                continuousWorld: true,
                worldCopyJump: false,
                inertiaDeceleration: 10000
            });

            L.control.layers(rtmap_maps, rtmap_overlayLayers).addTo(map);

            // add a search layer onto the maps and remember it
            var searchlayer = new L.featureGroup();
            map.addLayer(searchlayer);

            // save the map object in a dictionary so it can be accessed later
            rtmap_maps[panel] = map;

            // select the primary menu
            hud.selectPanelMenu(panel, 0);

            // attach handlers for remaining buttons, i.e. not selectmenu or select-action as these
            // are handled automatically by hud.js
            _registerButtons(panel);

            // listen for hub messages on these groups
            $("#sys_hub").on("Resource.Available Resource.Busy Resource.Enroute", function (group, msg) {
                _handleMessage(panel, group, georesLayer, msg);
            });

            // listen for local hub messages 
            $("#sys_hub").on("AddDocument", function (evt, data) {
                _boundingbox = data;
            });

            // panel was Swapped Expanded
            $("#sys_hub").on("Swapped Expanded Fullscreen", function (evt, data) {
                map.invalidateSize();
            });

            // listen for panel actions
            $('[data-panel-role=' + panel + ']').on("action", function (evt, action) {
                _handleAction(panel, action);
            });

            // send a MapBounds event if the map changes in any way
            map.on('moveend resize zoomend', function (ev) {
                hud.sendLocal("MapBounds", map.getBounds());
            });

            // listen for local hub messages  
            $("#sys_hub").on("SearchResults", function (evt, data) {
                _showSearchResults(map, panel, searchlayer, data);
            });
            
        });
    };

    // show search results on the map
    var _showSearchResults = function (map, panel, searchlayer, data) {

        searchlayer.clearLayers();

        for (docindex = 0; docindex < data.Documents.length; docindex++) {
            doc = data.Documents[docindex];
            score = doc.s;
            if (doc.l !== undefined && doc.l !== null) {
                latlng = new L.LatLng(doc.l.lat, doc.l.lon);
                feature = _getFeature(doc, latlng);

                searchlayer.addLayer(feature);
            }
        }

        //var coords = _getCoordinatesFromPoly(data.Bounds);
        //_addPolygon(searchlayer, coords);

        // zoom in..
        var isLocked = hud.getButtonState(panel, 'data-action', 'lock-map');

        if (!isLocked && data.Documents.length > 0) {
            var bounds = searchlayer.getBounds();
            if (bounds.isValid())
                map.fitBounds(bounds, { maxZoom: 18 });//works!    
        }

        map.invalidateSize();
        
    }

    // add a polygon to the layer
    var _addPolygon = function (layer, bounds, append, color) {
        if (color === undefined)
            color = "red";
        if (!append)
            layer.clearLayers();
        var feature = L.polygon(bounds, { color: color, fill: false });
        layer.addLayer(feature);
    }

    // create a leaflet polygon from a NEST polygon
    var _getCoordinatesFromPoly = function (shape) {
        var polylineCoordinates = [];
        if (shape !== null) {
            shape.coordinates.forEach(function (coords) {
                coords.forEach(function (pnt) {
                    var latlng = new L.LatLng(pnt[1], pnt[0]);
                    polylineCoordinates.push(latlng);
                });

            });
        }
        return polylineCoordinates;
    }
    
    // return a single feature for this address
    var _getFeature = function(address, latlng) {
        var latLongParms = {
            lng: address.l.lon,
            lat: address.l.lat
        };

        var latLongString = JSON.stringify(latLongParms);

        var feature;
        var content = "";

        var customOptions =
            {
                'maxWidth': "500",
                'minWidth': "300",
                keepInView: false
            }

        content += "<h4>" + address.d + "</h4>";
        if (address.st !== undefined && address.st !== "Active" && address.st !== null) {
            content += "<p>" + address.st + "</p>";
        }
        content += "<input type='hidden' id='latLong' value='" + latLongString + "'>";
        if (address.url !== undefined && address.url !== "" && address.url !== null) {
            content += "<a target=\"_blank\" href='" + address.url + "'>" + address.url + "</a>";
        }

        if (address.v !== undefined && address.v !== "" && address.v !== null) {
            content += "<video controls autoplay name=\"media\"><source src=" + address.v + " type=\"video/mp4\"></video>";
        } else {
            if (address.i !== undefined && address.i !== "" && address.i !== null) {
                content += "<image src='" + address.i + "'/>";
            }
        }

        if (address.c !== undefined && address.c !== "" && address.c !== null) {
            content += address.c;
        }

        var polylineCoordinates = [];

        // its a polygon
        if (address.pg !== null) {
            address.pg.coordinates.forEach(function (coords) {
                coords.forEach(function (pnt) {
                    var latlng = new L.LatLng(pnt[1], pnt[0]);
                    polylineCoordinates.push(latlng);
                });

            });
            feature = L.polygon(polylineCoordinates, { color: "lightyellow" });
            feature.on("click",
                function () {
                    // trigger local event to broadcast this object
                    hud.sendLocal("ObjectSelected", { "Type": "SearchItem", "Value": address });
                });
        }
        else
            // its a multi-line
            if (address.ml !== null) {
                address.ml.coordinates.forEach(function (coords) {
                    coords.forEach(function (pnt) {
                        var latlng = new L.LatLng(pnt[1], pnt[0]);
                        polylineCoordinates.push(latlng);
                    });

                }
                );
                feature = L.polyline(polylineCoordinates, { color: "blue" });

                feature.on("click",
                    function () {

                        // trigger local event to broadcast this object
                        hud.sendLocal("ObjectSelected", { "Type": "SearchItem", "Value": address });
                        //InformationSearch(latlng.lng, latlng.lat);
                    });
            }
            else {
                // normal location
                feature = L.userMarker(latlng, { pulsing: false, accuracy: 0, m: "loc", s: address.t });

                feature.on("click",
                    function () {
                        // trigger local event to broadcast this object
                        hud.sendLocal("ObjectSelected", { "Type": "SearchItem", "Value": address });
                        //InformationSearch(latlng.lng, latlng.lat);
                    });
            }
        return feature;
    }

    // handle message from service bus
    var _handleMessage = function (panel, group, georesLayer, msg) {
        switch (msg.$type) {
            case "Quest.Common.Messages.Resource.ResourceUpdate, Quest.Common":
                _updateResource(georesLayer, msg.Item);
                break;
            case "Quest.Common.Messages.Resource.ResourceStatusChange, Quest.Common":
                _updateResourceStatus(panel, georesLayer, msg);
                break;
        }
    };

    // handle actions from button push
    var _handleAction = function (panel, action) {
        switch (action) {
            case "select-overlay":
                var isOn = hud.toggleButton(panel, 'select-overlay', action);
                _selectLayer(panel, action, isOn);
                break;
            case "lock-map":
                break;
            default:
                hud.toggleButton(panel, 'select-action', action);
                _updateMap(panel);
        }
    };


    // attach click handlers to all the panel buttons
    var _registerButtons = function (panel) {
        $('[data-panel-role=' + panel + '] a[data-role="select-map"]').on('click', function (e) {
            e.preventDefault();
            selected_map = $(e.currentTarget).attr('data-action');
            _selectBaseLayer(panel, selected_map);
        });
    };

    var _updateMap = function (panel) {

        //$("*").css("cursor", "wait"); // this call or handling of results by leaflet my take some time 

        var map = rtmap_maps[panel];

        resourcesAvailable = hud.getButtonState(panel, "select-action", "select-avail");
        resourcesBusy = hud.getButtonState(panel, "select-action", "select-busy");
        incidentsImmediate = hud.getButtonState(panel, "select-action", "select-c1");
        incidentsOther = false;
        hospitals = hud.getButtonState(panel, "select-action", "select-hos");
        standby = hud.getButtonState(panel, "select-action", "select-sbp");
        stations = hud.getButtonState(panel, "select-action",  "select-stn");
        aeu = hud.getButtonState(panel, "select-action", "select-aeu");
        fru = hud.getButtonState(panel, "select-action", "select-fru");
        oth = hud.getButtonState(panel, "select-action", "select-oth");

        //TODO: get these from the controller
        var resourceGroups = [];
        if (aeu) resourceGroups.push("AMB");
        if (fru) resourceGroups.push("CAR");
        if (oth) {
            resourceGroups.push("BIKE");
            resourceGroups.push("MBIKE");
            resourceGroups.push("HELI");
        }

        //Make a new request for the new selection
        $.ajax({
            url: hud.getURL("RTM/GetMapItems"),
            data:
            {
                ResourceGroups: resourceGroups,
                ResourcesAvailable: resourcesAvailable,
                ResourcesBusy: resourcesBusy,
                IncidentsImmediate: incidentsImmediate,
                IncidentsOther: incidentsOther,
                Hospitals: hospitals,
                Standby: standby,
                Stations: stations
            },
            dataType: "json",
            success: function (layer) {
                //Create a new empty resources layer and add to map
                if (markersr !== undefined) markersr.clearLayers();

                _createResourcesLayer(map);

                if (layer.error !== undefined) {
                    $("#message").html(layer.error);
                    $("#message").show();
                    return;
                }

                if (layer.Result === null)
                    return;

                // add resources to the map
                if (layer.Resources !== undefined) {
                    layer.Resources.forEach(function (item) {
                        _updateResource(georesLayer, item);
                    });
                }

                if (layer.Destinations !== undefined) {
                    // add Destinations to the map
                    _updateDestinations(layer.Destinations, georesLayer);
                }

                // now register for updates
                hud.joinLeaveGroup("Resource.Available", panel, resourcesAvailable);
                hud.joinLeaveGroup("Resource.Busy", panel, resourcesBusy);
                hud.joinLeaveGroup("Resource.Enroute", panel, resourcesBusy);

            } // success
            //$("*").css("cursor", "default");
        });
    };

    // the status of a resource has changed. we need to remove it if we're not showing
    // this type of resource
    var _updateResourceStatus = function (panel, layer, item) {

        resourcesAvailable = hud.getButtonState(panel, "select-action", "select-avail");
        resourcesBusy = hud.getButtonState(panel, "select-action", "select-busy");

        if (resourcesAvailable === false && item.NewStatusCategory !== "Available")
            _removeExistingFeature(layer, item.FleetNo);

        if (resourcesBusy === false && (item.NewStatusCategory !== "Busy" || item.NewStatusCategory !== "Enroute"))
            _removeExistingFeature(layer, item.FleetNo);
    };

    var _updateResource = function (layer, item) {
        // for each item construct equiv geojson item
        var geojsonFeature = {
            "type": "Feature",
            "id": item.FleetNo,
            "properties": {
                "Type": "Resource",
                "Value": item,
                "Name": item.Callsign,
                "MarkerType": item.ResourceTypeGroup,
                "MarkerStatus": item.StatusCategory
            },
            "geometry": {
                "type": "Point",
                "coordinates": [item.Position.Longitude, item.Position.Latitude]
            }
        };
        _removeExistingFeature(layer, item.FleetNo);
        layer.addData(geojsonFeature);
    };

    var _updateDestinations = function (destinations, layer) {

        destinations.forEach(function (item) {
            _removeExistingFeature(layer, item.Id);
        });

        var features = [];
        destinations.forEach(function (item) {
            // for each item construct equiv geojson item
            var geojsonFeature = {
                "type": "Feature",
                "id": item.Id,
                "properties": {
                    "Type": "Destination",
                    "Value": item,
                    "Name": item.Name,
                    "MarkerType": _getDestinationMarkerType(item),
                    "MarkerStatus": item.Status
                },
                "geometry": {
                    "type": "Point",
                    "coordinates": [item.Position.Longitude, item.Position.Latitude]
                }
            };
            features.push(geojsonFeature);
        });

        layer.addData(features);
    };

    var _getDestinationMarkerType = function (questDestination) {
        if (questDestination.IsAandE)
            return "AE";
        if (questDestination.IsHospital)
            return "HOS";
        if (questDestination.IsStation)
            return "STA";
        if (questDestination.IsStandby)
            return "SBP";
        if (questDestination.IsRoad)
            return "RD";
        return "";
    };

    // remove a feature form the map
    var _removeExistingFeature = function (layer, id) {
        //Leaflet idiosyncracy, cant loop features for a specific layer ?
        layer.eachLayer(function (lyr) {
            if (lyr.feature !== null && lyr.feature.properties !== null) {
                if (lyr.feature.id === id) {
                    layer.removeLayer(lyr);
                    //console.log("Resource " + lyr.feature.id + " removed for update");
                }
            }
        });
    };

    var _createResourcesLayer = function (map) {
        try {
            var useclusters = hud.getStoreAsBool("#use-clusters");

            if (useclusters === true)
                markersr = new L.MarkerClusterGroup(
                    {
                        iconCreateFunction: function (cluster) {
                            var childCount = cluster.getChildCount();
                            return new L.DivIcon({ html: "<div><span><b>" + childCount + "</b></span></div>", className: "resourcecluster", iconSize: new L.Point(40, 40) });
                        }
                    });

            if (markersr === null || markersr === undefined)
                markersr = L.layerGroup();

            georesLayer = L.geoJson("", {
                style: function () {
                    return { color: "#0f0", opacity: 1, fillOpacity: 0.5 };
                },
                pointToLayer: function (feature, latlng) {
                    var marker = L.userMarker(latlng, { pulsing: false, accuracy: 0, m: feature.properties.MarkerType, s: feature.properties.MarkerStatus });
                    marker.title = feature.properties.Callsign;
                    return marker;
                },
                onEachFeature: function (feature, layer) {
                    layer.on("click", function () {

                        // trigger local event to broadcast this object
                        hud.sendLocal("ObjectSelected", { "Type": feature.properties.Type, "Value": feature.properties.Value });
                    });
                }
            });

            markersr.addLayer(georesLayer);
            markersr.addTo(map);
        }
        catch (e) {
            console.log(e.message);
        }
    };
        
    /// <summary>
    /// Re-centre the maps to new co-ordinates
    /// </summary
    var _panTo = function (panel, lat, lng) {
        var map = rtmap_maps[panel];
    };

    var _panAndMarkLocation = function (panel, locationName, lat, lng) {
        var map = rtmap_maps[panel];
    };

    var _setZoomLevel = function (panel, z) {
        var map = rtmap_maps[panel];
    };

    var _selectBaseLayer = function (panel, layerName) {
        var map = rtmap_maps[panel];
        for (var layer in rtmap_baseLayers) {
            if (map.hasLayer(rtmap_baseLayers[layer]))
                map.removeLayer(rtmap_baseLayers[layer]);
        }
        map.addLayer(rtmap_baseLayers[layerName]);
    };

    var _selectLayer = function (panel, layerName, on) {
        var map = rtmap_maps[panel];

        if (on)
            map.addLayer(rtmap_overlayLayers[layerName]);
        else
            if (map.hasLayer(rtmap_overlayLayers[layerName]))
                map.removeLayer(rtmap_overlayLayers[layerName]);

    };

    var _lockMap = function (panel, mode) {
        hud.setButtonState(panel, "select-action", "lock-map", mode);
    };

    return {
        initMap: _initMap,
        selectBaseLayer: _selectBaseLayer,
        lockMap: _lockMap,
        setZoom: _setZoomLevel,
        panTo: _panTo,
        panAndMarkLocation: _panAndMarkLocation
    };

})();