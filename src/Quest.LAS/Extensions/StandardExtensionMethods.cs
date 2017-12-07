using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quest.LAS.Extensions
{
    public static class StandardExtensionMethods
    {

        public static bool ContainsCaseInsensitive(this string[] array, string value)
        {
            return array.Any(entry => string.Equals(entry, value, StringComparison.CurrentCultureIgnoreCase));
        }

        public static int ToInt(this string fromString)
        {
            int returnValue;
            int.TryParse(fromString, out returnValue);
            return returnValue;
        }

        public static long ToLong(this string fromString)
        {
            long returnValue;
            long.TryParse(fromString, out returnValue);
            return returnValue;
        }

        public static float ToFloat(this string fromString)
        {
            float returnValue;
            float.TryParse(fromString, out returnValue);
            return returnValue;
        }

        public static double ToDouble(this string fromString)
        {
            double returnValue;
            double.TryParse(fromString, out returnValue);
            return returnValue;
        }

        public static bool ToBoolean(this string fromString)
        {
            return fromString.ToLower() == "y" || fromString.ToLower() == "yes" || fromString.ToLower() == "1" || fromString.ToLower() == "true" || fromString.ToLower() == "on";
        }

        public static bool ToBoolean(this int fromInt)
        {
            return fromInt != 0;
        }

        public static DateTime ToDateTime(this string fromString)
        {
            DateTime returnValue;
            DateTime.TryParse(fromString, out returnValue);
            return returnValue;
        }

        public static int RoundDownToNearest(this int fromValue, int value)
        {
            var remainder = fromValue % value;
            return fromValue - remainder;
        }

        public static int RoundUpToNearest(this int fromValue, int value)
        {
            var remainder = fromValue % value;
            return (fromValue - remainder) + value;
        }

        public static long RoundDownToNearest(this long fromValue, long value)
        {
            var remainder = fromValue % value;
            return fromValue - remainder;
        }

        public static long RoundUpToNearest(this long fromValue, long value)
        {
            var remainder = fromValue % value;
            return (fromValue - remainder) + value;
        }

        public static double RoundDownToNearest(this double fromValue, long value)
        {
            var remainder = fromValue % value;
            return fromValue - remainder;
        }

        public static double RoundUpToNearest(this double fromValue, long value)
        {
            var remainder = fromValue % value;
            return (fromValue - remainder) + value;
        }

        public static string GetDetailedException(Exception ex)
        {
            var sb = new StringBuilder();

            if (ex != null)
            {
                var exception = ex;
                do
                {
                    sb.AppendLine(exception.Message);

                    exception = ex.InnerException;
                }
                while (exception != null);

            }
            else
            {
                throw new ArgumentNullException();
            }

            return sb.ToString();
        }

        public static bool IsNumeric(this string checkString)
        {
            return !checkString.Any(x => x < "0".ToCharArray()[0] || x > "9".ToCharArray()[0]);
        }

        public static bool CheckBit(this int value, int bitNumber)
        {
            //return ((int) (Math.Pow(2, bitNumber)) & value) != 0;
            return (value & (1U << bitNumber)) != 0;
        }

        public static bool CheckBit(this short value, short bitNumber)
        {
            //return ((int) (Math.Pow(2, bitNumber)) & value) != 0;
            return (value & (1U << bitNumber)) != 0;
        }

        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        public static DateTime ToStandardDateTime(this DateTime dateTime)
        {
            return Convert.ToDateTime(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public static DateTime ToStandardDate(this DateTime dateTime)
        {
            return Convert.ToDateTime(dateTime.ToString("yyyy-MM-dd"));
        }

    }
}
