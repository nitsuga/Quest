using System;

namespace Quest.Lib.Routing
{
    /// <summary>
    ///     a class that represents the current status of the routing engine. This is used by the GUI to
    ///     display to the user how far the routing engine has got with its initialisation sequence.
    /// </summary>
    public class RouteEngineStatusArgs : EventArgs
    {
        public string Message = "";

        public bool StartupComplete = false;

        public int StartupProgress = 0;
    }
}