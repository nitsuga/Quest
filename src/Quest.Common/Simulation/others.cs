
using Quest.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Common.Simulation
{
    [Serializable]
    public class SimIncidentUpdate : Request
    {
        public enum UpdateTypes
        {
            CallStart,
            AMPDS,
        }

        public long IncidentId;
        public DateTime CallStart;
        public DateTime AMPDSTime;
        public int? Easting;
        public int? Northing;
        public string AMPDSCode;
        public int? Category;
        public bool? WasConveyed;
        public bool? WasDispatched;
        public bool? OutsideLAS;

        public DateTime UpdateTime;
        public UpdateTypes UpdateType;

        public override string ToString()
        {
            return $"IncidentUpdate {IncidentId}";
        }
    }


    public class AssignVehicle
    {
        public long IncidentId;
        public int ResourceId;
    }
    public class CancelVehicle
    {
        public long IncidentId;
        public int ResourceId;
    }
}
