using System;

namespace Quest.Common.Messages.Visual
{
    /// <summary>
    /// Data for a single item on the timeline
    /// </summary>

    public class TimelineData
    {
        public TimelineData()
        {
        }

        public override string ToString()
        {
            return $"{Id} {Start} {End} {Label} {DisplayClass}";
        }

        public TimelineData(long id, DateTime? start, DateTime? end, string label, string displayClass = null)
        {
            Id = id;

            if (start != null) Start = string.Format("{0:ddd MMM dd yyyy hh:mm:ss}", start, TimeZoneInfo.Local.StandardName);
            if (end != null) End = string.Format("{0:ddd MMM dd yyyy hh:mm:ss}", end, TimeZoneInfo.Local.StandardName);

            //if (start != null) Start = string.Format("{0:ddd MMM dd yyyy hh:mm:ss \"GMT\"K} ({1})", start, TimeZoneInfo.Local.StandardName);
            //if (end != null) End = string.Format("{0:ddd MMM dd yyyy hh:mm:ss \"GMT\"K} ({1})", end, TimeZoneInfo.Local.StandardName);
            Label = label;
            DisplayClass = displayClass ?? label;
        }

        
        public long Id;

        /// <summary>
        /// start time
        /// </summary>        
        public string Start;

        /// <summary>
        /// end time
        /// </summary>        
        public string End;

        /// <summary>
        /// Display in the timeline box
        /// </summary>        
        public string Label;

        /// <summary>
        /// display class
        /// </summary>        
        public string DisplayClass;

    }
}