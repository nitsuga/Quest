using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Quest.WebCore.Plugins.Properties
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("PropertiesPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    public class PropertiesPlugin : IHudPlugin
    {
        ILifetimeScope _scope;
        private IHostingEnvironment _env;

        public PropertiesPlugin(ILifetimeScope scope, IHostingEnvironment env)
        {
            _scope = scope;
            _env = env;
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// The name of the plugin
        /// </summary>
        public string Name => "PropertiesPlugin"; // <-- must be the same as the injected Name

        public Dictionary<string, object> Properties { get; set; }

        public string MenuText => "PROP";

        public bool IsMenuItem => true;

        public string RenderHtml(string role)
        {
            return DrawContainer(role);
        }

        public string OnInit()
        {
            return "hud.plugins.properties.initialize()";
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
            const string templateFileName = "Properties.html";
            var templateFolder = _env.WebRootPath + "/plugins/Properties/Lib";
            var gazHtml = File.ReadAllText($"{templateFolder}/{templateFileName}");
            return gazHtml;
        }
    }
}