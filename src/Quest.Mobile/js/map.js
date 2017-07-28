var svg;
var map;
var osm;
var markersi, markersr, markersd;
var info, legend;
var ccglayers;
var covlayer;
var loc_marker;
var searchlayer;        // for search items
var searchGroupsLyr;    // for pulsing
var BoundaryLyr;        // for nearby polygon 
var IOIlayer;
var georesLayer;
var geoincLayer;
var routeLayer;         // layer for displaying routes on
var wto;                        // keystroke time
var notificationService;        // link to resource playback
var searchText = "";
var currentPanel;

// visuals
var timeline_timeline;
var timeline_groups = new vis.DataSet();
var timeline_items = new vis.DataSet();
var timeLineContainer;
var vislayer;


var margin = { top: 20, right: 20, bottom: 200, left: 40 },
              width = 960 - margin.left - margin.right,
              height = 500 - margin.top - margin.bottom;


$(function () {

    L_PREFER_CANVAS = true;
    
    // set up exact matching to start with 
    var mode = getStore("relaxMode");
    if (mode === undefined)
        mode = 0;

    setSearchMode(mode);

    setupActions();

    setStore("#use-clusters", false, 365);

    initVisuals();

    drawMap();

    updateStaticMapData();

    updateDynamicMapData();

    // initialise the telephony
    initTelephony(map);

    // initialise the intel layer
    initIntel(map);

    $.pnotify.defaults.styling = "bootstrap3";
    $.pnotify.defaults.history = false;

    $("#search_input_text").focus();

});

function setupActions() {
    
    $("#SearchDialog").dialog({
        position: { my: "left top", at: "left bottom", of: "#banner" },
        show: { effect: "fade", duration: 300 },
        width: 460,
        draggable: true,
        title: "Location",
        collision: "fit",
        autoOpen: true
    });


    $("#RoutingDialog").dialog({
        position: { my: "left top", at: "left bottom", of: "#banner" },
        show: { effect: "fade", duration: 300 },
        width: 460,
        draggable: true,
        title: "Routing",
        collision: "fit",
        autoOpen: false
    });

    $("#RoutingCompareDialog").dialog({
        position: { my: "left top", at: "left bottom", of: "#banner" },
        show: { effect: "fade", duration: 300 },
        width: 460,
        draggable: true,
        title: "Routing Compare",
        collision: "fit",
        autoOpen: false
    });

    $("#routecmppanel-btn").click(function () {
        //showSidebar("#directions-sidebar");
        $("#RoutingCompareDialog").dialog("open");
        return true;
    });

    route_cmp_btn

    $("#ResultsDialog").dialog({
        position: { my: "left top", at: "left bottom", of: $("#SearchDialog").dialog("widget") },
//        show: {effect: "fade", duration: 300},
        width: 460,
        maxHeight: 500,
        draggable: true,
        title: "Location Results",
    collision: "fit",
        autoOpen: false
    });

    $("#LOCDialog").dialog({
        position: { my: "left top", at: "right top", of: $("#SearchDialog").dialog("widget") },
        // show: { effect: "fade", duration: 300},
        width: 380,
        draggable: true,
        title: "Location",
        collision: "fit",
        autoOpen: false
    });

    $("#IOIDialog").dialog({
        position: { my: "left top", at: "right top", of: $("#LOCDialog").dialog("widget") },
        //show: { effect: "fade", duration: 300 },
        draggable: true,
        width: 380,
        title: "Infomation",
        collision: "fit",
        autoOpen: false
    });


    $(window).keypress(function (e) {
        var code = e.which || e.keyCode;
        switch (code) {
            case 172:           // shift `
                //do stuff
                $("#search_input_text").focus();
                $("#search_input_text").val("");
                return false;
            default:
                break;
        }
        return true;
    });
     
    $("#boundsfilter").on("click", function () {
        LockSearchToMapArea(!LockSearchToMapArea());
    });

    $(".index-controls").on("click", function (x) {
        $(".index-controls.btn-warning").removeClass("btn-warning").addClass("btn-primary");
        $(this).addClass("btn-warning");
    });

    $("#testcall-btn").click(function () {
        $("#callModal").modal("show");
        return true;
    });
    $("#indexer-btn").click(function () {
        $("#indexerModal").modal("show");
        return true;
    });

    $("#about-btn").click(function () {
        $("#aboutModal").modal("show");
        return true;
    });


    $("#clrroute_btn").click(function () {
        if (routeLayer === undefined)
            routeLayer = new L.featureGroup();
        else
            routeLayer.clearLayers();
        });

        // test route
    $("#route_btn").click(function () {
        var from = $("#route-from").val();
        var to = $("#route-to").val();
        var vehicle = $("#route-vehicle").val();
        var hour = $("#route-hour").val();
        var sc = $("#route-sc option:selected").text();
        $.ajax({
            url: getURL("Home/Route"),
            data:
            {
                from: from,
                to: to,
                roadSpeedCalculator: sc,
                vehicle: vehicle,
                hour: hour
            },
            dataType: "json",
            success: function (response) {

                var pointList = [];
                response.forEach(function (pnt) {
                    var latlng = new L.LatLng(pnt.y, pnt.x);
                    pointList.push(latlng);
                });

                var route = new L.Polyline(pointList, {
                    color: 'blue',
                    weight: 4,
                    opacity: 0.75,
                    smoothFactor: 1
                });

                if (routeLayer === undefined)
                    routeLayer = new L.featureGroup();
                else
                    routeLayer.clearLayers();

                routeLayer.addLayer(route);

                if (routeLayer !== undefined) {
                    var bounds = routeLayer.getBounds();
                    if (bounds.isValid())
                        map.fitBounds(bounds);
                }

                routeLayer.addTo(map);

            },
            error: function (e) {
                alert("Route failed: " + e.responseText);
            }
        });
    });

    ///-------------------------------------------------------------------
    /// visualise a route comparison between routing engine and actual route
    ///-------------------------------------------------------------------
    $("#route_cmp_btn").click(function () {
        var id = $("#route-incrouteid").val();
        var sc = $("#route-sc option:selected").text();
        $.ajax({
            url: getURL("Home/RouteCompare"),
            data:
            {
                id: id,
                roadSpeedCalculator: sc
            },
            dataType: "json",
            success: function (response) {

                var pointList = [];
                response.forEach(function (pnt) {
                    var latlng = new L.LatLng(pnt.y, pnt.x);
                    pointList.push(latlng);
                });

                var route = new L.Polyline(pointList, {
                    color: 'blue',
                    weight: 4,
                    opacity: 0.75,
                    smoothFactor: 1
                });

                if (routeLayer === undefined)
                    routeLayer = new L.featureGroup();
                else
                    routeLayer.clearLayers();

                routeLayer.addLayer(route);

                if (routeLayer !== undefined) {
                    var bounds = routeLayer.getBounds();
                    if (bounds.isValid())
                        map.fitBounds(bounds);
                }

                routeLayer.addTo(map);

            },
            error: function (e) {
                alert("Route failed: " + e.responseText);
            }
        });
    });

    $("#Indexer_start").click(function () {
        $("#indexerModal").modal("hide");

        var Index = $("#Indexer_Index").val();
        var Indexers = $("#Indexer_Indexers").val();
        var IndexMode = $("#Indexer_IndexMode").val();
        var Shards = $("#Indexer_Shards").val();
        var Replicas = $("#Indexer_Replicas").val();
        var UseMaster = $("#Indexer_UseMaster").val();

        StartIndexing(Index, Indexers, IndexMode, Shards, Replicas, UseMaster)
    });

    // test caller number
    $("#testcli_btn").click(function () {
        $("#callModal").modal("hide");

        var cli = $("#test_cli").val();
        var extension = $("#test_ext").val();

        $.ajax({
            url: getURL("Home/SubmitCLI"),
            data:
                {
                    cli: cli,
                    extension: extension
                },
            dataType: "json",
            success: function (response) {
                NewCall(cli);
            },
            error: function () {
                alert("CLI failed");
            }
        });

        LocationSearch({ mode: 1 });                // override to RELAX
        return true;
    });

    $("#settings-res-btn").click(function () {
        $("#settings-res").modal("show");
        return true;
    });

    $("#settings-inc-btn").click(function () {
        $("#settings-inc").modal("show");
        return true;
    });

    $("#settings-cov-btn").click(function () {
        $("#settings-cov").modal("show");
        return true;
    });

    $("#settings-lay-btn").click(function () {
        $("#settings-lay").modal("show");
        return true;
    });

    $("#full-extent-btn").click(function () {
        if (searchlayer !== undefined) {
            var bounds = searchlayer.getBounds();
            if (bounds.isValid())
                map.fitBounds(bounds);
        }
                
        return true;
    });

    $("#searchpanel-btn").click(function () {
        //showSidebar("#locator-sidebar");
        $("#SearchDialog").dialog("open");
        return true;
    });

    $("#routepanel-btn").click(function () {
        //showSidebar("#directions-sidebar");
        $("#RoutingDialog").dialog("open");
        return true;
    });

    $("#timepanel-btn").click(function () {
        //showSidebar("#timeline-sidebar");
        $("#TimelineDialog").dialog("open");
        return true;
    });

    $("#legend-btn").click(function () {
        $("#legendModal").modal("show");
        return true;
    });

    $("#coverage-btn").click(function () {
        doAllCoverage();
    });

    $("#dispatch-btn").button().on("click", function () {
        $("#modalDispatch").modal("show");
    });

    $("#recommend-btn").button().on("click", function () {
        $("#modalDispatch").modal("show");
    });

    $("#dispatch").button().on("click", function () {
        var callsign = $(".dispatchCallsign")[0].value;
        var nearby = $(".resfirst")[0].value === "on";
        var eventId = $(".dispatchEvent").html();
        $.ajax({
            url: getURL("Home/AssignDevice"),
            data:
                {
                    callsign: callsign,
                    eventId: eventId,
                    nearby: nearby
                },
            dataType: "json",
            success: function () {
                //alert(msg);
            },
            error: function () {
                alert("Dispatch failed");
            }
        });

    });

    $("#relax").button().on("click", function () {
        toggleSearchMode();
    });

    $('[data-toggle="toggle"]').each(function (index, element) {
        setSliderFromStore("#" + element.id);
    });

    // save any settings from a toogle
    $('[data-toggle="toggle"]').change(function () {
        var target = $(this)[0];
        setStoreFromSlider("#" + target.id);

        setTimeout(function () {
            updateStaticMapData();
        }, 1000);
    });

    $("#alertcallsign").change(function () {
        if ($(this).is(":checked"))
            $("#modalCallsign").modal("show");
    });

    // someone clicked on the group-by radion button.. repeat the search
    $('input[name="group-opt-radio').change(function () {
        LocationSearch();
    });

    $("#search-coord").on("click", function () {
        var coord = $("#search-coord").data("coord")
        $("#search_input_text").val(coord);
        LocationSearch({ take: 1000 });
    });

    $("#to-coord").on("click", function () {
        var coord = $("#search-coord").data("coord") + " 200m";
        $("#route-to").val(coord);
        //LocationSearch({ take: 1000 });
    });

    $("#from-coord").on("click", function () {
        var coord = $("#search-coord").data("coord") + " 200m";;
        $("#route-from").val(coord);
        //LocationSearch({ take: 1000 });
    });

    

    $("#search_input_text").on("input", function () {
        LocationSearch({ take: 100 });
        clearTimeout(wto);
        wto = setTimeout(function () {
            // LocationSearch();
        }, 1000);
    });

    $("#help").on("show.bs.collapse", function () {
        $("#results").collapse("show");
    });


    $("#go-btn").on("click", function () {
        clearTimeout(wto);
        LocationSearch();
    });

    $("#send-btn").on("click", function () {
        // send back to CAD
    });

    // capture ctr+del and clear search box. 
    // effective only when in search box, if needed outside add to document instead
    $("#search_input_text").keydown(function (event) {
        if (event.ctrlKey && event.which === 46) //ctrl+del
        {
            $("#search_input_text").val("");
        }
    });

}

