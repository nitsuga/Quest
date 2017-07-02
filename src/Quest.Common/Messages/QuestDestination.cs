using GeoAPI.Geometries;
using System;

namespace Quest.Common.Messages
{
    /// <summary>
    /// A single destination
    /// </summary>
    [Serializable]    
    public class QuestDestination
    {
        public int DestinationId;
        public long revision;
        public Coordinate Position { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsHospital { get; set; }
        public bool IsAandE { get; set; }
        public bool IsRoad { get; set; }
        public bool IsStation { get; set; }
        public bool IsStandby { get; set; }
    }
}