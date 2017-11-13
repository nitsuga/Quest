var hud = hud || {};

hud.plugins = hud.plugins || {}

hud.plugins.gaz = (function() {

    var _apiKey = 'AIMjsK0VCky8jPANSUVwRQ10505';

    // current bounding box from the map
    var _boundingbox;
    var _timeout;

    var _initialize = function (panel) {

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

        $("[data-panel-role='" + panel + "'] #search_input_text").on("input", function () {
            clearTimeout(_timeout);
            _timeout = setTimeout(function () {
                var searchText = $("[data-panel-role='" + panel + "'] #search_input_text").val();
                var boundsfilter = $("#boundsfilter").hasClass("fa-lock");
                _doSearch(panel, searchText, 0, 100, boundsfilter);
            }, 500);
        });


        // listen for local hub messages 
        $("#sys_hub").on("MapBounds", function (evt, data) {
            _boundingbox = data;
        });

        // listen for local hub messages 
        $("#sys_hub").on("SearchResults", function (evt, data) {
            _showSearchResults( panel, data );
        });

    };

    var _doSearch = function (panel, searchText, mode, take, boundsfilter) {
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

                $("[data-panel-role='" + panel + "'] #message-wait").hide();

                if (json.error !== undefined) {
                    // show an error
                }
                else {

                    // simply emit the results onto the local service bus so that the map and the results list can update
                    hud.sendLocal("SearchResults", json);
                }

                $("[data-panel-role='" + panel + "'] #search_input_text").focus();

            },
            error: function (result) {
                alert('error from hud.plugins.pluginSelector._initialize \r\n' + result.responseText);
            }
        });
    };

    // show search results in the results list
    var _showSearchResults = function (panel, json) {
        _clrSearchItems(panel);
        _showGroupedSearchItems(panel, json);
    }

    var _showGroupedSearchItems = function (panel, items) {
        _clrSearchItems(panel);

        var itext = items.Count.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");

        if (items.Removed > 0)
            $("[data-panel-role='" + panel + "'] #message").html("  found: <B>" + itext + "</B> items (" + items.Removed + " dups) in " + items.ms + "ms");
        else
            $("[data-panel-role='" + panel + "'] #message").html("  found: " + itext + " items in " + items.ms + "ms");
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
                    _addSingleFeatureToResultsList("[data-panel-role='" + panel + "'] #grouped-results-list", doc, feature, score, latlng);

                    if (docindex === 0)
                        SetFinalAddress(doc.d, latlng, 0);
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
                    $("[data-panel-role='" + panel + "'] #grouped-results-list").append($("<div>").attr("id", "group-results-" + i));

                    if (items.Documents.length <= 20) {
                        $("[data-panel-role='" + panel + "'] #group-results-" + i).append("<a href='#' class='list-group-item' data-toggle='collapse' data-target='" + "#group-results-sm-" + i + "' data-parent='#menu'>" + "<span class='text-primary'>" + doc.grp + "</span>" + "<span class='glyphicon glyphicon-minus pull-right'></span></a>");
                        $("[data-panel-role='" + panel + "'] #group-results-" + i).append($("<div>").attr("id", "group-results-sm-" + i).attr("class", "sublinks"));
                    }
                    else {
                        $("[data-panel-role='" + panel + "'] #group-results-" + i).append("<a href='#' class='list-group-item' data-toggle='collapse' data-target='" + "#group-results-sm-" + i + "' data-parent='#menu'>" + "<span class='text-primary'>" + doc.grp + "</span>" + "<span class='glyphicon glyphicon-plus pull-right'></span></a>");
                        $("[data-panel-role='" + panel + "'] #group-results-" + i).append($("<div>").attr("id", "group-results-sm-" + i).attr("class", "sublinks collapse"));

                    }

                    for (var j = 0; j < grp.length; j++) {
                        docindex = grp[j];
                        doc = items.Documents[docindex];
                        score = doc.s;
                        latlng = new L.LatLng(doc.l.lat, doc.l.lon);
                        _addSingleFeatureToResultsList("[data-panel-role='" + panel + "'] #group-results-sm-" + i, doc, score, latlng);
                    }

                }
                else //these  are not grouped
                {
                    score = doc.s;
                    latlng = new L.LatLng(doc.l.lat, doc.l.lon);
                    _addSingleFeatureToResultsList("[data-panel-role='" + panel + "'] #grouped-results-list", doc, score, latlng);
                }
            }
        }

    }

    var _addSingleFeatureToResultsList = function (containerId, address, score, latlng) {
        $(containerId).append($("<a>").attr("class", "list-group-item").attr("href", "#")
            .on("click", function () {
                //SetFinalAddress(address.d, latlng, 0);
            })
            .append($("<span>").addClass("glyphicon glyphicon-map-marker text-muted c-info c-info-icon")
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

    var _clrSearchItems = function (panel) {
        $("[data-panel-role='" + panel + "'] #result-list").find("li").remove();
        $("[data-panel-role='" + panel + "'] #grouped-results-list").empty();
        $("[data-panel-role='" + panel + "'] #grouped-results-list").html("");
    }

    return {
        initialize: _initialize
    }
})();