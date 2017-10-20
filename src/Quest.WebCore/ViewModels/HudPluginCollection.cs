using System.Collections.Generic;

namespace Hud.ViewModels
{
    public class HudPluginCollection
    {
        public List<string> Plugins { get; set; }

        public HudPluginCollection()
        {
            Plugins = new List<string>();
        }
    }
}