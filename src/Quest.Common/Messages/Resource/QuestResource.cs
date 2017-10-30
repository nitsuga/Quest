using GeoAPI.Geometries;
using Quest.Common.Messages.GIS;
using System;

namespace Quest.Common.Messages.Resource
{
    /// <summary>
    /// represents a resource within the system. 
    /// The resource has a unique key of Agency and Callsign
    /// </summary>
    public class QuestResource
    {
        /// <summary>
        /// global revision number
        /// </summary>
        public long? Revision;

        /// <summary>
        /// PK: callsign
        /// </summary>
        public string Callsign;

        /// <summary>
        /// specific type of resource
        /// </summary>
        public string ResourceType;

        /// <summary>
        /// resource type group
        /// </summary>
        public string ResourceTypeGroup;

        /// <summary>
        /// PK: which organisation owns this resource
        /// </summary>
        public string Agency;

        /// <summary>
        /// workflow status
        /// </summary>
        public string Status;

        /// <summary>
        /// status category e.g. available, busy etc
        /// </summary>
        public string StatusCategory;

        /// <summary>
        /// skill code of the crew
        /// </summary>
        public string Skill;

        /// <summary>
        /// valid as of
        /// </summary>
        public DateTime? LastUpdated;

        /// <summary>
        /// vehicle number if appropriate
        /// </summary>
        public string FleetNo;

        /// <summary>
        /// owning sector
        /// </summary>
        public string Sector;

        /// <summary>
        /// Event current Assigned to
        /// </summary>
        public string EventId;

        /// <summary>
        /// Type of event being worked on
        /// </summary>
        public string EventType;

        /// <summary>
        /// Eta of the vehicle to next destination
        /// </summary>
        public DateTime? Eta;

        /// <summary>
        /// user comments
        /// </summary>
        public string Comment;

        /// <summary>
        /// coordinates in lat/lon
        /// </summary>
        public LatLongCoord Position;

        /// <summary>
        /// destination e.g. standby, hospital, station etc
        /// </summary>
        public String Destination;

        public Coordinate DestPosition;

        /// <summary>
        /// Speed in m/s
        /// </summary>
        public float? Speed;

        /// <summary>
        /// direction of travel degrees
        /// </summary>
        public float? Course;

        /// <summary>
        /// Horizontal Dilution of precision
        /// </summary>
        public float? HDoP;


    }
}
