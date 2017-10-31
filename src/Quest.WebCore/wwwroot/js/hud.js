﻿var hud = (function () {

    // connection the signalR
    var _connection;

    // our username
    var _username;

    // element emitting signalR events
    var _hubevents;

    // remember which message group are being subscribed to
    var groups = {};

    // send a message to the hub
    var _send = function (msg) {
        _connection.invoke('send', _username, $(messageTextbox).val());
    };

    var _joinLeaveGroup = function (group, panel, join) {
        if (join)
            _joinGroup(group, panel);
        else
            _leaveGroup(group, panel);
    };

    // join a panel to the message group
    var _joinGroup = function (group, panel) {
        var group_list = groups[group];
        if (group_list === undefined) {
            group_list = [];
        }
        var index = group_list.indexOf(panel);
        if (index === -1) {
            group_list.push(panel); // up the number of registrations            
            if (group_list.length===1)
                // first time registration
                _connection.invoke('joingroup', _username, group);
        }
        groups[group] = group_list;
    };

    // leave a message group.
    var _leaveGroup = function (group, panel) {
        var group_list = groups[group];
        if (group_list === undefined)
            return; // already unsubscribed
        var index = group_list.indexOf(panel);
        if (index === -1)
            return; // panel not found
        group_list.splice(index, 1);    // remove the panel
        if (group_list.length===0)      // no-one left in the group
            _connection.invoke('leavegroup', _username, group);
        groups[group] = group_list;
       
    };

    // start a connection with the central hub
    function startConnection(url, configureConnection) {

        return function start(transport) {

            console.log('Starting connection using ${signalR.TransportType[transport]} transport');
            var connection = new signalR.HubConnection(url, { transport: transport });

            // execut ecallback if provided
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

    var _initESB = function () {

        // Start the connection.
        startConnection('/hub', function (connection) {

            _connection = connection;

            connection.on('joingroup', function (name, group) {
                // someone has joined a group
                console.log("joingroup:" + group + ":" + name);
            });

            connection.on('leavegroup', function (name, group) {
                // someone has left a group
                console.log("leavegroup:" + group + ":" + name);
            });

            connection.on('send', function (name, message) {
            });

            // Create a function that the hub can call to broadcast messages to a specific group.
            connection.on('groupmessage', function (name, group, message) {
                $("#sys_hub").trigger(group, message);
            });

            connection.on('setusersonline', function (users) {
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
                            if (panel.panel >= 0 && panel.plugin !== null) {
                                _loadPanel(panel.plugin, panel.panel);
                            }
                        });
                    }
                        , 1000);
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

                // bind handlers - maybe use behaviours for this
                _bindPanelButtonHandlers(panelRole);

            },
            error: function (result) {
                alert('error from hud.plugins.pluginSelector._initialize \r\n' + result.responseText);
            }
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

        // wire up handlers for this panel
    var _bindPanelButtonHandlers = function (panelRole) {
        
        // The menu hamburger loads the plugin selector into the relevant panel
        $('[data-panel-role=' + panelRole + '] a[data-role="menu"]').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var pluginRole = $(panel).attr('data-panel-role');
                _showmenu(pluginRole);
            });

        // menus on the panel
        $('[data-panel-role=' + panelRole + '] a[data-role="select-menu"]').on('click',
            function (e) {
                e.preventDefault();
                btn_role = $(e.currentTarget).attr('data-role');
                btn_action = $(e.currentTarget).attr('data-action');
                _selectPanelMenu(panelRole, btn_action);
            });

        // actions on the panel
        $('[data-panel-role=' + panelRole + '] a[data-role="select-action"]').on('click',
            function (e) {
                e.preventDefault();
                btn_role = $(e.currentTarget).attr('data-role');
                btn_action = $(e.currentTarget).attr('data-action');
                $('[data-panel-role=' + panelRole + ']').trigger("action", btn_action);
            });

        $('[data-panel-role=' + panelRole + '] a[data-role="expand"]').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var pluginRole = $(panel).attr('data-panel-role');
                _expand(pluginRole);
            });

        $('[data-panel-role=' + panelRole + '] a[data-role="fullscreen"]').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var pluginRole = $(panel).attr('data-panel-role');
                _fullscreen(pluginRole);
            });

        $('[data-panel-role=' + panelRole + '] a[data-role="swap"]').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var pluginRole = $(panel).attr('data-panel-role');
                _swap(pluginRole);
            });
           
        //$('a.panel-btn').on('click',
        //    function (e) {
        //        e.preventDefault();

        //        var role = $(this).attr('data-role');

        //        var sidePanels = $('#side-panel-wrapper > div.row');

        //        var upperSidePanel = $(sidePanels).eq(0);
        //        var lowerSidePanel = $(sidePanels).eq(1);

        //        switch (role.toLowerCase()) {
        //            case 'move-to-main':
        //                var thisPanel = $(this).closest('div[data-role="panel"]');
        //                _moveToMainPanel($(thisPanel).attr('data-panel-role'));
        //                break;

        //            case 'expand-full-screen':
        //                _expandMainPanelToFullScreeen();
        //                break;

        //            case 'expand-up':

        //                if ($(upperSidePanel).hasClass('half-height')) {
        //                    // The lower panel expands upwards to fill the viewport height
        //                    // the upper panel reduces in height to zero, and its buttons are hidden
        //                    $(upperSidePanel).removeClass('half-height').addClass('no-height');
        //                    $(lowerSidePanel).removeClass('half-height').addClass('full-height');

        //                    $(upperSidePanel).find('a.panel-btn-bottom, a.panel-btn-left, a.menu-btn').addClass('hidden');
        //                    $(lowerSidePanel).find('a.panel-btn-top[data-role="expand-down"]').removeClass('hidden');
        //                    $(lowerSidePanel).find('a.panel-btn-top[data-role="expand-up"]').addClass('hidden');
        //                } else {
        //                    // The panels revert to the 50:50 split
        //                    $(lowerSidePanel).removeClass('no-height').addClass('half-height');
        //                    $(upperSidePanel).removeClass('full-height').addClass('half-height');

        //                    $(lowerSidePanel).find('a.panel-btn-top[data-role="expand-up"], a.panel-btn-left, a.menu-btn').removeClass('hidden');
        //                    $(upperSidePanel).find('a.panel-btn-bottom[data-role="expand-up"]').addClass('hidden');
        //                    $(upperSidePanel).find('a.panel-btn-bottom[data-role="expand-down"]').removeClass('hidden');
        //                }

        //                // Execute any javascript needed to re-render plugin
        //                var lowerPanel = $(lowerSidePanel).find('div[data-role="panel"]');
        //                if ($(lowerPanel).attr('data-on-moved').length > 0) {
        //                    eval($(lowerPanel).attr('data-on-moved'));
        //                }
        //                break;

        //            case 'expand-down':

        //                if ($(upperSidePanel).hasClass('half-height')) {
        //                    // The upper panel expands to fill the viewport height
        //                    // the lower panel reduces in height to zero
        //                    $(upperSidePanel).removeClass('half-height').addClass('full-height');
        //                    $(lowerSidePanel).removeClass('half-height').addClass('no-height');

        //                    $(lowerSidePanel).find('a.panel-btn-top, a.panel-btn-left, a.menu-btn').addClass('hidden');
        //                    $(upperSidePanel).find('a.panel-btn-bottom').toggleClass('hidden');
        //                } else {
        //                    // The panels revert to the 50:50 split
        //                    $(upperSidePanel).removeClass('no-height').addClass('half-height');
        //                    $(lowerSidePanel).removeClass('full-height').addClass('half-height');

        //                    $(upperSidePanel).find('a.panel-btn-bottom[data-role="expand-down"], a.panel-btn-left, a.menu-btn').removeClass('hidden');
        //                    $(lowerSidePanel).find('a.panel-btn-top[data-role="expand-down"]').addClass('hidden');
        //                    $(lowerSidePanel).find('a.panel-btn-top[data-role="expand-up"]').removeClass('hidden');
        //                }

        //                // Execute any javascript needed to re-render plugin
        //                var upperPanel = $(upperSidePanel).find('div[data-role="panel"]');
        //                if ($(upperPanel).attr('data-on-moved').length > 0) {
        //                    eval($(upperPanel).attr('data-on-moved'));
        //                }
        //                break;

        //            default:
        //                break;
        //        }
        //    });
    };
    
    var _setStore = function (cname, cvalue, exdays) {
        localStorage.setItem(cname, cvalue);
    };

    var _getStore = function (cname) {
        return localStorage.getItem(cname);
    };

    var _getStoreAsBool = function (cname) {
        return localStorage.getItem(cname) === "true" ? true : false;
    };

    // set a bootstrap slider from the store
    var _setSliderFromStore = function (name) {
        if (name === '#')
            return;
        var v = getStoreAsBool(name);
        setSlider(name, v);
    };

    // save bootstrap slider value
    var _setStoreFromSlider = function (name) {
        if (name === '#')
            return;
        var v = $(name).prop('checked');
        setStore(name, v, 365);
    };

    var _setSlider = function (name, position) {
        $(name).bootstrapToggle(position ? 'on' : 'off');
    };

    var _toggleSlider = function (name) {
        var v = !$(name).hasClass("off");
        v = !v;
        setStore(name, v, 365);
        setSlider(name, v);
    };

    // select a particular set of panel buttons
    var _selectPanelMenu = function (panel, menu) {
        // all anchors with panel-btn-p* 
        otherbuttons = "div[data-panel-role='" + panel + "'] a[data-role|='select']";
        //otherbuttons = "div[data-panel-role='" + role + "'] a[class|='panel-btn-p'][data-menu!='" + menu + "'] ";
        $(otherbuttons).removeClass("panel-btn-hide");
        $(otherbuttons).addClass("panel-btn-hide");

        // all anchors with panel-btn-p* and the menu we want
        buttons = "div[data-panel-role='" + panel + "'] a[data-menu='" + menu + "'] ";
        $(buttons).removeClass("panel-btn-hide");
    };

    var _setButtonState = function (panel, role, action, state) {
        selector = "div[data-panel-role='" + panel + "'] a[data-role='" + role + "'][data-action='" + action + "'] ";
        if (state) {
            $(selector).removeClass("panel-btn-off");
            $(selector).addClass("panel-btn-on");
        }
        else {
            $(selector).removeClass("panel-btn-on");
            $(selector).addClass("panel-btn-off");
        }
        return state;
    };

    var _getButtonState = function (panel, role, action) {
        selector = "div[data-panel-role='" + panel + "'] a[data-role='" + role + "'][data-action='" + action + "'] ";
        return $(selector).hasClass("panel-btn-on");
    };

    var _toggleButton = function (panel, role, action) {
        ison = _getButtonState(panel, role, action);
        return _setButtonState(panel, role, action, !ison);
    };
        
    var _initLocalStorage = function () {
        // support the case where localstorage is not intrinsically available.
        if (!window.localStorage) {
            Object.defineProperty(window, "localStorage", new function () {
                var aKeys = [], oStorage = {};
                Object.defineProperty(oStorage, "getItem", {
                    value: function (sKey) { return sKey ? this[sKey] : null; },
                    writable: false,
                    configurable: false,
                    enumerable: false
                });
                Object.defineProperty(oStorage, "key", {
                    value: function (nKeyId) { return aKeys[nKeyId]; },
                    writable: false,
                    configurable: false,
                    enumerable: false
                });
                Object.defineProperty(oStorage, "setItem", {
                    value: function (sKey, sValue) {
                        if (!sKey) { return; }
                        document.cookie = escape(sKey) + "=" + escape(sValue) + "; expires=Tue, 19 Jan 2038 03:14:07 GMT; path=/";
                    },
                    writable: false,
                    configurable: false,
                    enumerable: false
                });
                Object.defineProperty(oStorage, "length", {
                    get: function () { return aKeys.length; },
                    configurable: false,
                    enumerable: false
                });
                Object.defineProperty(oStorage, "removeItem", {
                    value: function (sKey) {
                        if (!sKey) { return; }
                        document.cookie = escape(sKey) + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/";
                    },
                    writable: false,
                    configurable: false,
                    enumerable: false
                });
                this.get = function () {
                    var iThisIndx;
                    for (var sKey in oStorage) {
                        iThisIndx = aKeys.indexOf(sKey);
                        if (iThisIndx === -1) { oStorage.setItem(sKey, oStorage[sKey]); }
                        else { aKeys.splice(iThisIndx, 1); }
                        delete oStorage[sKey];
                    }
                    for (aKeys; aKeys.length > 0; aKeys.splice(0, 1)) { oStorage.removeItem(aKeys[0]); }
                    for (var aCouple, iKey, nIdx = 0, aCouples = document.cookie.split(/\s*;\s*/); nIdx < aCouples.length; nIdx++) {
                        aCouple = aCouples[nIdx].split(/\s*=\s*/);
                        if (aCouple.length > 1) {
                            oStorage[iKey = unescape(aCouple[0])] = unescape(aCouple[1]);
                            aKeys.push(iKey);
                        }
                    }
                    return oStorage;
                };
                this.configurable = false;
                this.enumerable = true;
            }());
        }
    };

    var _initialize = function () {

        _username = $("#sys_username").data('username');

        _hubevents = $("#sys_hub");

        // hook up to service bus
        _initESB();

        _initLocalStorage();

    };

    return {
        initialize: _initialize,
        loadPanel: _loadPanel,
        loadLayout: _loadLayout,
        fullscreen: _fullscreen,
        expand: _expand,
        swap: _swap,
        showmenu: _showmenu,
        joinGroup: _joinGroup,
        leaveGroup: _leaveGroup,
        joinLeaveGroup: _joinLeaveGroup,
        toggleSlider: _toggleSlider,
        setSlider: _setSlider,
        setStoreFromSlider: _setStoreFromSlider,
        setSliderFromStore: _setSliderFromStore,
        getStoreAsBool: _getStoreAsBool,
        getStore: _getStore,
        setStore: _setStore,
        setButtonState: _setButtonState,
        getButtonState: _getButtonState,
        toggleButton: _toggleButton,
        selectPanelMenu: _selectPanelMenu

    };

})();