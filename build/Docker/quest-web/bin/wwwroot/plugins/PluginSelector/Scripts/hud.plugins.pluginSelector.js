var hud = hud || {};

hud.plugins = hud.plugins || {};

hud.plugins.pluginSelector = (function() {

    // Hook up to button click events that will load a selected plugin into the parent panel
    var _init = function (panelId, pluginId) {

        $(document).on('click', 'button[data-role="plugin-selector"]',
            function(e) {
                e.preventDefault();

                var id = hud.getPluginPanelId(pluginId);
                var pluginName = $(this).attr('data-plugin-name');
                hud.loadPanel(pluginName, id);
            });

    };

    return {
        init: _init
    }
})();