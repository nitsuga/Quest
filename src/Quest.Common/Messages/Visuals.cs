using System;
using System.Collections.Generic;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;

namespace Quest.Common.Messages
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

    /// <summary>
    ///     result - subclassed from VisualsSearchResult
    /// </summary>
    
    public class GetVisualsCatalogueResponse : Response
    {
        
        public List<Visual> Items { get; set; }
    }


    
    public class GetVisualsDataRequest : Request
    {
        
        public List<string> Ids;
    }

    
    public class QueryVisualRequest : Request
    {
        
        public string Provider;
        
        public string Query;
    }

    
    public class QueryVisualResponse : Response
    {
        /// <summary>
        /// list of visuals
        /// </summary>
        
        public List<Visual> Visuals { get; set; }
    }

    
    public class GetVisualsDataResponse : Response
    {
        /// <summary>
        /// list of visuals
        /// </summary>
        
        public FeatureCollection Geometry { get; set; }
    }


    
    public class Visual
    {
        
        public VisualId Id { get; set; }

        
        public List<TimelineData> Timeline { get; set; }

        /// <summary>
        /// This item as a feature
        /// </summary>
        
        [JsonIgnore]
        public FeatureCollection Geometry;
    }

    
    public class VisualPersistList
    {
        
        public List<VisualPersist> Visuals;
    }

    
    public class VisualPersist
    {
        public VisualPersist()
        {
        }

        
        public VisualId Id { get; set; }
        
        public byte[] Timeline { get; set; }
        
        public string GeoJson;
    }

    public class VisualId
    {
        
        public string Source { get; set; }
        
        public string Name { get; set; }
        
        public string Id { get; set; }
        
        public string VisualType { get; set; }

        public override string ToString()
        {
            return $"{Source} {Name} {Id} {VisualType}";
        }
    }


    /// <summary>
    /// Data for a single item on the timeline
    /// </summary>
    
    public class TimelineData
    {
        public TimelineData()
        {
        }

        public override string ToString()
        {
            return $"{Id} {Start} {End} {Label} {DisplayClass}";
        }

        public TimelineData(long id, DateTime? start, DateTime? end, string label, string displayClass = null)
        {
            Id = id;

            if (start != null) Start = string.Format("{0:ddd MMM dd yyyy hh:mm:ss}", start, TimeZoneInfo.Local.StandardName);
            if (end != null) End = string.Format("{0:ddd MMM dd yyyy hh:mm:ss}", end, TimeZoneInfo.Local.StandardName);

            //if (start != null) Start = string.Format("{0:ddd MMM dd yyyy hh:mm:ss \"GMT\"K} ({1})", start, TimeZoneInfo.Local.StandardName);
            //if (end != null) End = string.Format("{0:ddd MMM dd yyyy hh:mm:ss \"GMT\"K} ({1})", end, TimeZoneInfo.Local.StandardName);
            Label = label;
            DisplayClass = displayClass ?? label;
        }

        
        public long Id;

        /// <summary>
        /// start time
        /// </summary>
        
        public string Start;

        /// <summary>
        /// end time
        /// </summary>
        
        public string End;

        /// <summary>
        /// Display in the timeline box
        /// </summary>
        
        public string Label;

        /// <summary>
        /// display class
        /// </summary>
        
        public string DisplayClass;

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