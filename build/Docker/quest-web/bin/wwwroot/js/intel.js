var IntelLyr;          

function initIntel(map) {
    // ReSharper disable once InconsistentNaming
    IntelLyr = new L.featureGroup();
    IntelLyr.addTo(map);
}

function processIntelMessage(msg) {
    switch (msg.$type) {
        case "Quest.Lib.ServiceBus.Messages.IntelIncident, Quest.Lib":
            processIntelIncident(msg);
        return true;

        case "Quest.Lib.ServiceBus.Messages.IntelIncidentDelete, Quest.Lib":
            processIntelIncidentDelete(msg);
        return true;
    }
    return false;
}

function processIntelIncident(msg) {
    if (msg.geometries === undefined)
        return;
    var wicket = new Wkt.Wkt();

    if (msg.geometries.length > 0) {
        wicket.read(msg.geometries[0]);
        var feature1 = wicket.toObject({
            id: msg.id,
            editable: false,
            color: '#AA0000',
            weight: 3,
            opacity: 1.0,
            fillColor: '#AA0000',
            fillOpacity: 0.2
        });

        // Presumably featureGroup is already instantiated and added to your map.
        IntelLyr.addLayer(feature1);
    }
    if (msg.geometries.length > 1) {
        wicket.read(msg.geometries[1]);
        var feature2 = wicket.toObject({
            id: msg.id,
            editable: false,
            color: '#AAAA00',
            weight: 2,
            opacity: 1.0,
            fillColor: '#AAAA00',
            fillOpacity: 0.1
        });
        IntelLyr.addLayer(feature2);
    }
    if (msg.geometries.length > 2) {
        wicket.read(msg.geometries[2]);
        var feature3 = wicket.toObject({
            icon: L.icon({
                iconUrl: 'img\\glyphish-icons\\09-chat-2.png',
                iconRetinaUrl: 'img\\glyphish-icons\\09-chat-2.png',
                iconSize: [24, 22],
                iconAnchor: [0, 0]
            }),
            id: msg.id,
            msg: msg,
            editable: false,
            color: '#AA0000',
            weight: 3,
            opacity: 1.0,
            fillColor: '#AA0000',
            fillOpacity: 0.2
        });

        feature3.on("click",
            function () {
                $("#LOCContent").html("");
                $("#LOCContent").append($("<h4>").text("Themes"));
                var li = $("#LOCContent").append($("<ul>"));
                msg.themes.forEach(function (theme) {
                    if (theme.probability>0)
                        li.append($("<li>").text(theme.name + " (" + Math.floor(theme.probability * 1000)/10 + "%)"));
                });
                $("#LOCContent").append($("<h4>").text("Valid"));
                $("#LOCContent").append($("<div>").text("from: " + msg.times.start));
                $("#LOCContent").append($("<div>").text("to: " + msg.times.end));

                $("#LOCDialog").dialog("open");
                
            });

        IntelLyr.addLayer(feature3);
    }
}

function processIntelIncidentDelete(msg) {
        //Leaflet idiosyncracy, cant loop features for a specific layer ?
    IntelLyr.eachLayer(function (lyr) {
        if (lyr.options != null && lyr.options.id === msg.id) {
            IntelLyr.removeLayer(lyr);
            }
        });
}
