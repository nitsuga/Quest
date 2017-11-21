using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;

namespace Quest.WebCore.Plugins.Visuals
{
    /// <summary>
    /// </summary>
    [Injection("VisualsPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    public class VisualsPlugin : StandardPlugin
    {
        public VisualsPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("VisualsPlugin", "VIS", string.Empty, string.Empty, scope, env)
        {
        }

        public override string DrawContainer()
        {
            return $"<div></div>";
        }
    }
}