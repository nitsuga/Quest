﻿function ShowTimeline(tracks) {

    // Get start/end times
    var startTime = new Date(tracks[0].properties.time[0]);
    var endTime = new Date(tracks[0].properties.time[tracks[0].properties.time.length - 1]);

    // Create a DataSet with data
    var timelineData = new vis.DataSet([{ start: startTime, end: endTime, content: 'Demo GPS Tracks' }]);

    // Set timeline options
    var timelineOptions = {
        "width": "100%",
        "height": "120px",
        "style": "box",
        "axisOnTop": true,
        "showCustomTime": true
    };

    // Setup timeline
    var timeline = new vis.Timeline(document.getElementById('timeline'), timelineData, timelineOptions);

    // Set custom time marker (blue)
    timeline.setCustomTime(startTime);

    // =====================================================
    // =============== Playback ============================
    // =====================================================

    // Playback options
    var playbackOptions = {

        playControl: true,
        dateControl: true,

        // layer and marker options
        layer: {
            pointToLayer: function (featureData, latlng) {
                var result = {};

                if (featureData && featureData.properties && featureData.properties.path_options) {
                    result = featureData.properties.path_options;
                }

                if (!result.radius) {
                    result.radius = 5;
                }

                return new L.CircleMarker(latlng, result);
            }
        },

        marker: {
            getPopup: function (featureData) {
                var result = '';

                if (featureData && featureData.properties && featureData.properties.title) {
                    result = featureData.properties.title;
                }

                return result;
            }
        }

    };

    // Initialize playback
    var playback = new L.Playback(map, null, onPlaybackTimeChange, playbackOptions);

    playback.setData(demoTracks);

    // Set timeline time change event, so cursor is set after moving custom time (blue)
    timeline.on('timechange', onCustomTimeChange);

    // A callback so timeline is set after changing playback time
    function onPlaybackTimeChange(ms) {
        timeline.setCustomTime(new Date(ms));
    };

    // 
    function onCustomTimeChange(properties) {
        if (!playback.isPlaying()) {
            playback.setCursor(properties.time.getTime());
        }
    }
}