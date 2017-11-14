using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Quest.WebCore.Plugins.RealtimeMap
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("RealtimeMapPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    [Injection(typeof(RealtimeMapPlugin), Lifetime.PerDependency)]
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
            return "hud.plugins.rtmap.init(panelId, pluginId)";
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
            const string templateFileName = "index.html";
            var templateFolder = _env.WebRootPath + "/plugins/RealtimeMap/Lib";
            var html = File.ReadAllText($"{templateFolder}/{templateFileName}");
            return html;
        }

        public MapSettings GetSettings()
        {
            return new MapSettings {
                 Latitude=51.5,
                 Longitude=-0.2,
                 Zoom=12,
                 MapServer = "192.168.0.3:8090"
            };
        }
    }
}