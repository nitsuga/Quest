#pragma warning disable 0169,649
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Web.Mvc;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Newtonsoft.Json;
using Quest.Mobile.Attributes;
using Quest.Common.Messages;
using Quest.Mobile.Service;

namespace Quest.Mobile.Controllers
{
    public class SecurityController : Controller
    {

        private SecurityService _securityService;

        public SecurityController(
            SecurityService securityService
            )
        {
            _securityService = securityService;
        }

        // GET: Security
        public ActionResult Index()
        {
            return View();
        }
        

        [Authorize(Roles = "administrator")]
        [HttpGet]
        [NoCache]
        public ActionResult GetNetwork()
        {
            var js = JsonConvert.SerializeObject(_securityService.GetNetwork());
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };

            return result;
        }
    }
}