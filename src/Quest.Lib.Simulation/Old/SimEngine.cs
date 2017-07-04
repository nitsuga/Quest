using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using Quest.Lib.DataModel;
using Quest.Lib.ServiceBus.Messages;
using Quest.Lib.Simulation.Interfaces;
using Quest.Lib.Trace;
using Quest.Lib.Utils;
using System.ComponentModel.Composition;

namespace Quest.Lib.Simulation
{
    /// <summary>
    /// The simulator engine is responsible for loading and starting simulation modules. It also instantiates an
    /// event queue that holds and fires timed events.
    /// </summary>
    [Export]
    public class SimEngine : IDisposable
    {
        public double[] Constants;

        /// <summary>
        /// represents the database record created to represent this run of the simulation
        /// </summary>
        public SimulationRun RunRecord;

        /// <summary>
        /// event queue used maintain a list of timed events
        /// </summary>
        [Import]
        public TimedEventQueue EventQueue;

        [Import]
        public Parameters Parameters;

        private PerformanceRecorder _recorder;

        /// <summary>
        /// raised when the simulator engine wishes to display a message on the console
        /// </summary>
        public event System.EventHandler<MessageArgs> DisplayMessage;

        /// <summary>
        /// signalled when the simulation completes
        /// </summary>
        public AutoResetEvent Completed = new AutoResetEvent(false);

        /// <summary>
        /// returns the current status of the simulator
        /// </summary>
        public SimStatus Status { get; set; }

        /// <summary>
        /// a set of simulation parameters that can be used by the modules
        /// </summary>
        public DateTime StartDate;

        public DateTime EndDate;

        public Bounds Bounds;

        public String PerformanceFile = "performance";

        public Destination[] Destinations;

        /// <summary>
        /// an observable list of current live incidents
        /// </summary>
        public ObservableCollectionEx<Incident> LiveIncidents { get; set; }

        /// <summary>
        /// a list of simulation components
        /// </summary>
        private Dictionary<Type, SimPart> _simparts = new Dictionary<Type, SimPart>();
        
        
        private float _finalPerformance = 0;
        private String ConstantsMessage;

        //private DispatcherTimer myDispatcherTimer;
        private System.Timers.Timer myTimer;
        private int _incWaitingLimit = int.MaxValue;
        private int startupStep = 0;
        private int startupStep_last = 0;
        
        private int RefreshRate = 5;
        private DateTime Started;

        private const string SIM_MODULE = "Sim.Module";
        private const string SIM_NOTES = "Sim.Notes";
        private const string SIM_STARTDATE = "Sim.StartDate";
        private const string SIM_ENDDATE = "Sim.EndDate";
        private const string SIM_SPEED = "Sim.Speed";
        private const string SIM_INCLIMIT = "Sim.IncWaitingLimit";


        /// <summary>
        /// constructor
        /// </summary>
        public SimEngine()
        {
            LiveIncidents = new ObservableCollectionEx<Incident>();
            InitialiseStatus();
        }

