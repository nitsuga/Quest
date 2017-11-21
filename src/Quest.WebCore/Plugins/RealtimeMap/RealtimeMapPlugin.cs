using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;

namespace Quest.WebCore.Plugins.RealtimeMap
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("RealtimeMapPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    [Injection(typeof(RealtimeMapPlugin), Lifetime.PerDependency)]
    public class RealtimeMapPlugin : StandardPlugin
    {
        public RealtimeMapPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("RealtimeMapPlugin", "MAP", "hud.plugins.rtmap.init(panelId, pluginId)", "/plugins/RealtimeMap/Lib", scope, env)
        {
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