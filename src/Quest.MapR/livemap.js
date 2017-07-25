var notificationService;

$(function () {
  SetupNotifications();
});

function getBaseURL() {
    return "http://localhost:3840/";
//    return window.applicationBaseUrl;
}

function SetupNotifications() {
    console.log("Setting up notifications");
    if (notificationService === undefined) {

        var httpUrl = getBaseURL() + "/api/Notifications/Get";
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

                if (processMapMessage !== undefined)
                    processMapMessage(msg);

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
