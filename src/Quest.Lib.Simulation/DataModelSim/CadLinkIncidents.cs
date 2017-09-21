using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class CadLinkIncidents
    {
        public int CadLinkId { get; set; }
        public string XmlData { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
