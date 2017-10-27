using System;

namespace Quest.Common.Messages.Telephony
{

    [Serializable]
    public class CallDisconnectStatus
    {
        /// <summary>
        ///     The event Id / CAD number
        /// </summary>
        public string Serial { get; set; }

        public DateTime DisconnectTime { get; set; }

        public override string ToString()
        {
            return $"Call Disconnect Status {Serial} @ {DisconnectTime}";
        }
    }
}
