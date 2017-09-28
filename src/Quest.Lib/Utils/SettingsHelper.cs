using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.DataModel;
using Quest.Lib.Trace;

namespace Quest.Lib.Utils
{
    public class SettingsHelper
    {
        private static DateTime _lastPhysicalRead = DateTime.MinValue;
        private static readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        public static string[] GetVariable(string name, string[] defaultValue)
        {
            var list = GetVariable(name, "");

            if (list.Length == 0)
                return defaultValue;
            return list.Split(',');
        }

        /// <summary>
        /// inserts the currect application data directory into a string in place of {appdata}
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string SubstituteDataDirectory(string filename)
        {
            var path = (string)AppDomain.CurrentDomain.GetData("DataDirectory") ??
           (string)AppDomain.CurrentDomain.BaseDirectory + $"/Data/";
            filename = filename.Replace("{appdata}", path);
            return filename;
        }

        public static string GetFilename(string variable, string defaultValue)
        {
            var result = GetVariable(variable, defaultValue);
            return SubstituteDataDirectory(result);
        }


        public static string GetVariable(string variable, string defaultValue)
        {
            lock (_cache)
            {
                // cache to old??
                if (DateTime.Now.Subtract(_lastPhysicalRead).TotalSeconds > 10)
                {
                    _cache.Clear();
                }


                // return the cache version if it exists
                if (_cache.ContainsKey(variable))
                {
                    return _cache[variable];
                }


                string retval = null;

                try
                {
                    return _dbFactory.Execute<QuestContext, string>((db) =>
                    {
                        var v = db.Variable.FirstOrDefault(x => x.Variable1 == variable);

                        if (v == null)
                        {
                            v = new Variable
                            {
                                Description = "auto",
                                Type = "?",
                                Value = defaultValue,
                                Variable1 = variable
                            };
                            db.Variable.Add(v);
                            db.SaveChanges();
                        }

                        _cache.Add(variable, v.Value);
                        _lastPhysicalRead = DateTime.Now;
                        retval = v.Value;
                        return retval;
                    });
                }
                catch (Exception ex)
                {
                    Logger.Write($"Failed to get variable {variable}, {ex}","Settings");
                }
                finally
                {
                }
            }
        }

        public static int GetVariable(string name, int defaultValue)
        {
            var text = GetVariable(name, defaultValue.ToString());
            int result;
            int.TryParse(text, out result);
            return result;
        }

        public static bool GetVariable(string name, bool defaultValue)
        {
            var text = GetVariable(name, defaultValue.ToString());
            bool result;
            bool.TryParse(text, out result);
            return result;
        }

        public static double GetVariable(string name, double defaultValue)
        {
            var text = GetVariable(name, defaultValue.ToString());
            double result;
            double.TryParse(text, out result);
            return result;
        }
    }
}