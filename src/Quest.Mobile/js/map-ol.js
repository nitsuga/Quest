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
var BoundaryLyr;        // for EISEC and nearby polygon 
var IOIlayer;
var searchControl;
var georesLayer;
var wto;
var xReplayPlayerWs;        // link to resource playback
var CallWs;                 // link to call playback
var searchText = "";

var margin = { top: 20, right: 20, bottom: 200, left: 40 },
              width = 960 - margin.left - margin.right,
              height = 500 - margin.top - margin.bottom;


$(function () {

    L_PREFER_CANVAS = true;
    
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
    });

    $("#boundsfilter").on("click", function () {
        if ($("#boundsfilter").hasClass("fa-unlock")) {
            $("#boundsfilter").removeClass("fa-unlock").addClass("fa-lock");
            $("#boundsfilter").removeClass("btn-default").addClass("btn-warning");
        }
        else {
            $("#boundsfilter").removeClass("fa-lock").addClass("fa-unlock");
            $("#boundsfilter").removeClass("btn-warning").addClass("btn-default");
        }
    });

    // set up exact matching to start with 
    setSearchMode(0);

    $("#relax").on("click", function () {
        toggleSearchMode();
    });

    $("#testcall-btn").click(function () {
        $("#callModal").modal("show");
        $(".navbar-collapse.in").collapse("hide");
        return false;
    });

    $("#about-btn").click(function () {
        $("#aboutModal").modal("show");
        $(".navbar-collapse.in").collapse("hide");
        return false;
    });

    $("#testaddress_btn").click(function () {
        var text = $("#testaddress").val();
        $("#callModal").modal("hide");
        $("#calldata").html(text);
        LocationSearch(text, 1);                // override to RELAX
        return false;
    });

    $("#calldata").click(function () {
        var text = $("#testaddress").val();
        $("#search_input_text").val(text);
        return false;
    });

    $("#settings-res-btn").click(function () {
        $("#settings-res").modal("show");
        $(".navbar-collapse.in").collapse("hide");
        return false;
    });

    $("#settings-inc-btn").click(function () {
        $("#settings-inc").modal("show");
        $(".navbar-collapse.in").collapse("hide");
        return false;
    });

    $("#settings-cov-btn").click(function () {
        $("#settings-cov").modal("show");
        $(".navbar-collapse.in").collapse("hide");
        return false;
    });

    $("#settings-lay-btn").click(function () {
        $("#settings-lay").modal("show");
        $(".navbar-collapse.in").collapse("hide");
        return false;
    });

    $("#mapmatcher-btn").click(function () {
        $("#mapmatcher-dlg").modal("show");
        $(".navbar-collapse.in").collapse("hide");
        return false;
    });
    
    $("#full-extent-btn").click(function () {
        if (searchlayer != null)
            map.fitBounds(searchlayer.getBounds());
        $(".navbar-collapse.in").collapse("hide");
        return false;
    });

    $("#list-btn").click(function () {
        $("#sidebar").toggle();
        map.invalidateSize();
        return false;
    });

    $("#legend-btn").click(function () {
        $("#legendModal").modal("show");
        $(".navbar-collapse.in").collapse("hide");
        return false;
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
        var nearby = $(".resfirst")[0].value == "on";
        var eventId = $(".dispatchEvent").html();
        $.ajax({
            url: getURL("Home/AssignDevice"),
            data:
                {
                    Callsign: callsign,
                    EventId: eventId,
                    Nearby: nearby
                },
            dataType: "json",
            success: function (msg) {
                //alert(msg);
            },
            error: function (xhr, textStatus, errorThrown) {
                alert("Dispatch failed");
            }
        });

    });

    setStore("#use-clusters", false, 365);

    $(".toggle").each(function (index, element) {
        setSliderFromStore("#" + element.id);
    });

    // save any settings from a toogle
    $(".toggle").change(function (a) {
        var target = $(this)[0];
        setStoreFromSlider("#" + target.id);

        setTimeout(function () {
            updateMapData();
        }, 1000);


    });

    $("#alertcallsign").change(function () {
        if ($(this).is(":checked"))
            $("#modalCallsign").modal("show");
    });

    //----------------draw the freaking map hooraaaaa---------------
    drawMap();

    updateMapData();

    $.pnotify.defaults.styling = "bootstrap3";
    $.pnotify.defaults.history = false;

    $("#search_input_text").focus();

    SetCallNotifications();

});

function toggleSearchMode()
{
    setSearchMode(mode);
    var mode = $("#relax").data("mode");
    mode = mode + 1;
    setSearchMode(mode);
    LocationSearch();
};

