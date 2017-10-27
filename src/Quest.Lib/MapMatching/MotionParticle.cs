using Quest.Common.Messages;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.MapMatching
{
    public class MotionParticle
    {
        public double Weight { get; set; }

        /// <summary>
        ///     State estimate
        /// </summary>
        public MotionVector Vector { get; set; }

        public override string ToString() => $"X={Vector.Position.X:0} Y={Vector.Position.Y:0} S={Vector.Speed:0.#} B={Vector.Direction:0} W={Weight:0.###}";

        public virtual object Clone() => new MotionParticle {Vector = Vector.Clone() as MotionVector, Weight = Weight};
    }
}