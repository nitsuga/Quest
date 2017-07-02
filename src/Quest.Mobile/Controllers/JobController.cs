#pragma warning disable 0169,649
using System;
using System.IO;
using System.Linq;
using System.Text;
using Quest.Mobile.Models;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Quest.Common.Messages;
using Quest.Mobile.Attributes;
using Quest.Mobile.Service;
using Quest.Lib.Trace;

namespace Quest.Mobile.Controllers
{
    [Authorize]
    public class JobController : Controller
    {

        
        private RouteService _routeService;

        [Authorize(Roles = "administrator,user")]
        public ActionResult Index()
        {
            if (User?.Identity != null)
                Logger.Write($"Access: {Request.RawUrl} by {User.Identity.Name}", GetType().Name);

            // fill in with list of job templates
            GetJobTemplateRequest request = new GetJobTemplateRequest();
            var response = MvcApplication.MsgClientCache.SendAndWait<GetJobTemplateResponse>(request, new TimeSpan(0, 0, 10));
            JobsViewModel model=null;
            if (response != null)
            {
                model = new JobsViewModel( )
                {
                    //todo
                    Templates = response.Items.Select(x=>new JobTemplateModel { Template = new Common.Messages.JobTemplate(), Url="Job/StartTemplate/"+x.JobTemplateId}).ToList()
                };
            }

            return View(model);
        }

        [Authorize(Roles = "administrator,user")]
        public ActionResult Details(int id=0)
        {
            if (User == null)
                if (User?.Identity != null)
                    Logger.Write($"Access: {Request.RawUrl} by {User.Identity.Name}", GetType().Name);

            // fill in with list of job templates
            var response = MvcApplication.MsgClientCache.SendAndWait<GetJobLogResponse>(new GetJobLogRequest() { Jobid = id}, new TimeSpan(0, 0, 10));
            return View(response);
        }

        [Authorize(Roles = "administrator,user")]
        [HttpGet]
        public ActionResult GetJobs(int skip, int take)
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            var ser = new JsonSerializer();

            try
            {

                GetJobsRequest request = new GetJobsRequest {Skip = skip, Take = take};
                var response = MvcApplication.MsgClientCache.SendAndWait<GetJobsResponse>(request, new TimeSpan(0, 0, 10));
                if (response != null)
                {
                    var serializer = new JavaScriptSerializer {MaxJsonLength = int.MaxValue};
                    var result = new ContentResult
                    {
                        Content = serializer.Serialize(response),
                        ContentType = "application/json"
                    };

                    return result;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            var errorMsg = "ERROR: Cannot get jobs";
            ser.Serialize(writer, errorMsg);
            var error = new ContentResult
            {
                Content = builder.ToString(),
                ContentType = "application/json"
            };

            return error;
        }


        [Authorize(Roles = "administrator,user")]
        [HttpGet]
        [NoCache]
        public ActionResult StartJobTemplate(int id)
        {
            AddJobFromTemplateRequest request = new AddJobFromTemplateRequest( ) {JobTemplateId = id};
            var response = MvcApplication.MsgClientCache.SendAndWait<AddJobFromTemplateResponse>(request, new TimeSpan(0, 0, 10));
            var js = JsonConvert.SerializeObject(response);
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };
            return result;
        }

        [Authorize(Roles = "administrator,user")]
        [HttpGet]
        [NoCache]
        public ActionResult StartJob(string task, string parameters, string classname)
        {
            AddJobRequest request = new AddJobRequest() { TaskName = task, Classname = classname , Parameters=parameters};
            var response = MvcApplication.MsgClientCache.SendAndWait<AddJobFromTemplateResponse>(request, new TimeSpan(0, 0, 10));
            var js = JsonConvert.SerializeObject(response);
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };
            return result;
        }

        [Authorize(Roles = "administrator,user")]
        [HttpGet]
        [NoCache]
        public ActionResult AddJob(int jobid)
        {
            var job = _routeService.GetJob(jobid);
            var jsonp = job;

            var js = JsonConvert.SerializeObject(jsonp);
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };

            return result;
        }
    }

}