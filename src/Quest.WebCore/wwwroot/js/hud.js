var hud = (function () {

    // connection the signalR
    var _connection;

    // our username
    var _username;

    // element emitting signalR events
    var _hubevents;

    var _lastpluginid = 999;

    // remember which message group are being subscribed to
    var groups = {};

    // send a message locally on this session. These messages are NOT sent to the hub
    var _sendLocal = function (event, msg) {
        $("#sys_hub").trigger(event, msg);
    };

    // send a message to the hub
    var _sendGroup = function (msg) {
        _connection.invoke('groupmessage', _username, group, message);
    };

    // join or leave a group
    var _joinLeaveGroup = function (group, pluginId, join) {
        console.log('Join group leave ${group} ${pluginId} ${join}');
        if (join)
            _joinGroup(group, pluginId);
        else
            _leaveGroup(group, pluginId);
    };

    // join a plugin to the message group
    var _joinGroup = function (group, pluginId) {
        console.log('Join group ${group} ${pluginId}');
        var group_list = groups[group];
        if (group_list === undefined) {
            group_list = [];
        }
        var index = group_list.indexOf(pluginId);
        if (index === -1) {
            group_list.push(pluginId); // up the number of registrations            
            if (group_list.length===1)
                // first time registration
                _connection.invoke('joingroup', _username, group);
        }
        groups[group] = group_list;
    };

    // leave a message group.
    var _leaveGroup = function (group, pluginId) {
        console.log('Leave group ${group} ${pluginId}');
        var group_list = groups[group];
        if (group_list === undefined)
            return; // already unsubscribed
        var index = group_list.indexOf(pluginId);
        if (index === -1)
            return; // pluginId not found
        group_list.splice(index, 1);    // remove the pluginId
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

    // get a full url 
    var _getURL = function (partialurl) {
        var s = _getBaseURL() + "/" + partialurl;
        return s;
    };

    var _getBaseURL = function () {
        var url = location.href;  // entire url including querystring - also: window.location.href;
        var baseUrl = url.substring(0, url.indexOf("/", 10));
        //console.debug("b url = " + baseURL);
        return baseUrl;
    };

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
                var msg = JSON.parse(message);
                $("#sys_hub").trigger(group, msg);
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

                _populatePlugins( layoutName );
            }
        });


    };

    var _populatePlugins = function(layoutName)
    {
        var url = $('#renderNamedLayoutUrl').attr('data-url') + '/' + layoutName;

        // now get the model as well so we can populate the plugins
        var loaderurl = $('#layoutLoaderUrl').attr('data-url') + '/' + layoutName;

        console.log("Populate plugins for layout: " + layoutName);

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
                            else
                                // bind handlers for the empty panel
                                _bindPanelButtonHandlers(panel.panel);

                        });
                    }
                        , 1000);
                }

            },
            error: function (result) {
                alert('error from hud.plugins.pluginSelector._initialize \r\n' + result.responseText);
            }
        });
    }

    // load the specific plugin into the target panel
    var _loadPanel = function (pluginName, panelId) {
        console.log("Plugin Loader: " + pluginName+ " in panel " + panelId);

        if (typeof panelId === "string")
            panelId = parseInt(panelId);

        var selector = '[data-panel-role=' + panelId + ']';
        var url = $('#pluginLoaderUrl').attr('data-url') + '/' + pluginName;
        
        $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (json) {
                // this 
                _lastpluginid++;
                var pluginId = _lastpluginid;

                // if the panel returns html then insert the content under the placeholder
                if (json.html.length > 0) {
                    var panelContent = $(selector).find('div[data-role="panel-content"]');

                    // set the data-pluginid to the plugin id
                    $("[data-panel-role='" + panelId + "'] .hud-panel-content").attr('data-pluginid', pluginId);

                    panelContent.html(json.html);
                }

                // bind handlers - maybe use behaviours for this
                _bindPanelButtonHandlers(panelId, pluginId);

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

    // returns  ajquery selector for selecting a plugins' parent content div
    var _pluginSelector = function (pluginId) {
        return ".hud-panel-content[data-pluginid='" + pluginId + "']";
    }

    // returns the panel number of this plugin
    var _getPluginPanelId = function (pluginId) {
        var selector = _pluginSelector(pluginId);
        var containerPanel = $(selector).closest('div[data-role="panel"]');
        return containerPanel.attr('data-panel-role');
    }

    // swap two panels
    var _swap = function (panelSource, panelTarget) {

        console.log('Swap ' + panelSource + " with " + panelTarget);

        // move the source into the swap area
        $("[data-panel-role='" + panelSource + "'] div[data-role='panel-content']").children().appendTo('#swap');

        // move the target top the source
        $("[data-panel-role='" + panelTarget + "'] div[data-role='panel-content']").children().appendTo($("[data-panel-role='" + panelSource + "'] div[data-role='panel-content']"));

        // move the swap to the target
        $("#swap").children().appendTo($("[data-panel-role='" + panelTarget + "'] div[data-role='panel-content']"));

        _sendLocal("Swapped", {
            "panelSource": panelSource,
            "panelTarget": panelTarget
        });
    };

    // make this panel full screen
    var _fullscreen = function (panelSource) {
        if (typeof panelSource === "string")
            panelSource = parseInt(panelSource);

        console.log('Fullscreen %d', panelSource);

        _sendLocal("Fullscreen", {
            "panelSource": panelSource,
            "panelTarget": panelTarget
        });
    };

    // expand the panel
    var _expand = function (panelSource, panelTarget) {

        if (typeof panelSource === "string")
            panelSource = parseInt(panelSource);

        if (typeof panelTarget === "string")
            panelTarget = parseInt(panelTarget);

        console.log('Expand %d into %d',panelSource, panelTarget);
        var selectorFrom = '[data-panel-role=' + panelSource + ']';
        var selectorTo = '[data-panel-role=' + panelTarget + ']';
        var containerPanelFrom = $(selectorFrom).first();

        $(selectorTo).hide();

        _sendLocal("Expanded", {
            "panelSource": panelSource,
            "panelTarget": panelTarget
        });

    };

    // show menu in the panel
    var _showmenu = function (panelId) {
        if (typeof panelRole === "string")
            panelRole = parseInt(panelId);
        console.log('Show Menu %d', panelId);
        _loadPanel("PluginSelector", panelId);
    };

        // wire up handlers for this panel
    var _bindPanelButtonHandlers = function (panelRole, pluginId) {

        console.log("bindPanelButtonHandlers panel=" + panelRole + " plugin=" + pluginId);

        //----------------------------------------------
        // clear ALL handlers on this panel
        $("[data-panel-role='" + panelRole + "']").off();

        if (typeof panelRole === "string")
            panelRole = parseInt(panelRole);

        // The menu hamburger loads the plugin selector into the relevant panel
        $('[data-panel-role=' + panelRole + '] a[data-role="menu"]').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var pluginRole = $(panel).attr('data-panel-role');
                _showmenu(panelRole);
            });

        $('[data-panel-role="' + panelRole + '"] a[data-role="expand"]').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var panelRoleSource = $(panel).attr('data-panel-role');
                var panelRoleTarget = $(this).attr('data-target');

                _expand(panelRoleSource, panelRoleTarget);
            });

        $('[data-panel-role="' + panelRole + '"] a[data-role="fullscreen"]').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var pluginRole = $(panel).attr('data-panel-role');
                _fullscreen(pluginRole);
            });

        $('[data-panel-role="' + panelRole + '"] a[data-role="swap"]').on('click',
            function (e) {
                e.preventDefault();
                var panel = $(this).parent();
                var panelRoleSource = $(panel).attr('data-panel-role');
                var panelRoleTarget = $(this).attr('data-target');

                _swap(panelRoleSource, panelRoleTarget);

            });

        //----------------------------------------------
        // now bind plugin-specific events

        if (pluginId !== undefined) {
            var plugin = _pluginSelector(pluginId);

            // menus on the plugin itself
            $(plugin + ' a[data-role="select-menu"]').on('click',
                function (e) {
                    e.preventDefault();
                    btn_role = $(e.currentTarget).attr('data-role');
                    btn_action = $(e.currentTarget).attr('data-action');
                    _selectMenu(panelRole, btn_action);
                });

            // actions on the plugin itself
            $(plugin + ' a[data-role="select-action"]').on('click',
                function (e) {
                    e.preventDefault();
                    btn_role = $(e.currentTarget).attr('data-role');
                    btn_action = $(e.currentTarget).attr('data-action');
                    $(plugin).trigger("action", btn_action);
                });
        }
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
    var _selectMenu = function (pluginId, menu) {
        console.log('selectMenu plugin=%d menu=%d', pluginId, menu);
        // all anchors with panel-btn-p* 
        otherbuttons = _pluginSelector(pluginId) + " a[data-role|='select']";
        $(otherbuttons).removeClass("panel-btn-hide");
        $(otherbuttons).addClass("panel-btn-hide");

        // all anchors with panel-btn-p* and the menu we want
        buttons = _pluginSelector(pluginId) + " a[data-menu='" + menu + "'] ";
        $(buttons).removeClass("panel-btn-hide");

        _sendLocal("MenuChange", {
            "pluginId": pluginId,
            "menu": menu
        });
    };

    // set panel button state
    var _setButtonState = function (pluginId, role, action, state) {
        console.log('setButtonState %d %s %s %s', pluginId, role, action, state);
        var selector = _pluginSelector(pluginId) + " [data-role='" + role + "'][data-action='" + action + "'] ";

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

    var _getButtonState = function (pluginId, role, action) {
        
        var selector = _pluginSelector(pluginId) + " [data-role='" + role + "'][data-action='" + action + "'] ";
        console.log('getButtonState %d %s %s found %d', pluginId, role, action, $(selector).length);
        return $(selector).hasClass("panel-btn-on");
    };

    var _toggleButton = function (pluginId, role, action) {
        
        console.log('toggleButtonState Menu %d %s %s', panelRole, role, action);
        ison = _getButtonState(pluginId, role, action);
        return _setButtonState(pluginId, role, action, !ison);
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
        selectMenu: _selectMenu,
        sendLocal: _sendLocal,
        sendGroup: _sendGroup,
        getURL: _getURL,
        getBaseURL: _getBaseURL,
        pluginSelector: _pluginSelector,
        getPluginPanelId: _getPluginPanelId

    };

})();