using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     This request informs the Quest server that the device has begun navigating to
    ///     a destination.
    /// </summary>
    [Serializable]
    
    public class NavigateRequest : Request
    {
        
        private LocationVector CurrentVector { get; set; }

        /// <summary>
        ///     the name the destination the device is navigating to
        /// </summary>
        
        public string LocationName { get; set; }

        /// <summary>
        ///     the code the destination the device is navigating to if known
        /// </summary>
        
        public string LocationCode { get; set; }

        /// <summary>
        ///     how long in seconds the device estimates its arrival
        /// </summary>
        
        public int EstimatedDuration { get; set; }

        /// <summary>
        ///     destination coordinates of the destination
        /// </summary>
        
        public double Longitude { get; set; }

        /// <summary>
        ///     destination coordinates of the destination
        /// </summary>
        
        public double Latitude { get; set; }

        public override string ToString()
        {
            return string.Format("NavigateRequest ", AuthToken);
        }
    }

    
}