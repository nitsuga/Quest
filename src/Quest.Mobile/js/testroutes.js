var timer;

$(function () {

    $("#submit").button().on("click", function () {
        
        var routes = $('#Routes')[0].value;
        var RoadSpeedCalculator = $('#RoadSpeedCalculator')[0].value;

        $.ajax({
            cache: false,
            url: getURL('Home/BatchRoutes'),
            data:
                {
                    routes: routes,
                    roadSpeedCalculator: RoadSpeedCalculator
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
        url: getURL('Home/GetRoutesJob'),
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

    job.items.forEach(
        function (ss) {
            $("#results").find('tbody')
                .append($('<tr>')
                    .append($('<td>').append("<p>").text(ss.FromX + "/" + ss.FromY))
                    .append($('<td>').append("<p>").text(ss.ToX + "/" + ss.ToY))
                    .append($('<td>').append("<p>").text(ss.Hour))
                    .append($('<td>').append("<p>").text(ss.Vehicle))
                    .append($('<td>').append("<p>").text(ss.ActualTimeSecs))
                    .append($('<td>').append("<p>").text(ss.EstTimeSecs))
                    .append($('<td>').append("<p>").text(ss.status))
                    .append($('<td>').append("<p>").text(ss.complete))
                    );
        }
    );
    // display results..
}

