////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2014 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Quest.Lib.Routing;
using System.ComponentModel.Composition;
using Quest.Lib.ServiceBus.Messages;

namespace Quest.Lib.AutoDispatch
{
    /// <summary>
    /// This is the ARD2 module. The module listens for incident and resource events and makes
    /// approriate deployment suggestions by invoking methods on the C&C module. The class
    /// builds the current 'board'. Each board has the capability of determining a set of 
    /// redeployments or assignment and constructs a cloned board for each. It then uses a 
    /// LINQ tree search to determine which child boards produced the best result. The system
    /// is capable of searching down more than one level but performance is slow.
    /// </summary>
    [Export("ARDServer", typeof(ProcessorModule))]
    public class ARDServer : ProcessorModule
    {
        private const String ARD_MAXDISTANCE = "Ard.MaxDistance";
        private const String ARD_MAXDURATION = "Ard.MaxDuration";
        private const String ARD_WAITINGFACTOR = "Ard.WaitingFactor";
        private const String ARD_ENROUTEFACTOR = "Ard.EnrouteFactor";
        private const String ARD_INSTANCEMAX = "Ard.InstanceMax";
        private const String ARD_GENERATIONS = "Ard.Generations";
        private const String ARD_WRTHRESHOLD = "Ard.WaitingResourceThreshold";
        private const String ARD_ARCGRIDDIR = "Ard.ArcGridDir";
        private const String ARD_WEIGHTS = "Ard.Weights";
        private const String ARD_QUEUENAME = "Ard.Queue";
        private const String ARD_SCANFREQ = "Ard.ScanFrequence";


        // maintain an array of destination RoutingLocations
        private List<RoutingPoint> _destinations = new List<RoutingPoint>();
        // precomputed coverage maps for various destinations
        private Dictionary<int, DestinationCoverage> _destinationMap = new Dictionary<int, DestinationCoverage>();
        private int _tileSize = 500;
        private int _generationLimit = 1;
        private string _LastMove = "";
        private DateTime _lastScan = DateTime.MinValue;
        private int _maxDistance;
        private int _maxDuration;
        private double _waitingFactor;
        private double _enrouteFactor = 1.1;
        private int _instanceMax = 20;
        private int _waitingResourceThreshold;
        private CoverageMap _currentCoverage;
        private CoverageMap _incidentDensity;
        private string _arcgridDir = null;
        private string _queueName = null;
        private MessageHelper _msgSource;

        private static int _scanFrequenceSecs = 1;
        
        [Import]
        private IRouteEngine _Router;

        [Import]
        public IResourceCalculator _calculator;

        public ARDServer()
            : base("ARD Server", Worker)
        {
        }

        /// <summary>
        /// This routing gets called when requested to start.
        /// Perform initialisation here 
        /// </summary>
        /// <param name="module"></param>
        static void Worker(ProcessorModule module)
        {
            ARDServer me = module as ARDServer;

            bool outputmsg = false;

            Logger.Write(string.Format("ARD Starting"), "Trace", 0, 0, TraceEventType.Information, "ARD");
            // wait until routing engine is running
            while (!me._Router.IsInitialised)
            {
                if (!outputmsg)
                {
                    Logger.Write(string.Format("ARD waiting for routing engine to start"), "Trace", 0, 0, TraceEventType.Information, "ARD");
                    outputmsg = true;
                }

                Thread.Sleep(1000);
            }

            me.Initialise();

            for (; ; )
            {
                // StopRunning gets set when the system wants to shut us down
                if (module.StopRunning.WaitOne(_scanFrequenceSecs * 1000))
                {
                    if (me._msgSource != null)
                        me._msgSource.Stop();
                    return;
                }

                me.ScanForAssignments();
            }
        }



