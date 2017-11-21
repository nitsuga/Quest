using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.Utils;
using System.IO;
using SharpKml.Dom;
using SharpKml.Engine;
using SharpKml.Dom.GX;
using Quest.Lib.DependencyInjection;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.Research.Utils
{
    /// <summary>
    /// Read track from a file
    /// </summary>
    [Injection("kml:inc", typeof(ITrackProvider), Lifetime.Singleton)]
    public class KmlTracks : ITrackProvider
    {
        /// <summary>
        /// Get a track from the database for a specific RouteId. 
        /// <param name="id"></param>
        /// <param>
        ///     <name>minSeconds</name>
        /// </param>
        /// <returns></returns>
        /// </summary>
        public MapMatching.Track GetTrack(string urn, int skip = 0)
        {
            var track = BuildFromKml(urn);
            track.Fixes = track.Fixes.Skip(skip).ToList();
            return track;
        }

        public MapMatching.Track BuildFromKml(string filename)
        {
            var track = new MapMatching.Track
            {
                Incident = 0,
                Callsign = "",
                Fixes = new List<Fix>(),
                VehicleType = 1
            };

            using (var fs = File.Open(filename, FileMode.Open))
            {
                var file = SharpKml.Engine.KmlFile.Load(fs);
                Kml kml = file.Root as Kml;
                if (kml != null)
                {
                    foreach (var placemark in kml.Flatten().OfType<Placemark>())
                    {
                        Console.WriteLine(placemark.Name);
                        var geom = placemark.Geometry;
                        if (geom is MultipleTrack)
                        {
                            var t = (geom as MultipleTrack).Tracks.FirstOrDefault();
                            if (t!=null)
                            {
                                int seq = 0;
                                track.Fixes = t
                                    .Coordinates
                                    .Zip(t.When, (c, w) => new { c, w })
                                    .Select(x => new Fix
                                    {
                                        Position = GeomUtils.ConvertToCoordinate(x.c.Latitude, x.c.Longitude),
                                        Corrupt =  null,
                                        Sequence = seq++,
                                        Id = seq,
                                        Timestamp = x.w
                                    }).ToList();

                                var speed = t.ExtendedData.Flatten().OfType<SimpleArrayData>().FirstOrDefault(x => x.Name == "speed");
                                if (speed != null && speed.Values.Count() == track.Fixes.Count())
                                {
                                    var speed_array = speed.Values.ToList().Select(x => double.Parse(x==""?"0.1":x)).ToArray();
                                    for (int i = 0; i < speed_array.Length; i++)
                                    {
                                        track.Fixes[i].Speed = speed_array[i];
                                    }
                                }

                                var direction = t.ExtendedData.Flatten().OfType<SimpleArrayData>().FirstOrDefault(x => x.Name == "course");
                                if (direction != null && direction.Values.Count() == track.Fixes.Count())
                                {
                                    var direction_array = direction.Values.ToList().Select(x => double.Parse(x == "" ? "0" : x)).ToArray();
                                    for (int i = 0; i < direction_array.Length; i++)
                                    {
                                        track.Fixes[i].Direction = direction_array[i];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return track;
        }

        public List<string> GetTracks(string urn)
        {
            throw new NotImplementedException();
        }
    }
}
