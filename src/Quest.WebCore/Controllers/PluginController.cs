﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Quest.WebCore.Models;
using Quest.WebCore.Services;
using System.Collections.Generic;

namespace Quest.WebCore.Controllers
{
    /// <summary>
    /// json services for loading layouts and plugins
    /// </summary>
    public class PluginController : Controller
    {
        private readonly IPluginService _pluginService;
        private readonly IViewRenderService _viewRenderService;
        private IHostingEnvironment _env;

        public PluginController(IPluginService pluginService, IViewRenderService viewRenderService, IHostingEnvironment env)
        {
            _pluginService = pluginService;
            _viewRenderService = viewRenderService;
            _env = env;
        }

        /// <summary>
        /// get a specific layout
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet()]
        [ResponseCache(NoStore = true)]
        public HudLayout GetLayout(string id)
        {
            var model = _pluginService.GetLayout(id);
            return model;
        }

        /// <summary>
        /// get available layouts
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ResponseCache(NoStore = true)]
        public List<HudLayout> GetLayouts()
        {
            var model = _pluginService.GetLayouts();
            return model;
        }

        /// <summary>
        /// creates a plugin model for a spefic plug in a specific slot (role)
        /// </summary>
        /// <param name="id">name of the plugin</param>
        /// <param name="panelId">target role</param>
        /// <returns></returns>
        [HttpGet()]
        [ResponseCache(NoStore = true)]
        public HudPluginModel CreatePlugin(string id)
        {
            // Create the plugin object
            var plugin = _pluginService.Create(id);

            if (plugin == null)
                return null;

            // Set its properties - for now we will use the defaults
            // TODO: do we need to carry through plugin state?
            plugin.InitializeWithDefaultProperties();

            var v = _pluginService.GetPluginModel(plugin);

            return v;
        }

        /// <summary>
        /// Render a panel given the panel model
        /// </summary>
        /// <param name="id"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(NoStore = true)]
        public ActionResult RenderPanel([FromBody] HudPanel model)
        {
            // render it
            var view = PartialView("_HudPanel", model);
            return view;
        }

        /// <summary>
        /// render an entire layout
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(NoStore = true)]
        public ActionResult RenderLayout([FromBody] HudLayout model)
        {
            // render it
            var view = PartialView("_Hud", model);
            return view;
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        public ActionResult RenderLayoutByName(string id)
        {
            HudLayout layout = GetLayout(id);
            // render it
            var view = PartialView("_Hud", layout);
            return view;
        }
    }
}