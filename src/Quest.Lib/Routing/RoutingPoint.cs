using System;
using System.Runtime.Serialization;
using GeoAPI.Geometries;

namespace Quest.Lib.Routing
{
    [DataContract]
    [Serializable]
    public class RoutingPoint : Coordinate, IComparable
    {
        [DataMember] public string Identifier;

        public RoutingPoint()
        {
        }

        public RoutingPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public RoutingPoint(Coordinate coord)
        {
            X = coord.X;
            Y = coord.Y;
        }

        public new object Clone()
        {
            return new RoutingPoint {X = X, Y = Y, Identifier = Identifier /*, Tag = this.Tag*/};
        }
    }
}