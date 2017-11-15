using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;
using System.Collections.Generic;
using System.IO;

namespace Quest.WebCore.Plugins.Hospital
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("HospitalPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    internal class HospitalPlugin : StandardPlugin
    {
        public HospitalPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("HospitalPlugin", "HOS", "hud.plugins.Hospital.init(panelId, pluginId)", "/plugins/Hospital/Lib", scope, env)
        {
        }
    }
}