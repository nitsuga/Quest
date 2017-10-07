using System;

namespace Quest.Common.Messages
{


    /// <summary>
    /// A resource update has been received from CAD - this is a full update
    /// </summary>
    [Serializable]
    public class ResourceEtaUpdate : MessageBase
    {
        public string Callsign;
        public DateTime Eta;

        public override string ToString()
        {
            return $"ResourceEtaUpdate {Callsign} Eta={Eta}";
        }
    }
}