        /// <summary>
        /// initialise the simulator using the connections strings already passed.
        /// </summary>
        public void Initialise()
        {
            Initialise(PerformanceFile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="LASMIS"></param>
        /// <param name="Geotracker"></param>
        /// <param name="GeoRouting"></param>
        /// <param name="Simulator"></param>
        /// <param name="PerformanceFile"></param>
        public void Initialise(string PerformanceFile)
        {
            try
            {
                Started = DateTime.Now;

                this.PerformanceFile = PerformanceFile;

                LiveIncidents.Clear();

                Status.Initialised = false;

                // create a global task queue
                //EventQueue = new TimedEventQueue();
                EventQueue.TimeChanged += new EventHandler(Tasks_TimeChanged);
                EventQueue.Error += new EventHandler<ExceptionArgs>(Tasks_Error);

                Completed = new AutoResetEvent(false);

                startupStep = 0;
                startupStep_last = 0;
                
                Status.Initialised = true;
            }
            catch (Exception ex)
            {
                Status.Initialised = false;
                throw new Exception("Failed to initialise : " + ex.Message);
            }
        }

        /// <summary>
        /// start the simulation.
        /// </summary>
        /// <param name="parameters">a list of general parameters that will be used to define which models get loaded</param>
        /// <param name="controls">a list of display components that will be notified when a simulation starts/stops</param>
        /// <param name="constants">a set of constants used by (but not limited to) dispatch models</param>
        public void StartSimulation(CompositionContainer container, double[] constants)
        {
            Constants = constants;

            foreach (ProfileParameter p in Parameters)
                OnMessage("Parameter " + p.ProfileParameterType.Name + "=" + p.Value);

            if (Constants != null)
            {
                List<String> c = new List<string>();
                foreach (double d in Constants)
                    c.Add(d.ToString("#.######"));
                ConstantsMessage = String.Join(",", c.ToArray());
            }
            else
                ConstantsMessage = "None";

            OnMessage("Starting simulation with constants " + ConstantsMessage);


            _incWaitingLimit = Parameters.GetFirstParameter(SIM_INCLIMIT, int.MaxValue);

            // extract notes            
            string title = Parameters.GetFirstParameter(SIM_NOTES, "");
            CreateSimRunRecord(title, ConstantsMessage);

            int group = Parameters.GetFirstParameter("Sim.DataGroup", 0);
            // load destinations
            Destinations = LoadDestinations(group);

            // add the sim parts to the collection
            AddSimModules(container);

            _recorder = new PerformanceRecorder();// container.GetExport<PerformanceRecorder>().Value;
            _recorder.Initialise(container);

            // add performance recorder            
            if (_recorder!=null)
                _simparts.Add(_recorder.GetType(), _recorder);

            _simparts.ToList().ForEach(x =>x.Value.Initialise(container));

            StartTimer();

            startupStep_last = startupStep;
            startupStep = 1;
            
            OnMessage("Starting simulation: clock started");
        }

        public SimPart GetSimPart(Type type)
        {
            foreach (var part in _simparts.Values)
            {
                if (part.GetType() == type)
                    return part;
                if (part.GetType().GetInterfaces().Contains(type))
                    return part;
            }
            return null;
        }

        public float FinalPerformance
        {
            get { return _finalPerformance; }
            private set { _finalPerformance = value; }
        }

        public void StopSimulation()
        {

            Logger.Write("  Start Simulation @ " + Started.ToString("dd MMM yyyy HH:mm:ss"));
            Logger.Write("  Stop  Simulation @ " + DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));
            Logger.Write("  Duration           " + DateTime.Now.Subtract(Started).TotalSeconds.ToString("#.#") + " seconds");
            foreach (ProfileParameter p in Parameters)
                Logger.Write("  Parameter          " + p.ProfileParameterType.Name + "=" + p.Value);
            Logger.Write("  Constants          " + ConstantsMessage);

            // calculate the performance
            _finalPerformance = _recorder.CalculatePerformance();

            UpdateSimRunRecord(_finalPerformance);

            Status.Running = false;
            myTimer.Stop();
            EventQueue.Clear();
            EventQueue.Stop();

            _simparts.ToList().ForEach(x => x.Value.Stop());
            OnMessage("Stopped Simulation");

            Completed.Set();
        }

        public void SetSimDateRange(DateTime from, DateTime to)
        {
            if (Status.Initialised == false)
                throw new Exception("Not initialised");

            if (Status.Running == true)
                throw new Exception("Simulator is running");

            StartDate = from;
            EndDate = to;
        }

        public void SetSimulationSpeed(int simSpeed)
        {
            Logger.Write("SetSimulationSpeed: " + simSpeed.ToString());
            Status.SimSpeed = simSpeed;
            EventQueue.Speed = (double)simSpeed / 100.0;
        }

        public void OnMessage(String text)
        {
            OnMessage(text, false);
        }

        public void OnMessage(String text, bool isError)
        {
            Logger.Write(this.EventQueue.Now.ToString() + " " + text);

            if (DisplayMessage != null)
                DisplayMessage(this, new MessageArgs() { Message = text, IsError = isError });
        }

        public void OnError(Exception ex)
        {
            Logger.Write(ex.ToString());
            if (DisplayMessage != null)
                DisplayMessage(this, new MessageArgs() { IsError = true, Message = ex.Message });
        }

        /// <summary>
        /// user has asked for the cconfig, send it back to them
        /// </summary>
        public SimStatus GetStatus()
        {
            Status.SimTime = EventQueue.Now;
            return Status;
        }

        public void Dispose()
        {
            if (EventQueue != null)
                EventQueue.Dispose();
#if DISPATCHTIMER

            if (myDispatcherTimer != null)
                myDispatcherTimer.Stop();
#else
            if (myTimer != null)
                myTimer.Stop();
#endif
        }

        private void Tasks_Error(object sender, ExceptionArgs e)
        {
            OnError(e.exception);
        }

        /// <summary>
        /// control loop to sequence startup of all modules.
        /// </summary>
        private void StartupCheck()
        {
            switch (startupStep)
            {
                case 0:
                    break;

                case 1:
                    // wait for the initialisation to complete
                    int count = 0;
                    _simparts.ToList().ForEach(x => count += x.Value.IsInitialised ? 1 : 0);

                    if (count == _simparts.Count())
                    {
                        OnMessage("Starting simulation: initialisation complete");
                        startupStep_last = startupStep;
                        startupStep = 2;
                    }
                    else
                        OnMessage( String.Format("Starting simulation: initialised {0}/{1} components", count, _simparts.Count()));

                    break;

                case 2:

                    // first time in?
                    if (startupStep_last != startupStep)
                    {
                        myTimer.Stop();

                        // extract sim time range from parameters
                        ExtractDateRange();

                        EventQueue.Clear();
                        EventQueue.Now = StartDate;

                        //TODO: prepare controls via MEF
                        //_controls.ToList().ForEach(x => x.Prepare());

                        // load any required data
                        OnMessage("Starting simulation: modules are preparing to start");

                        _simparts.ToList().ForEach(x => x.Value.Prepare());

                        // start doing what you need to do.
                        OnMessage("Starting simulation: modules are starting");
                        _simparts.ToList().ForEach(x => x.Value.Start());

                        // start processing event tasks
                        EventQueue.Start();

                        if (Status.Initialised == false)
                            throw new Exception("Not initialised");

                        Status.Running = true;

                        myTimer.Start();

                        Logger.Write("StartSimulation");
                        startupStep_last = startupStep;
                        startupStep = 0;
                    }
                    break;
            }
        }

        private void InitialiseStatus()
        {
            Status = new Lib.SimStatus();
            Status.Initialised = false;
            Status.Profile = "";
            Status.RoutingStartup = 0;
            Status.Running = false;
            Status.SimSpeed = 0;
            Status.SimTime = DateTime.Now;
        }


        private void StartTimer()
        {
            myTimer = new System.Timers.Timer(RefreshRate * 1000);
            myTimer.Elapsed += new System.Timers.ElapsedEventHandler(myTimer_Elapsed);
            myTimer.Start();
        }

        void myTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                StartupCheck();

                int incwaiting_count = (from r in LiveIncidents where r.Status == ResourceStatus.Waiting select r).Count();

                // stop when the simulation end date has been reached or the number of waiting incidents exceeds a threshold
                if (EventQueue.Now > EndDate || incwaiting_count > _incWaitingLimit)
                {
                    EventQueue.Clear();
                    myTimer.Stop();
                    StopSimulation();
                }
            }
            catch (Exception ex)
            {
                StopSimulation();
                InitialiseStatus();
                myTimer.Stop();
                OnError(ex);
            }
        }

