using System;
using System.Collections.Generic;
using System.Text;

namespace Quest.Common.Messages.Resource
{
    /// <summary>
    /// assign a callsign to a vehicle
    /// </summary>
    public class AssignCallsign : MessageBase
    {
        public string Callsign;
        public string FleetNo;
    }
}
