var hud = hud || {};

hud.plugins = hud.plugins || {}

hud.plugins.gaz = (function() {

    var _apiKey = 'AIMjsK0VCky8jPANSUVwRQ10505';

    var _initialize = function() {

        $('div[data-role="gaz-search-container"] button[data-role="gaz-search"]').on('click',
            function() {
                var gazContainer = $(this).closest('div[data-role="gaz-container"]');
                var searchContainer = $(this).closest('div[data-role="gaz-search-container"]');
                var searchInput = $(searchContainer).find('input[data-role="gaz-search-input"]');
                var searchTerm = $(searchInput).val();
                var searchResultsList = $(gazContainer).find('ul[data-role="gaz-search-results"]');

                if (searchTerm.length === 0) 
                    return;

                var url = 'https://api.getAddress.io/find/' + searchTerm + '?api-key=' + _apiKey;

                $.ajax({
                    url: url,
                    type: 'GET',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    success: function (json) {
                        var lat = json.latitude;
                        var lng = json.longitude;
                        $(searchResultsList).html('');
                        for (var i = 0; i < json.addresses.length; i++) {
                            $(searchResultsList).append($('<li>').text(json.addresses[i]));
                        }

                        // TODO: broadcast co-ordinates to any Map plugin that is listening
                        // This is not strictly correct because the map might not be loaded
                        hud.plugins.map.panAndMarkLocation('', lat, lng);
                        hud.plugins.map.setZoom(14);
                    },
                    error: function (result) {
                        alert('error from hud.plugins.pluginSelector._initialize \r\n' + result.responseText);
                    }
                });

            });
    };

    return {
        initialize: _initialize
    }
})();