        private bool myfilter(Type tp, object data)
        {
            return true;
        }

        /// <summary>
        /// load up modules specified in the profile parameters
        /// </summary>
        private void AddSimModules(CompositionContainer container)
        {
            // get rid of old components
            _simparts.Clear();
            
            foreach (ProfileParameter s in Parameters)
            {
                if (s.ProfileParameterType.Name == SIM_MODULE)
                {
                    
                    string[] bits = s.Value.Split(',');
                    Assembly a = Assembly.Load(bits[0]);
                    
                    Type t = a.GetType(bits[1]);

                    Object o = a.CreateInstance(bits[1], true);

                    OnMessage("Loading " + bits[1]);

                    if (o == null)
                        throw new ApplicationException("Unable to load module: " + bits[1]);

                    SimPart part = o as SimPart;

                    if (part == null)
                        throw new ApplicationException("Module does not implement SimPart: " + bits[1]);

                    _simparts.Add(o.GetType(), part);
                }
            }
           
        }

        private void Tasks_TimeChanged(object sender, EventArgs e)
        {
            Status.SimTime = EventQueue.Now;
        }

        /// <summary>
        /// extract date ranges from the parameters
        /// </summary>
        /// <param name="parameters"></param>
        private void ExtractDateRange()
        {
            StartDate = Parameters.GetFirstParameter(SIM_STARTDATE, DateTime.Now);
            EndDate = Parameters.GetFirstParameter(SIM_ENDDATE, DateTime.MaxValue);
            EventQueue.Speed = Parameters.GetFirstParameter(SIM_SPEED, 0.0);            
        }

