using System;

namespace Quest.Common.Messages.Visual
{
    [Serializable]
    public class HeatmapUpdate: MessageBase
    {
        public Heatmap Item;
        public DateTime ValidFrom;
    }

    [Serializable]
    public class Heatmap
    {
        public int cols;
        public double lat;
        public double latBlocksize;
        public double lon;
        public double lonBlocksize;
        public byte[] map;
        public int rows;
        public string Name;
        public string Code;
    }

}