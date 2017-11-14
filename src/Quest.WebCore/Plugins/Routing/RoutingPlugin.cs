using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using System.Collections.Generic;
using System;
using Quest.WebCore.Plugins.Lib;

namespace Quest.WebCore.Plugins.Routing
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("RoutingPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    public class RoutingPlugin : StandardPlugin
    {
        public RoutingPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("RoutingPlugin", "ROUT", string.Empty, string.Empty, scope, env)
        {
        }

        public override string DrawContainer()
        {
            return $"<div></div>";
        }
    }
}