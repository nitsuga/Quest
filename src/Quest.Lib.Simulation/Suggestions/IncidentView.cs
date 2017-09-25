////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2014 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Linq;
using Quest.Lib.Suggestions;

namespace Quest.Lib
{
    public partial class IncidentView : System.EventArgs, ICloneable
    {
        public AssignedResource[] AssignedResources { get; set; }

        public object Clone()
        {
            IncidentView i = new IncidentView()
            {
                //TODO: fill in right bits to clone
#if false
                AMBRequired = this.AMBRequired,
                AmbulanceArrivalDelay = this.AmbulanceArrivalDelay,
                AmbulanceArrivalId = this.AmbulanceArrivalId,
                AMPDS = this.AMPDS,
                AtHospitalTime = this.AtHospitalTime,
                CallStart = this.CallStart,
                Category = this.Category,
                Easting = this.Easting,
                FirstResponderArrivalDelay = this.FirstResponderArrivalDelay,
                FirstResponderArrivalId = this.FirstResponderArrivalId,
                FRURequired = this.FRURequired,
                HospitalResourceId = this.HospitalResourceId,
                IncidentId = this.IncidentId,
                LeftHospitalTime = this.LeftHospitalTime,
                Location = this.Location,
                Northing = this.Northing,
                OnSceneDelay = this.OnSceneDelay,
                OnSceneTime = this.OnSceneTime,
                OnSceneTime1 = this.OnSceneTime1,
                Status = this.Status,
                TotalRequired = this.TotalRequired,
                TurnaroundTime = this.TurnaroundTime,
                WillConvey = this.WillConvey
#endif
            };

            if (AssignedResources != null)
                i.AssignedResources = (from x in this.AssignedResources select (AssignedResource)x.Clone()).ToArray();

            return i;
        }

        public override string ToString()
        {
            return String.Format("{0}/{1}/{2}", Incidentid , Category, Status );
        }
    }
}
