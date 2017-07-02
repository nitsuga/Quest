using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
