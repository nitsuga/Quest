﻿using System;

namespace Quest.Common.Simulation
{
    public class AssignedResource : ICloneable
    {
        public string Callsign;
        public DateTime? Dispatched;
        public DateTime? Enroute;
        public DateTime? OnSceneTime;
        public DateTime? Convey;
        public DateTime? Hospital;
        public DateTime? Released;

        public object Clone()
        {
            AssignedResource i = new AssignedResource()
            {
                Convey = Convey,
                Dispatched = Dispatched,
                Enroute = Enroute,
                Hospital = Hospital,
                OnSceneTime = OnSceneTime,
                Released = Released,
                Callsign = Callsign
            };
            return i;
        }
    }

}
