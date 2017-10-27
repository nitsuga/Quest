using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Telephony
{
    [Serializable]
    public class CallDisconnectStatusList : Request
    {
        /// <summary>
        ///     The event Id / CAD number
        /// </summary>
        public List<CallDisconnectStatus> Items { get; set; }

        public override string ToString()
        {
            if (Items != null)
                return $"Call Disconnect Status List count = {Items.Count}";
            return "Call Disconnect Status List Empty";
        }
    }
}