function LockSearchToMapArea(mode) {
    if (mode === undefined)
        return $("#boundsfilter").hasClass("fa-lock");

    if (mode) {
        $("#boundsfilter").removeClass("fa-unlock").addClass("fa-lock");
        $("#boundsfilter").removeClass("btn-default").addClass("btn-primary");
    }
    else {
        $("#boundsfilter").removeClass("fa-lock").addClass("fa-unlock");
        $("#boundsfilter").removeClass("btn-primary").addClass("btn-default");
    }
}

function toggleSearchMode()
{
    var mode = $("#relax").data("mode");

    if (mode === undefined)
        mode = 0;
    mode = mode + 1;
    setSearchMode(mode);
    LocationSearch();
};

function setSearchMode(mode) {
    mode = parseInt(mode);
    $("#relax").removeClass("btn-default");
    $("#relax").removeClass("btn-warning");
    $("#relax").removeClass("btn-danger");
    if (mode > 2 || mode < 0)
        mode = 0;

    setStore("relaxMode", mode);

    $("#relax").data("mode", mode);
    switch (mode) {
        case 0:
            $("#relax").addClass("btn-default");
            
            $("#relax").text("exact");
            break;
        case 1:
            $("#relax").addClass("btn-warning");
            $("#relax").text("relax");
            break;
        case 2:
            $("#relax").addClass("btn-danger");
            $("#relax").text("fuzzy");
            break;
    }
};

//===================DRAW MAP BEGIN=============================================

