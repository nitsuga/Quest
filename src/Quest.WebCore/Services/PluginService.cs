using Autofac;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Quest.Lib.Trace;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Quest.WebCore.Services
{
    public interface IPluginService
    {
        /// <summary>
        /// Creates an instance of a plugin
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="role">role</param>
        /// <returns></returns>
        IHudPlugin Create(string pluginName);

        HudModel GetLayoutModel(HudLayoutSummary summary);

        HudPluginModel GetPluginModel(string pluginName);

        HudPluginModel GetPluginModel(IHudPlugin plugin);

        /// <summary>
        /// return a list of styles required by the plugins
        /// </summary>
        /// <returns></returns>
        List<string> GetStyles();

        /// <summary>
        /// return a catalogue of layouts that the user can select
        /// </summary>
        /// <returns></returns>
        List<HudLayout> GetLayouts();

        /// <summary>
        /// returns a named layout
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        HudLayout GetLayout(string name);

        /// <summary>
        /// return a list of scripts required by the plugins. note that each plugin
        /// can also have a file called scripts.txt that defines the order in which the
        /// scripts will be loaded
        /// </summary>
        /// <returns></returns>
        List<string> GetScripts();

        /// <summary>
        /// return a default layout
        /// </summary>
        /// <returns></returns>
        String DefaultLayout();
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

        public String DefaultLayout()
        {
            return "Layout";
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
            IHudPlugin plugin = _scope.ResolveNamed<IHudPlugin>(pluginName);
            return plugin;
        }

        /// <summary>
        /// load scripts
        /// </summary>
        /// <returns></returns>
        public List<string> GetScripts()
        {
            var pluginPath = Path.Combine(_env.WebRootPath, "plugins");
            var files = new List<string>();

            Logger.Write($"Looking for scripts in {pluginPath}");

            var pluginFolder = new DirectoryInfo(pluginPath);
            foreach (var folder in pluginFolder.GetDirectories())
            {
                var scriptspath = Path.Combine(folder.FullName, "Scripts");
                var relativeScriptsPath = scriptspath.Replace(_env.WebRootPath, "");

                var scriptsInfo = new DirectoryInfo(scriptspath);
                if (!scriptsInfo.Exists) continue;

                var scriptOrderFile = Path.Combine(scriptspath, "scripts.txt");
                if (File.Exists(scriptOrderFile))
                {
                    var scripts = File.ReadAllLines(scriptOrderFile);
                    Logger.Write($"  .. found script order file with {scripts.Count()} scripts in {relativeScriptsPath}");
                    foreach (var script in scripts)
                    {
                        var scriptpath = $"{relativeScriptsPath}/{script}".Replace("\\", "/");
                        if (!files.Contains(scriptpath))
                            files.Add(scriptpath);
                    }
                }
                else
                {
                    var scripts = scriptsInfo.GetFiles();
                    Logger.Write($"  .. found {scripts.Count()} scripts in {scriptspath}");
                    foreach (var fileInfo in scripts)
                    {
                        var scriptpath = Path.Combine(relativeScriptsPath, fileInfo.Name).Replace("\\", "/");
                        if (!files.Contains(scriptpath))
                            files.Add(scriptpath);
                    }
                }

            }

            return files;
        }

        public List<string> GetStyles()
        {
            var pluginPath = Path.Combine(_env.WebRootPath, "plugins");
            var files = new List<string>();

            Logger.Write($"Looking for styles in {pluginPath}");

            var pluginFolder = new DirectoryInfo(pluginPath);
            foreach (var folder in pluginFolder.GetDirectories())
            {
                var stylesFolder = Path.Combine(folder.FullName, "Content");
                var stylesInfo = new DirectoryInfo(stylesFolder);
                var stylesRelativeFolder = stylesFolder.Replace(_env.WebRootPath, "");
                if (!stylesInfo.Exists) continue;

                var styles = stylesInfo.GetFiles("*.css");

                Logger.Write($"  .. found {styles.Count()} styles in {stylesRelativeFolder}");

                foreach (var fileInfo in styles)
                {
                    var stylepath = Path.Combine(stylesRelativeFolder, fileInfo.Name).Replace("\\", "/");
                    files.Add(stylepath);
                }

            }

            return files;
        }

        public HudModel GetLayoutModel(HudLayoutSummary summary)
        {
            HudModel layout = new HudModel()
            {
                Scripts = GetScripts(),
                Styles = GetStyles(),
            };
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

        public List<HudLayout> GetLayouts()
        {
            List<HudLayout> layouts = new List<HudLayout>();
            var layoutPath = Path.Combine(_env.WebRootPath, "layouts");
            var files = new List<string>();
            var layoutFolder = new DirectoryInfo(layoutPath);

            foreach (var fileInfo in layoutFolder.GetFiles("*.json"))
            {
                try
                {
                    var text = File.ReadAllText(fileInfo.FullName);
                    var layout = JsonConvert.DeserializeObject<HudLayout>(text);
                    layouts.Add(layout);
                }
                catch
                { }
            }
            return layouts;
        }

        public HudLayout GetLayout(string name)
        {
            var layouts = GetLayouts();
            return layouts.FirstOrDefault(x => x.Name == name);
        }

    }
}