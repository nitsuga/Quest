var hud = (function () {

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

    var _loadPanel = function (pluginName, panelRole) {
        
        var selector = '[data-panel-role=' + panelRole + ']';
        var containerPanel = $(selector).first();
        var url = $('#pluginLoaderUrl').attr('data-url') + '/' + pluginName;

        console.log("Plugin Loader: " + url);

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
    }

    var _loadAllPanelContents = function () {

        //$('div[data-role="panel"]').each(function (index, item) {

        //    var $this = $(this);

        //    // Identify the target content panel
        //    //var content = $this.find('div[data-role="panel-content"]')[0];
        //    //var contentHtml = _getPanelContent($this.attr('data-src'), $this.attr('data-panel-role'));
        //    //$(content).html(contentHtml);

        //    // Call any javascript init code
        //    var script = $this.attr('data-on-init');
        //    if (script.length > 0) {
        //        console.log("hud._loadAllPanelContents    Executing script: " + script);
        //        eval(script);
        //    }
        //});
    };

    var _moveToMainPanel = function(panelRole) {

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

    var _expandMainPanelToFullScreeen = function() {

        // Get the main panel
        var mainPanel = $('#main-panel-wrapper');

        // Get the side panel
        var sidePanel = $('#side-panel-wrapper ');

        // Expand the main panel to 12 columns
        $(mainPanel).removeClass('col-md-9').addClass('col-md-12');

        // Hide the side panel
        $(sidePanel).addClass('hidden');
    };

    var _bindPanelButtonHandlers = function () {

        // The menu hamburger loads the plugin selector into the relevant panel
        $('a.menu-btn').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var contentPanel = $(panel).find('div[data-role="panel-content"]');
                var pluginRole = $(panel).attr('data-panel-role');

                var url = $('#pluginLoaderUrl').attr('data-url') + '/PluginSelector';

                console.log("Menu btn clicked: " + url);

                $.ajax({
                    url: url,
                    type: 'GET',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    success: function (json) {

                         if (json.html.length > 0) {
                             $(contentPanel).html(json.html);
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
                        alert('error from menu-btn click \r\n' + result.responseText);
                    }
                });
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
        _loadAllPanelContents();
        _bindPanelButtonHandlers();
    }

    return {
        initialize: _initialize,
        loadPanel: _loadPanel
    }
})();