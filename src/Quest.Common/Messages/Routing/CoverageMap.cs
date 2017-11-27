using System;

namespace Quest.Common.Messages.Routing
{

    /// <summary>
    ///     provides a comprehensive coverage map. Units are generic
    ///     see Heatmap for WGS84 version
    /// </summary>
    [Serializable]
    public class CoverageMap
    {
        public int Blocksize;

        public int Columns;

        public byte[] Data;

        public double Percent;

        public int Rows;

        public string Name { get; set; }

        public string Code { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        public CoverageMap()
        {
        }

        public CoverageMap(string Name, string Code)
        {
            this.Name = Name;
            this.Code = Code;
        }

    }
}