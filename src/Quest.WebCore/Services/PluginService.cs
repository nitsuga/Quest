using Autofac;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
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
        /// <param name="role">role</param>
        /// <returns></returns>
        IHudPlugin Create(string pluginName);

        HudModel GetLayoutModel(HudLayoutSummary summary);

        HudPluginModel GetPluginModel(string pluginName, string role);

        HudPluginModel GetPluginModel(IHudPlugin plugin, string role);

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
        HudLayout DefaultLayout();
    }

    public class PluginService : Dictionary<string, Type>, IPluginService
    {
        private readonly ILifetimeScope _scope;
        private IHostingEnvironment _env;
        private List<HudLayout> _layouts;

        public PluginService(ILifetimeScope scope, IHostingEnvironment env)
        {
            _scope = scope;
            _env = env;
        }

        public HudLayout DefaultLayout()
        {
            return new HudLayout {
                Name="Double",
                ImagePath=null,
                Panels = new List<HudPanel>
                {
                    new HudPanel{ Role=0, Style="col-md-6 twothird-height right-border",
                        Actions =new List<HudPanelAction>
                        {
                            new HudPanelAction{ Action="fullscreen", Icon= "glyphicon-new-window", Position= "panel-btn-bottom", Label="Expand panel to full screen" },
                            new HudPanelAction{ Action="menu",       Icon= "glyphicon-menu-hamburger", Position= "panel-top-left" },
                        }
                    },
                    new HudPanel{ Role=1, Style="col-md-6 twothird-height",
                        Actions =new List<HudPanelAction>
                        {
                            new HudPanelAction{ Action="fullscreen", Icon= "glyphicon-new-window", Position= "panel-btn-bottom", Label="Expand panel to full screen" },
                            new HudPanelAction{ Action="menu",       Icon= "glyphicon-menu-hamburger", Position= "panel-top-left" },
                        }
                    },
                    new HudPanel{ Role=null },
                    new HudPanel{
                        Role =2, Style="col-md-3 onethird-height top-border right-border",
                        Actions =new List<HudPanelAction>
                        {
                            new HudPanelAction{ Action="swap", Target=0, Icon= "glyphicon-triangle-top", Position= "panel-btn-top" },
                            new HudPanelAction{ Action="expand", Target=3, Icon= "glyphicon-triangle-right", Position= "panel-btn-right" },
                            new HudPanelAction{ Action="menu",       Icon= "glyphicon-menu-hamburger", Position= "panel-top-left" },
                        }
                    },
                    new HudPanel{
                        Role =3, Style="col-md-3 onethird-height top-border right-border",
                        Actions =new List<HudPanelAction>
                        {
                            new HudPanelAction{ Action="expand", Target=2, Icon= "glyphicon-triangle-left", Position= "panel-btn-left" },
                            new HudPanelAction{ Action="expand", Target=4, Icon= "glyphicon-triangle-right", Position= "panel-btn-right" },
                            new HudPanelAction{ Action="swap",   Target=0, Icon= "glyphicon-triangle-top", Position= "panel-btn-top" },
                            new HudPanelAction{ Action="menu",       Icon= "glyphicon-menu-hamburger", Position= "panel-top-left" },
                        }
                    },
                    new HudPanel{
                        Role =4, Style="col-md-3 onethird-height top-border right-border",
                        Actions =new List<HudPanelAction>
                        {
                            new HudPanelAction{ Action="expand", Target=3, Icon= "glyphicon-triangle-right", Position= "panel-btn-right" },
                            new HudPanelAction{ Action="expand", Target=5, Icon= "glyphicon-triangle-left", Position= "panel-btn-left" },
                            new HudPanelAction{ Action="swap",   Target=1, Icon= "glyphicon-triangle-top", Position= "panel-btn-top" },
                            new HudPanelAction{ Action="menu",       Icon= "glyphicon-menu-hamburger", Position= "panel-top-left" },
                        }
                    },
                    new HudPanel{
                        Role =5, Style="col-md-3 onethird-height top-border right-border",
                        Actions =new List<HudPanelAction>
                        {
                            new HudPanelAction{ Action="expand", Target=4, Icon= "glyphicon-triangle-left", Position= "panel-btn-left" },
                            new HudPanelAction{ Action="swap", Target=1, Icon= "glyphicon-triangle-top", Position= "panel-btn-top" },
                            new HudPanelAction{ Action="menu",       Icon= "glyphicon-menu-hamburger", Position= "panel-top-left" },
                        }
                    },
                }
            };
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

        /// <summary>
        /// load scripts
        /// </summary>
        /// <returns></returns>
        public List<string> GetScripts()
        {
            var pluginPath = Path.Combine(_env.WebRootPath, "plugins");
            var files = new List<string>();

            var pluginFolder = new DirectoryInfo(pluginPath);
            foreach (var folder in pluginFolder.GetDirectories())
            {
                var scriptsFolder = new DirectoryInfo(folder.FullName + "\\Scripts");
                if (!scriptsFolder.Exists) continue;

                var scriptOrderFile = $"{scriptsFolder}/scripts.txt";
                if (File.Exists(scriptOrderFile))
                {
                    var scripts = File.ReadAllLines(scriptOrderFile);
                    foreach (var script in scripts)
                    {
                        var pluginFilePath = scriptsFolder.FullName.Replace(_env.WebRootPath,"");
                        var scriptpath = $"{pluginFilePath}/{script}".Replace("\\", "/");
                        if (!files.Contains(scriptpath))
                            files.Add(scriptpath);
                    }
                }
                else
                {
                    foreach (var fileInfo in scriptsFolder.GetFiles())
                    {
                        var pluginFilePath = fileInfo.DirectoryName.Replace(pluginPath, "");
                        var scriptpath = $"/plugins{pluginFilePath}/{fileInfo.Name}".Replace("\\", "/");
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

        public HudModel GetLayoutModel(HudLayoutSummary summary)
        {
            HudModel layout = new HudModel()
            {
                Scripts = GetScripts(),
                Styles = GetStyles(),
            };
            return layout;
        }

        public HudPluginModel GetPluginModel(IHudPlugin plugin, string role)
        {
            HudPluginModel source = new HudPluginModel
            {
                MenuText = plugin.MenuText,
                IsMenuItem = plugin.IsMenuItem,
                PluginSourceName = plugin.Name,
                Html = HttpUtility.HtmlDecode(plugin.RenderHtml(role)),
                OnInit = HttpUtility.HtmlDecode(plugin.OnInit()),
                OnPanelMoved = HttpUtility.HtmlDecode(plugin.OnPanelMoved()),
            };
            return source;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plugin"></param>
        public HudPluginModel GetPluginModel(string pluginName, string role)
        {
            if (string.IsNullOrEmpty(pluginName))
                return new HudPluginModel();

            var plugin = Create(pluginName);

            if (plugin != null)
                return GetPluginModel(plugin, role);

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