function drawMap() {

    //var apiKey = "AqTGBsziZHIJYYxgivLBf0hVdrAk9mWO5cQcb8Yux8sW5M8c8opEC2lZqKR1ZZXf";
    //var mapqaerial = new L.tileLayer(
    //            "http://otile{s}.mqcdn.com/tiles/1.0.0/sat/{z}/{x}/{y}.png", {
    //                attribution: "",
    //                subdomains: "1234",
    //            });

    //-----------ADD OSM BASE LAYER----------
    var osmUrl = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
    var osmAttrib = "Map data © OpenStreetMap contributors";
    osm = new L.TileLayer(osmUrl, { attribution: osmAttrib });
    var mbAttr = 'Map data &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors, ' +
				'<a href="http://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, ' +
				'Imagery © <a href="http://mapbox.com">Mapbox</a>',
			mbUrl = "https://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoibWFwYm94IiwiYSI6ImNpandmbXliNDBjZWd2M2x6bDk3c2ZtOTkifQ._QA7i5Mpkd_m30IGElHziw";

    var grayscale = L.tileLayer(mbUrl, { id: "mapbox.light", attribution: mbAttr }),
        streets = L.tileLayer(mbUrl, { id: "mapbox.streets", attribution: mbAttr });


    var localmode = getStoreAsBool("#localmode");

    var barts;
    var stations;
    if (localmode === true) {
        barts = L.tileLayer.wms("http://127.0.0.1:8090/cgi-bin/mapserv?MAP=/maps/extent.map&crs=EPSG:27700", { layers: "Barts", format: "image/png", maxZoom: 22, minZoom: 0, continuousWorld: true, noWrap: true });
        stations = L.tileLayer.wms("http://127.0.0.1:8090/cgi-bin/mapserv?MAP=/maps/extent.map", { layers: "Stations", format: "image/png", transparent: true, maxZoom: 22, minZoom: 0, continuousWorld: true, noWrap: true });
    } else {
        barts = L.tileLayer.wms("http://86.29.75.151:8090/cgi-bin/mapserv?MAP=/maps/extent.map&crs=EPSG:27700", { layers: "Barts", format: "image/png", maxZoom: 22, minZoom: 0, continuousWorld: true, noWrap: true });
        stations = L.tileLayer.wms("http://86.29.75.151:8090/cgi-bin/mapserv?MAP=/maps/extent.map", { layers: "Stations", format: "image/png", transparent: true, maxZoom: 22, minZoom: 0, continuousWorld: true, noWrap: true });
    }

    //TODO: setOpacity for layers should be on the UI and remembered
    barts.setOpacity(1);

    var lat = getStore("lat");
    if (lat === null)
        lat = 51.5;
    else
        lat = Number(lat);

    var lng = getStore("lng");
    if (lng === null)
        lng = -0.2;
    else
        lng = Number(lng);

    var zoom = getStore("zoom");
    if (zoom === null)
        zoom = 12;
    else
        zoom = Number(zoom);


    var baseLayers;
    var baseLayer;

    if (localmode === false)
    {
//        jQuery.getScript("http://maps.google.com/maps/api/js?v=3.2&sensor=false");
        var googleLayer1 = new L.Google('ROADMAP');
        var googleLayer2 = new L.Google('SATELLITE');
        var googleLayer3 = new L.Google('HYBRID');
        var googleLayer4 = new L.Google('TERRAIN');

        baseLayers = {
            "OSM": osm,
            "Grayscale": grayscale,
            "Mapbox Streets": streets,
            "Barts": barts
            , "Google Road": googleLayer1,
            "Google Satellite": googleLayer2,
            "Google Hybrid": googleLayer3,
            "Google Terrain": googleLayer4
        };
        baseLayer = osm;
    }
    else
    {
        baseLayers = {
            "Barts": barts
        };
        baseLayer = barts;
    }

    var overlayLayers = {
        "Stations": stations
    };


    map = new L.Map("map", {
        center: new L.LatLng(lat, lng),
        zoom: zoom,
        layers: baseLayer,
        zoomControl: false,
        continuousWorld: true,
        worldCopyJump: false,
        inertiaDeceleration: 10000
    });


    L.control.layers(baseLayers, overlayLayers).addTo(map);

    new L.Control.Zoom({ position: "topright" }).addTo(map);
    L.control.scale().addTo(map);


    var editableLayers = new L.FeatureGroup();
    map.addLayer(editableLayers);

    var MyCustomMarker = L.Icon.extend({
        options: {
            shadowUrl: null,
            iconAnchor: new L.Point(12, 12),
            iconSize: new L.Point(24, 24),
            iconUrl: 'link/to/image.png'
        }
    });

    var options = {
        position: 'topright',
        draw: {
            polyline: {
                shapeOptions: {
                    color: '#f357a1',
                    weight: 10
                }
            },
            polygon: {
                allowIntersection: false, // Restricts shapes to simple polygons
                drawError: {
                    color: '#e1e100', // Color the shape will turn when intersects
                    message: '<strong>Oh snap!<strong> you can\'t draw that!' // Message that will show when intersect
                },
                shapeOptions: {
                    color: '#bada55'
                }
            },
            circle: false, // Turns off this drawing tool
            rectangle: {
                shapeOptions: {
                    clickable: false
                }
            },
            marker: {
                icon: new MyCustomMarker()
            }
        },
        edit: {
            featureGroup: editableLayers, //REQUIRED!!
            remove: false
        }
    };

    var drawControl = new L.Control.Draw(options);
    map.addControl(drawControl);

        // ReSharper disable once InconsistentNaming
    BoundaryLyr = new L.featureGroup();
    BoundaryLyr.addTo(map);

    map.on("moveend", function () {
        var c = map.getCenter();
        setStore("lat", c.lat, 365);
        setStore("lng", c.lng, 365);

    });

    map.on("zoomend", function () {
        setStore("zoom", map.getZoom(), 365);
    });

    //map.on("popupopen", function (e) {
    //    var latlng = e.popup.getLatLng();
    //    InformationSearch(latlng.lng, latlng.lat);
    //});
    map.on("click", function (e)
    {
        var txt = Math.round(e.latlng.lat * 100000) / 100000 + "," + Math.round(e.latlng.lng * 100000) / 100000;
        $("#search-coord").data("coord",txt);
    });

    //TODO: This fails for some reason
    //map.on(L.Draw.Event.CREATED, function (e) {
    //    var type = e.layerType,
    //        layer = e.layer;

    //    if (type === 'marker') {
    //        layer.bindPopup('A popup!');
    //    }

    //    editableLayers.addLayer(layer);
    //});

}

function LocationSearch( options )//callback for 3rd party ajax requests
{
    var text = $("#search_input_text").val();
    var take = 20;
    var mode = $("#relax").data("mode");

    if (BoundaryLyr!==undefined)
        BoundaryLyr.clearLayers();

    if (options !== undefined) {
        if (options.text !== undefined)
            text = options.text;

        if (options.take !== undefined)
            take = options.take;

        if (options.mode !== undefined)
            mode = options.mode;
    }

// ReSharper disable once QualifiedExpressionMaybeNull
    text = text.trim();

    var resultsGroupOpt = $("input[name='group-opt-radio']:checked").val();

    var indexGroup = $(".index-controls.btn-warning").first();

    if (text === "")
        return false;

    $("#message").hide();
    $("#message-wait").show();

    var terms = ""; // GetFilterTerms();
    
    var bb = map.getBounds();
    var parms = {
        searchText: text,
        searchMode: mode,
        includeAggregates: false,
        skip: 0,
        take: take,
        boundsfilter: $("#boundsfilter").hasClass("fa-lock"),
        w: bb.getWest(),
        s: bb.getSouth(),
        e: bb.getEast(),
        n: bb.getNorth(),
        filterterms: terms,
        displayGroup: resultsGroupOpt,
        indexGroup: indexGroup[0].id
    };

    return $.ajax({
        url: getURL("api/Search/Find"),
        data: parms,
        dataType: "json",
        success: function (items) {
            clrSearchItems();
            clrInfoItems();

            $("#message-wait").hide();

            if (items !== undefined && items.error !== undefined) {
                $("#message").html(items.error);
                $("#message").show();
            }
            else {
                showGroupedSearchItems(items);
                var coords = GetCoordinatesFromPoly(items.Bounds);
                DisplayBoundary(BoundaryLyr, coords);

                $("#message").show();
                $("#message-wait").hide();
            }
            if ($("#ResultsDialog").dialog("isOpen") !== true) {
                $("#ResultsDialog").dialog("open");
                // position newly opened dialog (using its parent container) below $div.
                //$("#ResultsDialog")
                //    .dialog('widget')
                //    .position({
                //        my: "left top",
                //        at: "left bottom",
                //        of: "#SearchDialog"
                //    });
            }
            $("#search_input_text").focus();

        },
        error: function () {
            $("#message").show();
            $("#message-wait").hide();
        }
    });
}

