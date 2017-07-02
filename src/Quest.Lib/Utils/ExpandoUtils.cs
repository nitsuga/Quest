using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Quest.Lib.Trace;
using System;

namespace Quest.Lib.Utils
{
    public static class ExpandoUtils
    {
        public static int GetInt(this Dictionary<string, object> dict, string name, int defaultValue)
        {
            object v;
            dict.TryGetValue(name, out v);
            if (v != null)
                return (int)v;
            else
                return defaultValue;
        }

        public static int GetInt(this string value, int defaultValue)
        {
            var i = defaultValue;
            int.TryParse(value, out i);
            return i;
        }

        public static double GetDouble(this string value, double defaultValue)
        {
            var i = defaultValue;
            double.TryParse(value, out i);
            return i;
        }

        public static string GetString(this Dictionary<string, object> dict, string name, String defaultValue)
        {
            object v;
            dict.TryGetValue(name, out v);
            if (v != null)
                return (string)v;
            else
                return defaultValue;
        }

        public static DateTime GetDateTime(this Dictionary<string, object> dict, string name, DateTime defaultValue)
        {
            DateTime date;
            string datestr = dict.GetString(name,"1 Jan 1900");
            DateTime.TryParse(datestr, out date);
            if (datestr != null)
            {
                return date;
            }
            else
                return defaultValue;
        }

        public static DateTime GetDateTime(this string value, DateTime defaultValue)
        {
            DateTime date;
            DateTime.TryParse(value, out date);
            return date;
        }

        public static bool GetBoolean(this string value, bool defaultValue)
        {
            var result= defaultValue;
            bool.TryParse(value, out result);
            return result;
        }


        public static Dictionary<string, object> DictionaryFromString(string query)
        {
            Logger.Write($"Parsing parameters: {query}","Expando");

            var parmsX = new Dictionary<string, object>();

            if (query == null)
                query = "";

            var parameters = query.Replace("\n", "");
            var parts = parameters.Split(',');
            foreach (var p in parts)
            {
                var bits = p.Split('=');
                if (bits.Length == 2)
                {
                    var value = bits[1].Trim();
                    if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        parmsX.Add(bits[0].Trim(), bits[1].Substring(1, bits[1].Length - 2));
                    }
                    else
                    {
                        if (value == "true")
                        {
                            parmsX.Add(bits[0].Trim(), true);
                            continue;
                        }
                        if (value == "false")
                        {
                            parmsX.Add(bits[0].Trim(), false);
                            continue;
                        }

                        if (value.Contains("."))
                        {
                            double v;
                            double.TryParse(bits[1], out v);
                            parmsX.Add(bits[0].Trim(), v);
                        }
                        else
                        {
                            int v;
                            int.TryParse(bits[1], out v);
                            parmsX.Add(bits[0].Trim(), v);
                        }
                    }
                }
            }

            if (parmsX.Any())
            {
                Logger.Write($"Job parameters are","Expando");
                foreach (var v in parmsX)
                    Logger.Write($"  {v.Key} = {v.Value}","Expando");
            }
            return parmsX;
        }

        public static ExpandoObject MakeExpandoFromString(string query)
        {
            var dict = DictionaryFromString(query);
            dynamic parms = new ExpandoObject();
            var parmsX = (IDictionary<string, object>)parms;
            foreach (var d in dict)
            {
                parmsX.Add(d);
            }
            return parms;
        }

    }
}
