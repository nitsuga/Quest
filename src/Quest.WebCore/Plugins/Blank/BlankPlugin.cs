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
    [Injection("BlankPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    internal class BlankPlugin : StandardPlugin
    {
        public BlankPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("BlankPlugin", "BLANK", string.Empty, string.Empty, scope, env)
        {
        }
    }
}