using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.CAD
{
    [Serializable]
    public class CadEventStatusList : Request
    {
        /// <summary>
        ///     The event Id / CAD number
        /// </summary>
        public List<CadEventStatus> Items { get; set; }

        public override string ToString()
        {
            if (Items != null)
                return $"Event Status List count = {Items.Count} ";
            return "Event Status List Empty";
        }
    }
}