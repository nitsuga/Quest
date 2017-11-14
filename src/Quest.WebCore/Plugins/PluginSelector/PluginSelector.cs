using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Quest.Lib.DependencyInjection;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;
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
    public class PluginSelector : StandardPlugin
    {
        public PluginSelector(ILifetimeScope scope, IHostingEnvironment env)
            : base("PluginSelector", "MENU", "hud.plugins.pluginSelector.init(panelId, pluginId)", string.Empty, scope, env)
        {
        }

        public override string DrawContainer()
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