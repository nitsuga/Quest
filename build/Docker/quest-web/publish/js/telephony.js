var MobileLyr;          // for EISEC  

function initTelephony(map) {
    // ReSharper disable once InconsistentNaming
    MobileLyr = new L.featureGroup();
    MobileLyr.addTo(map);

    // copy down address result to gaz search
    $("#cla").click(function () {
        OnLocationButtonClick();
    });
}

function OnLocationButtonClick() {
    var mode = $("#cla").data("mode");
    var location = $("#cla").data("location");
    switch (mode) {
        case 0:
            break;
        case 1:
            $("#search_input_text").val(location);
            break;
        case 2: // grid location
            map.fitBounds(location, { maxZoom: 18 });
            var centre = location.getCenter();
            var txt = Math.round(centre.lat * 100000) / 100000 + "," + Math.round(centre.lng * 100000) / 100000;
            $("#search-coord").data("coord",txt);
            break;
    }
}

function processTelephonyMessage(msg) {
    switch (msg.$type) {
    case "Quest.Lib.ServiceBus.Messages.CallerDetails, Quest.Lib":
        processCallerDetails(msg);
        return true;

    case "Quest.Lib.ServiceBus.Messages.CallDetails, Quest.Lib":
        NewCall(msg.CLI);
        return true;
    }
    return false;
}

function processCallDetails(msg) {
    beep();

    NewCall(msg.CLI);
}

function processCallerDetails(msg) {
    beep();

    if (msg.IsMobile) {
        NewCallerMobile(msg);
    } else {
        NewCallerAddress(msg);
    }
}

// new mobile details.. may be more that one per call as the location gets refined
function NewCallerMobile(msg) {

    $("#cli").html(msg.TelephoneNumber);
    $("#cla").html(msg.Status);

    if (msg.StatusCode === 0) {
        setButtonState("#cla", "btn-default");
        // clr location in button 
        $("#cla").data("mode", 0);
        $("#cla").data("location", null);
        return;
    }

    if (msg.Shape === undefined)
        return;

    var coords = GetCoordinatesFromPoly(msg.Shape);

    DisplayBoundary(MobileLyr, coords, true, "blue");
    var bounds = L.latLngBounds(coords);

    // if not already locked to area of map then lock according to mobile geometry
    if (!LockSearchToMapArea()) {
        map.fitBounds(bounds, { maxZoom: 18 }); //works!    
        LockSearchToMapArea(true);
    }

    // add flashing icon if <20meters across
    var size = bounds.getSouthEast().distanceTo(bounds.getNorthWest());
    if (size < 50) {
        $("#cla").html("Accurate Location");
        var pulsingIcon = L.icon.pulse({ iconSize: [10, 10], color: "#3f007d" });
        var pulsingMarker = L.marker(bounds.getCenter(), { icon: pulsingIcon });
        MobileLyr.addLayer(pulsingMarker);
        setButtonState("#cla", "btn-success");

        // put coords in text box
        // centre on click..
        //TODO: 

    } else {
        $("#cla").html("Approximate Location");
        setButtonState("#cla", "btn-info");
    }

    // store location in button so we can fitBounds if they click on it
    $("#cla").data("mode", 2);  // 0=invalid 1=address 2= grid location
    $("#cla").data("location", bounds);

}

function NewCallerAddress(msg) {
    var address = msg.Address.join(" ");

    LockSearchToMapArea(false);

    $("#cla").html(address);

    setButtonState("#cla", "btn-success");

    var curText = $("#search_input_text").val();

    // user hasn't ytped anything yet so do search for him.
    if (curText === "") {
        $("#search_input_text").val(msg.SearchableAddress);
        LocationSearch();
        $("#search_input_text").focus();
    }

    // store searchable addres in button so it can be clicked
    $("#cla").data("mode", 1);  // 0=invalid 1=address 2= grid location
    $("#cla").data("location", msg.SearchableAddress);

}

// new call arriving
function ClrCall() {
    NewCall();
}

function NewCall(cli) {

    // clear the layers
    BoundaryLyr.clearLayers();
    MobileLyr.clearLayers();

    setButtonState("#cla", "btn-default");
    setButtonState("#cli", "btn-info");

    if (cli === undefined) {
        cli = "No CLI";
        setButtonState("#cli", "btn-default");
        $("#cli").html(cli);
    } else {
        setButtonState("#cli", "btn-info");
        $("#cli").html(cli);
    }

    $("#cla").html("no caller details");

    clrSearchItems();
    $("#search_input_text").val("");
    $("#search_input_text").focus();
    LockSearchToMapArea(false);
}


function beep() {
    var snd = new Audio("data:audio/wav;base64,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qJrx4KOQjahZxWbcZgztj2c49nKmkId44S71j0c8eV9yDK6uPRzx5X18eDvjvQ6yKo9ZSS6l//8elePK/Lf//IInrOF/FvDoADYAGBMGb7FtErm5MXMlmPAJQVgWta7Zx2go+8xJ0UiCb8LHHdftWyLJE0QIAIsI+UbXu67dZMjmgDGCGl1H+vpF4NSDckSIkk7Vd+sxEhBQMRU8j/12UIRhzSaUdQ+rQU5kGeFxm+hb1oh6pWWmv3uvmReDl0UnvtapVaIzo1jZbf/pD6ElLqSX+rUmOQNpJFa/r+sa4e/pBlAABoAAAAA3CUgShLdGIxsY7AUABPRrgCABdDuQ5GC7DqPQCgbbJUAoRSUj+NIEig0YfyWUho1VBBBA//uQZB4ABZx5zfMakeAAAAmwAAAAF5F3P0w9GtAAACfAAAAAwLhMDmAYWMgVEG1U0FIGCBgXBXAtfMH10000EEEEEECUBYln03TTTdNBDZopopYvrTTdNa325mImNg3TTPV9q3pmY0xoO6bv3r00y+IDGid/9aaaZTGMuj9mpu9Mpio1dXrr5HERTZSmqU36A3CumzN/9Robv/Xx4v9ijkSRSNLQhAWumap82WRSBUqXStV/YcS+XVLnSS+WLDroqArFkMEsAS+eWmrUzrO0oEmE40RlMZ5+ODIkAyKAGUwZ3mVKmcamcJnMW26MRPgUw6j+LkhyHGVGYjSUUKNpuJUQoOIAyDvEyG8S5yfK6dhZc0Tx1KI/gviKL6qvvFs1+bWtaz58uUNnryq6kt5RzOCkPWlVqVX2a/EEBUdU1KrXLf40GoiiFXK///qpoiDXrOgqDR38JB0bw7SoL+ZB9o1RCkQjQ2CBYZKd/+VJxZRRZlqSkKiws0WFxUyCwsKiMy7hUVFhIaCrNQsKkTIsLivwKKigsj8XYlwt/WKi2N4d//uQRCSAAjURNIHpMZBGYiaQPSYyAAABLAAAAAAAACWAAAAApUF/Mg+0aohSIRobBAsMlO//Kk4soosy1JSFRYWaLC4qZBYWFRGZdwqKiwkNBVmoWFSJkWFxX4FFRQWR+LsS4W/rFRb/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////VEFHAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAU291bmRib3kuZGUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMjAwNGh0dHA6Ly93d3cuc291bmRib3kuZGUAAAAAAAAAACU=");
    snd.play();
}
