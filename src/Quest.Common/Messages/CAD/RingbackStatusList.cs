using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.CAD
{
    [Serializable]
    public class RingbackStatusList
    {
        /// <summary>
        ///     The event Id / CAD number
        /// </summary>
        public List<RingbackStatus> Items { get; set; }

        public override string ToString()
        {
            if (Items != null)
                return $"RingbackStatus List count = {Items.Count} ";
            return "RingbackStatus List Empty";
        }
    }
}