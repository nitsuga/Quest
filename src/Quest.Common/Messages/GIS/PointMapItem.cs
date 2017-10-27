using System;

namespace Quest.Common.Messages.GIS
{
    /// <summary>
    ///     an item listed in a nearby search.
    /// </summary>
    [Serializable]
    public class PointMapItem
    {
        public long revision;
        public string ID;
        public double Y;
        public double X;
    }
}