using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Quest.WebCore.Plugins.Blank
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("HistoricMapPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    public class HistoricMapPlugin : IHudPlugin
    {
        ILifetimeScope _scope;
        private IHostingEnvironment _env;

        public HistoricMapPlugin(ILifetimeScope scope, IHostingEnvironment env)
        {
            _scope = scope;
            _env = env;
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// The name of the plugin
        /// </summary>
        public string Name => "HistoricMapPlugin"; // <-- must be the same as the injected Name

        public Dictionary<string, object> Properties { get; set; }

        public string MenuText => "HIST";

        public bool IsMenuItem => true;

        public string RenderHtml(string role)
        {
            return DrawContainer(role);
        }

        public string OnInit()
        {
            return "hud.plugins.histmap.initMap(panelRole)";
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

        private string DrawContainer(string role)
        {
            const string templateFileName = "Index.html";
            var templateFolder = _env.WebRootPath + "/plugins/HistoricMap/Lib";
            var html = File.ReadAllText($"{templateFolder}/{templateFileName}");
            return html;
        }
    }
}