using System;
using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    public class Logout : MessageBase
    {
        public string Callsign;
        public DateTime TimeStamp;
    }

}
