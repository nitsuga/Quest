var hud = hud || {};

hud.plugins = hud.plugins || {};

// keep 3 items per resource
var MAX_TRAIL_ITEMS = 4;

// keep track of map objects belonging to different map plugins
var rtmap_maps = {};

// keep track of res/inc/des items for each map
var georesLayer = {};

// display res trails for each map
var georestrailLayer = {};

// keep track of res items for each map
var reshistory = {};

// keep track of coverage layers for each map
var covlayer = {};

// keep track of searched items for each map
var searchlayer = {};

// keep track of select item for each map
var searchItem = {};

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
            var osmAttrib = "Map data � OpenStreetMap contributors";
            var osm = new L.TileLayer(osmUrl);
            //var osm = new L.TileLayer(osmUrl, { attribution: osmAttrib });
            var mbAttr = 'Map data &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors, ' +
                '<a href="http://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, ' +
                'Imagery � <a href="http://mapbox.com">Mapbox</a>',
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

            var clayer = L.layerGroup();
            clayer.id = "coverage";
            clayer.opacity = 0.3;
            clayer.addTo(map);
            covlayer[pluginId] = clayer;

            // save the map object in a dictionary so it can be accessed later
            rtmap_maps[pluginId] = map;

            // create geo overlay
            georesLayer[pluginId] = _createResourcesLayer(map);

            reshistory[pluginId] = {};
            georestrailLayer[pluginId] = L.layerGroup();
            georestrailLayer[pluginId].addTo(map);
            

            // add a search layer onto the maps and remember it
            searchlayer[pluginId] = new L.featureGroup();
            map.addLayer(searchlayer[pluginId]);

            // select the primary menu
            hud.selectMenu(pluginId, 0);

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

            // listen for local hub messages  
            $("#sys_hub").on("SelectSearchResult", function (evt, data) {
                _selectSearchResult(pluginId, data);
            });

            // listen for local hub messages
            $("#sys_hub").on("PanZoom", function (evt, data) {
                _panZoom(pluginId, data.lat, data.lon, data.zoom);
            });            
            
        });
    };

    // handle actions from button push
    var _handleAction = function (pluginId, action) {
        switch (action.action) {
            case "select-cov":
                _updateCoverage(pluginId, action);
                break;

            case "select-ovl":
                var isOn = hud.toggleButton(pluginId, action);
                _selectLayer(pluginId, action.data, isOn);
                break;

            case "select-inc":
                hud.toggleButton(pluginId, action);
                _updateMap(pluginId);
                break;

            case "select-res":
                hud.toggleButton(pluginId, action);
                _updateMap(pluginId);
                break;

            case "select-des":
                hud.toggleButton(pluginId, action);
                _updateMap(pluginId);
                break;

            case "select-lock":
                hud.toggleButton(pluginId, action);
                break;

            case "select-map":
                // turn off all other map buttons
                hud.setActionState(pluginId, action.action);
                // turn on this map button
                hud.toggleButton(pluginId, action);
                _selectBaseLayer(pluginId, action.data);
                break;

            default:
        }
    };

    // handle message from service bus
    var _handleMessage = function (pluginId, group, msg) {
        var layer = georesLayer[pluginId];
        var history = reshistory[pluginId];
        var traillayer = georestrailLayer[pluginId];     

        switch (msg.$type) {
            case "Quest.Common.Messages.Resource.ResourceUpdate, Quest.Common":
                _updateResource(traillayer, layer, history, msg.Item);
                break;
            case "Quest.Common.Messages.Resource.ResourceStatusChange, Quest.Common":
                _updateResourceStatus(pluginId, msg);
                break;
        }

        _updateTrail(pluginId);
    };

    // call whenever one of the menu selectors for coverage has changed
    var _updateCoverage = function (pluginId, action) {

        var isOn = hud.toggleButton(pluginId, action);

        var map = rtmap_maps[pluginId];

        // get id of coverage layer as an integer
        var id = action.data;
        if (typeof id === "string")
            id = parseInt(id);

        if (isOn) {
            _getCoverage(pluginId, id);
        }
        else {
            _undoCoverage(pluginId, id);
        }
    }

    var _base64DecToArr = function(sBase64, nBlocksSize) {

        var
            sB64Enc = sBase64.replace(/[^A-Za-z0-9\+\/]/g, ""), nInLen = sB64Enc.length,
            nOutLen = nBlocksSize ? Math.ceil((nInLen * 3 + 1 >> 2) / nBlocksSize) * nBlocksSize : nInLen * 3 + 1 >> 2, taBytes = new Uint8Array(nOutLen);

        for (var nMod3, nMod4, nUint24 = 0, nOutIdx = 0, nInIdx = 0; nInIdx < nInLen; nInIdx++) {
            nMod4 = nInIdx & 3;
            nUint24 |= b64ToUint6(sB64Enc.charCodeAt(nInIdx)) << 18 - 6 * nMod4;
            if (nMod4 === 3 || nInLen - nInIdx === 1) {
                for (nMod3 = 0; nMod3 < 3 && nOutIdx < nOutLen; nMod3++ , nOutIdx++) {
                    taBytes[nOutIdx] = nUint24 >>> (16 >>> nMod3 & 24) & 255;
                }
                nUint24 = 0;

            }
        }

        return taBytes;
    }

    var _getCoverage = function (pluginId, id) {

        var layer = covlayer[pluginId];

        hud.joinLeaveGroup("Coverage."+id, pluginId, true);

        return $.ajax({
            url: hud.getURL("RTM/GetVehicleCoverage"),
            data: { 'vehtype': id },               // 1=Amb 2=FRU 7=incidents 8=combined 9=holes 10=standby cover 11=standby compliance
            dataType: "json",
            success: function (msg) {
                console.log("got GetVehicleCoverage pluginId=" + pluginId + " type=" + id);

                if (msg.Success === true && msg.Map !== null) {
                    var res = msg.Map;
                    var data = [];
                    var i = 0;
                    if (res.map !== undefined) {
                        //var raw = window.atob(res.map);
                        for (var y = 0; y < res.rows; y++)
                            for (var x = 0; x < res.cols; x++) {
                                //if (raw.charCodeAt(i) > 0) {
                                var byteArr = window.atob(res.map);

                                //var l = res.map.length;
                                //var c = res.map[i];
                                //var c1 = res.map.charCodeAt(i);

                                var l = byteArr.length;
                                //var c = byteArr[i];
                                var c = byteArr.charCodeAt(i);

                                if (c > 0) {
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
                            nheatmap.id = id;
                            _undoCoverage(pluginId, id);
                            layer.addLayer(nheatmap);
                        }
                    }
                }
            }
        });
    }

    // remove coverge from the map
    var _undoCoverage = function (pluginId, id) {
        var layer = covlayer[pluginId];
        layer.eachLayer(function (sublayer) {
            if (sublayer.id === id) {
                layer.removeLayer(sublayer);
                return;
            }
        });
    }

    var _unselectSearchResult = function (pluginId, data) {
        if (data === undefined)
            return;
        var layer = searchlayer[pluginId];
        layer.eachLayer(function (sublayer) {
            if (sublayer.id === data.address.ID) {
                layer.removeLayer(sublayer);
                var latlng = new L.LatLng(data.address.l.lat, data.address.l.lon);
                var feature = _getFeature(data.address, latlng, false);
                layer.addLayer(feature);
                return;
            }
        });
    }

    var _selectSearchResult = function (pluginId, data) {

        _unselectSearchResult(pluginId, searchItem[pluginId]);

        var layer = searchlayer[pluginId];
        layer.eachLayer(function (sublayer) {
            if (sublayer.id === data.address.ID) {
                layer.removeLayer(sublayer);
                var latlng = new L.LatLng(data.address.l.lat, data.address.l.lon);
                var feature = _getFeature(data.address, latlng, true);
                layer.addLayer(feature);
                searchItem[pluginId] = data;
                return;
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
                feature = _getFeature(doc, latlng, false);
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
    var _getFeature = function (address, latlng, highlight) {
        var latLongParms = {
            lng: address.l.lon,
            lat: address.l.lat
        };

        var latLongString = JSON.stringify(latLongParms);
        var feature;

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
                feature = L.userMarker(latlng, { pulsing: highlight, accuracy: 0, m: "loc", s: address.t });

                feature.on("click",
                    function () {
                        // trigger local event to broadcast this object
                        hud.sendLocal("ObjectSelected", { "Type": "SearchItem", "Value": address });
                        //InformationSearch(latlng.lng, latlng.lat);
                    });
            }

        feature.id = address.ID
        return feature;
    }

    var _updateMap = function (pluginId) {

        //$("*").css("cursor", "wait"); // this call or handling of results by leaflet my take some time 

        var map = rtmap_maps[pluginId];

        var resourcesAvailable = hud.getButtonState(pluginId, { "action": "select-res", "data": "avail" } );
        var resourcesBusy = hud.getButtonState(pluginId, { "action": "select-res", "data": "busy" });
        var incidentsImmediate = hud.getButtonState(pluginId, "select-action", { "action": "select-inc", "data": "c1" });
        var incidentsOther = false;
        var hospitals = hud.getButtonState(pluginId, { "action": "select-des", "data": "hos" });
        var standby = hud.getButtonState(pluginId, { "action": "select-des", "data": "sbp" });
        var stations = hud.getButtonState(pluginId, { "action": "select-des", "data": "std" });

        var aeu = hud.getButtonState(pluginId, { "action": "select-res", "data": "aeu" });
        var fru = hud.getButtonState(pluginId, { "action": "select-res", "data": "fru" });
        var oth = hud.getButtonState(pluginId, { "action": "select-res", "data": "oth" });

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
            success: function (mapdata) {
                if (mapdata === null) {
                    $("#message").text('failed to get a response from the server');
                    return;
                }

                var layer = georesLayer[pluginId];
                var history = reshistory[pluginId];
                var traillayer = georestrailLayer[pluginId];     

                traillayer.clearLayers();
                layer.clearLayers();

                if (mapdata.error !== undefined) {
                    $("#message").html(mapdata.error);
                    $("#message").show();
                    return;
                }

                if (mapdata.Result === null)
                    return;

                // add resources to the map
                if (mapdata.Resources !== undefined) {
                    mapdata.Resources.forEach(function (item) {
                        _updateResource(traillayer, layer, history, item);
                    });
                }

                // update resource trails
                _updateTrail(pluginId);

                if (mapdata.Destinations !== undefined) {
                    // add Destinations to the map
                    _updateDestinations(layer, mapdata.Destinations);
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
        var resourcesAvailable = hud.getButtonState(pluginId, "select-action", "select-avail");
        var resourcesBusy = hud.getButtonState(pluginId, "select-action", "select-busy");

        if (resourcesAvailable === false && item.NewStatusCategory !== "Available")
            _removeExistingFeature(layer, item.FleetNo);

        if (resourcesBusy === false && (item.NewStatusCategory !== "Busy" || item.NewStatusCategory !== "Enroute"))
            _removeExistingFeature(layer, item.FleetNo);
    };

    // updates historic resource items cache and display trails if required
    var _updateTrail = function (pluginId) {
        var history = reshistory[pluginId];
        var traillayer = georestrailLayer[pluginId];     
        var reslayer = georesLayer[pluginId];
    }

    // add resource into the history. The history is a dictionary of resource lists, keyed by fleetno.
    var _addToTrail = function (traillayer, history, resource) {
        var hisrec = history[resource.FleetNo];
        if (hisrec !== undefined) {
            hisrec.unshift(resource); // push this resource to the HEAD of the list, i.e. most recent record
            if (hisrec.length > MAX_TRAIL_ITEMS)
                hisrec = hisrec.slice(0, MAX_TRAIL_ITEMS);

            traillayer.eachLayer(function (lyr) {
                if (lyr.id !== null && lyr.id === resource.FleetNo) {
                    // remove old trail
                    traillayer.removeLayer(lyr);
                }
            });

            // create new trail
            // build up an array of coordinates for the trail
            var coords = [];
            var i = 0;
            hisrec.forEach(function (item) {
                var ll = new L.LatLng(item.Position.Latitude, item.Position.Longitude)
                // hotline z value;
                ll.alt = i++; 
                coords.push(ll);
            });

            var polylineOptions = {
                color: 'blue',
                weight: 2,
                opacity: 0.9
            };

            var hotlineOptions = {
                palette: {
                    0.0: '#000000',
                    0.5: '#808080',
                    1.0: '#FFFFFF'
                },
                weight: 3,
                outlineWidth: 0,
                min: 0,
                max: MAX_TRAIL_ITEMS
            };

            

            //var line = L.polyline(coords, polylineOptions);
            var line = L.hotline(coords, hotlineOptions);

            line.id = resource.FleetNo;
            traillayer.addLayer(line);

        }
        else {
            hisrec = [resource];
            // no trail to display
        }

        history[resource.FleetNo] = hisrec;
    }

    // update layer with resources
    // layer: geojson leaflet layer
    // trail: stack of historic messages
    // item: QuestResource
    var _updateResource = function (traillayer, layer, history, item) {

        _addToTrail(traillayer, history, item);

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
    var _updateDestinations = function (layer, destinations) {

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
        
    var _panZoom = function (pluginId, lat, lon, zoom) {
        var map = rtmap_maps[pluginId];
        latlng = new L.LatLng(lat, lon);
        map.setView(latlng, zoom);
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
        panZoom: _panZoom,
    };

})();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              