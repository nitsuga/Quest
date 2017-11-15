var hud = hud || {};

hud.plugins = hud.plugins || {};

hud.plugins.properties = (function() {

    var _init = function (panelId, pluginId) {

        // select the primary menu
        hud.selectMenu(pluginId, 0);

        // listen for local hub messages 
        $("#sys_hub").on("ObjectSelected", function (evt, data) {
            _handleMessage(pluginId, evt, data);
        });

        // listen for panel actions
        var selector = hud.pluginSelector(pluginId);
        $(selector).on("action", function (evt, action) {
            _handleAction(pluginId, action);
        });
    };

    var _handleMessage = function (pluginId, evt, data) {
        switch (evt.type) {
            case "ObjectSelected":
                console.log("caught ObjectSelected: " + data.Type + "=" + data.Value);
                _loadTemplate(pluginId, data)
                break;
        }
    };

    // handle actions from button push
    var _handleAction = function (pluginId, action) {
        switch (action.action) {
            default:
                hud.toggleButton(pluginId, 'select-action', action);
        }
    };

    var _loadTemplate = function (pluginId, data) {

        var selector = hud.pluginSelector(pluginId) + " [data-role='properties-container']";

        var body = JSON.stringify(data);

        $.ajax({
            url: hud.getURL("Properties/RenderProperties"),
            type: 'POST',
            data: body,
            dataType: "html",
            contentType: "application/json; charset=utf-8",
            success: function (html) {
                $(selector).html(html);
            },
            error: function (result) {
                alert('error from hud.plugins.properties\r\n' + result.responseText);
            }
        });
    };


    return {
        init: _init
    };

})();