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

    var _handleMessage = function (panel, evt, data) {
        switch (evt.type) {
            case "ObjectSelected":
                console.log("caught ObjectSelected: " + data.Type + "=" + data.Value);
                _loadTemplate(panel, data)
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

    var _loadTemplate = function (panelRole, data) {

        var selector = "[data-panel-role='" + panelRole + "'] [data-role='properties-container']";

        var body = JSON.stringify(data);

        $.ajax({
            url: hud.getURL("Properties/RenderProperties"),
            type: 'POST',
            data: body,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (json) {
                if (json.html.length > 0) {
                    $(selector).html(json.html);
                }
            },
            error: function (result) {
                alert('error from hud.plugins.properties\r\n' + result.responseText);
            }
        });
    };


    return {
        initialize: _initialize
    };

})();