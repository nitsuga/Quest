using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System.Data;
using System.Diagnostics;

namespace Quest.XC
{
    public class DBSettings
    {
        private static DateTime _lastPhysicalRead = DateTime.MinValue;

        private static Dictionary<String, String> _cache = new Dictionary<string, string>();

        public static int GetSetting(string connectionString, string variable, int defaultValue)
        {
            string result = GetSetting(connectionString, variable, "");
            if (result.Length == 0)
                return defaultValue;

            int value;
            int.TryParse(result, out value);
            return value;
        }

        public static bool GetSetting(string connectionString, string variable, bool defaultValue)
        {
            string result = GetSetting(connectionString, variable, "");
            if (result.Length == 0)
                return defaultValue;

            bool value;
            bool.TryParse(result, out value);
            return value;
        }

        public static string GetSetting(string connectionString, string variable)
        {
            return GetSetting(connectionString, variable, "");
        }

        /// <summary>
        /// get a database setting. The setting is only refreshed from the database every 10 seconds
        /// to avoid too many requests.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="variable"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetSetting(string connectionString, string variable, string defaultValue)
        {
            // cache to old??
            if (DateTime.Now.Subtract(_lastPhysicalRead).TotalSeconds > 10)
            {
                Debug.WriteLine("cache cleared .. too old ");
                _cache.Clear();
            }

            // return the cache version if it exists
            if (_cache.ContainsKey(variable))
            {
                Debug.WriteLine("getting from cache " + variable);
                return _cache[variable];
            }

            // get here if the cache has been cleared or the variable is not in the cache anyway
            Debug.WriteLine("getting from database " + variable);
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Select value from Variable where variable='" + variable + "'", conn))
                    {
                        cmd.CommandType = CommandType.Text;

                        String result = Convert.ToString(cmd.ExecuteScalar());

                        _cache.Add(variable, result);
                        _lastPhysicalRead = DateTime.Now;
                        return result;
                    }
                }

            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, "TracePolicy"))
                    throw;
            }

            return defaultValue;
        }
    }
}
