var hud = hud || {};

hud.plugins = hud.plugins || {};

hud.plugins.map = (function() {


    var _initMap = function (role) {
        L_PREFER_CANVAS = true;

        var osmUrl = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
        var osmAttrib = "Map data © OpenStreetMap contributors";
        osm = new L.TileLayer(osmUrl, { attribution: osmAttrib });
        var mbAttr = 'Map data &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors, ' +
            '<a href="http://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, ' +
            'Imagery © <a href="http://mapbox.com">Mapbox</a>',
            mbUrl = "https://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoibWFwYm94IiwiYSI6ImNpandmbXliNDBjZWd2M2x6bDk3c2ZtOTkifQ._QA7i5Mpkd_m30IGElHziw";

        var grayscale = L.tileLayer(mbUrl, { id: "mapbox.light", attribution: mbAttr }),
            streets = L.tileLayer(mbUrl, { id: "mapbox.streets", attribution: mbAttr });

        var googleLayer1 = new L.Google('ROADMAP');
        var googleLayer2 = new L.Google('SATELLITE');
        var googleLayer3 = new L.Google('HYBRID');
        var googleLayer4 = new L.Google('TERRAIN');

        barts = L.tileLayer.wms("http://86.29.75.151:8090/cgi-bin/mapserv?MAP=/maps/extent.map&crs=EPSG:27700", { layers: "Barts", format: "image/png", maxZoom: 22, minZoom: 0, continuousWorld: true, noWrap: true });
        stations = L.tileLayer.wms("http://86.29.75.151:8090/cgi-bin/mapserv?MAP=/maps/extent.map", { layers: "Stations", format: "image/png", transparent: true, maxZoom: 22, minZoom: 0, continuousWorld: true, noWrap: true });

        baseLayers = {
            "OSM": osm,
            "Grayscale": grayscale,
            "Mapbox Streets": streets,
            "Barts": barts ,
            "Google Road": googleLayer1,
            "Google Satellite": googleLayer2,
            "Google Hybrid": googleLayer3,
            "Google Terrain": googleLayer4
        };
        baseLayer = osm;

        var overlayLayers = {
            "Stations": stations
        };

        lat = 51.5;
        lng = -0.2;
        zoom = 12;

        var mapdiv = 'map' + role;

        map = new L.Map(mapdiv, {
            center: new L.LatLng(lat, lng),
            zoom: zoom,
            layers: baseLayer,
            zoomControl: false,
            continuousWorld: true,
            worldCopyJump: false,
            inertiaDeceleration: 10000
        });

    };


    /// <summary>
    /// Re-centre the maps to new co-ordinates
    /// </summary
    var _panTo = function (lat, lng) {
    };

    var _panAndMarkLocation = function (locationName, lat, lng) {
    }

    var _setZoomLevel = function(z) {
    };

    var _redrawMaps = function () {
    }

    return {
        initMap: _initMap,
        redrawMaps: _redrawMaps,

        setZoom: _setZoomLevel,
        panTo: _panTo,
        panAndMarkLocation: _panAndMarkLocation
}
})();