using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.MapMatching;
using Quest.Lib.Research.Model;
using Quest.Lib.Utils;
using Quest.Common.Messages;

namespace Quest.Lib.Research.Utils
{
    public class IncidentTracks : ITrackProvider
    {
        /// <summary>
        /// Get a track from the database for a specific RouteId. 
        /// <param name="id"></param>
        /// <param>
        ///     <name>minSeconds</name>
        /// </param>
        /// <returns></returns>
        /// </summary>
        public Track GetTrack(string urn, int skip = 0)
        {
            int id = int.Parse(urn);
            using (var context = new QuestResearchEntities())
            {
                context.Database.CommandTimeout = 36000;
                context.Configuration.ProxyCreationEnabled = false;

                var routeinfo = context.IncidentRouteViews.FirstOrDefault(x => x.IncidentRouteID == id);

                var fixes = context.Avls
                    .AsNoTracking()
                    .Where(x => x.IncidentId == routeinfo.IncidentId)
                    .Where(x => x.Callsign.Trim() == routeinfo.Callsign.Trim())
                    .Where(x => x.Process)                  // Process flag must be set
                    .OrderBy(x => x.AvlsDateTime)
                    .Skip(skip)
                    .ToList();

                if (routeinfo != null)
                {
                    var track = MakeTrack(routeinfo.IncidentId ?? 0, routeinfo.Callsign.Trim(), fixes, routeinfo.VehicleId);
                    return track;
                }
            }

            return null;
        }

        /// <summary>
        /// Get tracks associated with the incident
        /// </summary>
        /// <param name="incident"></param>
        /// <returns></returns>
        public List<String> GetTracks(string urn)
        {
            long incident = long.Parse(urn);
            var tracks = new List<String>();

            using (var context = new QuestResearchEntities())
            {
                context.Database.CommandTimeout = 36000;
                context.Configuration.ProxyCreationEnabled = false;

                var routeinfo = context.IncidentRouteViews.Where(x => x.IncidentId == incident);
                foreach (var c in routeinfo)
                {
                    var track = $"db.inc:{c.IncidentRouteID}";
                    tracks.Add(track);
                }
            }


            return tracks;
        }

        private static Track MakeTrack(long incidentId, string callsign, List<Avl> fixes, int? vehicleType)
        {
            // remove bad coordinates
            List<Avl> fixesfiltered = fixes.Where(x => x.LocationX > -7 && (x.LocationX != -1 && x.LocationY != -1)).ToList();


            var track = new Track
            { 
                Incident = incidentId,
                Callsign = callsign,
                Fixes = MakeFixes(fixesfiltered),
                VehicleType = vehicleType ?? 1
            };

            for (int i = 0; i < track.Fixes.Count; i++)
                track.Fixes[i].Sequence = i;

            return track;
        }

        private static List<Fix> MakeFixes(IEnumerable<Avl> fixes)
        {
            return fixes.OrderBy(x => x.AvlsDateTime).Select(x =>
                new Fix()
                {
                    EstimatedSpeedMph = null,
                    Id = x.RawAvlsId,
                    Direction = x.Direction ?? 0,
                    Speed = x.Speed ?? 0,
                    Timestamp = x.AvlsDateTime ?? DateTime.MinValue,
                    Position = GeomUtils.ConvertToCoordinate((double)(x.LocationY ?? 0), (double)(x.LocationX ?? 0))
                }).ToList();
        }

    }
}
