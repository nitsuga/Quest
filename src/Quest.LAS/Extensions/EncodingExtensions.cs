using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quest.LAS.Extensions
{
    public static class EncodingExtensions
    {
        public static byte[] GetUnixBytes(this DateTime fromDateTime)
        {
            var diff = fromDateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            return BitConverter.GetBytes(Convert.ToUInt32(diff)).ToArray();
        }


        public static DateTime GetDateTimeFromBytes(this byte[] fromByteTime)
        {
            var fromTime = BitConverter.ToInt32(fromByteTime, 0);
            DateTime epoch = new DateTime(1989, 12, 30, 12, 0, 0);
            DateTime newDateTime = epoch.AddSeconds(fromTime);
            return newDateTime;
        }
    }
}
