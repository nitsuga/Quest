using System;

namespace Quest.Lib.Utils
{
    public static class Time
    {
        public static int HourOfWeek(this DateTime date)
        {
            var dow = ((int)date.DayOfWeek + 6) % 7;
            var how = date.Hour + dow * 24;
            return how;
        }

    }
}