        /// <summary>
        /// Initialise the processes that need to be operational for Service to work
        /// </summary>
        public void Initialise()
        {
            Logger.Write(string.Format("Initialising"), "Trace", 0, 0, TraceEventType.Information, "ARD");
            // get the routing engine
            _maxDistance = Utils.SettingsHelper.GetVariable(ARD_MAXDISTANCE, 20000);
            _maxDistance = Utils.SettingsHelper.GetVariable(ARD_MAXDISTANCE, 20000);
            _maxDuration = Utils.SettingsHelper.GetVariable(ARD_MAXDURATION, 3600);
            _waitingFactor = Utils.SettingsHelper.GetVariable(ARD_WAITINGFACTOR, 1.0);
            _enrouteFactor = Utils.SettingsHelper.GetVariable(ARD_ENROUTEFACTOR, 1.1);
            _instanceMax = Utils.SettingsHelper.GetVariable(ARD_INSTANCEMAX, 20);
            _generationLimit = Utils.SettingsHelper.GetVariable(ARD_GENERATIONS, 1);
            _waitingResourceThreshold = Utils.SettingsHelper.GetVariable(ARD_WRTHRESHOLD, 150);
            _arcgridDir = Utils.SettingsHelper.GetVariable(ARD_ARCGRIDDIR, "");
            _queueName = Utils.SettingsHelper.GetVariable(ARD_QUEUENAME, "Quest.ARD");

            _scanFrequenceSecs = Utils.SettingsHelper.GetVariable(ARD_WRTHRESHOLD, 1);

            // start llistening for messages
            _msgSource = new MessageHelper();
            _msgSource.Initialise(_queueName); // Telephony
            _msgSource.NewMessage += new EventHandler<ServiceBus.NewMessageArgs>(msgSource_NewMessage);

            PrecomputeDestinationCoveragebyVehicle();
            Logger.Write(string.Format("Initialisation complete"), "Trace", 0, 0, TraceEventType.Information, "ARD");
        }

        /// <summary>
        /// prebuild an array of destination coverage maps. these are used to compute current coverage
        /// </summary>
        private void PrecomputeDestinationCoveragebyVehicle()
        {
            _destinationMap.Clear();

            Logger.Write(string.Format("Precomputing destination coverage by vehicle type"), "Trace", 0, 0, TraceEventType.Information, "ARD");

            for (int vehicleTypeId = 1; vehicleTypeId <= 2; vehicleTypeId++)
            {
                Logger.Write(string.Format("..precomputing for vehicle {0}", vehicleTypeId), "Trace", 0, 0, TraceEventType.Information, "ARD");
                DestinationCoverage coverage = new DestinationCoverage();
                _destinationMap.Add(vehicleTypeId, coverage);

                CoverageCalculator.BuildCoverageMap(_destinations, _destinationMap, _Router, coverage, vehicleTypeId, tileSize: _tileSize);

            }
            Logger.Write(string.Format("Precompute complete"), "Trace", 0, 0, TraceEventType.Information, "ARD");
        }


        /// <summary>
        /// receive message from the outside world
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void msgSource_NewMessage(object sender, ServiceBus.NewMessageArgs e)
        {
            // capture incident coverage from the CoverageMap calculator
            CoverageMap p = e.Payload as CoverageMap;
            if (p != null)
            {
                switch (p.Name)
                {
                    case "Expected Incidents":
                        _incidentDensity = p;
                        break;
                }
            }
        }

