var hud = hud || {};

hud.plugins = hud.plugins || {};

hud.plugins.aac = (function () {

    var _init = function (panelId, pluginId) {
        // select the primary menu
        hud.selectMenu(pluginId, 0);

        // listen for hub messages on these groups
        $("#sys_hub").on("Resource.Available Resource.Busy Resource.Enroute", function (group, msg) {
            _handleMessage(pluginId, group, msg);
        });

        // listen for panel actions
        var selector = hud.pluginSelector(pluginId);
        $(selector).on("action", function (evt, action) {
            _handleAction(pluginId, action);
        });
    };

    // handle message from service bus
    var _handleMessage = function (pluginId, group, msg) {
        switch (msg.$type) {
            case "Quest.Common.Messages.Resource.ResourceUpdate, Quest.Common":
                break;
            case "Quest.Common.Messages.Resource.ResourceStatusChange, Quest.Common":
                break;
        }
    };

    // handle actions from button push
    var _handleAction = function (pluginId, action) {
        switch (action) {
            case "select-aac-details":
                var selected = hud.toggleButton(pluginId, 'select-action', action);
                if (selected)
                {
                    _selectPane(pluginId, "#panel1", false);
                    _selectPane(pluginId, "#panel2", true);
                }
                else
                {
                    _selectPane(pluginId, "#panel1", true);
                    _selectPane(pluginId, "#panel2", false);
                }
                break;
            default:
                break;
        }
    };

    // attach click handlers to all the panel buttons
    var _registerButtons = function (pluginId) {
    };

    var _selectPane = function (pluginId, pane, state) {
        
        selector = hud.pluginSelector(pluginId) + " "+ pane;
        if (state) {
            $(selector).removeClass("pane-hidden");
        }
        else {
            $(selector).removeClass("pane-hidden");
            $(selector).addClass("pane-hidden");
        }
        return state;
    };

    return {
        init: _init,
    };

})();