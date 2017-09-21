using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class Coverage
    {
        public int CoverageId { get; set; }
        public int VehicleTypeId { get; set; }
        public int DestinationId { get; set; }
        public byte[] CoverageMap { get; set; }
        public string Name { get; set; }
        public long OffsetX { get; set; }
        public long OffsetY { get; set; }
        public long BlockSize { get; set; }
        public long Rows { get; set; }
        public long Columns { get; set; }
    }
}
