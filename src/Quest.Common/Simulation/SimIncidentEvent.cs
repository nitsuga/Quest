using System;
using Quest.Common.Messages;

namespace Quest.Common.Simulation
{

    /// <summary>
    /// Post a new timed event request
    /// </summary>
    [Serializable]
    public class SimIncidentEvent : Request
    {
        public enum UpdateTypes
        {
            NewIncident,
            CallConnect,
            LocationFound,
            Determinant,
            Other,
        }

        public UpdateTypes UpdateType;

        public DateTime UpdateTime;

        public long IncidentId;

        public string Category;

        public string Determinant;

        public double Latitude;

        public int? Longitude;

    }

}
