﻿var hud = hud || {};

hud.plugins = hud.plugins || {};

var markersi, markersr, markersd, georesLayer;
var select_avail, select_busy, select_c1, select_c2, select_c3, select_c4, select_held, select_aeuc, select_fruc, select_stn, select_sbp, select_hos;

hud.plugins.rtmap = (function() {

    var _initMap = function (role) {
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

        var mapdiv = 'map' + role;

        map = new L.Map(mapdiv, {
            center: new L.LatLng(lat, lng),
            zoom: zoom,
            layers: baseLayer,
            zoomControl: false,
            continuousWorld: true,
            worldCopyJump: false,
            inertiaDeceleration: 10000
        });


        $("#sys_hub").on("Resource.Available", function () {

        });

        select_avail = "div[data-panel-role='" + role + "'] .map-container a[data-role='select-avail']";
        select_busy = "div[data-panel-role='" + role + "'] .map-container a[data-role='select-busy']";
        select_c1 = "div[data-panel-role='" + role + "'] .map-container a[data-role='select-c1']";
        select_c2 = "div[data-panel-role='" + role + "'] .map-container a[data-role='select-c2']";
        select_c3 = "div[data-panel-role='" + role + "'] .map-container a[data-role='select-c3']";
        select_c4 = "div[data-panel-role='" + role + "'] .map-container a[data-role='select-c4']";
        select_held = "div[data-panel-role='" + role + "'] .map-container a[data-role='select-held']";
        select_aeuc = "div[data-panel-role='" + role + "'] .map-container a[data-role='select-aeuc']";
        select_fruc = "div[data-panel-role='" + role + "'] .map-container a[data-role='select-fruc']";
        select_stn = "div[data-panel-role='" + role + "'] .map-container a[data-role='select-stn']";
        select_sbp = "div[data-panel-role='" + role + "'] .map-container a[data-role='select-sbp']";
        select_hos = "div[data-panel-role='" + role + "'] .map-container a[data-role='select-hos']";

        $(select_avail).on("click", function () {
            avail = hud.toggleButton(select_avail);
            _doResources();
        });

        $(select_busy).on("click", function () {
            hud.toggleButton(select_busy);
            _doResources();
        });

        $(select_c1).on("click", function () {
            hud.toggleButton(select_c1);
            _doResources();
        });
        $(select_c2).on("click", function () {
            hud.toggleButton(select_c2);
            _doResources();
        });
        $(select_c3).on("click", function () {
            hud.toggleButton(select_c3);
            _doResources();
        });
        $(select_c4).on("click", function () {
            hud.toggleButton(select_c4);
            _doResources();
        });

        $(select_held).on("click", function () {
            hud.toggleButton(select_held);
            _doResources();
        });

        $(select_aeuc).on("click", function () {
            hud.toggleButton(select_aeuc);
            _doResources();
        });

        $(select_fruc).on("click", function () {
            hud.toggleButton(select_fruc);
            _doResources();
        });

        $(select_hos).on("click", function () {
            hud.toggleButton(select_hos);
            _doResources();
        });

        $(select_sbp).on("click", function () {
            hud.toggleButton(select_sbp);
            _doResources();
        });

        $(select_stn).on("click", function () {
            hud.toggleButton(select_stn);
            _doResources();
        });


        // register for resource available messages
        hud.joinGroup("Resource.Available");
    };

    var _doResources = function() {
        //Create a new empty resources layer and add to map
        if (markersr !== undefined) markersr.clearLayers();

        _createResourcesLayer();

        $("*").css("cursor", "wait"); // this call or handling of results by leaflet my take some time 

        //Make a new request for the new selection
        $.ajax({
            url: _getURL("RTM/GetMapItems"),
            data:
            {
                ResourcesAvailable: hud.getButtonState(select_avail),
                ResourcesBusy: hud.getButtonState(select_busy),
                IncidentsImmediate: hud.getButtonState(select_c1),
                IncidentsOther: false,
                Hospitals: hud.getButtonState(select_hos),
                Standby: hud.getButtonState(select_sbp),
                Stations: hud.getButtonState(select_stn),
            },
            dataType: "json",
            success: function (layer) {

                if (layer.error !== undefined) {
                    $("#message").html(layer.error);
                    $("#message").show();

                }
                else {

                    if (layer.Result == null)
                        return;

                    // add resources to the map
                    if (layer.Result.Resources !== undefined) {
                        layer.Result.Resources.forEach(function (item) {
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
                            georesLayer.addData(geojsonFeature);
                        });
                    }

                    if (layer.Result.Destinations !== undefined) {
                        // add Destinations to the map
                        layer.Result.Destinations.forEach(function (item) {
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
                            georesLayer.addData(geojsonFeature);
                        });
                    }
                }

                $("*").css("cursor", "default");
            }
        });
    }

    var _getDestinationMarkerStatus = function(questDestination)
    {
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
    }

    // remove a feature form the map
    var _removeExistingFeature = function (layer, id) {
        //Leaflet idiosyncracy, cant loop features for a specific layer ?
        map.eachLayer(function (lyr) {
            if (lyr.feature !== null && lyr.feature.properties !== null) {
                if (lyr.feature.id === id) {
                    layer.removeLayer(lyr);
                    //console.log("Resource " + lyr.feature.id + " removed for update");
                }
            }
        });
    }

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
                    var marker = L.userMarker(latlng, { pulsing: false, accuracy: 0, m: feature.properties.MarkerType, s: feature.properties.MarkerStatus});
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
    }

    var updateStaticMapData = function() {
        //doIncidents();
        _doResources();
        //doDestinations();
        //doAllCoverage();
        //SetupNotifications();
    }

    var _getURL = function(url) {
        var s = _getBaseURL() + "/" + url;
        //console.debug("g url = " + s);
        return s;
    }

    var _getBaseURL = function() {
        var url = location.href;  // entire url including querystring - also: window.location.href;
        var baseUrl = url.substring(0, url.indexOf("/", 10));
        //console.debug("b url = " + baseURL);
        return baseUrl;
    }

    /// <summary>
    /// Re-centre the maps to new co-ordinates
    /// </summary
    var _panTo = function (lat, lng) {
    };

    var _panAndMarkLocation = function (locationName, lat, lng) {
    }

    var _setZoomLevel = function(z) {
    };

    var _redrawMaps = function () {
    }

    var _lockMap = function (mode) {
    }

    return {
        initMap: _initMap,
        redrawMaps: _redrawMaps,
        lockMap: _lockMap,
        setZoom: _setZoomLevel,
        panTo: _panTo,
        panAndMarkLocation: _panAndMarkLocation
}
})();