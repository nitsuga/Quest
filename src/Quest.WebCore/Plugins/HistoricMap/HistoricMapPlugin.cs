using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;

namespace Quest.WebCore.Plugins.Blank
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("HistoricMapPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    internal class HistoricMapPlugin : StandardPlugin
    {
        public HistoricMapPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("HistoricMapPlugin", "HIST", "hud.plugins.histmap.init(panelId, pluginId)", "/plugins/HistoricMap/Lib", scope, env)
        {
        }
    }
}