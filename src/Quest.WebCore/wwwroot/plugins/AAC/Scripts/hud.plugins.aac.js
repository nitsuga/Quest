var hud = hud || {};

hud.plugins = hud.plugins || {};

hud.plugins.aac = (function () {

    var _initAAC = function (panel) {
        // select the primary menu
        hud.selectPanelMenu(panel, 0);

        // listen for hub messages on these groups
        $("#sys_hub").on("Resource.Available Resource.Busy Resource.Enroute", function (group, msg) {
            _handleMessage(panel, group, msg);
        });

        // listen for panel actions
        $('[data-panel-role=' + panel + ']').on("action", function (evt, action) {
            _handleAction(panel, action);
        });
    };

    // handle message from service bus
    var _handleMessage = function (panel, group, msg) {
        switch (msg.$type) {
            case "Quest.Common.Messages.Resource.ResourceUpdate, Quest.Common":
                break;
            case "Quest.Common.Messages.Resource.ResourceStatusChange, Quest.Common":
                break;
        }
    };

    // handle actions from button push
    var _handleAction = function (panel, action) {
        switch (action) {
            case "select-aac-details":
                var selected = hud.toggleButton(panel, 'select-action', action);
                if (selected)
                {
                    _selectPane(panel, "#panel1", false);
                    _selectPane(panel, "#panel2", true);
                }
                else
                {
                    _selectPane(panel, "#panel1", true);
                    _selectPane(panel, "#panel2", false);
                }
                break;
            default:
                break;
        }
    };

    // attach click handlers to all the panel buttons
    var _registerButtons = function (panel) {
        $('[data-panel-role=' + panel + '] a[data-role="select-map"]').on('click', function (e) {
            e.preventDefault();
            selected_map = $(e.currentTarget).attr('data-action');
            _selectBaseLayer(panel, selected_map);
        });
    };

    var _selectPane = function (panel, pane, state) {
        selector = "div[data-panel-role='" + panel + "'] "+pane;
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
        initAAC: _initAAC,
    };

})();