// Triggered when a user wants to know more about a layer
//TODO: get the backend to pass comprehensive information about this item such as grid reference, lat lon, atom etc
function InformationSearch(longitude, latitude, callResponse)//callback for 3rd party ajax requests
{
    console.log("Clicked at lat: " + latitude + " long: " + longitude);

    var parms = {
        lng: longitude,
        lat: latitude
    };

    return $.ajax({
        url: getURL("Home/InformationSearch"),
        data: parms,
        dataType: "json",
        success: function (items) {
            showInfoItems(items);
            if (callResponse !== undefined)
                callResponse(items);
        },
        error: function () {
            //   alert("Search failed - " + textStatus + " " + errorThrown);
        }
    });
}

function updateStaticMapData() {
    doIncidents();
    doResources();
    doDestinations();
    doAllCoverage();

    SetupNotifications();
}

// update any dynamic data such as coverage
function updateDynamicMapData() {
    doAllCoverage();

    setTimeout(function () {
        updateDynamicMapData();
        },
        10000);
}

function clrSearchItems()
{
    $("#result-list").find("li").remove();
    $("#grouped-results-list").empty();
    $("#grouped-results-list").html("");

    if (searchlayer !== undefined)
        map.removeLayer(searchlayer);
}

function clrInfoItems() {
    if ($("#IOIDialog").dialog("isOpen")) {
        $("#IOIDialog").dialog("close");
    }

    $("#IOIContent").remove();
    $("#IOIDialog").append($("<div>").attr("id", "IOIContent"));
    if (IOIlayer !== undefined) {
        map.removeLayer(IOIlayer);
    }
}

function showInfoItems(items) {

    clrInfoItems();

    if (items.Documents.length === 0)
        return;

    IOIlayer = new L.featureGroup();

    items.Documents.forEach(
    function (address) {

        var latlng = new L.LatLng(address.l.lat, address.l.lon);
        var feature = getFeature(address, latlng);

        IOIlayer.addLayer(feature);

        var title = address.d.length > 27 ? address.d.substring(0, 30) + "..." : address.d;

        $("#IOIContent").append($("<h3>").text(title));
        $("#IOIContent").append($("<div>")
                .append($("<span>").addClass("glyphicon glyphicon-map-marker text-muted c-info c-info-icon")
                                    .attr("title", address.d)
                                    .attr("data-toggle", "tooltip")
                                    .on("click", function () {
                                        map.setView(latlng, 18, { animate: true });
                                    })

                )
                //.append($("<span>").text(address.t).addClass("text-muted"))
                .append($("<span>").text(address.d).addClass("name"))
                .append($("<span>")
                        .addClass("glyphicon glyphicon-pencil text-muted c-info")
                        .attr("title", "type")
                        .attr("data-toggle", "tooltip")
                )
                .append($("<span>").addClass("glyphicon glyphicon-stats text-muted c-info").attr("title", "details").attr("data-toggle", "tooltip"))
                .append($("<span>").html(address.c).addClass("text-muted"))
            );
        } // function
    ); // foreach

    IOIlayer.addTo(map);
    IOIlayer.bringToFront();
    if (!$("#IOIDialog").dialog("isOpen")) {
        $("#IOIDialog").dialog("open");

    }
    $("#IOIContent").accordion({
        heightStyle: "content",
        collapsible: true,
        active: false
    });
}

// make a map object for this address. could be point line or area
function getFeature(address, latlng) {
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
    if (address.pg !== null) {
        address.pg.coordinates.forEach(function (coords) {
            coords.forEach(function (pnt) {
                var latlng = new L.LatLng(pnt[1], pnt[0]);
                polylineCoordinates.push(latlng);
            });

        });
        feature = L.polygon(polylineCoordinates, { color: "lightyellow" });
//        feature.bindPopup(address.d);
            feature.on("click",
                function () {
                    $("#LOCDialog").dialog("open");
                    $("#LOCContent").html(content);
                    InformationSearch(latlng.lng, latlng.lat);
                });
                }
    else
        if (address.ml !== null) {
            address.ml.coordinates.forEach(function (coords) {
                coords.forEach(function (pnt) {
                    var latlng = new L.LatLng(pnt[1], pnt[0]);
                    polylineCoordinates.push(latlng);
                });

            }
        );
            feature = L.polyline(polylineCoordinates, { color: "blue" });
//            feature.bindPopup(content, customOptions);
            feature.on("click",
                function () {
                    $("#LOCDialog").dialog("open");
                    $("#LOCContent").html(content);
                    InformationSearch(latlng.lng, latlng.lat);
                });
        }
        else {
            feature = L.userMarker(latlng, { pulsing: false, accuracy: 0, m: "loc", s: address.t });
            //feature.bindPopup(content, customOptions);
            feature.on("click",
                function () {
                    clrInfoItems();
                    $("#LOCDialog").dialog("open");
                    $("#LOCContent").html(content);
                    InformationSearch(latlng.lng, latlng.lat);
                });
        }
    return feature;
}

function addSingleFeatureToResultsList(containerId, address, feature, score, latlng)
{
    $(containerId).append($("<a>").attr("class", "list-group-item").attr("href", "#")
    .on("click", function ()
    {
        SetFinalAddress(address.d, latlng, 0);

        if (feature !== undefined)
        {
            map.setView(latlng, 16, { animate: true });
            feature.openPopup();
        }
    })
    .hover(function ()
        {
        if (searchGroupsLyr === undefined) {
// ReSharper disable once InconsistentNaming
                searchGroupsLyr = new L.featureGroup();
                searchGroupsLyr.addTo(map);
            }
            else
            {
                searchGroupsLyr.clearLayers();
            }

// ReSharper disable once InconsistentNaming
            var tempGroupLyr = new L.featureGroup();
            tempGroupLyr.addLayer(feature);
            var extent = tempGroupLyr.getBounds();

            var pulsingIcon = L.icon.pulse({ iconSize: [20, 20], color: "#3f007d" });
            var pulsingMarker = L.marker(extent.getCenter(), { icon: pulsingIcon });
            searchGroupsLyr.addLayer(pulsingMarker);
        }, function ()
        {
            if (searchGroupsLyr !== undefined)
                searchGroupsLyr.clearLayers();
        })
    .append($("<span>").addClass("glyphicon glyphicon-map-marker text-muted c-info c-info-icon")
                        .attr("title", "go to ")
                        .attr("data-toggle", "tooltip")
                        .on("click", function ()
                        {
                            $("#search_input_text").val(address.d);
                        })
                        )
    .append($("<span>")
            .text(address.d)
            .addClass("name"))
            .attr("title", address.t + " from " + address.src + " Score= " + score + " " + address.st)
            .attr("data-toggle", "tooltip"));
}

function DisplayBoundary(layer, bounds, append, color) {
    if (color === undefined)
        color = "red";
    if (!append)
        layer.clearLayers();
    var feature = L.polygon(bounds, { color: color, fill: false });
    layer.addLayer(feature);
}

function SetFinalAddress(text, latlng, id) {
    $("#final_text").val(text);
};

