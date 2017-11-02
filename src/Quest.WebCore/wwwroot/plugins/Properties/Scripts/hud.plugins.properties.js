var hud = hud || {};

hud.plugins = hud.plugins || {};

hud.plugins.properties = (function() {

    var _initialize = function (panel) {

        // select the primary menu
        hud.selectPanelMenu(panel, 0);

        // listen for local hub messages 
        $("#sys_hub").on("ObjectSelected", function (evt, data) {
            _handleMessage(panel, evt, data);
        });

        // listen for panel actions
        $('[data-panel-role=' + panel + ']').on("action", function (evt, action) {
            _handleAction(panel, action);
        });

    };

    // handle message from service bus
    var _handleMessage = function (panel, evt, data) {
        switch (evt.type) {
            case "ObjectSelected":
                console.log("caught ObjectSelected: " + data.Type + "=" + data.Value);
                break;
        }
    };

    var _handleMessage = function (panel, evt, data) {
        switch (evt.type) {
            case "ObjectSelected":
                console.log("caught ObjectSelected: " + data.Type + "=" + data.Value);
                break;
        }
    };


    // handle actions from button push
    var _handleAction = function (panel, action) {
        switch (action) {
            default:
                hud.toggleButton(panel, 'select-action', action);
        }
    };

    return {
        initialize: _initialize
    };

})();