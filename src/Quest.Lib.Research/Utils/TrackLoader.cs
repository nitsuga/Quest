using System;
using System.Collections.Generic;
using Quest.Lib.MapMatching;
using Autofac;
using Quest.Lib.DependencyInjection;

namespace Quest.Lib.Research.Utils
{
    [Injection]
    public class TrackLoader
    {
        private ILifetimeScope _scope;

        public TrackLoader(ILifetimeScope scope)
        {
            _scope = scope;
        }

        /// <summary>
        /// Get a track from the database for a specific RouteId. 
        /// <param name="incidentRouteId"></param>
        /// <param>
        ///     <name>minSeconds</name>
        /// </param>
        /// <returns></returns>
        /// </summary>
        public Track GetTrack(string uri, int skip = 0)
        {
            Uri uri1 = new Uri(uri);
            var provider = _scope.ResolveNamed<ITrackProvider>(uri1.Scheme);
            return provider?.GetTrack(uri1.AbsolutePath, skip);
        }

        public List<Track> GetTracks(string uri)
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
        public List<String> GetTrackUris(string uri)
        {
            Uri uri1 = new Uri(uri);
            var provider = _scope.ResolveNamed<ITrackProvider>(uri1.Scheme);
            return provider?.GetTracks(uri1.AbsolutePath);
        }
    }
}