function showGroupedSearchItems(items) {
    clrSearchItems();
// ReSharper disable once InconsistentNaming
    searchlayer = new L.featureGroup();

    var itext = items.Count.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");

    if (items.Removed > 0)
        $("#message").html("  found: <B>" + itext + "</B> items (" + items.Removed + " dups) in " + items.ms + "ms");
    else
        $("#message").html("  found: " + itext + " items in " + items.ms + "ms");
    var docindex;
    var doc;
    var score;
    var latlng;
    var feature;

    if (items.Grouping === null || items.Grouping.length === 1) {

        for (docindex = 0; docindex < items.Documents.length; docindex++)
        {
            doc = items.Documents[docindex];
            score = doc.s;
            if (doc.l !== undefined && doc.l!==null) {
                latlng = new L.LatLng(doc.l.lat, doc.l.lon);
                feature = getFeature(doc, latlng);
                searchlayer.addLayer(feature);

                addSingleFeatureToResultsList("#grouped-results-list", doc, feature, score, latlng);

                if (docindex === 0)
                    SetFinalAddress(doc.d, latlng, 0);
            }
        }
    }
    else {

        for (var i = 0 ; i < items.Grouping.length; i++) {
            var grp = items.Grouping[i];

            // get first document
            docindex = grp[0];
            doc = items.Documents[docindex];
            if (grp.length > 1) //these are grouped
            {
                $("#grouped-results-list").append($("<div>").attr("id", "group-results-" + i));

                if (items.Documents.length <= 20) {
                    $("#group-results-" + i).append("<a href='#' class='list-group-item' data-toggle='collapse' data-target='" + "#group-results-sm-" + i + "' data-parent='#menu'>" + "<span class='text-primary'>" + doc.grp + "</span>" + "<span class='glyphicon glyphicon-minus pull-right'></span></a>");
                    $("#group-results-" + i).append($("<div>").attr("id", "group-results-sm-" + i).attr("class", "sublinks"));
                }
                else {
                    $("#group-results-" + i).append("<a href='#' class='list-group-item' data-toggle='collapse' data-target='" + "#group-results-sm-" + i + "' data-parent='#menu'>" + "<span class='text-primary'>" + doc.grp + "</span>" + "<span class='glyphicon glyphicon-plus pull-right'></span></a>");
                    $("#group-results-" + i).append($("<div>").attr("id", "group-results-sm-" + i).attr("class", "sublinks collapse"));

                }

                //var groupFeatures = new Array();
                for (var j = 0; j < grp.length; j++) {
                    docindex = grp[j];
                    doc = items.Documents[docindex];
                    score = doc.s;
                    latlng = new L.LatLng(doc.l.lat, doc.l.lon);
                    feature = getFeature(doc, latlng);
                    searchlayer.addLayer(feature);
                    addSingleFeatureToResultsList("#group-results-sm-" + i, doc, feature, score, latlng);
                }

            }
            else //these  are not grouped
            {
                score = doc.s;
                latlng = new L.LatLng(doc.l.lat, doc.l.lon);
                feature = getFeature(doc, latlng);
                searchlayer.addLayer(feature);
                addSingleFeatureToResultsList("#grouped-results-list", doc, feature, score, latlng);
            }
        }
    }

    var locked = $("#boundsfilter").hasClass("fa-lock");

    // zoom in..
    if (!locked && items.Documents.length > 0)
    {
        var bounds = searchlayer.getBounds();
        if (bounds.isValid())
            map.fitBounds(bounds, { maxZoom: 18 });//works!    

    }
    searchlayer.addTo(map);
}

function addGroupHighlightFeature(groupDivId)
{
    var data = $(groupDivId).data("features");

    //we only use it for calculating extent
    if (searchGroupsLyr === undefined) {
// ReSharper disable once InconsistentNaming
        searchGroupsLyr = new L.featureGroup();
        searchGroupsLyr.addTo(map);
    }
    else
    {
        searchGroupsLyr.clearLayers();
    }

// ReSharper disable once InconsistentNaming
    var tempGroupLyr = new L.featureGroup();
    for (var i = 0; i < data.length; i++)
    {
        var f = data[i];
        tempGroupLyr.addLayer(f);
    }

    var extent = tempGroupLyr.getBounds();

    var pulsingIcon = L.icon.pulse({ iconSize: [30, 30], color: "#3f007d" });
    var pulsingMarker = L.marker(extent.getCenter(), { icon: pulsingIcon });
    searchGroupsLyr.addLayer(pulsingMarker);

    $(groupDivId).data("pulsingMarker", pulsingMarker);
    $(groupDivId).data("extent", extent);
}

function setHighlightForGroupResults(i)
{
    //store the features for this group
    $("#group-results-" + i).hover(function (e)
    {
        var pulsingMarker = $("#" + e.currentTarget.id).data("pulsingMarker");

        if (e.type === "mouseenter")
        {
            if (pulsingMarker === null)
            {
                addGroupHighlightFeature("#" + e.currentTarget.id);
            }
            else
            {
                searchGroupsLyr.addLayer(pulsingMarker);
            }
        }
        else //mouse leave
        {
            searchGroupsLyr.clearLayers();
        }
    });

    $("#group-results-" + i).on("show.bs.collapse", function (e)
    {
        if (searchGroupsLyr !== undefined)
            searchGroupsLyr.clearLayers();

        var pulsingMarker = $("#" + e.currentTarget.id).data("pulsingMarker");

        if (pulsingMarker === null)
        {
            addGroupHighlightFeature("#" + e.currentTarget.id);
        }
        else //we cant be sure if it was on or of
        {
            searchGroupsLyr.removeLayer(pulsingMarker);
            searchGroupsLyr.addLayer(pulsingMarker);
        }
        
        var extent = searchGroupsLyr.getBounds();
        if (extent.isValid())
        {
            map.fitBounds(extent, { maxZoom: 20 });
        }

        $(this).find(".glyphicon-plus").removeClass("glyphicon-plus").addClass("glyphicon-minus");
    });

    $("#group-results-" + i).on("hide.bs.collapse", function ()
    {
        if (searchGroupsLyr !== undefined)
            searchGroupsLyr.clearLayers();

        $(this).find(".glyphicon-minus").removeClass("glyphicon-minus").addClass("glyphicon-plus");
    });
}

function customTip(text) {
    return '<a href="#">' + text + '<em style="background:' + text + '; width:14px;height:14px;float:right"></em></a>';
}

