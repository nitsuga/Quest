using System;

namespace Quest.Common.Messages.Device
{
    /// <summary>
    ///     in response to a navigation request the server will reply with some routing info and
    ///     estimates.
    /// </summary>
    [Serializable]
    
    public class NavigateResponse : Response
    {
        /// <summary>
        ///     current estimated road
        /// </summary>
        
        public string currentRoad { get; set; }

        /// <summary>
        ///     absolute bearing
        /// </summary>
        
        public double Bearing { get; set; }

        /// <summary>
        ///     computed road distance in meters
        /// </summary>
        
        public double Distance { get; set; }

        /// <summary>
        ///     computed duration in seconds
        /// </summary>
        
        public int EstimatedDuration { get; set; }
    }

    
}