function setSearchMode(mode) {
    $("#relax").removeClass("btn-default");
    $("#relax").removeClass("btn-warning");
    $("#relax").removeClass("btn-danger");
    if (mode > 2 || mode < 0)
        mode = 0;
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

    var lat = getStore("lat");
    if (lat == null)
        lat = 51.5;
    else
        lat = Number(lat);

    var lng = getStore("lng");
    if (lng == null)
        lng = -0.2;
    else
        lng = Number(lng);

    var zoom = getStore("zoom");
    if (zoom == null)
        zoom = 12;
    else
        zoom = Number(zoom);


    var baseLayers = new ol.Collection();

	mbUrl = "https://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoibWFwYm94IiwiYSI6ImNpandmbXliNDBjZWd2M2x6bDk3c2ZtOTkifQ._QA7i5Mpkd_m30IGElHziw";

	baseLayers.push(new ol.layer.Tile({
	    source: new ol.source.TileWMS({
	        url: mbUrl,
	        crossOrigin: '',
	        params: {
	            id: "mapbox.light"
	        },
	        projection: 'EPSG:4326'
	    })
	}));

    baseLayers.push(new ol.layer.Tile({
        source: new ol.source.TileWMS({
            url: mbUrl,
            crossOrigin: '',
            params: {
                id: "mapbox.streets"
            },
            projection: 'EPSG:4326'
        })
    }));

    var localmode = getStoreAsBool("#localmode");

    var quest_map_base = "http://127.0.0.1:8080/cgi-bin/mapserv.exe?MAP=c:/ms4w/apps/extent/wms-server.map";
    if (localmode == false)
        quest_map_base = "http://86.29.75.151:8080/cgi-bin/mapserv.exe?MAP=g:/ms4w/apps/extent/wms-server.map";

    baseLayers.push(new ol.layer.Tile({
        title: 'Barts',
        type: 'base',
        visible: false,
        source: new ol.source.TileWMS({
            url: quest_map_base ,
            crossOrigin: '',
            params: {
                layers: "Barts",
                format: "image/png",
                maxZoom: 22,
                minZoom: 0,
                continuousWorld: true,
                noWrap: true
            },
            projection: 'EPSG:4326'
        })
    }));


    baseLayers.push( new ol.layer.Tile({
        title: 'OSM',
        type: 'base',
        visible: true,
        source: new ol.source.OSM()
    }));

    baseLayers.push(new ol.layer.Tile({
        title: 'Stations',
        visible: true,
        source: new ol.source.TileWMS({
            url: quest_map_base,
            crossOrigin: '',
            params: {
                layers: "Stations",
                format: "image/png",
                maxZoom: 22,
                minZoom: 0,
                continuousWorld: true,
                noWrap: true,
                transparent: true
            }
        })
    }));


    var controls = [
          new ol.control.Attribution(),
          new ol.control.MousePosition({
              projection: 'EPSG:27700',
              coordinateFormat: function (coordinate) {
                  return ol.coordinate.format(coordinate, '{y}, {x}', 4);
              }
          }),
          new ol.control.ScaleLine(),
          new ol.control.Zoom(),
          new ol.control.ZoomSlider(),
          new ol.control.ZoomToExtent(),
          new ol.control.FullScreen()
    ];

    map = new ol.Map({
        controls: controls,
        target: 'map',
        layers:  [ new ol.layer.Group({ 'title': 'Base Maps', layers: baseLayers }) ],
        view: new ol.View({
            center: ol.proj.fromLonLat([lng, lat]),
            zoom: zoom
        })
    });

    var layerSwitcher = new ol.control.LayerSwitcher();
    map.addControl(layerSwitcher);

    $("#go-btn").on("click", function () {
        clearTimeout(wto);
        LocationSearch();
    });

    // capture ctr+del and clear search box. 
    // effective only when in search box, if needed outside add to document instead
    $("#search_input_text").keydown(function(event) {
        if (event.ctrlKey && event.which==46) //ctrl+del
        {
            $("#search_input_text").val("");
            }
    });

    // someone clicked on the group-by radion button.. repeat the search
    $('input[name="group-opt-radio').change(function (e)
    {
        LocationSearch();
    });

    $("#search-coord").on("click", function () {
        var coord = $("#search-coord").data("coord");
        $("#search_input_text").val(coord);
        LocationSearch();
    });

    $("#search_input_text").on("input", function (e) {
        clearTimeout(wto);
        wto = setTimeout(function () {
            LocationSearch();
        }, 1000);
    });

    $("#help").on("show.bs.collapse", function (e) {
        $("#results").collapse("show");
    });

    map.on("popupopen", function (e) {
        var content = e.popup.getContent();

        if (searchGroupsLyr != undefined)
            searchGroupsLyr.clearLayers();

        if (content.indexOf("Information of Interest") == -1) {
            var html = $.parseHTML(content)
            for (i = 0; i < html.length; i++) {
                if (html[i].id == "latLong") {
                    var latLongstring = html[i].value;
                    var latLongJson = $.parseJSON(latLongstring);

                    var ioiInfo = GetAddressIoI(latLongJson.lng, latLongJson.lat);

                    if (ioiInfo != null && ioiInfo.Documents.length > 0) {
                        e.popup.setContent(""); //clear any content that may have been set previously
                        var ioiContent = content;
                        ioiContent += "<b>Information of Interest</b>";
                        ioiContent += "<ul>";
                        ioiInfo.Documents.forEach(
                        function (address) {
                            ioiContent += "<li>" + address.d + "</li>";
                        });
                        ioiContent += "</ul>";
                        e.popup.setContent(ioiContent);
                    }
                }
            }
        }
    });

    map.on("click", function (evt)
    {
        var coordinate = evt.coordinate;
        var coordinate4326 = ol.proj.transform(coordinate, 'EPSG:3857', 'EPSG:4326');
        var txt = Math.round(coordinate4326[1] * 100000) / 100000 + "," + Math.round(coordinate4326[0] * 100000) / 100000;
        $("#search-coord").data("coord",txt);
    });

    map.on("moveend", function () {
        var coordinate = map.getView().getCenter();
        var coordinate4326 = ol.proj.transform(coordinate, 'EPSG:3857', 'EPSG:4326');
        setStore("lat", coordinate4326[1], 365);
        setStore("lng", coordinate4326[0], 365);
    });

    map.getView().on("change:resolution", function (e) {
        var zoom = map.getView().getZoom();
        setStore("zoom", zoom, 365);
    });


}

