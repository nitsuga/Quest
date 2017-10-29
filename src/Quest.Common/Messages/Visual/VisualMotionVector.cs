namespace Quest.Common.Messages.Visual
{
    /// <summary>
    ///     a motion vector at a given time
    /// </summary>

    public class VisualMotionVector
    {
        public double Speed { get; set; }

        public double Direction { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }


}