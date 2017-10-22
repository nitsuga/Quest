var hud = hud || {};

hud.plugins = hud.plugins || {};

hud.plugins.layoutSelector = (function() {

    var _initialized = false;

    // Hook up to button click events that will load a selected plugin into the parent panel
    var _initialize = function() {

        if(_initialized === true) return;

        $(document).on('click', 'button[data-role="layout-selector"]',
            function(e) {
                e.preventDefault();

                var pluginName = $(this).attr('data-layout-name');
                var containerPanel = $(this).closest('div[data-role="panel"]');

                hud.loadLayout(pluginName);
            });

        _initialized = true;
    };

    return {
        initialize: _initialize
    }
})();