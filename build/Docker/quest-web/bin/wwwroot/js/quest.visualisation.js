var timeline_timeline;
var timeline_groups = new vis.DataSet();
var timeline_items = new vis.DataSet();
var timeLineContainer;
var vislayer;

function initVisuals() {

    $("#properties").dialog({
        position: { my: "left top", at: "right top", of: $("#SearchDialog").dialog("widget") },
        show: { effect: "fade", duration: 300},
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

    var dp1 = timeline_StartTimeDatePicker.data('datepicker');
    dp1.selectDate(new Date("August 08, 2014 07:00:00"));

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
        

        if ((incident+resource) !== "" ) {
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

    $("#visquery-go").click(function () {
        visualQuery();
        return true;
    });

}

// process the response containing visuals and display
// on the timeline and on screen
function displayVisuals(response) {

    if (response == null ) {
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
            item.Timeline.forEach(function(dataitem) {
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

function getBaseURL() {
    var url = location.href;  // entire url including querystring - also: window.location.href;
    var baseUrl = url.substring(0, url.indexOf("/", 10));
    return baseUrl;
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
        error: function (e,e1,e2) {
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

