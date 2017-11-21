using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;

namespace Quest.WebCore.Plugins.Gazetteer
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("GazetteerPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    [Injection(typeof(GazetteerPlugin))]
    public class GazetteerPlugin : StandardPlugin
    {
        public GazetteerPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("GazetteerPlugin", "GAZ", "hud.plugins.gaz.init(panelId, pluginId)", "/plugins/Gazetteer/Lib", scope, env)
        {
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