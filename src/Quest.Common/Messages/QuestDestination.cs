using System;

namespace Quest.Common.Messages
{
    /// <summary>
    /// A single destination
    /// </summary>
    [Serializable]    
    public class QuestDestination: PointMapItem
    {
        public string Name;
        public string Group;
        public bool IsEnabled;
        public bool IsHospital;
        public bool IsAandE;
        public bool IsRoad;
        public bool IsStation;
        public bool IsStandby;
    }
}