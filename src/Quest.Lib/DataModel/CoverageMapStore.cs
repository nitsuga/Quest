using System;

namespace Quest.Lib.DataModel
{
    public partial class CoverageMapStore
    {
        public int CoverageMapStoreId { get; set; }
        public string Name { get; set; }
        public byte[] Data { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int Blocksize { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public DateTime Tstamp { get; set; }
        public double? Percent { get; set; }
    }
}
