using Newtonsoft.Json;
using System.Collections.Generic;

namespace Quest.WebCore.Models
{
    public class HudLayoutSummary
    {
        [JsonProperty(propertyName: "plugins")]
        public List<string> Plugins { get; set; }

        [JsonProperty(propertyName: "format")]
        public int Format { get; set; }


    }
}