using Quest.WebCore.Interfaces;
using System.Collections.Generic;

namespace Quest.WebCore.Models
{
    /// <summary>
    /// A layout definition containing panels
    /// </summary>
    public class HudLayout
    {
        /// <summary>
        /// name of the layout
        /// </summary>
        public string Name;

        /// <summary>
        /// image representing the layout
        /// </summary>
        public string ImagePath;

        /// <summary>
        /// list of panel defintions
        /// </summary>
        public List<HudPanel> Panels { get; set; }
    }
}