function doDestinations() {
    var layerhospother = getStoreAsBool("#layer-hosp-other");
    var layersbp = getStoreAsBool("#layer-sbp");
    var layerhospae = getStoreAsBool("#layer-hosp-ae");
    var layerstation = getStoreAsBool("#layer-station");
    var layerroad = getStoreAsBool("#layer-road");

    if (layersbp === false && layerhospother === false && layerhospae === false && layerstation === false && layerroad === false && layerhospother === false) {
        if (markersd !== undefined)
            map.removeLayer(markersd);
        return;
    }

    $.ajax({
        url: getURL("Home/GetDestinations"),
        data:
            {
                hosp: layerhospother,
                standby: layersbp,
                station: layerstation,
                road: layerroad,
                ae: layerhospae
            },
        dataType: "json",
        success: function (layer) {
            var useclusters = getStoreAsBool("#use-clusters");

            if (markersd !== undefined)
                map.removeLayer(markersd);

            if (useclusters === true)
                markersd = L.MarkerClusterGroup(
                     {
                         iconCreateFunction: function (cluster) {
                             var childCount = cluster.getChildCount();
                             return new L.DivIcon(
                                 {
                                     html: "<div><span><b>" + childCount + "</b></span></div>", className: "destinationcluster", iconSize: new L.Point(40, 40)
                                 });
                         }
                     });

            if (markersd === undefined)
                markersd = L.layerGroup();

            var geoJsonLayer = L.geoJson(layer, {

                pointToLayer: function (feature, latlng) {
                    var marker = L.userMarker(latlng, { pulsing: false, accuracy: 0, m: "des", s: feature.properties.Status });
                    return marker;
                },

                onEachFeature: function (feature, layer) {
                    layer.on("click", function () {
                        switch (feature.properties.covertier) {
                            case 0:
                                $(".des_covertier").html("N/A");
                                break;
                            case 1:
                                $(".des_covertier").html("Desirable");
                                break;
                            case 2:
                                $(".des_covertier").html("Essential");
                                break;
                        }

                        $(".des_destination").html(feature.properties.destination);
                        $(".des_nearby").html(feature.properties.nearby);
                        $(".des_destype").html(feature.properties.destype);

                        $("#modalDestinationDetails").modal("show");

                    });

                }
            });
            markersd.addLayer(geoJsonLayer);
            markersd.addTo(map);
        }
    });

}

function doIncidents() {
    var catA = getStoreAsBool("#layer-inc-cata");
    var catC = getStoreAsBool("#layer-inc-catc");

    //Create a new empty resources layer and add to map
    if (markersi !== undefined) markersi.clearLayers();

    if (catA === false && catC === false) {
        return;
    }

    CreateIncidentsLayer();

    $("*").css("cursor", "wait"); // this call or handling of results by leaflet my take some time 

    $.ajax({
        url: getURL("Home/GetIncidents"),
        data:
            {
                includeCatA: catA,
                includeCatB: catC
            },
        dataType: "json",
        success: function (layer) {
            if (layer.error !== undefined) {
                $("#message").html(layer.error);
                $("#message").show();

            }
            else {
                //if this is not the first time, we may have existing resources on 
                geoincLayer.addData(layer);
                map.spin(false);
            }
            $("*").css("cursor", "default");
        }
        
    });

}

function doResources() {
    var avail = getStoreAsBool("#layer-res-avail");
    var busy = getStoreAsBool("#layer-res-busy");

    //Create a new empty resources layer and add to map
    if (markersr !== undefined) markersr.clearLayers();
    CreateResourcesLayer();

    if (avail === false && busy === false)
        return;


    $("*").css("cursor", "wait"); // this call or handling of results by leaflet my take some time 

    //Make a new request for the new selection
    $.ajax({
        url: getURL("Home/GetResources"),
        data:
            {
                avail: avail,
                busy: busy
            },
        dataType: "json",
        success: function (layer) {

            if (layer.error !== undefined) {
                $("#message").html(layer.error);
                $("#message").show();

            }
            else {
                georesLayer.addData(layer);
                map.spin(false);
            }
            $("*").css("cursor", "default");
        }
    });
}

function doAllCoverage() {

    if (covlayer === undefined) {
        covlayer = L.layerGroup();
        covlayer.addTo(map);
        covlayer.opacity = 0.3;
    }

    // 1=Amb 2=FRU 7=incidents 8=combined 9=holes 10=standby cover 11=standby compliance
    var covamb = getStoreAsBool("#cov-amb");
    var covfru = getStoreAsBool("#cov-fru");
    var covcombined = getStoreAsBool("#cov-combined");
    var covholes = getStoreAsBool("#cov-holes");
    var covinc = getStoreAsBool("#cov-inc");

    if (covamb === true)
        doCoverage(1, "amb");
    else
        undoCoverage("amb");

    if (covfru === true)
        doCoverage(2, "fru");
    else
        undoCoverage("fru");

    if (covinc === true)
        doCoverage(7, "inc");
    else
        undoCoverage("inc");


    if (covcombined === true)
        doCoverage(8, "combined");
    else
        undoCoverage("combined");

    if (covholes === true)
        doCoverage(9, "holes");
    else
        undoCoverage("holes");

}

function undoCoverage(name) {
    // remove previous layer with same name;
    //covlayer.removeLayer(name);

    covlayer.eachLayer(function (layer) {
        if (layer.id === name) {
            covlayer.removeLayer(layer);
            return;
        }
    });
}

