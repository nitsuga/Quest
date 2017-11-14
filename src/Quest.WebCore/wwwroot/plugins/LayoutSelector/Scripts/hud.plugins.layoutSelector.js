var hud = hud || {};

hud.plugins = hud.plugins || {};

hud.plugins.layoutSelector = (function() {

    // Hook up to button click events that will load a selected plugin into the parent panel
    var _init = function (panelId, pluginId) {

        $(document).on('click', 'button[data-role="layout-selector"]',
            function(e) {
                e.preventDefault();
                var layoutName = $(this).attr('data-layout-name');
                hud.loadLayout("#panel-container", layoutName);
            });
    };

    return {
        init: _init
    }
})();