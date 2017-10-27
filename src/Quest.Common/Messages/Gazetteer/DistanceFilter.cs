using System;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class DistanceFilter
    {
        public string distance;
        public double lat;
        public double lng;
    }
}