var hud = hud || {};

hud.plugins = hud.plugins || {};

hud.plugins.rtmap = (function() {

    var markersi, markersr, markersd, georesLayer;

    var _initMap = function (panel) {
        L_PREFER_CANVAS = true;

        var osmUrl = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
        var osmAttrib = "Map data © OpenStreetMap contributors";
        osm = new L.TileLayer(osmUrl, { attribution: osmAttrib });
        var mbAttr = 'Map data &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors, ' +
            '<a href="http://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, ' +
            'Imagery © <a href="http://mapbox.com">Mapbox</a>',
            mbUrl = "https://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoibWFwYm94IiwiYSI6ImNpandmbXliNDBjZWd2M2x6bDk3c2ZtOTkifQ._QA7i5Mpkd_m30IGElHziw";

        var grayscale = L.tileLayer(mbUrl, { id: "mapbox.light", attribution: mbAttr }),
            streets = L.tileLayer(mbUrl, { id: "mapbox.streets", attribution: mbAttr });

        //var googleLayer1 = new L.Google('ROADMAP');
        //var googleLayer2 = new L.Google('SATELLITE');
        //var googleLayer3 = new L.Google('HYBRID');
        //var googleLayer4 = new L.Google('TERRAIN');

        barts = L.tileLayer.wms("http://86.29.75.151:8090/cgi-bin/mapserv?MAP=/maps/extent.map&crs=EPSG:27700", { layers: "Barts", format: "image/png", maxZoom: 22, minZoom: 0, continuousWorld: true, noWrap: true });
        stations = L.tileLayer.wms("http://86.29.75.151:8090/cgi-bin/mapserv?MAP=/maps/extent.map", { layers: "Stations", format: "image/png", transparent: true, maxZoom: 22, minZoom: 0, continuousWorld: true, noWrap: true });

        baseLayers = {
            "OSM": osm,
            "Grayscale": grayscale,
            "Mapbox Streets": streets,
            "Barts": barts //,
            //"Google Road": googleLayer1,
            //"Google Satellite": googleLayer2,
            //"Google Hybrid": googleLayer3,
            //"Google Terrain": googleLayer4
        };
        baseLayer = osm;

        var overlayLayers = {
            "Stations": stations
        };

        lat = 51.5;
        lng = -0.2;
        zoom = 12;

        var mapdiv = 'map' + panel;

        map = new L.Map(mapdiv, {
            center: new L.LatLng(lat, lng),
            zoom: zoom,
            layers: baseLayer,
            zoomControl: false,
            continuousWorld: true,
            worldCopyJump: false,
            inertiaDeceleration: 10000
        });

        //_registerButtons(panel);

        hud.selectPanelMenu(panel, 0);
        
        // listen for hub messages on these groups
        $("#sys_hub").on("Resource.Available Resource.Busy Resource.Enroute", function (group, msgtxt) {
            _handleMessage(panel, georesLayer, msgtxt);
        });

        // listen for panel actions
        $('[data-panel-role=' + panel + ']').on("action", function (evt, action) {
            _handleAction(panel, action);
        });

    };

    // handle message from service bus
    var _handleMessage = function (panel, georesLayer, msgtxt) {
        var msg = JSON.parse(msgtxt);
        switch (msg.$type) {
            case "Quest.Common.Messages.Resource.ResourceUpdate, Quest.Common":
                _updateResource(georesLayer, msg.Item);
                break;
            case "Quest.Common.Messages.Resource.ResourceStatusChange, Quest.Common":
                _updateResourceStatus(panel, georesLayer, msg);
                break;
        }
    };

    // get selector for a named button
    var _buttonSelector = function (panel, name) {
        return "div[data-panel-role='" + panel + "'] .map-container a[data-action='" + name + "']";
    };
        
    // handle actions from button push
    var _handleAction = function (panel, action) {
        hud.toggleButton(panel, 'select-action', action);
        _updateMap(panel);
    };

    // attach click handlers to all the panel buttons
    //var _registerButtons = function (panel) {
    //    select_button = "div[data-panel-role='" + panel + "'] .map-container .panel-btn";
    //    $(select_button).on("click", function (btn) {
    //        // get name of button
    //        btn_role = $(btn.currentTarget).attr('data-role');
    //        btn_action = $(btn.currentTarget).attr('data-action');
    //        switch (btn_role)
    //        {
    //            case 'select-map':
    //                // select map 'btn_action'
    //                hud.toggleButton(btn.currentTarget);
    //                _updateMap(panel);
    //                break;
    //            case 'select-action':
    //                hud.toggleButton(btn.currentTarget);
    //                _updateMap(panel);
    //                break;
    //        }
    //    });
    //}

    var _updateMap = function (panel) {


        //$("*").css("cursor", "wait"); // this call or handling of results by leaflet my take some time 

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
            url: _getURL("RTM/GetMapItems"),
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

                _createResourcesLayer();

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

        resourcesAvailable = hud.getButtonState(_buttonSelector(panel, "select-avail"));
        resourcesBusy = hud.getButtonState(_buttonSelector(panel, "select-busy"));

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
                "name": item.Callsign,
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
                    "name": item.Name,
                    "MarkerType": "DES",
                    "MarkerStatus": _getDestinationMarkerStatus(item)
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

    var _getDestinationMarkerStatus = function (questDestination) {
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

    var _createResourcesLayer = function () {
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
                        $(".resCallsignValue").html(feature.properties.Callsign + " (" + feature.properties.Fleet + ") " + feature.properties.ResourceType);
                        $(".resStatusValue").html(feature.properties.currStatus);
                        $(".resTimeValue").html(feature.properties.Timestamp);
                        $(".resAreaValue").html(feature.properties.Area);
                        $(".resDestinationValue").html(feature.properties.Destination);
                        $(".resEtaValue").html(feature.properties.ETA);
                        $(".resIncidentValue").html(feature.properties.IncSerial);
                        $(".resSkillValue").html(feature.properties.Skill);
                        $(".resCommentValue").html(feature.properties.Comment);
                        //$(".resStandbyValue").html(feature.properties.Standby);
                        $("#modalResourceDetails").modal("show");
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
        
    var _updateStaticMapData = function (panel) {
        _updateMap(panel);
    };

    var _getURL = function (url) {
        var s = _getBaseURL() + "/" + url;
        //console.debug("g url = " + s);
        return s;
    };

    var _getBaseURL = function () {
        var url = location.href;  // entire url including querystring - also: window.location.href;
        var baseUrl = url.substring(0, url.indexOf("/", 10));
        //console.debug("b url = " + baseURL);
        return baseUrl;
    };

    /// <summary>
    /// Re-centre the maps to new co-ordinates
    /// </summary
    var _panTo = function (lat, lng) {
    };

    var _panAndMarkLocation = function (locationName, lat, lng) {
    };

    var _setZoomLevel = function(z) {
    };

    var _redrawMaps = function () {
    };

    var _lockMap = function (mode) {
    };

    return {
        initMap: _initMap,
        redrawMaps: _redrawMaps,
        lockMap: _lockMap,
        setZoom: _setZoomLevel,
        panTo: _panTo,
        panAndMarkLocation: _panAndMarkLocation
    };

})();