        /// <summary>
        /// create a run record. This is referenced in the SimulationResults table
        /// </summary>
        /// <param name="notes"></param>
        private void CreateSimRunRecord(string notes, String constants)
        {
            try
            {
                RunRecord = new SimulationRun() { Notes = notes, Constants = constants, Started = DateTime.Now };
                using (SimulatorEntities context = new SimulatorEntities())
                {
                    context.SimulationRuns.Add(RunRecord);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                OnMessage("Error writing performance: " + ex.ToString(), true);
            }

        }

        /// <summary>
        /// create a run record. This is referenced in the SimulationResults table
        /// </summary>
        /// <param name="notes"></param>
        private void UpdateSimRunRecord(float performance)
        {
            using (SimulatorEntities context = new SimulatorEntities())
            {
                var record = (from r in context.SimulationRuns where r.SimulationRunId == RunRecord.SimulationRunId select r).FirstOrDefault();
                if (record != null)
                {
                    record.Performance = performance;
                    record.Stopped = DateTime.Now;
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// add an assignment message
        /// </summary>
        /// <param name="action"></param>
        /// <param name="callsign"></param>
        public void UpdateAssignmentRecord(Resource res, Incident inc, String action)
        {
            using (SimulatorEntities context = new SimulatorEntities())
            {
                String callsign = res != null ? res.Callsign : "";
                String incident = inc != null ? inc.IncidentId.ToString() : "";

                SimulationAssignment assignment = new SimulationAssignment() { Action = action, Callsign = callsign, Incident = incident, tstamp = Status.SimTime, SimulationRunId = RunRecord.SimulationRunId };
                context.SimulationAssignments.Add(assignment);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Load the predefined destinations from the database
        /// </summary>
        /// <returns></returns>
        private Destination[] LoadDestinations(int GroupId)
        {
            List<Destination> dests = new List<Destination>();
            try
            {
                using (SimulatorEntities context = new SimulatorEntities())
                {
                    context.DestinationsViews.Where(x => x.GroupId == GroupId).ToList().ForEach(x =>
                        {
                            Destination d = new Destination();
                            d.DestinationId = x.DestinationId;
                            d.Name = x.Destination;
                            d.Easting = (int)x.e;
                            d.Northing = (int)x.n;
                            d.IsStandbyPoint = x.IsStandby;
                            d.IsHospital = x.IsHospital;
                            dests.Add(d);
                        }
                        );
                }
            }
            catch 
            {

            }

            return dests.ToArray();
        }
    }
}
