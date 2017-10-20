using Quest.WebCore.Interfaces;
using System.Collections.Generic;

namespace Quest.WebCore.Models
{
    public class HudLayout
    {
        /// <summary>
        /// todo: 1=fullscreen 2= L-R2
        /// </summary>
        public int Format;

        public List<string> Panels { get; set; }
        
        public List<string> Scripts;

        public List<string> Styles;
    }
}