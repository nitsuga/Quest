﻿using System;
using GeoAPI.Geometries;
using System.Collections.Generic;
using Quest.Common.Messages.Resource;

namespace Quest.Common.Simulation
{
    // temporary for renaming
    public class SimIncident
    {
        public ResourceStatus Status;
        public string AMPDS;
        public long IncidentId;
        public bool WillConvey;
        public Coordinate Position;
        //public double Easting;
        //public double Northing;
        public int? Category;
        public List<AssignedResource> AssignedResources;
        public int AmbAssigned;
        public int FruAssigned;
        public string FirstResponderArrival;
        public int FirstResponderArrivalDelay;
        public int AmbulanceArrivalDelay;
        public string AmbulanceArrival;
        public DateTime CallStart;
        public DateTime OnSceneTime;
        public int OnSceneDelay;
        public bool Incomplete;
        public string Location;
        public DateTime Date;
        public int TurnaroundTime;
    }

}