function LocationSearch(text, overrideMode)//callback for 3rd party ajax requests
{
    if (text == undefined)
        var text = $("#search_input_text").val();

    text = text.trim();
    clrSearchItems();
    clrInfoItems();

    resultsGroupOpt = $("input[name='group-opt-radio']:checked").val();

    if (text == "")
        return;

    $("#message").hide();
    $("#message-wait").show();

    var terms = ""; // GetFilterTerms();

    if (overrideMode == undefined)
        mode = $("#relax").data("mode");
    else
        mode = overrideMode;

    var bb = map.getView().calculateExtent(map.getSize());

    var bl = ol.proj.transform([bb[0], bb[1]], 'EPSG:3857', 'EPSG:4326');
    var tr = ol.proj.transform([bb[2], bb[3]], 'EPSG:3857', 'EPSG:4326');

    var parms = {
        searchText: text,
        searchMode: mode,
        includeAggregates: false,
        skip: 0,
        take: 700,
        boundsfilter: $("#boundsfilter").hasClass("fa-lock"),
        w: bl[0],
        s: bl[1],
        e: tr[0],
        n: tr[1],
        filterterms: terms,
        displayGroup: resultsGroupOpt
    };

    return $.ajax({
        url: getURL("Home/SemanticSearch"),
        data: parms,
        dataType: "json",
        success: function (items) {
            $("#message-wait").hide();

            if (items.error != undefined) {
                $("#message").html(items.error);
                $("#message").show();
            }
            else {
                showGroupedSearchItems(items);
                DisplayBoundary(items.Bounds);

                $("#message").show();
                $("#message-wait").hide();
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            $("#message").show();
            $("#message-wait").hide();
        }
    });
}

function InformationSearch(longitude, latitude, callResponse)//callback for 3rd party ajax requests
{
    console.log("Clicked at lat: " + latitude + " long: " + longitude);

    var parms = {
        lng: longitude,
        lat: latitude,
    };

    return $.ajax({
        url: getURL("Home/InformationSearch"),
        data: parms,
        dataType: "json",
        success: function (items) {
            showInfoItems(items);
            if (callResponse != undefined)
                callResponse(items);
        },
        error: function (xhr, textStatus, errorThrown) {
            //   alert("Search failed - " + textStatus + " " + errorThrown);
        }
    });
}

function updateMapData() {

    return;

    //    map.spin(true);  //show loading spinner

    doIncidents();
    doResources();
    doDestinations();

    //doCCG();
    doAllCoverage();

    //    ma.spin(false);
}

String.prototype.capitalizeFirstLetter = function () {
    return this.charAt(0).toUpperCase() + this.slice(1);
}

function clrSearchItems()
{
    $("#result-list").find("li").remove();
    $("#grouped-results-list").empty();
    $("#grouped-results-list").html("");

    if (searchlayer != null)
        map.removeLayer(searchlayer);
}

function clrInfoItems() {

    $("#IOIGrp").find("li").remove();
    $("#IOI").hide();

    if (IOIlayer != null) {
        map.removeLayer(IOIlayer);
    }

}

function showInfoItems(items) {

    clrInfoItems();

    IOIlayer = new L.featureGroup();

    items.Documents.forEach(
        function (address) {

            var latlngfocus = new ol.Coordinate(0,0);

            // add map marker
            var polylineCoordinates = [];
            if (address.PolyGeometry != undefined) {
                address.PolyGeometry.coordinates.forEach(function (coords) {
                    coords.forEach(function (pnt) {
                        var latlng = new ol.Coordinate(pnt[0], pnt[1]);
                        latlngfocus = latlng;
                        polylineCoordinates.push(latlng);
                    });

                });
                var polygon = L.polygon(polylineCoordinates, { color: "lightyellow" });
                polygon.bindPopup(address.d)
                IOIlayer.addLayer(polygon);
            }
            else {
                //var marker = GetMarker(address);

                //var l = latlng.toString().replace('LatLng(', '');
                //l = l.replace(')', '');

                //marker.bindPopup(address.d);
                //IOIlayer.addLayer(marker);
            }

            $("#IOIGrp").append($("<li>").attr("class", "list-group-item").on("click", function (e) { })
                .append($("<span>").text(address.d).addClass("name"))
                .append($("<br>"))
                .append($("<span>").addClass("glyphicon glyphicon-map-marker text-muted c-info c-info-icon")
                                    .attr("title", address.d)
                                    .attr("data-toggle", "tooltip")
                                    .on("click", function (e) {
                                        map.setView(latlngfocus, 16, { animate: true });
                                    })

                )

                .append($("<span>").text(address.ValidFrom).addClass("text-muted"))

                .append($("<span>")
                        .addClass("glyphicon glyphicon-pencil text-muted c-info")
                        .attr("title", "type")
                        .attr("data-toggle", "tooltip")
                )
                .append($("<span>").text(address.Category).addClass("text-muted"))

                .append($("<span>").addClass("glyphicon glyphicon-stats text-muted c-info").attr("title", "status").attr("data-toggle", "tooltip"))
                .append($("<span>").text(address.Status).addClass("text-muted"))

                );

        }
        );

    IOIlayer.addTo(map);
    $("#IOI").show();
}

// make a map object for this address. could be point line or area
function getFeature(address, latlng) {

    var feature;
    var content = "";

    var customOptions =
     {
         'maxWidth': "500",
         'minWidth': "300",
         keepInView: true
     }

    content += "<p>" + address.d + "</p>";
    if (address.st != undefined && address.st != "Active") {
        content += "<p>" + address.st + "</p>";
    }
    content += "<input type='hidden' id='latLong' value='" + latLongString + "'>";
    if (address.url != undefined && address.url != "") {
        content += "<img src='" + address.url + "'/>";
    }

    var polylineCoordinates = [];
    if (address.pg != undefined) {
        address.pg.coordinates.forEach(function (coords) {
            coords.forEach(function (pnt) {
                var latlng = new ol.Coordinate(pnt[0], pnt[1]);
                latlngfocus = latlng;
                polylineCoordinates.push(latlng);
            });

        });
        feature = L.polygon(polylineCoordinates, { color: "lightyellow" });
        feature.bindPopup(address.d)
    }
    else
        if (address.ml != null) {
            address.ml.coordinates.forEach(function (coords) {
                coords.forEach(function (pnt) {
                    var latlng = new ol.Coordinate(pnt[0], pnt[1]);
                    polylineCoordinates.push(latlng);
                });

            }
        );
            feature = L.polyline(polylineCoordinates, { color: "blue" });
            feature.bindPopup(content, customOptions);
        }
        else {
            feature = L.userMarker(latlng, { pulsing: false, accuracy: 0, m: "loc", s: address.t });

            var l = latlng.toString().replace("LatLng(", "");
            l = l.replace(")", "");
            feature.bindPopup(content, customOptions);
        }

    return feature;
}

// add a doc to the map and results
function addSingleFeatureToResultsList(containerId, address, feature, score, latlng)
{
    searchlayer.addFeature(new ol.Feature(new ol.geom.Circle([5e6, 7e6], 1e6)));

    $(containerId).append($("<a>").attr("class", "list-group-item").attr("href", "#").on("click", function (e)
    {
        if (feature != undefined)
        {
            map.setView(latlng, 16, { animate: true });
            feature.openPopup();
        }
    })
    .hover(function ()
        {
            if (searchGroupsLyr == null)
            {
                searchGroupsLyr = new L.featureGroup()
                searchGroupsLyr.addTo(map);
            }
            else
            {
                searchGroupsLyr.clearLayers();
            }

            var tempGroupLyr = new L.featureGroup();
            tempGroupLyr.addLayer(feature);
            var extent = tempGroupLyr.getBounds();

            var pulsingIcon = L.icon.pulse({ iconSize: [20, 20], color: "#3f007d" });
            pulsingMarker = L.marker(extent.getCenter(), { icon: pulsingIcon });
            searchGroupsLyr.addLayer(pulsingMarker);
        }, function ()
        {
            if (searchGroupsLyr != undefined)
                searchGroupsLyr.clearLayers();
        })
    .append($("<span>").addClass("glyphicon glyphicon-map-marker text-muted c-info c-info-icon")
                        .attr("title", "go to ")
                        .attr("data-toggle", "tooltip")
                        .on("click", function (e)
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

function DisplayBoundary(bounds)
{
    if (BoundaryLyr == null) {
        BoundaryLyr = new L.featureGroup()
        BoundaryLyr.addTo(map);
    }
    else {
        BoundaryLyr.clearLayers();
    }

    // display boundary if supplied
    var polylineCoordinates = [];
    if (bounds != undefined) {
        bounds.coordinates.forEach(function (coords) {
            coords.forEach(function (pnt) {
                var latlng = new ol.Coordinate(pnt[0], pnt[1]);
                latlngfocus = latlng;
                polylineCoordinates.push(latlng);
            });

        });
        feature = L.polygon(polylineCoordinates, { color: "red", fill: false });
        if (BoundaryLyr != undefined)
            BoundaryLyr.addLayer(feature);
    }

}

function showGroupedSearchItems(items)
{
    clrSearchItems();
    searchlayer = new ol.source.Vector();

    var itext = items.Count.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")

    if (items.Removed > 0)
        $("#message").html("  found: <B>" + itext + "</B> items (" + items.Removed + " dups) in " + items.ms + "ms");
    else
        $("#message").html("  found: " + itext + " items in " + items.ms + "ms");


    if (items.Grouping == null || items.Grouping.length == 1) {
        for (var docindex = 0; docindex < items.Documents.length; docindex++)
        {
            var doc = items.Documents[docindex];
            var score = doc.s;
            var latlng = new ol.Coordinate(doc.l.lon, doc.l.lat);
            var feature = getFeature(doc, latlng);
            addSingleFeatureToResultsList("#grouped-results-list", doc, feature, score);
        }
    }
    else {

        for (var i = 0 ; i < items.Grouping.length; i++) {
            var grp = items.Grouping[i];

            // get first document
            var docindex = grp[0];
            var doc = items.Documents[docindex];

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
                    var docindex = grp[j];
                    var doc = items.Documents[docindex];
                    var score = doc.s;
                    var latlng = new ol.Coordinate(doc.l.lon, doc.l.lat);
                    var feature = getFeature(doc, latlng);

                    addSingleFeatureToResultsList("#group-results-sm-" + i, doc, feature, score);
                }

            }
            else //these  are not grouped
            {
                var score = doc.s;
                var feature = getFeature(doc, latlng);
                addSingleFeatureToResultsList("#grouped-results-list", doc, feature, score);
            }
        }
    }

    //Add aggregates panels
    //loadAggregates(items);
    //if (items.Documents.length > 0)
    //{
    //    initialisefilters(items);
    //    $('#filter-btn').show();
    //}
    //else
    //{
    //    $('#filter-btn').hide();
    //}

    var locked = $("#boundsfilter").hasClass("fa-lock");

    // zoom in..
    if (!locked && items.Documents.length > 0)
    {
        var bounds = searchlayer.getBounds();
        map.fitBounds(bounds, { maxZoom: 18 });//works!    

    }
    searchlayer.addTo(map);
}

function addGroupHighlightFeature(groupDivId)
{
    var data = $(groupDivId).data("features");

    var groupHighlightFeatures = new Array();

    //we only use it for calculating extent
    if (searchGroupsLyr == null)
    {
        searchGroupsLyr = new L.featureGroup()
        searchGroupsLyr.addTo(map);
    }
    else
    {
        searchGroupsLyr.clearLayers();
    }

    var tempGroupLyr = new L.featureGroup()
    for (var i = 0; i < data.length; i++)
    {
        var f = data[i];
        tempGroupLyr.addLayer(f);
    }

    var extent = tempGroupLyr.getBounds();

    var pulsingIcon = L.icon.pulse({ iconSize: [30, 30], color: "#3f007d" });
    pulsingMarker = L.marker(extent.getCenter(), { icon: pulsingIcon });
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

        if (e.type == "mouseenter")
        {
            if (pulsingMarker == null)
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
        if (searchGroupsLyr != undefined)
            searchGroupsLyr.clearLayers();

        var pulsingMarker = $("#" + e.currentTarget.id).data("pulsingMarker");

        if (pulsingMarker == null)
        {
            addGroupHighlightFeature("#" + e.currentTarget.id);
        }
        else //we cant be sure if it was on or of
        {
            searchGroupsLyr.removeLayer(pulsingMarker);
            searchGroupsLyr.addLayer(pulsingMarker);
        }
        
        var extent = searchGroupsLyr.getBounds();
        if (extent.isValid)
        {
            map.fitBounds(extent, { maxZoom: 20 });
        }

        $(this).find(".glyphicon-plus").removeClass("glyphicon-plus").addClass("glyphicon-minus");
    });

    $("#group-results-" + i).on("hide.bs.collapse", function (e)
    {
        if (searchGroupsLyr != undefined)
            searchGroupsLyr.clearLayers();

        $(this).find(".glyphicon-minus").removeClass("glyphicon-minus").addClass("glyphicon-plus");
    });
}

function customTip(text, val) {
    return '<a href="#">' + text + '<em style="background:' + text + '; width:14px;height:14px;float:right"></em></a>';
}

function doDestinations() {
    var layerhospother = getStoreAsBool("#layer-hosp-other");
    var layersbp = getStoreAsBool("#layer-sbp");
    var layerhospae = getStoreAsBool("#layer-hosp-ae");
    var layerstation = getStoreAsBool("#layer-station");
    var layerroad = getStoreAsBool("#layer-road");

    if (layersbp == false && layerhospother == false && layerhospae == false && layerstation == false && layerroad == false && layerhospother == false) {
        if (markersd != null)
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
                ae: layerhospae,
            },
        dataType: "json",
        success: function (layer) {
            var useclusters = getStoreAsBool("#use-clusters");

            if (markersd != null)
                map.removeLayer(markersd);

            if (useclusters == true)
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

            if (markersd == null)
                markersd = L.layerGroup();

            var geoJsonLayer = L.geoJson(layer, {

                pointToLayer: function (feature, latlng) {
                    var marker = L.userMarker(latlng, { pulsing: false, accuracy: 0, m: "des", s: feature.properties.status });
                    return marker;
                },

                onEachFeature: function (feature, layer) {
                    layer.on("click", function (e) {
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
    var catB = getStoreAsBool("#layer-inc-catb");

    if (catA == false && catB == false) {
        if (markersi != null)
            map.removeLayer(markersi);
        return;
    }

    $.ajax({
        url: getURL("Home/GetIncidents"),
        data:
            {
                IncludeCatA: catA,
                IncludeCatB: catB,
            },
        dataType: "json",
        success: function (layer) {
            var useclusters = getStoreAsBool("#use-clusters");

            if (markersi != null)
                map.removeLayer(markersi);

            if (useclusters == true)
                markersi = L.MarkerClusterGroup(
                     {
                         iconCreateFunction: function (cluster) {
                             var childCount = cluster.getChildCount();
                             return new L.DivIcon(
                                 {
                                     html: "<div><span><b>" + childCount + "</b></span></div>", className: "incidentcluster", iconSize: new L.Point(40, 40)
                                 });
                         }
                     });

            if (markersi == null)
                markersi = L.layerGroup();

            var geoJsonLayer = L.geoJson(layer, {

                pointToLayer: function (feature, latlng) {
                    var marker = L.userMarker(latlng, { pulsing: false, accuracy: 0, m: "inc", s: feature.properties.status + "-" + feature.properties.priority });
                    return marker;
                },

                onEachFeature: function (feature, layer) {
                    layer.on("click", function (e) {
                        $(".incLocationValue").html("---");
                        $(".incLocationValue").html(feature.properties.location);
                        $(".incPriorityValue").html(feature.properties.status + " " + feature.properties.priority);
                        $(".incCreatedValue").html(feature.properties.lastupdate);
                        $(".incResourcesValue").html(feature.properties.resources);
                        //$(".incCadValue").html(feature.properties.incidentid);
                        $(".incCadValue").html("---");
                        $(".incDeterminantValue").html(feature.properties.determinant + " " + feature.properties.description);
                        $(".dispatchEvent").html(feature.properties.incidentid);
                        $(".incPatientValue").html(feature.properties.sex + "/" + feature.properties.age);
                        //$(".incLocationCommentValue").html(feature.properties.loccomment);
                        $(".incMaprefValue").html(feature.properties.az);
                        $(".incProblemValue").html(feature.properties.prob);


                        $("#showDispatch").button().on("click", function () {
                            $("#modalDispatch").modal("show");
                        });


                        $("#modalIncidentDetails").modal("show");
                    });

                }
            });
            markersi.addLayer(geoJsonLayer);
            markersi.addTo(map);
        }
    });

}

function doResources() {
    var avail = getStoreAsBool("#layer-res-avail");
    var busy = getStoreAsBool("#layer-res-busy");

    if (markersr != null) {
        RemoveUnWantedResources(avail, busy);
    }
    else //resources layer does not exist
    {
        if (avail == false && busy == false) {
            return;
        }
    }

    //Create a new empty resources layer and add to map
    if (markersr != null) markersr.clearLayers()
    CreateResourcesLayer();

    $("*").css("cursor", "wait"); // this call or handling of results by leaflet my take some time 

    //Make a new request for the new selection
    $.ajax({
        url: getURL("Home/GetResources"),
        data:
            {
                Avail: avail,
                Busy: busy,
            },
        dataType: "json",
        success: function (layer) {

            if (layer.error != undefined) {
                $("#message").html(layer.error);
                $("#message").show();

            }
            else {
                //if this is not the first time, we may have existing resources on 
                georesLayer.addData(layer);

                //Now set the web socket to open for updates to resources
                if (xReplayPlayerWs == null) {
                    SetXReplayPlayerNotifications();
                }
                else {
                    //update the parameters of the socket server
                    var resourceParams = { Avail: avail, Busy: busy };
                    xReplayPlayerWs.send(JSON.stringify(resourceParams));
                }

                map.spin(false);
            }
            $("*").css("cursor", "default");
        }
    });
}

function CreateCCGLegend() {

    if (info == null) {
        info = L.control();
        info.onAdd = function (map) {
            this._div = L.DomUtil.create("div", "info"); // create a div with a class "info"
            this.update();
            return this._div;
        };

        // method that we will use to update the control based on feature properties passed
        info.update = function (props) {
            if (this._div != null) {
                this._div.innerHTML = "<h4>CCG Coverage</h4>" + (props ?
                    "<b>" + props.name + "</b><br /> Holes: " + props.holes + " %"// / mi<sup>2</sup>'
                    + "<br /> AMB: " + props.amb + " %"
                    + "<br /> FRU: " + props.fru + " %"
                    + "<br /> INC: " + props.inc + " %"
                    : "Hover over a CCG");
            }
        };
    }

    var dolegend = getStoreAsBool("#legend-ccg");

    if (dolegend == false) {
        if (legend != null) {
            legend.removeFrom(map);
            legend = null;
            return;
        }
    }
    else {
        // legend = true
        if (legend == null) {
            legend = L.control({ position: "bottomleft" });

            legend.onAdd = function (map) {

                var div = L.DomUtil.create("div", "info legend"),
                    grades = [0, 10, 30, 50, 70, 90],
                    labels = [];

                // loop through our density intervals and generate a label with a colored square for each interval
                for (var i = 0; i < grades.length; i++) {
                    div.innerHTML +=
                        '<i style="background:' + getColor(grades[i] + 1) + '"></i> ' +
                        grades[i] + (grades[i + 1] ? "&ndash;" + grades[i + 1] + "<br>" : "+");
                }

                return div;
            };

            legend.addTo(map);
        }
    }
}

function doCCG() {
    //---------------CUSTOM INFO CONTROL-----------------------

    CreateCCGLegend();

    var doccg = getStoreAsBool("#layer-ccg");

    if (doccg != true) {
        if (ccglayers != null)
            map.removeLayer(ccglayers);
        return;
    }

    //--------------------------------------------------------------
    return $.ajax({
        url: getURL("Home/GetCCGCoverage"),
        dataType: "json",
        success: function (layer) {

            if (ccglayers != null)
                map.removeLayer(ccglayers);

            ccglayers = new L.LayerGroup();

            var ccg = L.geoJson(layer, {
                style: style,
                onEachFeature: function (feature, layer) {
                    layer.bindPopup(
                        "<b> CCG: </b>" + feature.properties.name + "<br/>" +
                        "<b> Holes: </b>" + feature.properties.holes + "% <br/>"
                        );
                    layer.on({
                        mouseover: function highlightFeature(e) {
                            var layer = e.target;

                            layer.setStyle({
                                weight: 1,
                                color: "black",
                                dashArray: "",
                                fillOpacity: 0.1
                            });

                            // if (!L.Browser.ie && !L.Browser.opera) {
                            layer.bringToFront();
                            //}

                            info.update(layer.feature.properties);
                        },
                        mouseout: function resetHighlight(e) {
                            ccg.resetStyle(e.target);
                            info.update();
                        }
                        //click: zoomToFeature
                    });
                }
            });

            ccglayers.addLayer(ccg);
            ccglayers.addTo(map);
        }

    });
}

function doAllCoverage() {

    if (covlayer == null) {
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

    if (covamb == true)
        doCoverage(1, "amb");
    else
        undoCoverage("amb");

    if (covfru == true)
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
        if (layer.id == name) {
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
        success: function (msg, a, b) {

            if (msg.Success == true && msg.Map != null) {
                var res = msg.Map;
                var data = [];
                var i = 0;
                if (res.map != undefined) {
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

                    switch (res.vehtype) {
                        case 1:
                            var nheatmap = L.heatLayer(data, { max: 1.0, radius: 50, gradient: { 0.1: "lime", 0.2: "lime", 0.3: "lime", 0.4: "lime" } });
                            break;
                        case 2:
                            var nheatmap = L.heatLayer(data, { max: 1.0, radius: 50, gradient: { 0.1: "green", 0.2: "yellow", 0.3: "yellow", 0.4: "yellow" } });
                            break;
                        case 8:
                            var nheatmap = L.heatLayer(data, { max: 1.0, radius: 50, gradient: { 0.1: "purple", 0.2: "lime", 0.3: "yellow", 0.4: "purple" } });
                            break;
                        case 9:
                            var nheatmap = L.heatLayer(data, { max: 1.0, radius: 50, gradient: { 0.1: "orange", 0.2: "lime", 0.3: "yellow", 0.4: "red" } });
                            break;
                    }

                    //covlayer.opacity = 0.3;
                    nheatmap.id = name;
                    undoCoverage(name);
                    covlayer.addLayer(nheatmap);
                }
            }
            else {
                // alert(msg);
            }
        }
    });
}

//colour thresholds for legend on ccg map
function getColor(d) {
    return d > 90 ? "#800026" :
           d > 80 ? "#3f007d" :
           d > 70 ? "#E31A1C" :
           d > 50 ? "#FC4E2A" :
           d > 30 ? "#FD8D3C" :
           d > 20 ? "#FEB24C" :
           d > 10 ? "#FED976" :
                      "#FFFFFF";
}

function getOpacity(d) {
    return d > 10 ? 0.75 : 0;
}

//mouseover event for ccg hover
function highlightFeature(e) {
    var layer = e.target;

    layer.setStyle({
        weight: 5,
        color: "#666",
        dashArray: "",
        fillOpacity: 0.7
    });

    if (!L.Browser.ie && !L.Browser.opera) {
        layer.bringToFront();
    }

    info.update(layer.feature.properties);
}

//mouseout event for ccg hover
function resetHighlight(e) {
    geojson.resetStyle(e.target);
}

//styling for ccg layer
function style(feature) {
    return {
        fillColor: getColor(feature.properties.holes),
        weight: 3,
        opacity: 1,
        color: "grey",
        dashArray: "3",
        fillOpacity: getOpacity(feature.properties.holes)
    };
}

//current location for map
function mapLocate() {
    if (loc_marker == null) {
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

function getURL(url) {
    var s = getBaseURL() + "/" + url;
    //console.debug("g url = " + s);
    return s;
}

function getBaseURL() {
    var url = location.href;  // entire url including querystring - also: window.location.href;
    var baseURL = url.substring(0, url.indexOf("/", 10));
    //console.debug("b url = " + baseURL);
    return baseURL;
}

function RegisterForEvents() {
    if (!!window.EventSource) {
        var url = getBaseURL() + "/api/Notifications/Get";
        var source = new EventSource(url);
        source.addEventListener("message", function (e) {
            var json = JSON.parse(e.data);

            if (georesLayer != undefined)
                georesLayer.addData(json);

        }, false);
        source.addEventListener("open", function (e) {
            console.log("open!");
        }, false);
        source.addEventListener("error", function (e) {
            if (e.readyState == EventSource.CLOSED) {
                console.log("error!");
            }
        }, false);
    } else {
        // not supported!
        //fallback to something else
    }
}

function SetXReplayPlayerNotifications() {
    var avail = getStoreAsBool("#layer-res-avail");
    var busy = getStoreAsBool("#layer-res-busy");

    console.log("connecting");

    var httpUrl = getBaseURL() + "/api/XReplayPlayer/Get?avail=" + avail + "&busy=" + busy;
    var url = httpUrl.replace("http", "ws");

    console.log(url);
    xReplayPlayerWs = new WebSocket(url);

    xReplayPlayerWs.onopen = function () {
        console.log("connected");
    };

    xReplayPlayerWs.onmessage = function (evt) {
        var geoJsonFe = JSON.parse(evt.data);
        AddResourceUpdate(geoJsonFe);
    };
    xReplayPlayerWs.onerror = function (evt) {
        console.log(evt.message);
    };
    xReplayPlayerWs.onclose = function () {
        console.log("disconnected");
    };
}

function AddResourceUpdate(jsonFeObject) {
    try {
        RemoveExistingFeature(jsonFeObject.properties.callsign)
        georesLayer.addData(jsonFeObject);
    }
    catch (e) {
        console.log("AddResourceUpdate exception: " + e.message);
    }
}

function RemoveExistingFeature(callSign) {
    //Leaflet idiosyncracy, cant loop features for a specif layer ?
    map.eachLayer(function (lyr) {
        if (lyr.feature != null && lyr.feature.properties != null) {
            if (lyr.feature.properties.callsign == callSign) {
                georesLayer.removeLayer(lyr);
                //console.log("Resource " + lyr.feature.id + " removed for update");
            }
        }
    });
}

//Removes the resources the user has deselected
function RemoveUnWantedResources(avail, busy) {
    if (avail == false && busy == false)//remove all
    {
        map.eachLayer(function (lyr) {
            if (lyr.feature != null && lyr.feature.properties != null) {
                georesLayer.removeLayer(lyr);
            }
        });
    }
    else if (avail == true || avail == true) //clear exising features, they will be refreshed from the db to get to the starting phase ?
    {
        map.eachLayer(function (lyr) {
            if (lyr.feature != null && lyr.feature.properties != null) {
                georesLayer.removeLayer(lyr);
            }
        });
    }
    else if (avail == false) // remove available resources on the map
    {
        map.eachLayer(function (lyr) {
            if (lyr.feature != null && lyr.feature.properties != null && lyr.feature.properties.available != null) {
                if (lyr.feature.properties.available == 1) {
                    georesLayer.removeLayer(lyr);
                    //console.log("Available resource " + lyr.feature.id + " removed");
                }
            }
        });
    }
    else if (busy == false) //remove busy resources on the map
    {
        map.eachLayer(function (lyr) {
            if (lyr.feature != null && lyr.feature.properties != null && lyr.feature.properties.busy != null) {
                if (lyr.feature.properties.busy == 1) {
                    georesLayer.removeLayer(lyr);
                    //console.log("Busy resource " + lyr.feature.id + " removed");
                }
            }
        });
    }
}

function CreateResourcesLayer() {
    try {
        var useclusters = getStoreAsBool("#use-clusters");

        if (useclusters == true)
            markersr = new L.MarkerClusterGroup(
                {
                    iconCreateFunction: function (cluster) {
                        var childCount = cluster.getChildCount();
                        //var c = ' resourcecluster';
                        //if (childCount < 20) {
                        //    c += 'small';                
                        //} else {
                        //    c += 'large';
                        //}
                        return new L.DivIcon({ html: "<div><span><b>" + childCount + "</b></span></div>", className: "resourcecluster", iconSize: new L.Point(40, 40) });
                    }
                });

        if (markersr == null)
            markersr = L.layerGroup();

        georesLayer = L.geoJson("", {
            style: function (feature) {
                return { color: "#0f0", opacity: 1, fillOpacity: 0.5 };
            },
            pointToLayer: function (feature, latlng) {
                var marker = L.userMarker(latlng, { pulsing: false, accuracy: 0, m: "res", s: feature.properties.status });
                marker.title = feature.properties.callsign;
                return marker;
            },
            onEachFeature: function (feature, layer) {
                layer.on("click", function (e) {
                    $(".resCallsignValue").html(feature.properties.callsign + " (" + feature.properties.fleet + ") " + feature.properties.resourcetype);
                    $(".resStatusValue").html(feature.properties.status);
                    $(".resTimeValue").html(feature.properties.timestamp);
                    $(".resAreaValue").html(feature.properties.area);
                    //$(".resDestinationValue").html(feature.properties.destination);
                    //$(".resEtaValue").html(feature.properties.eta);
                    $(".resIncidentValue").html(feature.properties.incserial);
                    $(".resSkillValue").html(feature.properties.skill);
                    $(".resCommentValue").html(feature.properties.comment);
                    $(".resStandbyValue").html(feature.properties.standby);
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

function GetAddressIoI(longitude, latitude) {
    try {
        var parms = {
            lng: longitude,
            lat: latitude,
        };

        var ioiQueryResults = null;

        var jqxhr = $.ajax({
            url: getURL("Home/InformationSearch"),
            data: parms,
            dataType: "json",
            async: false
        })
        .then(function (items) {
            showInfoItems(items);
            ioiQueryResults = items;
        })
        .fail(function () {
            alert("GetAddressIoI error");
        })
        .always(function () {
            console.log("GetAddressIoI complete");
        });;

        return ioiQueryResults;
    }
    catch (e) {
        alert(e.message);
    }
}

function ClrCallNotifications() {
    CallWs.close();
}

function SetCallNotifications() {
    var extension = getStoreAsBool("#extension");

    var httpUrl = getBaseURL() + "/api/Call/Get?extension=" + extension;
    var url = httpUrl.replace("http", "ws");

    CallWs = new WebSocket(url);

    CallWs.onopen = function () {
        console.log("connected");
    };

    CallWs.onmessage = function (evt) {
        var geoJsonFe = JSON.parse(evt.data);
        AddResourceUpdate(geoJsonFe);
    };
    CallWs.onerror = function (evt) {
        console.log(evt.message);
    };
    CallWs.onclose = function () {
        console.log("disconnected");
    };
}

