var hud = hud || {};

hud.plugins = hud.plugins || {};

hud.plugins.heldcalls = (function () {

    var _init = function (panelId, pluginId) {
        // select the primary menu
        hud.selectMenu(pluginId, 0);

        // listen for hub messages on these groups
        $("#sys_hub").on("ResourceAssignments", function (group, msg) {
            _handleMessage(pluginId, group, msg);
        });

        // listen for panel actions
        var selector = hud.pluginSelector(pluginId);
        $(selector).on("action", function (evt, action) {
            _handleAction(pluginId, action);
        });

        _getAssignments();
    };

    // handle message from service bus
    //var _handleMessage = function (pluginId, group, msg) {
    //    switch (msg.$type) {
    //        case "Quest.Common.Messages.Resource.ResourceUpdate, Quest.Common":
    //            break;
    //        case "Quest.Common.Messages.Resource.ResourceStatusChange, Quest.Common":
    //            break;
    //    }
    //};

    // handle actions from button push
    var _handleAction = function (pluginId, action) {
        switch (action) {
            default:
                break;
        }
    };

    // attach click handlers to all the panel buttons
    var _registerButtons = function (pluginId) {
    };

    return {
        init: _init,
    };

})();