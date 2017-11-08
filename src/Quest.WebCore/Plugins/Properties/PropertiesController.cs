﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quest.WebCore.Services;
using System;

namespace Quest.WebCore.Plugins.PropertiesPlugin
{
    public class PropertiesController : Controller
    {
        private readonly IPluginService _pluginService;

        public PropertiesController(IPluginService pluginFactory)
        {
            _pluginService = pluginFactory;
        }

        [HttpPost]
        public ActionResult RenderProperties([FromBody] dynamic obj)
        {
            var type = obj["Type"].Value;
            var value = obj["Value"];

            // render it
            var view = PartialView($"/Views/Shared/Plugins/Properties/{type}.cshtml", value);
            return view;
        }
    }

    [Serializable]
    public class HudProperty
    {
        public string Type;
        public dynamic Value;
    }
}