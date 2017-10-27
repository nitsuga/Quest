using System;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class BBFilter
    {
        public double br_lat;
        public double br_lon;
        public double tl_lat;
        public double tl_lon;
    }
}