using System.Collections.Generic;

namespace Quest.WebCore.Models
{
    public class HudModel
    {
        /// <summary>
        /// scripts to load
        /// </summary>
        public List<string> Scripts;

        /// <summary>
        ///  styles to load
        /// </summary>
        public List<string> Styles;

        /// <summary>
        /// name of the layout to load
        /// </summary>
        public string Layout;
    }
}