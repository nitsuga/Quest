var hud = (function () {

    var _send = function (msg) {
        connection.invoke('send', username, $(messageTextbox).val());
    };

    // start a connection with the central hub
    function startConnection(url, configureConnection) {
        return function start(transport) {
            console.log('Starting connection using ${signalR.TransportType[transport]} transport');
            var connection = new signalR.HubConnection(url, { transport: transport });
            if (configureConnection && typeof configureConnection === 'function') {
                configureConnection(connection);
            }
            return connection.start()
                .then(function () {
                    return connection;
                })
                .catch(function (error) {
                    console.log('Cannot start the connection use ${signalR.TransportType[transport]} transport. ${error.message}');
                    if (transport !== signalR.TransportType.LongPolling) {
                        return start(transport + 1);
                    }
                    return Promise.reject(error);
                });
        }(signalR.TransportType.WebSockets);
    }

    function startStreaming(connection) {
        connection.stream("StreamMessages").subscribe({
            complete: (message) => {
                console.log("completed message: " + message);
            },
            next: (message) => {
                console.log("got message: " + message);
            },
            error: function (err) {
                logger.log(err);
            }
        });
    }

    var _initESB = function () {

        // Start the connection.
        startConnection('/hub', function (connection) {
            // Create a function that the hub can call to broadcast messages.
            connection.on('send', function (name, message) {
            });

            connection.on('SetUsersOnline', function (users) {
                console.log("SetUsersOnline:" + users);
            });
            
        })
        .then(function (connection) {
            console.log('connection started');

            //startStreaming(connection);
        })
        .catch(error => {
            console.error(error.message);
        });
    };

    var _getPanelContent = function (panelSrcId, panelRole) {

        var source;
        var isSingleton = $('#' + panelSrcId).attr('data-singleton');

        if (isSingleton === 'true') {
            source = $('#' + panelSrcId).find('div[data-role="main"]');
        } else {
            source = $('#' + panelSrcId).find('div[data-role="' + panelRole + '"]');
        }

        return $(source).html();
    };

    // load the specific layout into the div id
    var _loadLayout = function (id, layoutName) {
        var url = $('#renderNamedLayoutUrl').attr('data-url') + '/' + layoutName;

        console.log("Layout renderer: " + url);

        // render the hud, note this only builds the panels but doesn't load
        // the plugins.. we'll do that in the next step
        $.get(url, function (json) {

            if (json.length > 0) {
                // 
                $(id).empty();
                $(id).append(json);

                // wire up?
                _bindPanelButtonHandlers();
            }
        });

        // now get the model as well so we can populate the plugins
        var loaderurl = $('#layoutLoaderUrl').attr('data-url') + '/' + layoutName;

        console.log("Populate plugins: " + url);

        $.ajax({
            url: loaderurl,
            type: 'GET',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (json) {

                if (json.panels.length > 0) {
                    setTimeout(function () {
                        json.panels.forEach(function (panel) {
                            if (panel.role >= 0 && panel.plugin !== null) {
                                _loadPanel(panel.plugin, panel.role);
                            }
                        });
                    }
                        , 100);
                }

            },
            error: function (result) {
                alert('error from hud.plugins.pluginSelector._initialize \r\n' + result.responseText);
            }
        });

    };

    // load the specific plugin into the target panel
    var _loadPanel = function (pluginName, panelRole) {

        var selector = '[data-panel-role=' + panelRole + ']';
        var containerPanel = $(selector).first();
        var pluginRole = $(containerPanel).attr('data-panel-role');
        var url = $('#pluginLoaderUrl').attr('data-url') + '/' + pluginName + '?role=' + pluginRole;

        console.log("Plugin Loader: " + url);

        $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (json) {

                if (json.html.length > 0) {
                    var panelContent = $(containerPanel).find('div[data-role="panel-content"]');
                    panelContent.html(json.html);
                }

                console.log(json.onInit);
                if (json.onInit.length > 0) {
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
    };

    // swap two panels
    var _swap = function(panelRole) {

        // Get the panel to be moved to the main panel
        var sidePanel = $('#side-panel-wrapper div.hud-panel[data-panel-role="' + panelRole + '"]');
        var sidePanelContent = $(sidePanel).find('div[data-role=panel-content]');

        // Get the main panel
        var mainPanel = $('#main-panel-wrapper div.hud-panel[data-panel-role="Panel1"]');
        var mainPanelContent = $(mainPanel).find('div[data-role=panel-content]');

        // Make a note of the main panel's inner html
        var mainPanelContentHtml = $(mainPanelContent).html();

        // Put the side panel contents into the main panel
        $(mainPanelContent).html($(sidePanelContent).html());

        // Put the former main panel's contents into the side panel
        $(sidePanelContent).html(mainPanelContentHtml);

        // we need to swap the js attributes
        var mainPanelOnInit = $(mainPanel).attr('data-on-init');
        var mainPanelOnMove = $(mainPanel).attr('data-on-moved');

        var sidePanelOnInit = $(sidePanel).attr('data-on-init');
        var sidePanelOnMove = $(sidePanel).attr('data-on-moved');

        // Put the side panel js attributes onto the main panel
        $(mainPanel).attr('data-on-init', $(sidePanel).attr('data-on-init'));
        $(mainPanel).attr('data-on-moved', $(sidePanel).attr('data-on-moved'));

        // Update the side panel js attributes with the former main panel's
        $(sidePanel).attr('data-on-init', mainPanelOnInit);
        $(sidePanel).attr('data-on-moved', mainPanelOnMove);

        // Finally, run any OnMoved javascript
        if (sidePanelOnMove.length > 0)
            eval(sidePanelOnMove);

        if (mainPanelOnMove.length > 0)
            eval(mainPanelOnMove);

    };

    // make this panel full screen
    var _fullscreen = function(panelRole)  {

        var selector = '[data-panel-role=' + panelRole + ']';
        var containerPanel = $(selector).first();
        var pluginRole = $(containerPanel).attr('data-panel-role');

        // Expand the main panel to 12 columns
        $(containerPanel).removeClass('col-md-9').addClass('col-md-12');

        // Hide the side panel
    };

    // expand the panel
    var _expand = function(panelRoleSource, panelRoleTarget)  {
        var selectorFrom = '[data-panel-role=' + panelRoleSource + ']';
        var containerPanelFrom = $(selectorFrom).first();
    };

    // show menu in the panel
    var _showmenu = function(panelRole)  {
        console.log("Menu btn clicked");
        _loadPanel("PluginSelector", panelRole);
    };

    var _bindPanelButtonHandlers = function () {

        // The menu hamburger loads the plugin selector into the relevant panel
        $('a[data-role="menu"]').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var pluginRole = $(panel).attr('data-panel-role');
                _showmenu(pluginRole);
            });

        $('a[data-role="expand"]').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var pluginRole = $(panel).attr('data-panel-role');
                _expand(pluginRole);
            });

        $('a[data-role="fullscreen"]').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var pluginRole = $(panel).attr('data-panel-role');
                _fullscreen(pluginRole);
            });

        $('a[data-role="swap"]').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var pluginRole = $(panel).attr('data-panel-role');
                _swap(pluginRole);
            });
           
        $('a.panel-btn').on('click',
            function (e) {
                e.preventDefault();

                var role = $(this).attr('data-role');

                var sidePanels = $('#side-panel-wrapper > div.row');

                var upperSidePanel = $(sidePanels).eq(0);
                var lowerSidePanel = $(sidePanels).eq(1);

                switch (role.toLowerCase()) {
                    case 'move-to-main':
                        var thisPanel = $(this).closest('div[data-role="panel"]');
                        _moveToMainPanel($(thisPanel).attr('data-panel-role'));
                        break;

                    case 'expand-full-screen':
                        _expandMainPanelToFullScreeen();
                        break;

                    case 'expand-up':

                        if ($(upperSidePanel).hasClass('half-height')) {
                            // The lower panel expands upwards to fill the viewport height
                            // the upper panel reduces in height to zero, and its buttons are hidden
                            $(upperSidePanel).removeClass('half-height').addClass('no-height');
                            $(lowerSidePanel).removeClass('half-height').addClass('full-height');

                            $(upperSidePanel).find('a.panel-btn-bottom, a.panel-btn-left, a.menu-btn').addClass('hidden');
                            $(lowerSidePanel).find('a.panel-btn-top[data-role="expand-down"]').removeClass('hidden');
                            $(lowerSidePanel).find('a.panel-btn-top[data-role="expand-up"]').addClass('hidden');
                        } else {
                            // The panels revert to the 50:50 split
                            $(lowerSidePanel).removeClass('no-height').addClass('half-height');
                            $(upperSidePanel).removeClass('full-height').addClass('half-height');

                            $(lowerSidePanel).find('a.panel-btn-top[data-role="expand-up"], a.panel-btn-left, a.menu-btn').removeClass('hidden');
                            $(upperSidePanel).find('a.panel-btn-bottom[data-role="expand-up"]').addClass('hidden');
                            $(upperSidePanel).find('a.panel-btn-bottom[data-role="expand-down"]').removeClass('hidden');
                        }

                        // Execute any javascript needed to re-render plugin
                        var lowerPanel = $(lowerSidePanel).find('div[data-role="panel"]');
                        if ($(lowerPanel).attr('data-on-moved').length > 0) {
                            eval($(lowerPanel).attr('data-on-moved'));
                        }
                        break;

                    case 'expand-down':

                        if ($(upperSidePanel).hasClass('half-height')) {
                            // The upper panel expands to fill the viewport height
                            // the lower panel reduces in height to zero
                            $(upperSidePanel).removeClass('half-height').addClass('full-height');
                            $(lowerSidePanel).removeClass('half-height').addClass('no-height');

                            $(lowerSidePanel).find('a.panel-btn-top, a.panel-btn-left, a.menu-btn').addClass('hidden');
                            $(upperSidePanel).find('a.panel-btn-bottom').toggleClass('hidden');
                        } else {
                            // The panels revert to the 50:50 split
                            $(upperSidePanel).removeClass('no-height').addClass('half-height');
                            $(lowerSidePanel).removeClass('full-height').addClass('half-height');

                            $(upperSidePanel).find('a.panel-btn-bottom[data-role="expand-down"], a.panel-btn-left, a.menu-btn').removeClass('hidden');
                            $(lowerSidePanel).find('a.panel-btn-top[data-role="expand-down"]').addClass('hidden');
                            $(lowerSidePanel).find('a.panel-btn-top[data-role="expand-up"]').removeClass('hidden');
                        }

                        // Execute any javascript needed to re-render plugin
                        var upperPanel = $(upperSidePanel).find('div[data-role="panel"]');
                        if ($(upperPanel).attr('data-on-moved').length > 0) {
                            eval($(upperPanel).attr('data-on-moved'));
                        }
                        break;

                    default:
                        break;
                }
            });
    };

    var _initialize = function () {

        // hook up to service bus
        _initESB();

        // bind handlers - maybe use behaviours for this
        _bindPanelButtonHandlers();
    };

    return {
        initialize: _initialize,
        loadPanel: _loadPanel,
        loadLayout: _loadLayout,
        fullscreen: _fullscreen,
        expand: _expand,
        swap: _swap,
        showmenu: _showmenu
    };

})();