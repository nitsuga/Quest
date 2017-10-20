var hud = hud || {};

hud.plugins = hud.plugins || {};

hud.plugins.pluginSelector = (function() {

    var _initialized = false;

    // Hook up to button click events that will load a selected plugin into the parent panel
    var _initialize = function() {

        if(_initialized === true) return;

        $(document).on('click', 'button[data-role="plugin-selector"]',
            function(e) {
                e.preventDefault();

                var pluginName = $(this).attr('data-plugin-name');

                var containerPanel = $(this).closest('div[data-role="panel"]');
                var pluginRole = $(containerPanel).attr('data-panel-role');

                var url = $('#pluginLoaderUrl').attr('data-url') + '/' + pluginName;

                console.log("Plugin Selector btn clicked: " + url);

                $.ajax({
                    url: url,
                    type: 'GET',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    success: function (json) {

                        if (json.html.length > 0) {
                            $(containerPanel).find('div[data-role="panel-content"]').html(json.html);    
                        }
                        
                        console.log(json.onInit);
                        if (json.onInitJs.length > 0) {
                            eval(json.onInit);
                        }

                        if (json.onPanelMoved.length > 0) {
                            $(containerPanel).attr('data-on-moved', json.onPanelMoved);
                        }
                    },
                    error: function (result) {
                        alert('error from hud.plugins.pluginSelector._initialize \r\n' + result.responseText);
                    }
                });

            });

        _initialized = true;
    };

    return {
        initialize: _initialize
    }
})();