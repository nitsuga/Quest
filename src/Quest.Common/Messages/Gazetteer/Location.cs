using System;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class Location
    {
        public double[] Coordinate; // centroid or point
        public string Precision = "1m";
        public string Radius;
        public string Type;
        public string WKT;

        public override string ToString()
        {
            return $"{Coordinate[0]} {Coordinate[1]}";
        }
    }
}