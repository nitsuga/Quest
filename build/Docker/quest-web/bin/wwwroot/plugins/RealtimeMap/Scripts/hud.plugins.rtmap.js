var hud = hud || {};

hud.plugins = hud.plugins || {};

var rtmap_maps = {};
var rtmap_baseLayers;
var rtmap_overlayLayers;
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
                zoomControl: true,
                continuousWorld: true,
                worldCopyJump: false,
                inertiaDeceleration: 10000
            });

            L.control.layers(rtmap_maps, rtmap_overlayLayers).addTo(map);

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

            // listen for panel actions
            $('[data-panel-role=' + panel + ']').on("action", function (evt, action) {
                _handleAction(panel, action);
            });

        });
    };

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
                if (layer.Result.Resources !== undefined) {
                    layer.Result.Resources.forEach(function (item) {
                        _updateResource(georesLayer, item);
                    });
                }

                if (layer.Result.Destinations !== undefined) {
                    // add Destinations to the map
                    _updateDestinations(layer.Result.Destinations, georesLayer);
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