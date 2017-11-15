var hud = hud || {};

hud.plugins = hud.plugins || {};

// keep track of map objects belonging to different map plugins
var rtmap_maps = {};

var georesLayer = {};

var searchlayer = {};

// set of base map layers
var rtmap_baseLayers;

// set of overlays
var rtmap_overlayLayers;

// global settings
var rtmap_settings;

hud.plugins.rtmap = (function () {

    var _init = function (panelId, pluginId) {

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

            // change the map div to be unique for this plugin
            var mapdiv = 'map' + pluginId;
            $('#mapdivplaceholder').attr('id', mapdiv);            

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

            // save the map object in a dictionary so it can be accessed later
            rtmap_maps[pluginId] = map;

            // create geo overlay
            georesLayer[pluginId] = _createResourcesLayer(map);

            // add a search layer onto the maps and remember it
            searchlayer[pluginId] = new L.featureGroup();
            map.addLayer(searchlayer[pluginId]);

            // select the primary menu
            hud.selectMenu(pluginId, 0);

            // attach handlers for remaining buttons, i.e. not selectmenu or select-action as these
            // are handled automatically by hud.js
            _registerButtons(pluginId);

            // listen for hub messages on these groups
            $("#sys_hub").on("Resource.Available Resource.Busy Resource.Enroute", function (group, msg) {
                _handleMessage(pluginId, group, msg);
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
            var selector = hud.pluginSelector(pluginId);
            $(selector).on("action", function (evt, action) {
                _handleAction(pluginId, action);
            });

            // send a MapBounds event if the map changes in any way
            map.on('moveend resize zoomend', function (ev) {
                hud.sendLocal("MapBounds", map.getBounds());
            });

            // listen for local hub messages  
            $("#sys_hub").on("SearchResults", function (evt, data) {
                _showSearchResults(map, pluginId, data);
            });
            
        });
    };

    var _updateCoverage = function(type, name) {
        return $.ajax({
            url: getURL("Home/GetVehicleCoverage"),
            data: { 'vehtype': type },               // 1=Amb 2=FRU 7=incidents 8=combined 9=holes 10=standby cover 11=standby compliance
            dataType: "json",
            success: function (msg) {

                if (msg.Success === true && msg.Map !== null) {
                    var res = msg.Map;
                    var data = [];
                    var i = 0;
                    if (res.map !== undefined) {
                        //var raw = window.atob(res.map);
                        for (var y = 0; y < res.rows; y++)
                            for (var x = 0; x < res.cols; x++) {
                                //if (raw.charCodeAt(i) > 0) {
                                if (res.map[i] > 0) {
                                    var lat = res.lat + y * res.latBlocksize;
                                    var lon = res.lon + x * res.lonBlocksize;
                                    var newpoint = [lat, lon];
                                    data.push(newpoint);
                                }

                                i++;
                            }
                        var nheatmap;
                        switch (res.vehtype) {
                            case 1:     // AEU
                                nheatmap = L.heatLayer(data, { max: 1.0, radius: 50, maxZoom: 15, gradient: { 0.1: "lime", 0.2: "lime", 0.3: "lime", 0.4: "lime" } });
                                break;
                            case 2:     // FRU
                                nheatmap = L.heatLayer(data, { max: 1.0, radius: 50, maxZoom: 15, gradient: { 0.1: "green", 0.2: "yellow", 0.3: "yellow", 0.4: "yellow" } });
                                break;
                            case 7:     // incident
                                nheatmap = L.heatLayer(data, { max: 5.0, radius: 50, maxZoom: 11, gradient: { 0.1: "black", 0.2: "black", 0.3: "black", 0.4: "black" } });
                                break;
                            case 8:     // combined coverage
                                nheatmap = L.heatLayer(data, { max: 1.0, radius: 50, gradient: { 0.1: "purple", 0.2: "lime", 0.3: "yellow", 0.4: "purple" } });
                                break;
                            case 9:     // resource holes
                                nheatmap = L.heatLayer(data, { max: 1.0, radius: 50, maxZoom: 15, gradient: { 0.1: "orange", 0.2: "lime", 0.3: "yellow", 0.4: "red" } });
                                break;
                        }

                        if (nheatmap !== undefined) {
                            nheatmap.opacity = 0.3;
                            nheatmap.id = name;
                            undoCoverage(name);
                            covlayer.addLayer(nheatmap);
                        }
                    }
                }
            }
        });
    }

    // show search results on the map
    var _showSearchResults = function (map, pluginId, data) {

        var layer = searchlayer[pluginId];
        layer.clearLayers();

        for (docindex = 0; docindex < data.Documents.length; docindex++) {
            doc = data.Documents[docindex];
            score = doc.s;
            if (doc.l !== undefined && doc.l !== null) {
                latlng = new L.LatLng(doc.l.lat, doc.l.lon);
                feature = _getFeature(doc, latlng);
                layer.addLayer(feature);
            }
        }

        //var coords = _getCoordinatesFromPoly(data.Bounds);
        //_addPolygon(searchlayer, coords);

        // zoom in..

        var isLocked = hud.getButtonState(pluginId, 'data-action', 'lock-map');

        if (!isLocked && data.Documents.length > 0) {
            var bounds = layer.getBounds();
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
    var _handleMessage = function (pluginId, group, msg) {
        switch (msg.$type) {
            case "Quest.Common.Messages.Resource.ResourceUpdate, Quest.Common":
                _updateResource(pluginId, msg.Item);
                break;
            case "Quest.Common.Messages.Resource.ResourceStatusChange, Quest.Common":
                _updateResourceStatus(pluginId, msg);
                break;
        }
    };

    // handle actions from button push
    var _handleAction = function (pluginId, action) {
        switch (action) {
            case "select-aeuc":
            case "select-fruc":
                break;

            case "select-overlay":
                var isOn = hud.toggleButton(pluginId, 'select-overlay', action);
                _selectLayer(pluginId, action, isOn);
                break;

            case "lock-map":
                break;

            default:
                hud.toggleButton(pluginId, 'select-action', action);
                _updateMap(pluginId);
        }
    };


    // attach click handlers to all the panel buttons
    var _registerButtons = function (pluginId) {
        var selector = hud.pluginSelector(pluginId);
        $(selector + ' a[data-role="select-map"]').on('click', function (e) {
            e.preventDefault();
            selected_map = $(e.currentTarget).attr('data-action');
            _selectBaseLayer(pluginId, selected_map);
        });
    };

    var _updateMap = function (pluginId) {

        //$("*").css("cursor", "wait"); // this call or handling of results by leaflet my take some time 

        var map = rtmap_maps[pluginId];

        resourcesAvailable = hud.getButtonState(pluginId, "select-action", "select-avail");
        resourcesBusy = hud.getButtonState(pluginId, "select-action", "select-busy");
        incidentsImmediate = hud.getButtonState(pluginId, "select-action", "select-c1");
        incidentsOther = false;
        hospitals = hud.getButtonState(pluginId, "select-action", "select-hos");
        standby = hud.getButtonState(pluginId, "select-action", "select-sbp");
        stations = hud.getButtonState(pluginId, "select-action",  "select-stn");
        aeu = hud.getButtonState(pluginId, "select-action", "select-aeu");
        fru = hud.getButtonState(pluginId, "select-action", "select-fru");
        oth = hud.getButtonState(pluginId, "select-action", "select-oth");

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
                        _updateResource(pluginId, item);
                    });
                }

                if (layer.Destinations !== undefined) {
                    // add Destinations to the map
                    _updateDestinations(pluginId, layer.Destinations);
                }

                // now register for updates
                hud.joinLeaveGroup("Resource.Available", pluginId, resourcesAvailable);
                hud.joinLeaveGroup("Resource.Busy", pluginId, resourcesBusy);
                hud.joinLeaveGroup("Resource.Enroute", pluginId, resourcesBusy);

            } // success
            //$("*").css("cursor", "default");
        });
    };

    // the status of a resource has changed. we need to remove it if we're not showing
    // this type of resource
    var _updateResourceStatus = function (pluginId, item) {
        var layer = georesLayer[pluginId];

        resourcesAvailable = hud.getButtonState(pluginId, "select-action", "select-avail");
        resourcesBusy = hud.getButtonState(pluginId, "select-action", "select-busy");

        if (resourcesAvailable === false && item.NewStatusCategory !== "Available")
            _removeExistingFeature(layer, item.FleetNo);

        if (resourcesBusy === false && (item.NewStatusCategory !== "Busy" || item.NewStatusCategory !== "Enroute"))
            _removeExistingFeature(layer, item.FleetNo);
    };

    // update layer with resources
    var _updateResource = function (pluginId, item) {
        var layer = georesLayer[pluginId];

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

    // update layer with destinations
    var _updateDestinations = function (pluginId, destinations) {

        var layer = georesLayer[pluginId];

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

    // create a layer for resources, incidents and other features to live in
    var _createResourcesLayer = function (map) {
        try {
            var georesLayer = L.geoJson("", {
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
            georesLayer.addTo(map);

            return georesLayer;
        }
        catch (e) {
            console.log(e.message);
        }
    };
        
    /// <summary>
    /// Re-centre the maps to new co-ordinates
    /// </summary
    var _panTo = function (pluginId, lat, lng) {
        var map = rtmap_maps[pluginId];
    };

    var _panAndMarkLocation = function (pluginId, locationName, lat, lng) {
        var map = rtmap_maps[pluginId];
    };

    var _setZoomLevel = function (pluginId, z) {
        var map = rtmap_maps[pluginId];
    };

    var _selectBaseLayer = function (pluginId, layerName) {
        var map = rtmap_maps[pluginId];
        for (var layer in rtmap_baseLayers) {
            if (map.hasLayer(rtmap_baseLayers[layer]))
                map.removeLayer(rtmap_baseLayers[layer]);
        }
        map.addLayer(rtmap_baseLayers[layerName]);
    };

    var _selectLayer = function (pluginId, layerName, on) {
        var map = rtmap_maps[pluginId];

        if (on)
            map.addLayer(rtmap_overlayLayers[layerName]);
        else
            if (map.hasLayer(rtmap_overlayLayers[layerName]))
                map.removeLayer(rtmap_overlayLayers[layerName]);

    };

    var _lockMap = function (pluginId, mode) {
        hud.setButtonState(pluginId, "select-action", "lock-map", mode);
    };

    return {
        init: _init,
        selectBaseLayer: _selectBaseLayer,
        lockMap: _lockMap,
        setZoom: _setZoomLevel,
        panTo: _panTo,
        panAndMarkLocation: _panAndMarkLocation
    };

})();