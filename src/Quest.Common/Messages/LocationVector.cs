using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     Contains information relating to a devices position, speed and direction.
    /// </summary>
    [Serializable]
    
    public class LocationVector
    {        
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double VDoP { get; set; }
        public double HDoP { get; set; }
        public double AltitudeM { get; set; }
        public double BearingDeg { get; set; }
        public double SpeedMS { get; set; }
        public string CaptureMethod { get; set; } // e.g. GPS/Wireless etc

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

    
}