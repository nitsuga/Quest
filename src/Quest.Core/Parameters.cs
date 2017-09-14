using System.Linq;

namespace Quest.Core
{
    internal static class Parameters
    {
        /// <summary>
        ///     Get a parameter from the argument list
        /// </summary>
        /// <param name="args"></param>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        internal static string GetParameter(string[] args, string param, string defaultValue, string sep = "=")
        {
            var withSep = param.ToLower().Trim() + sep;
            var p = args.Where(x => x.ToLower().StartsWith(withSep)).ToList();

            if (p == null || p.Count==0)
                return defaultValue;
           
            return p.FirstOrDefault().Substring(withSep.Length); 
        }

        /// <summary>
        ///     check if a command line parameter exists
        /// </summary>
        /// <param name="args"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        internal static bool ParameterExists(string[] args, string param)
        {
            var p = args.FirstOrDefault(x => x.StartsWith(param));
            return p != null;
        }
    }
}
