using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Quest.WebCore.Services
{
    public interface IPluginService
    {
        /// <summary>
        /// Creates an instance of a plugin
        /// </summary>
        /// <param name="pluginName"></param>
        /// <returns></returns>
        IHudPlugin Create(string pluginName);

        HudLayout GetLayoutModel(HudLayoutSummary summary);

        HudPluginModel GetPluginModel(string pluginName);

        HudPluginModel GetPluginModel(IHudPlugin plugin);
    }

    public class PluginService : Dictionary<string, Type>, IPluginService
    {
        private readonly ILifetimeScope _scope;
        private IHostingEnvironment _env;

        public PluginService(ILifetimeScope scope, IHostingEnvironment env)
        {
            _scope = scope;
            _env = env;
        }

        /// <summary>
        /// Creates an instance of a plugin
        /// </summary>
        /// <param name="pluginName"></param>
        /// <returns></returns>
        public IHudPlugin Create(string pluginName)
        {
            if (string.IsNullOrWhiteSpace(pluginName))
                return null;
            IHudPlugin plugin = _scope.ResolveNamed<IHudPlugin>( pluginName);
            return plugin;
        }

        private List<string> GetScripts()
        {
            var pluginPath = Path.Combine(_env.WebRootPath, "plugins");
            var files = new List<string>();

            var pluginFolder = new DirectoryInfo(pluginPath);
            foreach (var folder in pluginFolder.GetDirectories())
            {
                var scriptsFolder = new DirectoryInfo(folder.FullName + "\\Scripts");
                if (!scriptsFolder.Exists) continue;

                foreach (var fileInfo in scriptsFolder.GetFiles())
                {
                    var pluginFilePath = fileInfo.DirectoryName.Replace(pluginPath, "");
                    files.Add($"/plugins{pluginFilePath}/{fileInfo.Name}".Replace("\\","/"));
                }

            }
            return files;
        }

        private List<string> GetStyles()
        {
            var pluginPath = Path.Combine(_env.WebRootPath, "plugins");
            var files = new List<string>();

            var pluginFolder = new DirectoryInfo(pluginPath);
            foreach (var folder in pluginFolder.GetDirectories())
            {
                var stylesFolder = new DirectoryInfo(folder.FullName + "\\Content");
                if (!stylesFolder.Exists) continue;

                foreach (var fileInfo in stylesFolder.GetFiles("*.css"))
                {
                    var pluginFilePath = fileInfo.DirectoryName.Replace(pluginPath, "");
                    files.Add($"/plugins{pluginFilePath}/{fileInfo.Name}".Replace("\\", "/"));
                }

            }

            return files;
        }

        public HudLayout GetLayoutModel(HudLayoutSummary summary)
        {
            HudLayout layout = new HudLayout()
            {
                Scripts = GetScripts(),
                Styles = GetStyles(),
                Format = summary.Format,
                Panels =new List<HudPluginModel>()
            };

            foreach (var pluginName in summary.Plugins)
                layout.Panels.Add(GetPluginModel(pluginName));
            return layout;
        }

        public HudPluginModel GetPluginModel(IHudPlugin plugin)
        {
            HudPluginModel source = new HudPluginModel
            {
                MenuText = plugin.MenuText,
                IsMenuItem = plugin.IsMenuItem,
                PluginSourceName = plugin.Name,
                Html = HttpUtility.HtmlDecode(plugin.RenderHtml()),
                OnInit = HttpUtility.HtmlDecode(plugin.OnInit()),
                OnPanelMoved = HttpUtility.HtmlDecode(plugin.OnPanelMoved()),
            };
            return source;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plugin"></param>
        public HudPluginModel GetPluginModel(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName))
                return new HudPluginModel();

            var plugin = Create(pluginName);

            if (plugin != null)
                return GetPluginModel(plugin);

            // could return an Error plugin
            return null;
        }
    }
}