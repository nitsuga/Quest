using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;

namespace Quest.WebCore.Plugins.AAC
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("AACPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    internal class AACPlugin : StandardPlugin
    {
        public AACPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("AACPlugin", "AAC", "hud.plugins.aac.init(panelId, pluginId)", "/plugins/AAC/Lib", scope, env)
        {
        }
    }
}