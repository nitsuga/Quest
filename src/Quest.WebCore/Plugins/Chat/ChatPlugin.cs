using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;

namespace Quest.WebCore.Plugins.ChatPlugin
{
    /// <summary>
    /// </summary>
    [Injection("ChatPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    internal class ChatPlugin : StandardPlugin
    {
        public ChatPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("ChatPlugin", "CHAT", "hud.plugins.chat.init(panelId, pluginId)", "/plugins/Chat/Lib", scope, env)
        {
        }
    }
}