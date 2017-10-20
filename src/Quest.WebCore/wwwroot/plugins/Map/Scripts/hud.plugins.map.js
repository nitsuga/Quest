var hud = hud || {};

hud.plugins = hud.plugins || {};

hud.plugins.map = (function() {

    var _maps = [];

    // TODO: Get this key from a config file
    var _apiKey = 'AIzaSyD20hy8wOit2U3ES37heiH-D8yAPXVawjU';

    //var _mapId;
    var _centreLat;
    var _centreLng;
    var _zoomLevel;

    var _markers = [];

    var _initMap = function (mapId) {

        console.log("init map " + mapId);
        _maps.push(new google.maps.Map(document.getElementById(mapId),
                {
                    center: { lat: _centreLat, lng: _centreLng },
                    zoom: _zoomLevel
                })
        );

    };


    //var _initializeMaps = function (mapId, centreLat, centreLng, zoomLevel) {
    //    _mapId = mapId;
    //    _centreLat = centreLat;
    //    _centreLng = centreLng;
    //    _zoomLevel = zoomLevel;

    //    var script = document.createElement('script');
    //    script.type = 'text/javascript';
    //    script.src = 'https://maps.googleapis.com/maps/api/js?key=' + _apiKey + '&callback=hud.samplemap.initMap';
    //    document.body.appendChild(script);
    //};

    var _initializeMaps = function (centreLat, centreLng, zoomLevel) {
        _centreLat = centreLat;
        _centreLng = centreLng;
        _zoomLevel = zoomLevel;

        // Find all the map canvases on the page
        $('div[data-role="map-canvas"]').each(function (index, item) {
            var mapId = $(item).attr('id');
            _initMap(mapId);
        });

        //if (_googleApiScriptExists === false) {
        //    var script = document.createElement('script');
        //    script.type = 'text/javascript';
        //    script.src = 'https://maps.googleapis.com/maps/api/js?key=' + _apiKey + '&callback=hud.samplemap.initMap';
        //    document.body.appendChild(script);

        //    _googleApiScriptExists = true;
        //} else {
        //    hud.samplemap.initMap();
        //}
    };

    /// <summary>
    /// Re-centre the maps to new co-ordinates
    /// </summary
    var _panTo = function (lat, lng) {
        var latlng = new google.maps.LatLng(lat, lng);

        for (var i = 0; i < _maps.length; i++) {
            _maps[i].panTo(latlng);
        }

        return latlng;
    };

    var _panAndMarkLocation = function (locationName, lat, lng) {
        var latlng = _panTo(lat, lng);

        if (_.where(_markers, { title: locationName }).length === 0) {

            var marker;
            for (var i = 0; i < _maps.length; i++) {
                marker = new google.maps.Marker({
                    position: latlng,
                    map: _maps[i],
                    title: locationName
                });
            }
            if (marker)
                _markers.push(marker);
        }
    }

    var _setZoomLevel = function(z) {
        for (var i = 0; i < _maps.length; i++) {
            _maps[i].setZoom(z);
        }
    };

    var _redrawMaps = function () {
        for (var i = 0; i < _maps.length; i++) {
            google.maps.event.trigger(_maps[i], "resize");
        }
    }

    return {
        initializeMaps: _initializeMaps,
        initMap: _initMap,
        redrawMaps: _redrawMaps,

        setZoom: _setZoomLevel,
        panTo: _panTo,
        panAndMarkLocation: _panAndMarkLocation
}
})();