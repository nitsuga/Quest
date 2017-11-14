﻿using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;
using System.Collections.Generic;
using System.IO;

namespace Quest.WebCore.Plugins.Properties
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("PropertiesPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    public class PropertiesPlugin : StandardPlugin
    {
        ILifetimeScope _scope;
        private IHostingEnvironment _env;

        public PropertiesPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("PropertiesPlugin", "PROP", "hud.plugins.properties.init(panelId, pluginId)", "/plugins/Properties/Lib", scope, env)
        {
        }
    }
}