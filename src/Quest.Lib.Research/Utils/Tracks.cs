using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.MapMatching;
using Quest.Lib.Research.Model;
using Quest.Lib.Utils;
using Quest.Common.Messages;

namespace Quest.Lib.Research.Utils
{
    public class Tracks
    {
        /// <summary>
        /// Get a track from the database for a specific RouteId. 
        /// <param name="incidentRouteId"></param>
        /// <param>
        ///     <name>minSeconds</name>
        /// </param>
        /// <returns></returns>
        /// </summary>
        public static Track GetTrack(string uri, int skip = 0)
        {
            Uri uri1 = new Uri(uri);

            switch (uri1.Scheme)
            {
                case "file.kml":
                    KmlTracks ik = new KmlTracks();
                    return ik.GetTrack(uri1.AbsolutePath, skip);
                case "db.inc":
                    IncidentTracks it = new IncidentTracks();
                    return it.GetTrack(uri1.AbsolutePath, skip);
            }
            return null;
        }

        public static List<Track> GetTracks(string uri)
        {
            List<Track> tracks = new List<Track>();
            List<string> uris= GetTrackUris(uri);
            foreach (var u in uris)
                tracks.Add(GetTrack(u));
            return tracks;
        }

        /// <summary>
        /// Get track uris associated with the incident
        /// </summary>
        /// <param name="incident"></param>
        /// <returns></returns>
        public static List<String> GetTrackUris(string uri)
        {
            Uri uri1 = new Uri(uri);

            switch (uri1.Scheme)
            {
                case "file.kml":
                    KmlTracks ik = new KmlTracks();
                    return ik.GetTracks(uri1.AbsolutePath);
                case "db.inc":
                    IncidentTracks it = new IncidentTracks();
                    return it.GetTracks(uri1.AbsolutePath);
            }
            return null;
        }

    }
}
