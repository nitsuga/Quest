using Quest.Lib.MapMatching;
using System.Collections.Generic;

namespace Quest.Lib.Research.Utils
{
    public interface ITrackProvider
    {
        Track GetTrack(string urn, int skip = 0);
        List<string> GetTracks(string urn);
    }
}