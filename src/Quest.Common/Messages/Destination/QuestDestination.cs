using Quest.Common.Messages.GIS;
using System;

namespace Quest.Common.Messages.Destination
{
    /// <summary>
    /// A single destination
    /// </summary>
    [Serializable]    
    public class QuestDestination
    {
        public string Id;

        public string Name;

        public string Code;

        public bool IsEnabled;

        public bool IsHospital;

        public bool IsAandE;

        public bool IsRoad;

        public bool IsStation;

        public bool IsStandby;

        public string Status;

        public LatLng Position;
    }
}