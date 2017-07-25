function processMapMessage(msg) {
      switch (msg.$type) {
        case "Quest.Mobile.Models.ResourceFeature, Quest.Mobile":
//            processResourceFeature(georesLayer, msg);
            break;

        case "Quest.Mobile.Models.IncidentFeature, Quest.Mobile":
//            processIncidentFeature(geoincLayer, msg);
            break;

        case "Quest.Lib.ServiceBus.Messages.RoutingEngineStatus, Quest.Lib":
//            processRoutingEngineStatus(msg);
            break;
            
    }
}
