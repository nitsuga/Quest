﻿using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using System.Collections.Generic;
using System;

namespace Quest.WebCore.Plugins.Routing
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("RoutingPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    public class RoutingPlugin : IHudPlugin
    {
        ILifetimeScope _scope;
        private IHostingEnvironment _env;

        public RoutingPlugin(ILifetimeScope scope, IHostingEnvironment env)
        {
            _scope = scope;
            _env = env;
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// The name of the plugin
        /// </summary>
        public string Name => "RoutingPlugin"; // <-- must be the same as the injected Name

        internal RoutingSettings GetSettings()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> Properties { get; set; }

        public string MenuText => "ROUTE";

        public bool IsMenuItem => true;

        public string RenderHtml()
        {
            return DrawContainer();
        }

        public string OnInit()
        {
            return string.Empty;
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
            return $"<div></div>";
        }
    }
}