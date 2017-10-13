using System;
using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    public class Login:MessageBase
    {
        public string Callsign;
        public DateTime TimeStamp;
    }

}
