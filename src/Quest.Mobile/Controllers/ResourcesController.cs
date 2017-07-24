using Quest.Mobile.Models;
using Quest.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Quest.Mobile.Controllers
{
    public class ResourcesController : ApiController
    {
        private ResourceService _resourceService;

        public ResourcesController(ResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        [HttpGet]
        public ResourceFeatureCollection Get(bool avail = true, bool busy = true)
        {
            return _resourceService.GetResources(avail, busy);
        }
    }
}