using System.Collections.Generic;

namespace Quest.WebCore.Models
{
    /// <summary>
    /// Panel definition
    /// </summary>
    public class HudPanel
    {
        /// <summary>
        /// window number or null for row break
        /// </summary>
        public int? Panel;

        /// <summary>
        /// name of plugin for this window
        /// </summary>
        public string Plugin;

        /// <summary>
        /// bootstrap control class for normal, unexpanded, style
        /// </summary>
        public string Style;

        /// <summary>
        /// expanded style to use
        /// </summary>
        public string ExpandedStyle;

        /// <summary>
        /// list of actions on the panel
        /// </summary>
        public List<HudPanelAction> Actions=new List<HudPanelAction>();
    }
}