        private void ScanForAssignments()
        {
            Logger.Write(string.Format("Scanning for assignments"), "Trace", 0, 0, TraceEventType.Information, "ARD");

            using (QuestEntities db = new QuestEntities())
            {
                // get tuning weights
                string weightvar = Utils.SettingsHelper.GetVariable(ARD_WEIGHTS, "201,210,-3,0.194,0.138");
                string[] weightparts = weightvar.Split(',');
                double[] weights = weightvar.Split(',').Select(x => double.Parse(x)).ToArray();

                ARD2Board root = new ARD2Board(weights)
                {
                    Calculator = _calculator,
                    Router = _Router,
                    Name = "0",
                    DestinationMap = _destinationMap,
                    Destinations = _destinations,
                    CurrentCoverage = CoverageMapUtil.Clone(_currentCoverage),
                    IncidentDensity = _incidentDensity,
                    Generation = 0,
                    CurrentTime = DateTime.Now,
                    GenerationLimit = _generationLimit,
                    WaitingFactor = _waitingFactor,
                    MaxDistance = _maxDistance,
                    MaxDuration = _maxDuration,
                    InstanceMax = _instanceMax,
                    Incidents = (from r in db.IncidentViews select r).ToList(),
                    Resources = (from r in db.ResourceViews select r).ToList(),
                    WaitingResourceThreshold = _waitingResourceThreshold
                };

                // calculate current board value
                root.Value = root.CalcValue("Root");
                root.Delta = 0;

                // constructs a tree of possible moves
                List<ARD2Board> result = root.AsDepthFirstEnumerable(n => n.Children).ToList();

                Logger.Write(string.Format("Analysis complete"), "Trace", 0, 0, TraceEventType.Information, "ARD");

                // work out which element returns the best position (min value)
                var m = result.Aggregate((agg, next) =>
                    {
                        return next.Value < agg.Value ? next : agg;
                    }
                );

                bool foundbetterBoard = root.Value > m.Value;

                // apply the moves that led to this situation by adding them to the event queue
                if (!foundbetterBoard)
                    Logger.Write(string.Format("no better board positions " + m.Name), "Trace", 0, 0, TraceEventType.Information, "ARD");
                else
                {

                    // only apply a move if it results in a better board
                    Logger.Write(string.Format("  best move is " + m.Name), "Trace", 0, 0, TraceEventType.Information, "ARD");

                    for (; m.Parent != null; m = m.Parent)
                    {
                        if (m != null && m.AppliedMove != null)
                        {
                            // dont repeat the same move... its pointless
                            if (m.AppliedMove.ToString() != _LastMove)
                            {
                                Debug.Print(m.AppliedMove.ToString());

                                _LastMove = m.AppliedMove.ToString();

                                // add trigger for the new incident
                                //TaskKey key2 = new TaskKey(Guid.NewGuid().ToString(), null);
                                //new TaskEntry(_simEngine.EventQueue, key2, PerformAction, m.AppliedMove, this._simEngine.EventQueue.Now);
                            }
                        }
                    }
                }
            }

            Logger.Write(string.Format("End of scan"), "Trace", 0, 0, TraceEventType.Information, "ARD");
        }

#if false
        /// <summary>
        /// perform a queued action
        /// </summary>
        /// <param name="te"></param>
        private void PerformAction(TaskEntry te)
        {

            IMove m = (IMove)te.DataTag;
            AssignMove am = m as AssignMove;
            RelocateMove rm = m as RelocateMove;


            if (am != null)
            {
                ResourceView res = (from r in _ResManager.Resources where r.Callsign == am.Resource.Callsign select r).FirstOrDefault();
                Incident inc = (from i in _simEngine.LiveIncidents where i.IncidentId == am.Incident.IncidentId select i).FirstOrDefault();
                if (inc != null && res != null)
                    AssignResource(inc, res);
            }

            if (rm != null)
            {
                ResourceView res = (from r in _ResManager.Resources where r.Callsign == rm.Resource.Callsign select r).FirstOrDefault();
                _CadIn.NavigateTo(new NavigateTo() { DestinationId = rm.dest.DestinationId, ResourceId = res.ResourceId, Reason = "Coverage" });
            }
        }

        void AssignResource(IncidentView inc, ResourceView resource)
        {
            if (resource.Status == ResourceStatus.Enroute)
            {
                Debug.Print("Assigning enroute vehicle: " + resource.Callsign);
                _CadIn.CancelVehicle(resource.Incident.IncidentId, resource.ResourceId);
                _CadIn.AssignVehicle(inc.IncidentId, resource.ResourceId);
            }
            else
            {
                Debug.Print("C&C Assigning waiting vehicle: " + resource.Callsign);
                _CadIn.AssignVehicle(inc.IncidentId, resource.ResourceId);
            }
        }

#endif

    } // End of Class

    public class DestinationCoverage : Dictionary<int, CoverageMap>
    {
    }


} //End of Namespace
