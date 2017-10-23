var hud = hud || {}

hud.plugins = hud.plugins || {}

/*
    There could be multiple instances of a chat window, so this component does not reference any #Ids
*/
hud.plugins.chat = (function() {

    var _hub;

    // This optional function html-encodes messages for display in the page.
    var  _htmlEncode = function(value) {
        var encodedValue = $('<div />').text(value).html();
        return encodedValue;
    }

    var _connectionDeferred;
    var _subscribeToConnectionStart = function(callback) {
        if (!_connectionDeferred) // start connection if not yet initialized
            _connectionDeferred = $.connection.hub.start();

        if ($.connection.hub.state === $.connection.connectionState.connected && callback) {
            // already connected
            callback();
        } else if (callback) {
            // register handler
            _connectionDeferred.done(callback);
        }
    };

    function startConnection(url, configureConnection) {
        return function start(transport) {
            console.log(`Starting connection using ${signalR.TransportType[transport]} transport`)
            var connection = new signalR.HubConnection(url, { transport: transport });
            if (configureConnection && typeof configureConnection === 'function') {
                configureConnection(connection);
            }
            return connection.start()
                .then(function () {
                    return connection;
                })
                .catch(function (error) {
                    console.log(`Cannot start the connection use ${signalR.TransportType[transport]} transport. ${error.message}`);
                    if (transport !== signalR.TransportType.LongPolling) {
                        return start(transport + 1);
                    }
                    return Promise.reject(error);
                });
        }(signalR.TransportType.WebSockets);
    }

    var _initChatConnection = function(chatContainer) {

        // Reference the auto-generated proxy for the hub.

        _hub = new signalR.HubConnection('/hub');

        _hub.on('send', data => {
            console.log(data);
        });

        _hub.connection.start();

        // Find the message container
        var messageList = $(chatContainer).find('div.chat-msg-container > ul[data-role="discussion"]');

        // Create a function that the hub can call back to display messages.
        //_hub.client.addNewMessageToPage = function (name, message) {
            // Add the message to the page.
            //$(messageList).append('<li><strong>' + _htmlEncode(name)  + '</strong>: ' + _htmlEncode(message) + '</li>');
        //};

        // Find the username
        var username = $(chatContainer).attr('data-username');

        // Set initial focus to message input box.
        var messageTextbox = $(chatContainer).find('div.chat-msg-container input[data-role="message"]');
        $(messageTextbox).focus();

        // Find the send message button
        var sendButton = $(chatContainer).find('div.chat-msg-container input[data-role="sendmessage"]');

        // Start the connection.
        startConnection('/hub', function (connection) {
            // Create a function that the hub can call to broadcast messages.
            connection.on('broadcastMessage', function (name, message) {
                // Html encode display name and message.
                var encodedName = name;
                var encodedMsg = message;
                // Add the message to the page.
                var liElement = document.createElement('li');
                liElement.innerHTML = '<strong>' + encodedName + '</strong>:&nbsp;&nbsp;' + encodedMsg;
                document.getElementById('discussion').appendChild(liElement);
            });
        })
            .then(function (connection) {
                console.log('connection started');
                document.getElementById('sendmessage').addEventListener('click', function (event) {
                    // Call the Send method on the hub.
                    connection.invoke('send', name, messageInput.value);
                    // Clear text box and reset focus for next comment.
                    messageInput.value = '';
                    messageInput.focus();
                    event.preventDefault();
                });
            })
            .catch(error => {
                console.error(error.message);
            });
        // Initialize the query string for the hub connection with the user's name
        //$.connection.hub.qs = "userId=" + username;

        // Start the connection.
        //_subscribeToConnectionStart(function () {

            $(sendButton).click(function () {

                // Call the Send method on the hub.
                _hub.invoke('send', username, $(messageTextbox).val());

                // Clear text box and reset focus for next comment.
                $(messageTextbox).val('').focus();
            });

        //});
    };

    var _initChat = function() {

        // Set up the chat login button click handlers
        $('div[data-role="chat-login"] button[data-role="chat-login-button"]').on('click',
            function() {
                // TODO: this function should authenticate with the server.
                // For now, we simply accept the username
                var input = $(this).closest('form').find('input[data-role="chat-username-input"]');
                var username = $(input).val();
                if (username.length === 0) {
                    alert('Please enter a username');
                    $(input).focus();
                    return;
                }

                // Set the username
                var container = $(this).closest('div.chat-container');
                $(container).attr('data-username', username);

                // Hide the login form
                $(this).closest('div[data-role="chat-login"]').hide();

                // Change the title
                var h4 = $(container).find('h4');
                $(h4).text(username);

                // Display the message container
                var messageContainer = $(container).find('div.chat-msg-container');
                $(messageContainer).removeClass('hidden').show();

                _initChatConnection(container);
            });
    };

    return {
        initChat: _initChat,
        initChatConnection: _initChatConnection
    };

})();

$(function() {
    //hud.plugins.chat.initChat();
})