function doCoverage(type, name) {
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
                            nheatmap = L.heatLayer(data, { max: 5.0, radius: 50, maxZoom:11,  gradient: { 0.1: "black", 0.2: "black", 0.3: "black", 0.4: "black" } });
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

//current location for map
function mapLocate() {
    if (loc_marker === undefined) {
        map.locate({ setView: true, maxZoom: 16 });
        map.on("locationfound", onLocationFound);
        map.on("locationerror", onLocationError);
    }
    else {
        map.removeLayer(loc_marker);
        loc_marker = null;
    }
}

function onLocationFound(e) {
    var radius = e.accuracy / 2;
    loc_marker = L.userMarker(e.latlng, { pulsing: true, accuracy: e.accuracy, m: "loc", s: "standard" });
    loc_marker.addTo(map).bindPopup("You are within " + radius + " meters from this point").openPopup();
}

function onLocationError(e) {
    alert(e.message);
    loc_marker = null;
}

function setButtonState(btn, state) {
    $(btn).removeClass("btn-default btn-success btn-warning btn-info");
    $(btn).addClass(state);
}

function GetCoordinatesFromPoly(shape) {
    var polylineCoordinates = [];
    if (shape !== null) {
        shape.coordinates.forEach(function(coords) {
            coords.forEach(function(pnt) {
                var latlng = new L.LatLng(pnt[1], pnt[0]);
                polylineCoordinates.push(latlng);
            });

        });
    }
    return polylineCoordinates;
}

function processRoutingEngineStatus(msg) {
    if (msg.Ready===false)
        $(".routing_engine_status").show();
    else
        $(".routing_engine_status").hide();
}

//Set the web socket to open for updates 
function SetupNotifications() {
    
    if (notificationService === undefined) {

        var httpUrl = getBaseURL() + "api/Notifications/Get";
        var url = httpUrl.replace("http", "ws");

        notificationService = new WebSocket(url);

        notificationService.onopen = function() {
            console.log("connected");
            UpdateNotifications();
        };
        notificationService.onmessage = function (evt) {

            // got a notification, now figure out what it is
            var msg = JSON.parse(evt.data);

            try {
                if (processIntelMessage !== undefined)
                    processIntelMessage(msg);

                if (processTelephonyMessage !== undefined)
                    processTelephonyMessage(msg);

                switch (msg.$type) {
                    case "Quest.Mobile.Models.ResourceFeature, Quest.Mobile":
                        processResourceFeature(georesLayer, msg);
                        break;

                    case "Quest.Mobile.Models.IncidentFeature, Quest.Mobile":
                        processIncidentFeature(geoincLayer, msg);
                        break;

                    case "Quest.Lib.ServiceBus.Messages.RoutingEngineStatus, Quest.Lib":
                        processRoutingEngineStatus(msg);
                        break;
                        
                }
            }
            catch (e) {
                console.log("Update exception: " + e.message);
            }
        };
        notificationService.onerror = function(evt) {
            console.log(evt.message);
        };
        notificationService.onclose = function() {
            console.log("disconnected");
        };
    } else {
        UpdateNotifications();
    }
}

// Send message to notifications controller, passing our update requirements
function UpdateNotifications() {
    //update the parameters of the socket server
    var avail = getStoreAsBool("#layer-res-avail");
    var busy = getStoreAsBool("#layer-res-busy");
    var catA = getStoreAsBool("#layer-inc-cata");
    var catC = getStoreAsBool("#layer-inc-catc");
    var extension= getStore("#extension");

    // this structure should match StateFlags in the Notiifcations controller
    var stateFlags = {
        Avail: avail,
        Busy: busy,
        CatA: catA,
        CatC: catC,
        Extension: extension
    };

    notificationService.send(JSON.stringify(stateFlags));
}

function processIncidentFeature(layer, msg) {
    RemoveExistingFeature(layer, msg.id);
    if (msg.a === "u") {
        layer.addData(msg);
    }
}

function processResourceFeature(layer, msg) {
    RemoveExistingFeature(layer, msg.id);
    if (msg.a === "u") {
        layer.addData(msg);
    }
}

function RemoveExistingFeature(layer, id) {
    //Leaflet idiosyncracy, cant loop features for a specific layer ?
    map.eachLayer(function (lyr) {
        try
        {
            if (lyr.feature !== null && lyr.feature.properties !== null) {
                if (lyr.feature.id === id) {
                    layer.removeLayer(lyr);
                    //console.log("Resource " + lyr.feature.id + " removed for update");
                }
            }
            if (lyr.id === id) {
                layer.removeLayer(lyr);
                //console.log("Resource " + lyr.feature.id + " removed for update");
            }
        }
        catch (e)
        {

        }
    });
}

function CreateIncidentsLayer() {
    try {
        var useclusters = getStoreAsBool("#use-clusters");

        if (useclusters === true)
            markersi = L.MarkerClusterGroup(
            {
                iconCreateFunction: function(cluster) {
                    var childCount = cluster.getChildCount();
                    return new L.DivIcon(
                    {
                        html: "<div><span><b>" + childCount + "</b></span></div>",
                        className: "incidentcluster",
                        iconSize: new L.Point(40, 40)
                    });
                }
            });

        if (markersi === null || markersi === undefined)
            markersi = L.layerGroup();

        geoincLayer = L.geoJson("", {
            style: function () {
                return { color: "#0f0", opacity: 1, fillOpacity: 0.5 };
            },
            pointToLayer: function(feature, latlng) {
                var marker = L.userMarker(latlng,
                {
                    pulsing: false,
                    accuracy: 0,
                    m: "inc",
                    s: feature.properties.Status + "-" + feature.properties.Priority
                });
                marker.title = feature.properties.ID;
                return marker;
            },

            onEachFeature: function(feature, layer) {
                layer.on("click",
                    function() {
                        $(".incLocationValue").html("---");
                        $(".incLocationValue").html(feature.properties.Location);
                        $(".incPriorityValue").html(feature.properties.Status + " " + feature.properties.Priority);
                        $(".incCreatedValue").html(feature.properties.LastUpdate);
                        $(".incResourcesValue").html(feature.properties.Resources);
                        $(".incCadValue").html(feature.properties.IncidentId);
                        $(".incCadValue").html("---");
                        $(".incDeterminantValue").html(feature.properties.Determinant + " " + feature.properties.Description);
                        $(".dispatchEvent").html(feature.properties.incidentid);
                        $(".incPatientValue").html(feature.properties.Sex + "/" + feature.properties.Age);
                        $(".incLocationCommentValue").html(feature.properties.LocationComment);
                        $(".incMaprefValue").html(feature.properties.AZ);
                        $(".incProblemValue").html(feature.properties.ProblemDescription);

                        $("#showDispatch")
                            .button()
                            .on("click",
                                function() {
                                    $("#modalDispatch").modal("show");
                                });

                        $("#modalIncidentDetails").modal("show");
                    });

            }
        });
        markersi.addLayer(geoincLayer);
        markersi.addTo(map);
    } catch (e) {
        console.log(e.message);
    }
}

function CreateResourcesLayer() {
    try {
        var useclusters = getStoreAsBool("#use-clusters");

        if (useclusters === true)
            markersr = new L.MarkerClusterGroup(
                {
                    iconCreateFunction: function (cluster) {
                        var childCount = cluster.getChildCount();
                        return new L.DivIcon({ html: "<div><span><b>" + childCount + "</b></span></div>", className: "resourcecluster", iconSize: new L.Point(40, 40) });
                    }
                });

        if (markersr === null || markersr===undefined)
            markersr = L.layerGroup();

        georesLayer = L.geoJson("", {
            style: function () {
                return { color: "#0f0", opacity: 1, fillOpacity: 0.5 };
            },
            pointToLayer: function (feature, latlng) {
                var marker = L.userMarker(latlng, { pulsing: false, accuracy: 0, m: feature.properties.ResourceTypeGroup, s: feature.properties.StatusCategory });
                marker.title = feature.properties.Callsign;
                return marker;
            },
            onEachFeature: function (feature, layer) {
                layer.on("click", function () {
                    $(".resCallsignValue").html(feature.properties.Callsign + " (" + feature.properties.Fleet + ") " + feature.properties.ResourceType);
                    $(".resStatusValue").html(feature.properties.currStatus );
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

// ReSharper disable once NativeTypePrototypeExtending
String.prototype.capitalizeFirstLetter = function () {
    return this.charAt(0).toUpperCase() + this.slice(1);
}


function StartIndexing(Index, Indexers, IndexMode, Shards, Replicas, UseMaster) {
        //string Index, string Indexers, string IndexMode, int Shards, int Replicas, bool UseMaster
    return $.ajax({
        url: getURL("Home/StartIndexing"),
        data: {
            'Index': Index,
            'Indexers': Indexers,
            'IndexMode': IndexMode,
            'Shards': Shards,
            'Replicas': Replicas,
            'UseMaster': UseMaster
        },              
        dataType: "json",
        success: function (msg) {

            if (msg!== null && msg.Success === true && msg.Map !== null) {
                alert("Indexers started");
            }
        }
    });
}

function initVisuals() {

    $("#properties").dialog({
        position: { my: "left top", at: "right top", of: $("#SearchDialog").dialog("widget") },
        show: { effect: "fade", duration: 300 },
        width: 380,
        draggable: true,
        title: "Properties",
        collision: "fit",
        autoOpen: false
    });

    //Initialise the visualisation datepicker
    var timeline_StartTimeDatePicker = $('#visStartTimeDatetimepicker').datepicker({
        timepicker: true,
        dateFormat: 'dd/mm/yyyy',
        position: 'bottom left',
        language: 'en',
        offset: 350,
        onSelect: function onSelect(fd, date) {
            console.log("Selected date: " + date);
        }
    });

    timeline_StartTimeDatePicker.data('datepicker').selectDate(new Date("August 08, 2014 07:00:00"));

    //Initialise the visualisation datepicker
    var timeline_EndTimeDatePicker = $('#visEndTimeDatetimepicker').datepicker({
        timepicker: true,
        language: 'en',
        dateFormat: 'dd/mm/yyyy',
        position: 'bottom left',
        offset: 300,
        onSelect: function onSelect(fd, date) {
            console.log("Selected date: " + date);
        }
    });

    timeline_EndTimeDatePicker.data('datepicker').selectDate(new Date("August 08, 2014 12:00:00"));

    $("#timeline-btn").click(function () {
        openTimelineBar();
        return false;
    });

    $("#TimelineDialog").dialog({
        position: { my: "left top", at: "left bottom", of: "#banner" },
        show: { effect: "fade", duration: 300 },
        width: 460,
        draggable: true,
        title: "Timeline",
        collision: "fit",
        autoOpen: false
    });

    $("#visualise-btn").click(function () {
        var resource = $("#visSearchResource").val();
        var incident = $("#visSearchIncident").val();
        var start = new Date($("#visStartTimeDatetimepicker").val());
        var end = new Date($("#visEndTimeDatetimepicker").val());


        if ((incident + resource) !== "") {
            var selectedVisualLabels = [];

            $("#selectVisuals").children().each(function () {
                if ($(this).hasClass("active")) {
                    var selectedVisual = $(this).children();
                    if (selectedVisual !== "" && selectedVisual !== undefined) {
                        selectedVisualLabels.push(selectedVisual.val());
                    }
                }
            });

            var baseUrl = getBaseURL();

            $.ajax({
                url: baseUrl + "/Visual/GetCatalogue",
                data: {
                    dateFrom: start.toUTCString(),
                    dateTo: end.toUTCString(),
                    resource: resource,
                    incident: incident,
                    visuals: selectedVisualLabels
                },
                dataType: "json",
                success: function (response) {
                    displayVisuals(response);
                },
                error: function (e) {
                    alert("Visuals failed: " + e.statusText);
                }
            });

        }
    });

    $("#mapmatcher-btn").click(function () {
        $("#mapmatcher-dialog").dialog("open");
        return true;
    });

    $("#managespatial-btn").click(function () {
        $("#managespatial-dialog").dialog("open");
        return true;
    });

    $("#visquery-clr").click(function () {
        // clear out previous data
        timeline_groups.clear();
        timeline_items.clear();
        clrVisLayer();
        $("#AnimationDialog").dialog("close");
        return true;
    });

    $("#mapmatcher-dialog").dialog({
        position: { my: "right top", at: "right bottom", of: "#banner" },
        show: { effect: "fade", duration: 300 },
        width: 500,
        height: 500,
        draggable: true,
        title: "Visual Query",
        collision: "fit",
        autoOpen: false
    });

    $("#managespatial-dialog").dialog({
        position: { my: "right top", at: "right bottom", of: "#banner" },
        show: { effect: "fade", duration: 300 },
        width: 500,
        height: 500,
        draggable: true,
        title: "Manage Spatial Data",
        collision: "fit",
        autoOpen: false
    });

    $("#visquery-go").click(function () {
        visualQuery();
        return true;
    });


};


// process the response containing visuals and display
// on the timeline and on screen
function displayVisuals(response) {

    if (response == null) {
        alert("Empty response from server!");
        return;
    }

    if (response !== null && response.Success === false) {
        alert(response.Message);
        return;
    }

    openTimelineBar();

    // clear out previous data
    timeline_groups.clear();
    timeline_items.clear();

    var groupId = 0;
    var itemId = 0;
    var visuals = [];

    response.Visuals.forEach(function (item) {

        visuals.push(item.Id.Id);

        // add group
        groupId++;
        timeline_groups.add({ id: groupId, content: item.Id.Name + ": " + item.Id.VisualType, visualId: item.Id });

        // add data for group
        if (item.Timeline !== null) {
            item.Timeline.forEach(function (dataitem) {
                itemId++;
                timeline_items.add({
                    id: itemId,
                    ownerid: dataitem.Id,
                    content: dataitem.Label,
                    start: dataitem.Start,
                    end: dataitem.End,
                    className: dataitem.DisplayClass,
                    group: groupId,
                    visuals: dataitem.Visuals
                });
            });
        }
    });

    timeline_timeline.setGroups(timeline_groups);
    timeline_timeline.setItems(timeline_items);
    timeline_timeline.fit();

    GetVisualsdata(visuals);
}

function openTimelineBar() {
    var w = window.innerWidth;
    $("#AnimationDialog").dialog({
        position: { my: "left bottom", at: "left bottom", of: window },
        show: { effect: "fade", duration: 200 },
        width: w,
        height: 300,
        draggable: true,
        title: "Timeline",
        //collision: "fit",
        autoOpen: true
    });

    if (timeline_timeline === undefined) {
        timeLineContainer = document.getElementById('timeline-container');
        var options = {
            groupOrder: 'id', // groupOrder can be a property name or a sorting function
            stack: false,
            orientation: 'top',
            //clickToUse: true,
            multiselect: true,
            margin: {
                item: 3, // minimal margin between items
                axis: 2 // minimal margin between items and the axis
            }
        };

        timeline_timeline = new vis.Timeline(timeLineContainer);
        timeline_timeline.setOptions(options);
    }

    if (!$("#AnimationDialog").dialog("isOpen"))
        $("#AnimationDialog").dialog("open");
}

function clrVisLayer() {
    if (vislayer !== undefined) {
        map.removeLayer(vislayer);
    }
}

// get the visual GeoJSON
function GetVisualsdata(visuals) {

    $.ajax({
        url: getURL("Visual/GetVisualsData"),
        data: { 'visuals': visuals.join(",") },
        dataType: "json",
        success: function (features) {
            clrVisLayer();

            // ReSharper disable once InconsistentNaming
            vislayer = new L.featureGroup();

            addFeature(features);
        }
    });
}

function getRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

function addFeature(feature) {
    var mmLayer = L.geoJson(feature,
        {
            style: function () {
                return {
                    color: getRandomColor(),
                    opacity: 0.5,
                    fillOpacity: 0.5,
                    weight: 5,
                    clickable: true
                };
            },
            pointToLayer: function (singlefeature, latlng) {
                // user marker using the properties to define style in the usermarker.css
                return L.userMarker(latlng, { pulsing: false, accuracy: 0, m: singlefeature.properties.type, s: singlefeature.properties.status });
            },
            onEachFeature: function (singlefeature, layer) {
                layer.on("click",
                    function () {
                        showProperties(singlefeature.properties);
                    });
            }
        }
    );
    vislayer.addLayer(mmLayer);
    vislayer.addTo(map);

    var bounds = vislayer.getBounds();
    if (bounds.isValid())
        map.fitBounds(bounds, { maxZoom: 18 });
}


function visualQuery() {

    clrVisLayer();

    var params = $("#visquery_parameters").val();

    vislayer = new L.featureGroup();

    $.ajax({
        url: getBaseURL() + "/Visual/Query",
        data: {
            provider: "MapMatcher",
            parameters: params
        },
        dataType: "json",
        success: function (response) {
            displayVisuals(response);
        },
        error: function (e, e1, e2) {
            alert("Visuals failed: " + e.statusText);
        }
    });

}

// show property dialog box 
function showProperties(values) {
    $("#property-table").find('tbody').find('tr').remove();

    if (values === undefined) {
        $("#properties").dialog("close");
        return;
    }
    $("#properties").dialog("open");

    for (var key in values) {
        // ReSharper disable once QualifiedExpressionMaybeNull
        if (values.hasOwnProperty(key)) {
            $("#property-table")
                .find("tbody")
                .append($("<tr>")
                    .append($("<td>").append("<p>").html(key))
                    .append($("<td>").append("<p>").text(values[key])));

        }
    }

}

