using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Quest.WebCore.Models;
using Quest.WebCore.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Quest.WebCore.Controllers
{
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

        [HttpGet()]
        public HudLayout GetLayout(string id)
        {
            var model = _pluginService.GetLayout(id);
            return model;
        }

        [HttpGet()]
        public List<HudLayout> GetLayouts()
        {
            var model = _pluginService.GetLayouts();
            return model;
        }

        [HttpGet()]
        public HudPluginModel Create(string id, [FromQuery]string role)
        {
            // Create the plugin object
            var plugin = _pluginService.Create(id);

            if (plugin == null)
                return null;

            // Set its properties - for now we will use the defaults
            // TODO: do we need to carry through plugin state?
            plugin.InitializeWithDefaultProperties();

            var v = _pluginService.GetPluginModel(plugin, role);

            return v;
        }
    }
}