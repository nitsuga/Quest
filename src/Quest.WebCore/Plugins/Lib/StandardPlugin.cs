using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.WebCore.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Quest.WebCore.Plugins.Lib
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    public class StandardPlugin : IHudPlugin
    {
        internal ILifetimeScope _scope;
        private IHostingEnvironment _env;

        public StandardPlugin(string name, string menu, string initScript, string folder, ILifetimeScope scope, IHostingEnvironment env)
        {
            Folder = folder;
            InitScript = initScript;
            Name = name;
            MenuText = menu;
            _scope = scope;
            _env = env;
            Properties = new Dictionary<string, object>();
        }

        public Dictionary<string, object> Properties { get; set; }

        public virtual string InitScript { get; set; }

        public virtual string Name { get; set; }

        public virtual string Folder { get; set; }

        public virtual string MenuText { get; set; }

        public virtual bool IsMenuItem { get; set; } = true;

        public virtual string RenderHtml()
        {
            return DrawContainer();
        }

        public virtual string OnInit()
        {
            return InitScript;
        }

        public virtual void InitializeWithProperties(Dictionary<string, object> properties)
        {
            // Do nothing
        }

        public virtual void InitializeWithDefaultProperties()
        {
            // Do nothing
        }

        public virtual string DrawContainer()
        {
            if (Folder==string.Empty)
                return $"<div></div>";

            const string templateFileName = "index.html";
            var templateFolder = _env.WebRootPath + Folder;
            var html = File.ReadAllText($"{templateFolder}/{templateFileName}");
            return html;
        }
    }
}
