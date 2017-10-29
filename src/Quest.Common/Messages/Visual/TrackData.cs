using System.Collections.Generic;

namespace Quest.Common.Messages.Visual
{
    /// <summary>
    ///     Track data can be used to plot tracks and routes
    /// </summary>

    public class TrackData : VisualData
    {
        public List<VisualTrack> Tracks { get; set; }

        /// <summary>
        ///     represents a single track consisting of (typically) GPS points or waypoints
        /// </summary>
        
        public class VisualTrack
        {
            public List<VisualMotionVector> Vectors;
        }
    }


}