using System;
using GeoAPI.Geometries;

namespace Quest.Common.Messages
{
    [Serializable]
    public class MotionVector : ICloneable
    {
        /// <summary>
        /// Speed 
        /// </summary>
        public double Speed;

        /// <summary>
        /// Direction
        /// </summary>
        public double Direction { get; set; }

        public Coordinate Position { get; set; }

        public object Clone()
        {
            return new MotionVector {Speed = Speed, Direction = Direction, Position = Position.Clone() as Coordinate};
        }

        public override string ToString()
        {
            return $"{Speed}m/s {Direction}deg {Position.X}/{Position.Y}";
        }

        /// <summary>
        ///     distance in meters
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>        
        public double DistanceFrom(MotionVector from)
        {
            var dX = Position.X - from.Position.X;
            var dY = Position.Y - from.Position.Y;

            return Math.Sqrt(dX*dX + dY*dY);
        }
    }
}