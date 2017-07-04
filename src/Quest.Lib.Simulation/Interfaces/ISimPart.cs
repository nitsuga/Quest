using System;
using Quest.Lib.Utils;

namespace Quest.Lib.Simulation.Interfaces
{
    /// <summary>
    ///     all modules used by the EISECSimulator shoud be derived from this class. It allows the modules to be controlled
    ///     within the simulation framework.
    /// </summary>
    public abstract class SimPart: IOptionalComponent
    {
        //[Import]
        //private SimEngine _simEngine;

        /// <summary>
        ///     set by the module to indicate that its is initialised
        /// </summary>
        public bool IsInitialised { get; set; } = false;

        public virtual void Initialise()
        {
        }

        /// <summary>
        ///     used by derived classes to display a message
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isError"></param>
        public void OnMessage(string text, bool isError)
        {
            //if (_simEngine!=null)
            //    _simEngine.OnMessage(text, isError);
        }

        /// <summary>
        ///     used by derived classes to display a message
        /// </summary>
        public void OnMessage(string text)
        {
            //_simEngine.OnMessage(text);
        }

        /// <summary>
        ///     used by derived classes to display an error message
        /// </summary>
        public void OnError(Exception ex)
        {
            //   _simEngine.OnError(ex);
        }


        /// <summary>
        ///     do whatever is necessary just prior to starting the simulation.. e.g. loading incident. You can
        ///     rely on other components being initialised
        /// </summary>
        public abstract void Prepare();

        /// <summary>
        ///     start running the simulation by placing events into the event queue
        /// </summary>
        public abstract void Start();

        /// <summary>
        ///     close down
        /// </summary>
        public abstract void Stop();
    }
}