/**
 * Leaflet.UserMarker v1.0
 * 
 * Author: Jonatan Heyman <http://heyman.info>
 */

(function(window) {
    var icon;

    var circleStyle = {
        stroke: true,
        color: "red",
        weight: 3,
        opacity: 0.5,
        fillOpacity: 0.1,
        clickable: false
    };

    L.UserMarker = L.Marker.extend({
        options: {
            pulsing: false,
            accuracy: 0,
            m: '',                  // marker type
            s: '',                  // status
            t: 'mapmarker-norm'     // theme
        },

        initialize: function(latlng, options) {
            options = L.Util.setOptions(this, options);            
            this.setMarker(this.options.m, this.options.s, this.options.pulsing, this.options.t);

            if (this.options.accuracy>0)
                this._accMarker = L.circle(latlng, this.options.accuracy, circleStyle);
        
            // call super
            L.Marker.prototype.initialize.call(this, latlng, this.options);
        
            this.on("move", function () {
                if (this._accMarker != null)
                    this._accMarker.setLatLng(this.getLatLng());

            }).on("remove", function () {
                if (this._accMarker != null)
                    this._map.removeLayer(this._accMarker);
            });
        },

        setMarker: function(m,s, pulsing, theme)
        {            
            this._m = m;
            icon = L.divIcon({
                className: theme + "-" + this.options.m + " " + this.options.m + "-" + s + " " + (pulsing?"pulsate":""),
                iconSize: [24, 24],
                iconAnchor: [12, 12],
                popupAnchor: [0, -10],
                labelAnchor: [3, -4],
                html: ''
            });
            this.setIcon(icon);
        },
    
        setAccuracy: function(accuracy)	{
            this._accuracy = accuracy;
            if (accuracy === 0)
                return;
            if (!this._accMarker) {
                this._accMarker = L.circle(this._latlng, accuracy, circleStyle).addTo(this._map);
            } else {
                this._accMarker.setRadius(accuracy);
            }
        },
    
        onAdd: function(map) {
            // super
            L.Marker.prototype.onAdd.call(this, map);
            if (this._accMarker != null)
                this._accMarker.addTo(map);
        }
    });

    L.userMarker = function (latlng, options) {
        return new L.UserMarker(latlng, options);
    };

})(window);
