﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quest.Common.Messages.Resource;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Services;
using System;
using System.Threading.Tasks;

namespace Quest.WebCore.Plugins.RealtimeMap
{
    public class AACController : Controller
    {
        private AsyncMessageCache _messageCache;
        private SecurityService _securityService;
        private readonly IPluginService _pluginService;

        public AACController(AsyncMessageCache messageCache,
                IPluginService pluginFactory,
                SecurityService securityService
            )
        {
            _messageCache = messageCache;
            _securityService = securityService;
            _pluginService = pluginFactory;
        }

        [HttpPost]
        public async Task<AssignToDestinationResponse> AssignResource(AssignToDestinationRequest request)
        {
            var result = await _messageCache.SendAndWaitAsync<AssignToDestinationResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }


        [HttpGet]
        public async Task<GetResourceAssignmentsResponse> GetAssignmentStatus()
        {
            GetResourceAssignmentsRequest request = new GetResourceAssignmentsRequest();
            var result = await _messageCache.SendAndWaitAsync<GetResourceAssignmentsResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        [HttpPost]
        public async Task<ActionResult> RenderAAC()
        {
            GetResourceAssignmentsRequest request = new GetResourceAssignmentsRequest();
            var result = await _messageCache.SendAndWaitAsync<GetResourceAssignmentsResponse>(request, new TimeSpan(0, 0, 10));
            // render it
            var view = PartialView($"/Plugins/AAC/Views/AAC.cshtml", result);
            return view;
        }

    }
}