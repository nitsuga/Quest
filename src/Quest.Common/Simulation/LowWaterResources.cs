using Quest.Common.Messages;
using System;

namespace Quest.Common.Simulation
{
    [Serializable]
    public class LowWaterResources : MessageBase
    {
        public LowWaterResources()
            {
            }

        public DateTime lastId;
    }
}