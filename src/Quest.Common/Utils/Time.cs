using System;

namespace Quest.Common.Utils
{
    public static class Time
    {
        public static long CurrentUnixTime()
        {
            return UnixTime(DateTime.UtcNow);
        }

        public static DateTime UnixTime(long unixTime)
        {
            long ticks = (unixTime + 62135596800L) * 10000000L;
            var time = new DateTime(ticks);
            return time;
        }
            

        public static long UnixTime(this DateTime date)
        {
            var secondsSince0000 = date.Ticks / 10000000;
            var secondsSince1970 = 62135596800;

            // calculate the number of seconds from 1/1/1970
            return secondsSince0000 - secondsSince1970;
        }

        public static int HourOfWeek(this DateTime date)
        {
            var dow = ((int)date.DayOfWeek + 6) % 7;
            var how = date.Hour + dow * 24;
            return how;
        }

    }
}
