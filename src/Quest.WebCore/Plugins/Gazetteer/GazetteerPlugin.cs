﻿using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Quest.WebCore.Plugins.Gazetteer
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("GazetteerPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    [Injection(typeof(GazetteerPlugin))]
    public class GazetteerPlugin : IHudPlugin
    {
        ILifetimeScope _scope;
        private IHostingEnvironment _env;

        public GazetteerPlugin(ILifetimeScope scope, IHostingEnvironment env)
        {
            _scope = scope;
            _env = env;
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// The name of the plugin
        /// </summary>
        public string Name => "GazetteerPlugin"; // <-- must be the same as the injected Name

        public Dictionary<string, object> Properties { get; set; }

        public string MenuText => "GAZ";

        public bool IsMenuItem => true;

        public string RenderHtml()
        {
            return DrawContainer();
        }

        public string OnInit()
        {
            return "hud.plugins.gaz.init(panelId, pluginId)";
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
            var templateFolder = _env.WebRootPath + "/plugins/Gazetteer/Lib";
            var html = File.ReadAllText($"{templateFolder}/{templateFileName}");
            return html;
        }

        public GazSettings GetSettings()
        {
            return new GazSettings
            {
                Latitude = 51.5,
                Longitude = -0.2,
                Zoom = 12,
                MapServer = "192.168.0.3:8090"
            };
        }
    }
}