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

    }
}