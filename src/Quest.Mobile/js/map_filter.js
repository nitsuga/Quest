var filterlength = 0;
var filters = new Array();

$(function () {
    $("#filter-btn").click(function () {
        $("#categoriesmodal").modal("show");
        return false;
    });

    $('#filter-div').hide();
    $('#filter-btn').hide();
    $('#categoriesmodal').on('hidden.bs.modal', function () {
        LocationSearch();
    });

    $('#categoriesmodal').on('show.bs.modal', function () {
        $('#categoriesmodal .modal-body').css('height', $(window).height() * 0.6);
        $('#categoriesmodal .modal-body').css('overflow-y', 'auto');
    });
});

function loadAggregates(items) {

    $('#aggregatesGrp').find(".panel.panel-default").remove();

    if (items.Aggregates == undefined)
        return;

    items.Aggregates.forEach(function (aggregate) {

        if (aggregate.Items.length > 0) {
            if (aggregate.Name.toLowerCase() != 'thoroughfare') {
                var panelname = aggregate.Name.capitalizeFirstLetter() + "panel";
                $('#aggregatesGrp').append($('<div>').addClass("panel panel-default")
                                    .attr("id", panelname)
                                    .append($('<div>').addClass("panel-heading")
                                    .append($('<h6>').addClass("panel-title").text(aggregate.Name.capitalizeFirstLetter())
                                     )
                                    )
                                    .append($('<div>')

                                    .attr("id", aggregate.Name.capitalizeFirstLetter() + "Classes")
                                    .append($('<div>').addClass("aggregate-list-group").attr("id", aggregate.Name.capitalizeFirstLetter() + "-list"))
                                    )
                                    );

                aggregate.Items.forEach(function (item) {
                    var item_count = Number(item.Value);
                    if (item_count && item_count > 0) {
                        var ulname = aggregate.Name.capitalizeFirstLetter() + "-list";
                        var ulselector = "#" + ulname;
                        var percent = Math.floor(100 * Number(item.Value) / items.Count).toString();
                        var percentStr = percent + "%";
                        $(ulselector).append($('<div>').addClass("aggregate-list-item")
                                     .append($('<span>').append($('<span>').text(item.Name.capitalizeFirstLetter() + " (" + item_count.toString() + ")"))
                                     .append($('<span>')
                                     .css("float", "right")

                                     .append($('<a>').attr("href", "#")
                                     .append($('<span>').addClass("glyphicon glyphicon-plus-sign").css("color", "grey")
                                     .on("click", function (e) {
                                         if (FilterExists(aggregate.Name, item.Name) == false) {
                                             addFilter(aggregate.Name, item.Name, true);
                                             filterlength += 1;
                                         }
                                         else {
                                             updateFilterUI(aggregate.Name, item.Name, true);
                                         }

                                         changeFilters(aggregate.Name, item.Name, true);

                                     })

                                     ))

                                     .append($('<a>').attr("href", "#").css("margin-left", "5px")
                                     .append($('<span>').addClass("glyphicon glyphicon-minus-sign").css("color", "grey")
                                        .css("margin-right", "2px")
                                        .on("click", function (e) {
                                            if (FilterExists(aggregate.Name, item.Name) == false) {
                                                addFilter(aggregate.Name, item.Name, false);
                                                filterlength += 1;
                                            }
                                            else {
                                                updateFilterUI(aggregate.Name, item.Name, false)
                                            }
                                            changeFilters(aggregate.Name, item.Name, false);

                                        })

                                     )

                                     )

                                     )

                                     )

                                     .append($('<div>').addClass("progress")
                                     .append($('<div>').text(percentStr).addClass("progress-bar-warning")
                                     .attr("data-toggle", "tooltip")
                                     .attr("title", item_count)
                                     .attr("role", "progressbar")
                                     .attr("aria-valuenow", percent)
                                     .attr("aria-valuemin", "0")
                                     .attr("aria-valuemax", "100")
                                     .css("width", percentStr)
                                     )

                                     )

                                     );
                    }


                });
            }

        }

    });

}

