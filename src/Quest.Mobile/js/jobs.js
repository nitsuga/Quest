var timer;
var pagesize = 10;
var page = 0;
var pages = 0;

$(function () {

    $(".template-btn").click(function () {
        var id = $(this).attr("data-id");
        StartJobTemplate(id);
        return true;
    });

    $("#next").click(function () {
        page++;
        if (page >= pages)
            page = pages - 1;
        getJobs();
        return true;
    });

    $("#prev").click(function () {
        page--;
        if (page <0)
            page = 0;
        getJobs();
        return true;
    });

    $('[data-toggle="toggle"]').each(function (index, element) {
        setSliderFromStore("#" + element.id);
    });

    // save any settings from a toogle
    $('[data-toggle="toggle"]').change(function () {
        var target = $(this)[0];
        setStoreFromSlider("#" + target.id);
        setPoll();
    });

    // show the start job dialog
    $("#do-submit-dlg").click(function () {
        $("#submit-dlg").modal("show");
        return true;
    });

    // submit a new job for processing
    $("#job-submit").click(function () {
        $("#submit-dlg").modal("hide");

        var task = $("#job-task").val();
        var classname = $("#job-class").val();
        var parameters = $("#job-parameters").val();

        $.ajax({
            url: getURL("Job/StartJob"),
            data:
                {
                    task: task,
                    parameters: parameters,
                    classname: classname
                },
            dataType: "json",
            success: function (response) {
            },
            error: function () {
                alert("Job submission failed");
            }
        });
        return false;
    });

    setPoll();

    $("#submit").button().on("click", function () {
        
        var locs = $('#Locations')[0].value;

        $.ajax({
            cache: false,
            url: getURL('Job/AddJob'),
            data:
                {
                    Locations: locs
                },
            dataType: 'json',
            success: function (msg) {
            },
            error: function (xhr, textStatus, errorThrown) {
                alert("failed");
            }
        });

    });
    
});

function setPoll() {
    var realtime = getStoreAsBool("#realtime-flag");
    getJobs();
    if (realtime) {
        pollJobs();
    } else {
        clearTimeout(timer);
    }
}

function pollJobs() {
    clearTimeout(timer);
    timer = setInterval(function (event) {

        getJobs();


    }, 5000);
}

function getJobs() {
    $.ajax({
        cache: false,
        url: getURL('Job/GetJobs'),
        dataType: 'json',
        data:
        {
            skip: page * pagesize,
            take: pagesize
        },
        success: function (msg) {
            pages = (msg.Total / pagesize) + 1;
            displayJobs(msg);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert("failed");
        }
    });
}

function StartJobTemplate(id) {
    $.ajax({
        cache: false,
        url: getURL('Job/StartJobTemplate'),
        data:
            {
                id: id
            },
        dataType: 'json',
        success: function (msg) {
            getJobs();
        },
        error: function (xhr, textStatus, errorThrown) {
            alert("failed");
        }
    });
}

function displayJobs(job) {
    $("#jobs").find('tbody').find('tr').remove();
    if (job.Items === undefined)
        return;
    job.Items.forEach(
        
        function (ss) {
            if (ss !== undefined) {
                var anchor = '<a href="Details?id=' + ss.JobInfoId + '">' + ss.JobInfoId + "</a>";
                $("#jobs")
                    .find('tbody')
                    .append($('<tr>')
                        .append($('<td>').append("<p>").html(anchor))
                        .append($('<td>').append("<p>").text(ss.JobStatus))
                        .append($('<td>').append("<p>").text(ss.Taskname))
                        .append($('<td>').append("<p>").text(ss.Description))
                        .append($('<td>').append("<p>").text(ss.Success))
                        .append($('<td>').append("<p>").text(ss.Message))
                        .append($('<td>').append("<p>").text(dt(ss.Created)))
                        .append($('<td>').append("<p>").text(dt(ss.Scheduled)))
                        .append($('<td>').append("<p>").text(dt(ss.Started)))
                        .append($('<td>').append("<p>").text(dt(ss.Stopped)))
                    );
            }
        }
    );
    // display results..
}

function dt(jdate) {
    if (jdate == undefined || jdate === "")
        return "";
    var dateNow = new Date();
    var dateCreated = new Date(parseInt(jdate.substr(6)));
    var dn = dateNow.toLocaleDateString("en-EN");
    var d = dateCreated.toLocaleDateString("en-EN");
    var t = dateCreated.toLocaleTimeString("en-EN");
    if (dn === d)
        return t;
    else {
        return d + " " + t;
    }
}

