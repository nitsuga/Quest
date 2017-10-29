using System;

namespace Quest.Common.Messages.Visual
{
    [Serializable]
    public class Heatmap
    {
        public int cols;
        public double lat;
        public double latBlocksize;
        public double lon;
        public double lonBlocksize;
        //public string map;
        public byte[] map;
        public int rows;
        public int vehtype;
    }

}