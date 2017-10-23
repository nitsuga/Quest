using Autofac;
using Microsoft.AspNetCore.Mvc.Rendering;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;

namespace Quest.WebCore.Plugins.LayoutSelector
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available layouts
    /// </summary>
    [Injection("LayoutSelector", typeof(IHudPlugin), Lifetime.PerDependency)]
    public class LayoutSelector : IHudPlugin
    {
        ILifetimeScope _scope;
        IPluginService _pluginService;

        public LayoutSelector(ILifetimeScope scope, IPluginService pluginService)
        {
            _scope = scope;
            _pluginService = pluginService;
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// The name of the plugin
        /// </summary>
        public string Name => "LayoutSelector"; // <-- must be the same as the injected Name

        public Dictionary<string, object> Properties { get; set; }

        public string MenuText => "LAYOUT";

        public bool IsMenuItem => true;

        /// <summary>
        /// A method call to render the Html for the Main Frame of a layout
        /// </summary>
        /// <returns></returns>
        public string RenderHtml(string role)
        {
            return DrawSelectorGrid(role);
        }

        /// <summary>
        /// A single javascript command that will kick off any front-end initialization
        /// </summary>
        /// <returns></returns>
        public string OnInit()
        {
            return "hud.plugins.layoutSelector.initialize()";
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


        private string DrawSelectorGrid(string role)
        {
            var div = new TagBuilder("div");
            div.MergeAttribute("id", "layoutSelector");
            div.MergeAttribute("data-role", "layout-selector");
            div.AddCssClass("layout-selection-grid");

            var allLayouts = _pluginService
                .GetLayouts()
                .Where(x=>x.Selectable==true)
                .ToList();

            if (allLayouts.Count == 0)
            {
                var h3 = new TagBuilder("h3");
                h3.InnerHtml.Append("No layouts detected");
                div.InnerHtml.AppendHtml(h3);
            }
            else
            {
                var h3 = new TagBuilder("h3");
                for (var r = 0; r < 3; r++)
                {
                    var row = new TagBuilder("div");
                    row.AddCssClass("row");

                    for (var c = 0; c < 3; c++)
                    {
                        if (r * 3 + c > allLayouts.Count - 1) continue;

                        var p = allLayouts[r * 3 + c];

                        var col = new TagBuilder("div");
                        col.AddCssClass("col-sm-4");

                        var btn = new TagBuilder("button");
                        btn.AddCssClass("btn");
                        btn.AddCssClass("btn-default");
                        btn.AddCssClass("btn-layout-selector");
                        btn.MergeAttribute("data-role", "layout-selector");
                        btn.MergeAttribute("data-layout-name", p.Name);
                        btn.InnerHtml.Append(p.Name);

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