function showFilterItems(items) {

    searchlayer = new L.featureGroup();

    var itext = items.Count.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")

    if (items.Removed > 0)
        $('#message').html('  found: <B>' + itext + '</B> items (' + items.Removed + " dups) in " + items.millisecs + 'ms');
    else
        $('#message').html('  found: ' + itext + ' items in ' + items.millisecs + 'ms');


    items.Documents.forEach(
        function (address) {
            // add map marker
            var polylineCoordinates = [];

            var latlng = new L.LatLng(address.Location.coordinates[1], address.Location.coordinates[0]);

            var latLongParms = {
                lng: address.Location.coordinates[0],
                lat: address.Location.coordinates[1],
            };

            var latLongString = JSON.stringify(latLongParms);

            var content = "";
            content += "<p>" + address.Description + "</p>";
            content += "<input type='hidden' id='latLong' value='" + latLongString + "'>";

            if (address.Type.toUpperCase() == 'ROADLINK') {
                address.LineGeometry.coordinates.forEach(function (coords) {
                    coords.forEach(function (pnt) {
                        var latlng = new L.LatLng(pnt[1], pnt[0]);
                        polylineCoordinates.push(latlng);
                    });

                });
                var polyline = L.polyline(polylineCoordinates, { color: 'blue' });
                //polyline.bindPopup(address.Description)
                polyline.bindPopup(content);
                searchlayer.addLayer(polyline);
            }
            else {
                var marker = L.userMarker(latlng, { pulsing: false, accuracy: 0, m: 'loc', s: address.Type });

                var l = latlng.toString().replace('LatLng(', '');
                l = l.replace(')', '');

                $('#result-list').append($('<li>').attr("class", "list-group-item").on('click', function (e) { })
                    //.append($('<br>'))
                    .append($('<span>').addClass('glyphicon glyphicon-map-marker text-muted c-info c-info-icon')
                                       .attr("title", "go to ")
                                       .attr("data-toggle", "tooltip")
                                       .on("click", function (e) {
                                           map.setView(latlng, 17, { animate: true });
                                           marker.openPopup();
                                       })

                    )
                    .append($('<span>')
                            .text(address.Description)
                            .addClass('name'))
                            .attr("title", address.Type + " " + address.Status)
                            .attr("data-toggle", "tooltip")

                    );

                //marker.bindPopup(address.Description);
                marker.bindPopup(content);
                searchlayer.addLayer(marker);
            }
        }//#roadlink
        );

    if (items.Documents.length > 0) {
        $('#filter-btn').show();

    }
    else {
        $('#filter-btn').hide();
    }


    // zoom in..
    if (items.Documents.length > 0) {

        var bounds = searchlayer.getBounds();
        //map.fitBounds(bounds);//works!    
        map.fitBounds(bounds, { maxZoom: 18 });//works!    

    }
    searchlayer.addTo(map);
}

function addFilter(field, value, include) {
    $('#filter-list').append($('<li>')
        .attr("id", field + "_" + value)
        .addClass("list-group-item filter-li").css("background-color", include == true ? "blue" : "red")
        .append($('<span>').text(value))
        .append($('<span>').css("padding", "2px").css("margin", "2px").append($('<input>').attr("type", "checkbox").attr("checked", include).attr("data-toggle", "tooltip").attr("title", "Include")
        .on("click", function (e) {
            var id = $(this).closest('li').attr('id');
            var update_field = id.split("_")[0];
            var update_value = id.split("_")[1];
            var boolchecked = e.target.checked;
            changeFilters(update_field, update_value, boolchecked);
            var parent_li = $(this).closest('li').css("background-color", boolchecked == true ? "blue" : "red");
            LocationSearch();
            //FilterResults(text);
        })
        ))
        .append($('<a>')
        .attr("href", "#")
        .append($('<span>').css("margin", "1px").css("padding", "1px").css("color", "white")
        .addClass("glyphicon glyphicon-trash")
        .attr("data-toggle", "tooltip")
        .attr("title", "Remove from filters")
        .on("click", function (e) {
            $(this).closest('li').remove();
            var id = $(this).closest('li').attr('id');
            var del_field = id.split("_")[0];
            var del_value = id.split("_")[1];
            deleteFilter(del_field, del_value)
            filterlength -= 1;

            if (filterlength <= 0) {
                $('#filter-div').hide();

            }
            LocationSearch();
            //FilterResults(text);
        })
        )
        )
        )

    $('#filter-div').show();

}

