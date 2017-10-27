using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Common.Messages;
using Quest.Lib.Constants;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.MapMatching
{
    /// <summary>
    /// Represents a list of fixes that a vehicle is taking on a journey to the given incident
    /// </summary>
    public class Track
    {
        public string Callsign;
        public List<Fix> Fixes;
        public long Incident;
        public int VehicleType;
        public string ErrorMessage;
    }

    public static class TrackUtil
    {
        public static Track RemoveCloseFixes(this Track track, int minSeconds, int minDistance)
        {
            if (track.Fixes.Count < 2)
                return track;

            var last = track.Fixes.First();
            foreach (var fix in track.Fixes.ToArray())
            {
                if (fix.Equals(last))
                    continue;

                var diff = fix.Timestamp - last.Timestamp;
                var dist = fix.Position.Distance(last.Position);
                
                if (diff.TotalSeconds < minSeconds || dist< minDistance)
                    track.Fixes.Remove(fix);
                else
                    last = fix;
            }

            return track;
        }

        public static Track RemoveDuplicates(this Track track)
        {
            if (track.Fixes.Count < 2)
                return track;

            Fix last = null;
            foreach (var fix in track.Fixes.ToArray())
            {
                if (fix.Position.X == last?.Position.X && fix.Position.Y == last.Position.Y &&
                    fix.Speed == last.Speed && fix.Direction == last.Direction)
                    track.Fixes.Remove(last);
                last = fix;
            }

            return track;
        }

        public static Track MarkSuspectFixes(this Track track, int minSeconds, int minDistance)
        {
            if (track.Fixes.Count < 2)
                return track;

            Fix last = null;
            foreach (var fix in track.Fixes.ToArray())
            {
                if (fix.Position.X == last?.Position.X && Math.Abs(fix.Position.Y - last.Position.Y) < 0.1 &&
                    Math.Abs(fix.Speed - last.Speed) < 0.1 && Math.Abs(fix.Direction - last.Direction) < 0.1 && 
                        fix.Speed>0 )
                {
                    fix.Corrupt= Fix.CurruptReason.Duplicate;
                    continue;
                }

                if (last != null)
                {
                    var diff = fix.Timestamp - last.Timestamp;
                    var dist = fix.Position.Distance(last.Position);

                    if (dist < minDistance)
                    {
                        last.Corrupt = Fix.CurruptReason.TooCloseDistance;
                        fix.Corrupt = Fix.CurruptReason.TooCloseDistance;
                        continue;
                    }

                    if (diff.TotalSeconds < minSeconds)
                    {
                        fix.Corrupt = Fix.CurruptReason.TooCloseTime;
                        continue;
                    }
                }

                last = fix;
            }

            return track;
        }

        public static Track CalculateEstimateSpeeds(this Track track)
        {
            Fix f0 = null;

            foreach (var f1 in track.Fixes.Where(x => x.Corrupt == null))
            {
                try
                {
                    if (f0 != null)
                    {
                        var t = f1.Timestamp - f0.Timestamp;

                        var secs = t.TotalSeconds + double.Epsilon;
                        var meters = f0.DistanceFrom(f1);
                        var mph = meters / secs * Constant.ms2mph;
                        f1.EstimatedSpeedMph = double.IsPositiveInfinity(mph) ? (double?)null : mph;
                    }
                    f0 = f1;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return track;

        }

        /// <summary>
        /// clean up a track 
        /// </summary>
        /// <param name="track"></param>
        /// <param name="minSeconds"></param>
        /// <param name="minDistance"></param>
        /// <param name="maxSpeed"></param>
        /// <returns></returns>
        public static bool CleanTrack(this Track track, int minSeconds, int minDistance, int maxSpeed, int take)
        {
            track.MarkSuspectFixes(minSeconds, minDistance)
                .CalculateEstimateSpeeds();

            if (track.Fixes.Any(x => x.EstimatedSpeedMph > maxSpeed))
            {
                track.ErrorMessage = $"Part of the track exceeded estimated speed of {maxSpeed} mph";
                return false;
            }

            // keep only the good fixes
            track.Fixes = track.Fixes.Where(x => x.Corrupt == null).ToList();

            // only take top n
            track.Fixes = track.Fixes.Take(take).ToList();

            if (track.Fixes.Count < 4)
            {
                track.ErrorMessage = $"Not enough good fixes";
                return false;
            }

            for (int i = 0; i < track.Fixes.Count; i++)
                track.Fixes[i].Sequence = i;

            return true;
        }

    }
}