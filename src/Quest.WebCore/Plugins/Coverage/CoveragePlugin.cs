using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;

namespace Quest.WebCore.Plugins.Coverage
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("CoveragePlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    [Injection(typeof(CoveragePlugin))]
    public class CoveragePlugin : StandardPlugin
    {
        public CoveragePlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("CoveragePlugin", "COV", "hud.plugins.coverage.init(panelId, pluginId)", "/plugins/Coverage/Lib", scope, env)
        {
        }

        public CoverageSettings GetSettings()
        {
            return new CoverageSettings
            {
                Latitude = 51.5,
                Longitude = -0.2,
                Zoom = 12,
                MapServer = "192.168.0.3:8090"
            };
        }
    }
}