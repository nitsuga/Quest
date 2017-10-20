﻿using System;
using System.Web;
using Quest.WebCore.Interfaces;
using Newtonsoft.Json;

namespace Quest.WebCore.Models
{
    [JsonObject(MemberSerialization.OptOut)]
    public class HudPluginModel
    {
        /// <summary>
        /// The name of the plugin
        /// </summary>
        [JsonProperty(propertyName: "name")]
        public string PluginSourceName { get; set; } = string.Empty;

        [JsonProperty(propertyName: "menutext")]
        public string MenuText { get; set; } = string.Empty;

        [JsonProperty(propertyName: "ismenuitem")]
        public bool IsMenuItem { get; set; } = false;

        /// <summary>
        /// The Html for the large sized component
        /// </summary>
        public string Html { get; set; } = string.Empty;

        /// <summary>
        /// A single javascript call to be made when the DOM is loaded that will initialize the component
        /// </summary>
        public string OnInit { get; set; } = string.Empty;

        /// <summary>
        /// A single javascript command that will be executed when the plugin has been moved to a new container
        /// </summary>
        /// <returns></returns>
        public string OnPanelMoved { get; set; } = string.Empty;
    }
}