using System;

namespace Quest.Common.Messages.Visual
{
    /// <summary>
    ///     request a catalogue of visuals
    /// </summary>

    [Serializable]
    public class GetVisualsCatalogueRequest : Request
    {
        
        public DateTime DateFrom;
        
        public DateTime DateTo;
        
        public string Resource;
        
        public string Incident;
        
        public string[] Visuals;
    }

#if false

    /// <summary>
    ///     core visual data - just enough to get something onto a timeline/
    ///     subclass this for more capable visuals
    /// </summary>
    
    public abstract class VisualData
    {
        public int Id { get; set; }
    }

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

    /// <summary>
    ///     visulise a route
    /// </summary>
    
    public class RouteData : VisualData
    {
        public List<VisualRoute> Routes { get; set; }
    }

    /// <summary>
    /// </summary>
    
    public class ParticleData : VisualData
    {
        public List<VisualRoute> Particles { get; set; }
    }

    /// <summary>
    ///     a cloud of particles at a given time
    /// </summary>
    
    public class VisualParticleCloud
    {
        public DateTime Timestamp { get; set; }

        public List<VisualParticle> Particles { get; set; }

        /// <summary>
        ///     a single particle
        /// </summary>
        
        public class VisualParticle : VisualMotionVector
        {
            public double Probability { get; set; }
        }
    }

    /// <summary>
    ///     a motion vector at a given time
    /// </summary>
    
    public class VisualMotionVector
    {
        public double Speed { get; set; }

        public double Direction { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

    
    public class VisualRoute
    {
        public List<Segment> Segments;

        
        public class Segment
        {
            public string Wkt { get; set; }
            public string Roadname { get; set; }
            public int Speed { get; set; }
        }
    }
#endif

}