using System.Collections.Generic;

namespace Quest.Common.Messages.Visual
{
    public class QueryVisualResponse : Response
    {
        /// <summary>
        /// list of visuals
        /// </summary>
        
        public List<Visual> Visuals { get; set; }
    }
}