using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;

namespace Quest.WebCore.Plugins.HeldCalls
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("HeldCallsPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    public class HeldCallsPlugin : StandardPlugin
    {
        public HeldCallsPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("HeldCallsPlugin", "HLD", string.Empty, string.Empty, scope, env)
        {
        }

        public override string DrawContainer()
        {
            return "<div>held calls</div>";
        }
    }
}