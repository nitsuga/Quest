using System;

namespace Quest.Common.Messages
{
    [Serializable]
    
    public class CallsignNotification : IDeviceNotification
    {
        
        public string Callsign { get; set; }

        public override string ToString()
        {
            return $"CallsignNotification: {Callsign}";
        }
    }
    
}