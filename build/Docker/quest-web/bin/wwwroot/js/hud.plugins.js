var hud = hud || {};

//******************************************************************************************************
//
// Any plugin that wants its methods available to be called by other plugins must register them here
// NOTE: all such method must take their parameters as a single json object.
//
// e.g. hud.plugins.registerMethod('Map', 'CenterChanged')
//
// When the center of the Map is moved, the following call will broadcast the event to other plugins
//
// e.g. hud.plugins.broadcast('Map', 'CenterChanged', { 'lat': newLat, 'lng': newLng })
//
//******************************************************************************************************
hud.plugins = (function() {

    // All methods registered from plugins are stored here in the form { "PluginName": pluginName, "MethodName": methodName }
    var _publicMethods = [];

    var _registerMethod = function (sourcePlugin, methodName) {

        // Make sure we only register methods once
        if (_.find(_publicMethods, function (m) { return m.PluginName === sourcePlugin && m.MethodName === methodName }) !== 'undefined')
            return;

        _publicMethods.push({ 'PluginName': sourcePlugin, 'MethodName': methodName });
    }

    var _handleEventsFromGazeteer = function(eventName, jsonParams) {
        
    }

    // This method is the central controller that will know which plugins will react to which events
    // Amend this method as necessary to wire up plugin events
    // TODO: consider if this is the best way to do this
    var _broadcastEvent = function(sourcePlugin, eventName, jsonParams) {
        
        switch(sourcePlugin) {
            case 'gaz':
                _handleEventsFromGazeteer(eventName, jsonParams);
                break;

            default:
                break;
        }
    };

    return {
        registerMethod: _registerMethod,
        broadcastEvent: _broadcastEvent
    }
})();