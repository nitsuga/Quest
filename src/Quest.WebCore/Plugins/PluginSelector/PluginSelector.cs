using Autofac;
using Microsoft.AspNetCore.Mvc.Rendering;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;

namespace Quest.WebCore.Plugins.PluginSelector
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("PluginSelector", typeof(IHudPlugin), Lifetime.PerDependency)]
    public class PluginSelector : IHudPlugin
    {
        ILifetimeScope _scope;
        public PluginSelector(ILifetimeScope scope)
        {
            _scope = scope;
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// The name of the plugin
        /// </summary>
        public string Name => "PluginSelector"; // <-- must be the same as the injected Name

        public Dictionary<string, object> Properties { get; set; }

        public string MenuText => "MENU";

        public bool IsMenuItem => false;

        /// <summary>
        /// A method call to render the Html for the Main Frame of a layout
        /// </summary>
        /// <returns></returns>
        public string RenderHtml()
        {
            return DrawContainer();
        }

        /// <summary>
        /// A single javascript command that will kick off any front-end initialization
        /// </summary>
        /// <returns></returns>
        public string OnInit()
        {
            return "hud.plugins.pluginSelector.init(panelId, pluginId)";
        }

        /// <summary>
        /// A single javascript command that will be executed when the plugin has been moved to a new container
        /// </summary>
        /// <returns></returns>
        public string OnPanelMoved()
        {
            return string.Empty;
        }

        public void InitializeWithProperties(Dictionary<string, object> properties = null)
        {
            // Do nothing - no additional properties are required for this plugin
        }

        public void InitializeWithDefaultProperties()
        {
            // Do nothing - no additional properties are required for this plugin
        }

        /// <summary>
        /// When called, this method checks the dictioary of properties and confirms that all the properties required by this plugin
        /// have been set.
        /// </summary>
        public bool ValidateProperties()
        {
            // This plugin required no additional properties - always return true
            return true;
        }


        private string DrawContainer()
        {
            var div = new TagBuilder("div");
            div.MergeAttribute("id", "pluginSelector");
            div.MergeAttribute("data-role", "plugin-selector");
            div.AddCssClass("plugin-selection-grid");

            var pluginList = _scope.Resolve<IEnumerable<IHudPlugin>>();

            var allPlugins = pluginList.Where(x=>x.IsMenuItem).OrderBy(x=>x.Name).ToList();

            if (allPlugins.Count == 0)
            {
                var h3 = new TagBuilder("h3");
                h3.InnerHtml.Append("No plugins detected");
                div.InnerHtml.AppendHtml(h3);
            }
            else
            {
                var h3 = new TagBuilder("h3");

                for (var r = 0; r < (allPlugins.Count/3)+1; r++)
                {
                    var row = new TagBuilder("div");
                    row.AddCssClass("row");

                    for (var c = 0; c < 3; c++)
                    {
                        if (r * 3 + c > allPlugins.Count - 1) continue;

                        var p = allPlugins[r * 3 + c];

                        var col = new TagBuilder("div");
                        col.AddCssClass("col-sm-4");

                        var btn = new TagBuilder("button");
                        btn.AddCssClass("btn");
                        btn.AddCssClass("btn-default");
                        btn.AddCssClass("btn-plugin-selector");
                        btn.MergeAttribute("data-role", "plugin-selector");
                        btn.MergeAttribute("data-plugin-name", p.Name);
                        btn.InnerHtml.Append(p.MenuText);

                        // Add the button to the cell
                        col.InnerHtml.AppendHtml(btn);

                        // Add the cell to the row
                        row.InnerHtml.AppendHtml(col);
                    }

                    // Add the row to the div
                    div.InnerHtml.AppendHtml(row);
                }
            }

            using (var writer = new StringWriter())
            {
                div.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }
    }
}