using System;

namespace Quest.Common.Messages.Resource
{
    [Serializable]
    public class ResourceStatusChange : MessageBase
    {
        public string Callsign;
        public string FleetNo;
        public string OldStatus;
        public string NewStatus;
        public string OldStatusCategory;
        public string NewStatusCategory;
    }
}