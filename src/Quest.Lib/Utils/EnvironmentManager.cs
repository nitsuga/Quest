using System.Collections.Generic;
using Quest.Lib.Constants;

namespace Quest.Lib.Utils
{
    public interface IEnvironmentManager
    {
    }

    public class EnvironmentManagerSettings
    {
        /// <summary>
        ///     Physical path of the system
        /// </summary>
        public string Path;

        public Dictionary<string, string> CommandLineArgs { get; set; }

        public Stage Stage { get; set; }

    }

    public class EnvironmentManager : IEnvironmentManager
    {
        /// <summary>
        ///     Get the current stage
        /// </summary>
         public EnvironmentManagerSettings Settings;

        public bool HonourValidityDates()
        {
            switch (Settings.Stage)
            {
                case Stage.Development:
                    return false;

                case Stage.Production:
                    return false;

                case Stage.Test:
                    return true;

                case Stage.Public:
                    return true;
            }

            return true;
        }
    }
}