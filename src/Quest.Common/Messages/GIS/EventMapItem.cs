using System;

namespace Quest.Common.Messages.GIS
{

    /// <summary>
    ///     an item listed in a nearby search.
    /// </summary>
    [Serializable]
    public class EventMapItem: PointMapItem
    {
        public string EventId;
        public string Notes;
        public string Priority;
        public string Status;
        public int AssignedResources;
        public string PatientAge;
        public string Location;
        public string LocationComment;
        public string ProblemDescription;
        public string DeterminantDescription;
        public string Determinant;
        public string PatientSex;
    }
}