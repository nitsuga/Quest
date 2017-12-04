var hud = hud || {};

hud.plugins = hud.plugins || {}

hud.plugins.gaz = (function() {

    var _apiKey = 'AIMjsK0VCky8jPANSUVwRQ10505';

    // current bounding box from the map
    var _boundingbox;
    var _timeout;

    var _init = function (panelId, pluginId) {

        $(window).keypress(function (e) {
            var code = e.which || e.keyCode;
            switch (code) {
                case 172:           // shift `
                    //do stuff
                    $("#search_input_text").focus();
                    $("#search_input_text").val("");
                    return false;
                default:
                    break;
            }
            return true;
        });


        // listen for local hub messages 
        $("#sys_hub").on("MapBounds", function (evt, data) {
            _boundingbox = data;
        });

        // listen for local hub messages 
        $("#sys_hub").on("SearchResults", function (evt, data) {
            _showSearchResults(pluginId, data);
        });

        // has panel been swapped with another, update your panel ID
        $("#sys_hub").on("Swapped", function (evt, data) {
        });

        // bind plugin dependent events
        _bindPanel(pluginId);

        // select the primary menu
        hud.selectMenu(pluginId, 0);

    };

    // bind any plugin-related events.
    // called on init and when the panel content is moved into another panel
    // we 
    var _bindPanel = function (pluginId) {

        var selector = hud.pluginSelector(pluginId);

        $(selector).on("input", function () {
            clearTimeout(_timeout);
            _timeout = setTimeout(function () {
                var searchText = $(selector + " #search_input_text").val();
                var boundsfilter = $(selector + " #boundsfilter").hasClass("fa-lock");
                _doSearch(pluginId, searchText, 0, 100, boundsfilter);
            }, 500);
        });
    }

    var _doSearch = function (pluginId, searchText, mode, take, boundsfilter) {

        var selector = hud.pluginSelector(pluginId);

        if (searchText.length === 0)
            return;

        var url = hud.getURL("Gazetteer/SemanticSearch");
        
        var parms = {
            SearchText: searchText,
            SearchMode: mode,
            IncludeAggregates: false,
            Skip: 0,
            Take: take,
            BoundsFilter: boundsfilter,
            W: _boundingbox !== undefined ? _boundingbox.getWest() : 0,
            S: _boundingbox !== undefined ? _boundingbox.getSouth() : 0,
            E: _boundingbox !== undefined ? _boundingbox.getEast() : 0,
            N: _boundingbox !== undefined ? _boundingbox.getNorth() : 0,
            FilterTerms: "",
            DisplayGroup: "",
            IndexGroup: ""
        };

        $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            data: parms,
            contentType: "application/json; charset=utf-8",
            success: function (json) {

                $(selector + " #message-wait").hide();

                if (json.error !== undefined) {
                    // show an error
                }
                else {

                    // simply emit the results onto the local service bus so that the map and the results list can update
                    hud.sendLocal("SearchResults", json);
                }

                $(selector + " #search_input_text").focus();

            },
            error: function (result) {
                alert('error from hud.plugins.pluginSelector._initialize \r\n' + result.responseText);
            }
        });
    };

    // show search results in the results list
    var _showSearchResults = function (pluginId, json) {
        _clrSearchItems(pluginId);
        _showGroupedSearchItems(pluginId, json);
    }
    var _setFinalAddress = function (doc, latlng)
    {
    }

    var _showGroupedSearchItems = function (pluginId, items) {

        var selector = hud.pluginSelector(pluginId);

        _clrSearchItems(pluginId);

        var itext = items.Count.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");

        if (items.Removed > 0)
            $(selector + " #message").html("  found: <B>" + itext + "</B> items (" + items.Removed + " dups) in " + items.ms + "ms");
        else
            $(selector + " #message").html("  found: " + itext + " items in " + items.ms + "ms");
        var docindex;
        var doc;
        var score;
        var latlng;
        var feature;

        if (items.Grouping === null || items.Grouping.length === 1) {

            for (docindex = 0; docindex < items.Documents.length; docindex++) {
                doc = items.Documents[docindex];
                score = doc.s;
                if (doc.l !== undefined && doc.l !== null) {
                    latlng = new L.LatLng(doc.l.lat, doc.l.lon);
                    feature = getFeature(doc, latlng);
                    _addSingleFeatureToResultsList(pluginId, selector + " #grouped-results-list", doc, feature, score, latlng);

                    if (docindex === 0)
                        _setFinalAddress(doc.d, latlng);
                }
            }
        }
        else {

            for (var i = 0; i < items.Grouping.length; i++) {
                var grp = items.Grouping[i];

                // get first document
                docindex = grp[0];
                doc = items.Documents[docindex];
                if (grp.length > 1) //these are grouped
                {
                    $(selector + " #grouped-results-list").append($("<div>").attr("id", "group-results-" + i));

                    if (items.Documents.length <= 20) {
                        $(selector + " #group-results-" + i).append("<a href='#' class='list-group-item' data-toggle='collapse' data-target='" + "#group-results-sm-" + i + "' data-parent='#menu'>" + "<span class='text-primary'>" + doc.grp + "</span>" + "<span class='glyphicon glyphicon-minus pull-right'></span></a>");
                        $(selector + " #group-results-" + i).append($("<div>").attr("id", "group-results-sm-" + i).attr("class", "sublinks"));
                    }
                    else {
                        $(selector + " #group-results-" + i).append("<a href='#' class='list-group-item' data-toggle='collapse' data-target='" + "#group-results-sm-" + i + "' data-parent='#menu'>" + "<span class='text-primary'>" + doc.grp + "</span>" + "<span class='glyphicon glyphicon-plus pull-right'></span></a>");
                        $(selector + " #group-results-" + i).append($("<div>").attr("id", "group-results-sm-" + i).attr("class", "sublinks collapse"));

                    }

                    for (var j = 0; j < grp.length; j++) {
                        docindex = grp[j];
                        doc = items.Documents[docindex];
                        score = doc.s;
                        latlng = new L.LatLng(doc.l.lat, doc.l.lon);
                        _addSingleFeatureToResultsList(pluginId, selector + " #group-results-sm-" + i, doc, score, latlng);
                    }

                }
                else //these  are not grouped
                {
                    score = doc.s;
                    latlng = new L.LatLng(doc.l.lat, doc.l.lon);
                    _addSingleFeatureToResultsList(pluginId, selector + " #grouped-results-list", doc, score, latlng);
                }
            }
        }

    }

    var _addSingleFeatureToResultsList = function (pluginId, containerId, address, score, latlng) {

        var selector = hud.pluginSelector(pluginId);

        $(containerId).append($("<a>").attr("class", "list-group-item").attr("href", "#")
            .on("click", function () {
                _setFinalAddress(address.d, latlng);
                hud.sendLocal("PanZoom", { 'lat': address.l.lat, 'lon': address.l.lon, 'zoom': 20 });
                hud.sendLocal("ObjectSelected", { "Type": "SearchItem", "Value": address });
                hud.sendLocal("SelectSearchResult", { "address": address });
            })
            .append($("<span>").addClass("text-muted c-info c-info-icon")
                .attr("title", "go to ")
                .attr("data-toggle", "tooltip")
                .on("click", function () {
                    $("#search_input_text").val(address.d);
                })
            )
            .append($("<span>")
                .text(address.d)
                .addClass("name"))
            .attr("title", address.t + " from " + address.src + " Score= " + score + " " + address.st)
            .attr("data-toggle", "tooltip"));
    }

    var _clrSearchItems = function (pluginId) {

        var selector = hud.pluginSelector(pluginId);

        $(selector + " #result-list").find("li").remove();
        $(selector + " #grouped-results-list").empty();
        $(selector + " #grouped-results-list").html("");
    }

    return {
        init: _init
    }
})();