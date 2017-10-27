using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Quest.Common.Simulation;
using Quest.Lib.Routing;
using Quest.Lib.Processor;
using Autofac;
using Quest.Common.ServiceBus;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
using Quest.Common.Messages;
using Quest.Lib.Utils;
using Quest.Lib.Simulation.Destinations;
using Quest.Common.Utils;
using Quest.Common.Messages.Routing;
using Quest.Common.Messages.Resource;

namespace Quest.Lib.Simulation.Resources
{
    /// <summary>
    /// roster simulator, brings resources online and offline in line with what actually happened at LAS
    /// on a specific hour. To bring a resource online the Resource table is searched for a free (offline) one, 
    /// allocated the callsign, and positioned at the station where it came on duty.
    /// To take a resource offline it is powered off but only if its "Waiting". MDT simulator automatically
    /// powers itself off when instructed to go to standby point and is OffDuty
    /// </summary>
    
    public class RosterSimulator : ServiceBusProcessor
    {
        public ObservableCollection<SimResource> Resources { get; set; }

        private IRouteEngine _router;
        private ILifetimeScope _scope;
        private RoutingData _data;
        private IDestinationStore _destinationStore;
        private IRosterStore _rosterStore;
        private SimResourceManager _resManager;
        private SimContext _context;
        private string _roadSpeedCalculator;
        private int _callsign = 0;

        public RosterSimulator(
            IDestinationStore destinationStore,
            SimResourceManager resManager,
            SimContext context,
            string router,
            string rosterStore,
            string roadSpeedCalculator,
            RoutingData data,
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _roadSpeedCalculator = roadSpeedCalculator;
            _resManager = resManager;
            _rosterStore = scope.ResolveNamed<IRosterStore>(rosterStore);
            _router = scope.ResolveNamed<IRouteEngine>(router);
            _context = context;
            _destinationStore = destinationStore;
            _data = data;
            _scope = scope;
            Resources = new ObservableCollection<SimResource>();
        }

        protected override void OnPrepare()
        {
            // create a list of actions associated with each object type arriving from the queue
            //MsgHandler.AddHandler<IncidentUpdate>(IncidentUpdateHandler);

            // get the store to load its roster
            _rosterStore.LoadRoster(_context.StartDate, _context.EndDate);
       
        }

        protected override void OnStart()
        {
            QueueNextCheck(0);
        }

        protected override void OnStop()
        {
        }


        /// <summary>
        /// queue a check to adjust the number of vehicles on duty
        /// </summary>
        private void QueueNextCheck(int mins)
        {
            DateTime nextcheck = EventQueue.Now.AddMinutes(mins);
            SetTimedEvent("Roster-Mark", nextcheck, ()=> RosterCheck());
        }


        private void RosterCheck()
        {
            ProcessRoster();
            QueueNextCheck(60);
        }

        /// <summary>
        /// returns true if MDT could be powered on ok
        /// </summary>
        /// <param name="mdt"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private bool PowerOn(SimResource mdt, VehicleRoster r, bool showMessage)
        {
            mdt.Callsign = $"A{_callsign++}";
            mdt.PoweredOn = true;
            mdt.OnDuty = true;
            mdt.VehicleType = r.VehicleType;
            mdt.Status = ResourceStatus.Available;
            mdt.Destination = null;
            mdt.Position = r.StartPosition;
            mdt.Callsign = r.Callsign;

            if (_router != null)
            {
                mdt.StandbyPoint = FindNearestDestination(mdt);
                mdt.RoutingPoint = mdt.Position;

                StartMDT msg = new StartMDT { Resource = mdt };
                ServiceBusClient.Broadcast(msg);

             //   if (showMessage)
                    Logger.Write("Powered on vehicle " + mdt.Callsign);

                return true;
            }

            return false;
        }
        

        private void BringOnDuty(IEnumerable<VehicleRoster> OnDutyvehicles)
        {
            StringBuilder sysMessage = new StringBuilder(".. On duty..");

            // find all callsigns that dont exist in the current list of online vehicles
            foreach (VehicleRoster r in OnDutyvehicles)
            {
                var mdts = from mdt in Resources where mdt.Callsign == r.Callsign && mdt.PoweredOn==true && mdt.OnDuty==true select mdt;
                if (mdts.Count() == 0)
                {
                    // this vehicle needs to come on duty.
                    var resource = _resManager.MakeResource(r.Callsign, r.StartPosition, r.VehicleType);

                    // and power it up
                    if (_router != null)
                        resource.StandbyPoint = FindNearestDestination(resource);
                    
                    PowerOn(resource, r, false);
                }
            }

            Logger.Write(sysMessage.ToString());
        }

        private void BringOffDuty(IEnumerable<VehicleRoster> OnDutyvehicles)
        {
            StringBuilder sysMessage = new StringBuilder(".. Off duty..");

            // go through the list and power down  MDTs not required anymorem, 
            // i.e. there is a powered on mdt with a callsign that is not in the roster list

            var query = from active in Resources
                        where active.PoweredOn == true
                        join required in OnDutyvehicles on active.Callsign equals required.Callsign into gj
                        from subActual in gj.DefaultIfEmpty()
                        select new { active, required = subActual };

            foreach (var q in query)
            {
                if (q.required == null)
                {
                    sysMessage.Append(q.active.Callsign + " ");

                    // this will signal the MDT to shut down when it finishes a job
                    q.active.OnDuty = false;

                    // not required..
                    if (q.active.Status == ResourceStatus.Available)
                    {
                        ServiceBusClient.Broadcast(new ShutdownMDT { Resource = q.active });
                    }
                }
            }

            Logger.Write(sysMessage.ToString());

        }

        private void ProcessRoster()
        {
            string sysMessage;

            DateTime currentHour = new DateTime(EventQueue.Now.Year, EventQueue.Now.Month, EventQueue.Now.Day,
                EventQueue.Now.Hour, 0, 0);

            List<VehicleRoster> OnDutyvehicles = _rosterStore.GetRoster(EventQueue.Now);

            BringOnDuty(OnDutyvehicles);

            BringOffDuty(OnDutyvehicles);

            var mdts = from mdt in Resources where mdt.PoweredOn == true && mdt.OnDuty == true select mdt;

            sysMessage = string.Format("Need {0} resources on duty at time {1}, {2} are actually on duty",
                OnDutyvehicles.Count(), currentHour.ToString("dd MM yyyy HH:mm"), mdts.Count());
            Logger.Write(sysMessage);
        }

        private SimDestination FindNearestDestination(SimResource resource)
        {
            SimDestination nearest = null;

            var start = _data.GetEdgeFromPoint(resource.Position);

            RouteRequestMultiple request = new RouteRequestMultiple()
            {
                RoadSpeedCalculator = _roadSpeedCalculator,
                StartLocation = start,
                EndLocations = _destinationStore.GetDestinations(false, true, true).Select(x => x.RoadPosition).ToList(),
                DistanceMax = double.MaxValue,
                DurationMax = double.MaxValue,
                InstanceMax = 1, 
                VehicleType = resource.VehicleType,
                SearchType = RouteSearchType.Quickest,
                HourOfWeek = EventQueue.Now.HourOfWeek(),
                Map = null
            };
                            
            var result = _router.CalculateRouteMultiple(request);

            if (result == null || result.Items.Count() == 0)
                return null;

            var bestLocation = result.Items[0].EndEdge;
            nearest = _destinationStore.GetDestinations(false, true, true).First(x => x.RoadPosition== bestLocation);

            //
            return nearest;
        }

    }
}
