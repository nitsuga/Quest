using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;

namespace Quest.WebCore.Plugins.Camera
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("CameraPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    internal class CameraPlugin : StandardPlugin
    {
        public CameraPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("CameraPlugin", "CAM", "hud.plugins.camera.init(panelId, pluginId)", "/plugins/Camera/Lib", scope, env)
        {
        }
    }
}