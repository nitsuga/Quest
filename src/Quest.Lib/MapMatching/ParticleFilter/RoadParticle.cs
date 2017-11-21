using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Routing;

namespace Quest.Lib.MapMatching.ParticleFilter
{
    public class RoadParticle : MotionParticle
    {
        /// <summary>
        ///     distance along the edge
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        ///     Which road semgent are we on
        /// </summary>
        internal RoadEdge Edge { get; set; }

        public override string ToString()
        {
            return
                $"X={Vector.Position.X:0} Y={Vector.Position.Y:0} S={Vector.Speed:0.#} B={Vector.Direction:0} W={Weight:0.###} D={Distance:0}";
        }

        public override object Clone()
        {
            return new RoadParticle
            {
                Distance = Distance,
                Vector = Vector.Clone() as MotionVector,
                Weight = Weight,
                Edge = Edge
            };
        }
    }
}