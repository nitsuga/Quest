using System;
using System.Web;
using Quest.WebCore.Interfaces;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Quest.WebCore.Models
{
    public static class CookieProxy
    {
        /// <summary>
        /// This method looks for the layout cookie and, if it exists, converts it a fully formed HudLayout object
        /// TODO: Need to consider state of the plugins
        /// </summary>
        /// <returns></returns>
        public static HudLayoutSummary GetSelectedPluginLayout(HttpRequest httpRequest, HttpResponse httpResponse)
        {
            var cookie = httpRequest.Cookies[CookieConstants.PluginLayout];

            // If the cookie doesn't exist, we need to create a new instance of an empty layout and set the cookie
            HudLayoutSummary layout = null;
            if (cookie == null)
            {
                layout = new HudLayoutSummary { Format = 0, Plugins = new List<string> { "PluginSelector" } };
                var value = JsonConvert.SerializeObject(layout);
                httpResponse.Cookies.Append(CookieConstants.PluginLayout, value);
            }
            else
                layout = JsonConvert.DeserializeObject<HudLayoutSummary>(cookie);

            return layout;
        }

    }

    public class CookieConstants
    {
        public const string PluginLayout = "HudPluginLayout";
    }
}