var timer;

$(function () {

    $("#submit").button().on("click", function () {
        
        var locs = $('#Locations')[0].value;

        $.ajax({
            cache: false,
            url: getURL('Home/BatchSearchLocations'),
            data:
                {
                    locations: locs,
                },
            dataType: 'json',
            success: function (msg) {
                $('#submit').prop('disabled', true);
                displayJob(msg);
                pollJob(msg.jobid);
            },
            error: function (xhr, textStatus, errorThrown) {
                alert("failed");
            }
        });

    });
    
});

function pollJob(id) {    
    clearTimeout(timer);
    timer = setInterval(function (event) {
        getJob(id);
    }, 2000);
}

function getJob(id) {
    $.ajax({
        cache: false,
        url: getURL('Home/GetLocationsJob'),
        data:
            {
                jobid: id,
            },
        dataType: 'json',
        success: function (msg) {
            displayJob(msg);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert("failed");
        }
    });
}
function displayJob(job) {
    //$('#jobid').html('job id is ' + job.jobid);
    $("#results").find('tbody').find('tr').remove();
    $('#submit').prop('disabled', !job.complete);

    if (job.complete != false)
        clearTimeout(timer);

    job.request.forEach(
        function (ss) {

            if (ss.bestmatch == undefined)
                if (ss.header == true)
                    $("#results").find('tbody')
                        .append($('<tr>')
                            .append($('<td>').append($('<h4>').text(ss.searchText)))
                            .append($('<td>').append("<p>").text(""))
                            .append($('<td>').append("p").text(ss.status))
                            .append($('<td>').append("<p>").text(""))
                            .append($('<td>').append("<p>").text(""))
                            );
                else
                    $("#results").find('tbody')
                    .append($('<tr>')
                        .append($('<td>').append("p").text(ss.searchText))
                        .append($('<td>').append("<p>").text(""))
                        .append($('<td>').append("<p>").text(ss.status))
                        .append($('<td>').append("<p>").text(""))
                        .append($('<td>').append("<p>").text(""))
                        );

                else
                $("#results").find('tbody')
                    .append($('<tr>')
                        .append($('<td>').append("<p>").text(ss.searchText))
                        .append($('<td>').append("<p>").text(ss.bestmatch.Description))
                        .append($('<td>').append("<p>").text(ss.status))
                        .append($('<td>').append("<p>").text(ss.count))
                        .append($('<td>').append("<p>").text(ss.score))
                        );

        }
    );
    // display results..
}

