﻿using System;

namespace Quest.Common.Messages.GIS
{
    /// <summary>
    ///     Contains information relating to a devices position, speed and direction.
    /// </summary>
    [Serializable]
    public class LocationVector
    {
        public LatLongCoord Coord { get; set; }
        public double VDoP { get; set; }
        public double HDoP { get; set; }
        public double Altitude { get; set; }
        public double Course { get; set; }
        public double Speed { get; set; }
        public string CaptureMethod { get; set; } // e.g. GPS/Wireless etc

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

    [Serializable]
    public class LatLongCoord
    {
        public LatLongCoord()
        {
        }

        public LatLongCoord(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}