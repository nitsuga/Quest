

function HandleCancel() {
    $('#modalCallsign').modal('hide');
}

function IncidentsTip() {

    var message = '<h4>Alerts for Nearby Incidents</h4>';
    message += '<p> Enable this option to get alerts for nearby incidents which are within 100 metres of your current location. </p>';
    bootbox.alert(message);

}


function CallsignTip() {

    var message = '<h4>Alerts for Callsign</h4>';
    message += '<p> Enable this option to get alerts for your specific Callsign. </p>';
    message += '<p> This will require you to enter your desired callsign. Only one callsign is allowed. </p>';
    bootbox.alert(message);

}

function AuthorizeTip() {

    var message = '<h4>Authorize Users</h4>';
    message += '<p> This allows an Admin user to authorize a new user and allocate that invidual to a specicif role within the site. </p>';
    bootbox.alert(message);

}

$(function () {

    $('[data-toggle="toggle"]').each(function (index, element) {
        setSliderFromStore("#" + element.id);
    });

    // save any settings from a toogle
    $('[data-toggle="toggle"]').change(function () {
        var target = $(this)[0];
        setStoreFromSlider("#" + target.id);
    });

});

