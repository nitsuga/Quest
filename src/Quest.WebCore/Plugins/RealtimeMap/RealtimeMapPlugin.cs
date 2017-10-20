using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;

namespace Quest.WebCore.Plugins.ChatPlugin
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("RealtimeMapPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    public class RealtimeMapPlugin : IHudPlugin
    {
        ILifetimeScope _scope;
        private IHostingEnvironment _env;

        public RealtimeMapPlugin(ILifetimeScope scope, IHostingEnvironment env)
        {
            _scope = scope;
            _env = env;
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// The name of the plugin
        /// </summary>
        public string Name => "RealtimeMapPlugin"; // <-- must be the same as the injected Name

        public Dictionary<string, object> Properties { get; set; }

        public string MenuText => "MAP";

        public bool IsMenuItem => true;

        public string RenderHtml()
        {
            return DrawContainer();
        }

        public string OnInit()
        {
            return "hud.plugins.map.initMap()";
        }

        public string OnPanelMoved()
        {
            return string.Empty;
        }

        public void InitializeWithProperties(Dictionary<string, object> properties)
        {
            // Do nothing
        }

        public void InitializeWithDefaultProperties()
        {
            // Do nothing
        }

        private string DrawContainer()
        {
            const string templateFileName = "Map.html";
            var templateFolder = _env.WebRootPath + "/plugins/Map/Lib";
            var gazHtml = File.ReadAllText($"{templateFolder}/{templateFileName}");
            return gazHtml;
        }
    }
}