function toggleFilters() {
    $("#filter-div").find('li').remove();
    $("#filter-div").hide();
    $("#filter-btn").hide();
    if (filters != undefined)
    {
        filters = new Array();
        filterlength = 0;
    }
}

function GetFilterTerms() {
    var filterString = "";
    if (filterlength > 0) {
        //Get subset of filters with values
        var populatedfilters = filters.filter(function (x) {
            return x.values.length > 0;
        });

        var filteredArray = [];
        populatedfilters.forEach(function (filter) {
            filter.values.forEach(function (flvalue) {
                var newFilter = {};
                newFilter["field"] = filter.field;
                newFilter["value"] = flvalue.value;
                newFilter["include"] = flvalue.include;
                filteredArray.push(newFilter);
            })

        })

        filterString = JSON.stringify(filteredArray);
    }
    return filterString;
}

function initialisefilters(items) {
    if (items.Aggregates == undefined)
        return;
    filters = [];
    items.Aggregates.forEach(function (aggregate) {

        if (aggregate.Items.length > 0) {
            if (aggregate.Name.toLowerCase() != 'thoroughfare') {
                var filter = {}
                filter["field"] = aggregate.Name;
                filter.values = [];
                filters.push(filter);
            }
        }
    });
}

function deleteFilter(field, value) {
    var filter_array = filters.filter(function (obj) {
        return obj.field == field;
    });

    if (filter_array != null && filter_array.length > 0) {
        var filter = filter_array[0];
        var values = filter.values;

        var filter_object_index = values.map(function (obj) {
            return obj.value;
        }).indexOf(value);

        if (filter_object_index != null && filter_object_index >= 0) {
            filter.values.splice(filter_object_index, 1);
        }

    }
}

function FilterExists(field, value) {
    var exists = false;
    var filter = filters.filter(function (x) { return x.field == field })[0];
    var values = filter.values.filter(function (y) { return y.value == value })
    if (values != null && values.length > 0) {
        var itm = values[0]
        exists = true;
    }
    return exists;
}

function changeFilters(infield, invalue, in_include) {


    //identify the relevant filter object from array of filters
    var target_filterArray = filters.filter(function (obj) {
        return obj.field.toLowerCase() == infield.toLowerCase();
    });

    var target_filter = target_filterArray[0];

    if (target_filter != null) {
        var values_array = target_filter.values;
        var valueobj = values_array.filter(function (x) { return x.value.toLowerCase() == invalue.toLowerCase() })[0];

        if (valueobj != null) {
            valueobj.include = in_include;
        }
        else {
            var newval = {};
            newval["value"] = invalue;
            newval["include"] = in_include;
            target_filter["values"].push(newval);
        }

    }

    return;
}

function updateFilterUI(field, value, include) {
    var id = field + "_" + value;
    var checkbox = $('#' + id).find("input[type='checkbox']");
    checkbox.prop("checked", include);
    $(checkbox).closest('li').css("background-color", include == true ? "blue" : "red");
    

}

function FilterResults(text, callResponse) {
    text = text.trim();
    clrSearchItems();
    clrInfoItems();

    if (text == "")
        return;

    $("message").hide();
    $("message-wait").show();

    var bb = map.getBounds();
    var parms = {
        searchText: text,
        exact: $('#fuzzy').hasClass("fa-question"),
        includeAggregates: false,                       // setting to true slows down the search - and only used for filters
        skip: 0,
        take: 1000,
        boundsfilter: $('#boundsfilter').hasClass("fa-lock"),
        w: bb.getWest(),
        s: bb.getSouth(),
        e: bb.getEast(),
        n: bb.getNorth(),
        filterterms: GetFilterTerms(),
    };

    return $.ajax({
        url: getURL('Home/SemanticSearch'),
        data: parms,
        dataType: 'json',
        success: function (items) {
            //alert("Search success");
            showFilterItems(items);
            if (callResponse != undefined)
                callResponse(items);

            $("message").show();
            $("message-wait").hide();

        },
        error: function (xhr, textStatus, errorThrown) {
            $("message").show();
            $("message-wait").hide();
        }
    });
}

