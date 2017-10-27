using System;

namespace Quest.Common.Messages.GIS
{
    [Serializable]
    public class Fix : MotionVector
    {
        /// <summary>
        /// suspect fixes are marked as corrupt and given a reason
        /// </summary>
        public enum CurruptReason
        {
            TooCloseTime,
            TooCloseDistance,
            Duplicate
        }

        /// <summary>
        /// time
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// ordered sequence of this fix in the track
        /// </summary>
        public int Sequence;

        /// <summary>
        /// Id for this fix
        /// </summary>
        public int Id;

        /// <summary>
        /// the reason, if marked as corrupt
        /// </summary>
        public CurruptReason? Corrupt;

        /// <summary>
        /// estimated speed
        /// </summary>
        public double? EstimatedSpeedMph;

        public override string ToString()
        {
            return $"{Timestamp} {base.ToString()} {Corrupt}";
        }
    }
}