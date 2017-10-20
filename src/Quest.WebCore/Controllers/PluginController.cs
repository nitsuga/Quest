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
        private readonly IPluginService _pluginFactory;
        private readonly IViewRenderService _viewRenderService;
        private IHostingEnvironment _env;

        public PluginController(IPluginService pluginFactory, IViewRenderService viewRenderService, IHostingEnvironment env)
        {
            _pluginFactory = pluginFactory;
            _viewRenderService = viewRenderService;
            _env = env;
        }

        [HttpGet()]
        public HudPluginModel Create(string id)
        {
            // Create the plugin object
            var plugin = _pluginFactory.Create(id);

            if (plugin == null)
                return null;

            // Set its properties - for now we will use the defaults
            // TODO: do we need to carry through plugin state?
            plugin.InitializeWithDefaultProperties();

            var v = _pluginFactory.GetPluginModel(plugin);

            return v;
        }
    }
}