using System;
using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    public class Logout : MessageBase
    {
        public int ResourceId;
        public DateTime TimeStamp;
    }

}
