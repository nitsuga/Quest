using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using Quest.Lib.Trace;
using Quest.Common.Simulation;
using Quest.Lib.Utils;
using Quest.Lib.Processor;
using Quest.Lib.Routing;
using Quest.Lib.Simulation.Probability;
using Autofac;
using Quest.Lib.ServiceBus;
using Quest.Common.ServiceBus;
using Quest.Common.Messages;
using GeoAPI.Geometries;
using Quest.Lib.Simulation.Destinations;

namespace Quest.Lib.Simulation.Resources
{

    /// <summary>
    /// This class simulates the behaviour of both Ambulances and Fast Response vehicles.
    /// The class responds to new incident events sent from the CAD module and then navigates
    /// toward the incident. The module also uses the ProbabilityEngine to wait on scene
    /// for a period of time determined by the CDF. The ambulances may convey to the 
    /// nearest hospital and then back to the nearest standby point.
    /// </summary>
    public class VehicleSimulator : ServiceBusProcessor
    {
        private const string DATETIMEFORMAT = "dd MMM yyyy HH:mm:ss";
        /// <summary>
        /// random number generator 
        /// </summary>
        private Random _rnd;

        private IRouteEngine _router;
        /// <summary>
        /// hold routes for all vehicles
        /// </summary>
        private RouteSet _routes = new RouteSet();
        private OnSceneProbability _OSprobability;
        private RoutingData _data;
        private RosterSimulator _resManager;
        private IDestinationStore _destinationStore;
        
        // not used normally.
        public double _randomMovementRange { get; set; } = 0;
        public double _randomOnSceneRangeStart { get; set; } = 20 * 60;
        public double _randomOnSceneRangeStop { get; set; } = 30 * 30;
        public double _randomAtHospRangeStart { get; set; } = 20 * 60;
        public double _randomAtHospRangeStop { get; set; } = 30 * 30;
        public bool _noShutdown { get; set; } = false;

        private ILifetimeScope _scope;

        public VehicleSimulator(
            RosterSimulator resManager,
            IRouteEngine router,
            IDestinationStore destinationStore,
            RoutingData data,
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue,serviceBusClient, msgHandler)
        {
            _resManager = resManager;
            _router = router;
            _destinationStore = destinationStore;
            _data = data;
            _scope = scope;
        }

        /// <summary>
        /// initialise the MDT simulator
        /// </summary>
        /// <param name="parent"></param>
        protected override void OnPrepare()
        {
            Logger.Write("Vehicle Simulator starting");

            MsgHandler.AddHandler<StartMDT>(StartMDT);
            MsgHandler.AddHandler<MDTIncident>(Incident);
            MsgHandler.AddHandler<CancelIncident>(CancelIncident);
            MsgHandler.AddHandler<ResMessage>(ResMessage);
            MsgHandler.AddHandler<CallsignUpdate>(CallsignUpdate);
            MsgHandler.AddHandler<SetStatus>(SetStatus);
            MsgHandler.AddHandler<NavigateTo>(NavigateTo);

            // set up task queue and create a random config
            _rnd = new Random(DateTime.Now.Millisecond);

            _OSprobability = new OnSceneProbability();
          
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
            ShutdownMDT(0, "Stop Simulation");
        }

        /// <summary>
        /// read an integer value from the profile
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private int GetInt(Dictionary<String, String> parameters, string name, int defaultValue)
        {
            int result = 0;
            string value = defaultValue.ToString();
            result = defaultValue;

            if (parameters != null)
                parameters.TryGetValue(name, out value);

            int.TryParse(value, out result);
            return result;
        }

        /// <summary>
        /// queues a request for the router to calculate the route or waits another 5 seconds
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private int MakeRouteToDestination(SimResource resource)
        {
            // calc distance and ETA
            resource.TTG = 0;
            resource.DTG = 0;
            resource.LastChanged = DateTime.Now;

            SetSysMessage(resource, string.Format("Calculating route to   :  {0} {1}", resource.ResourceId, resource.Destination.Name));

            Coordinate startPoint = resource.Position;
            Coordinate endPoint = new Coordinate(resource.Destination.X, resource.Destination.Y);

            var result = _router.CalculateQuickestRoute(new RouteRequest
            {
                FromLocation = startPoint,
                ToLocation = endPoint,
                VehicleType = resource.VehicleType,
                HourOfWeek = EventQueue.Now.Hour
            });

            SetRoute(resource, result);

            return resource.GPSFrequency;
        }

        /// <summary>
        /// set a vehicle at a random position within range of the standby point
        /// </summary>
        /// <param name="Resource"></param>
        private void SetRandomPosition(SimResource Resource)
        {
            if (Resource.Destination == null)
            {
                if (Resource.StandbyPoint == null)
                {
                    var easting = _data.Bounds.MinX + (int)(_rnd.NextDouble() * _data.Bounds.Width);
                    var northing = _data.Bounds.MinY + (int)(_rnd.NextDouble() * _data.Bounds.Height);
                    Resource.Position = new Coordinate(easting, northing);
                }
                else
                {
                    var easting = Resource.StandbyPoint.X + (int)((_rnd.NextDouble() - 0.5) * 2000);
                    var northing = Resource.StandbyPoint.Y + (int)((_rnd.NextDouble() - 0.5) * 2000);
                    Resource.Position = new Coordinate(easting, northing);
                }
            }
            else
            {
                var easting = Resource.Destination.X + (int)((_rnd.NextDouble() - 0.5) * 2000);
                var northing = Resource.Destination.Y + (int)((_rnd.NextDouble() - 0.5) * 2000);
                Resource.Position = new Coordinate(easting, northing);
            }

            MoveVehicleTowardsTarget(Resource);

            // this will cause a new route to be calculated
            _routes.Remove(Resource.ResourceId);

            Resource.LastChanged = DateTime.Now;
        }

        /// <summary>
        /// fires when the timer says we should have arrived at our next waypoint - Move the vehicle towards the destination and then do something
        /// </summary>
        /// <param name="te"></param>
        private void NextNavigationEventHandler(SimResource resource)
        {

            // diregard if powered off.
            if (!resource.PoweredOn)
                return ;

            if (!IsAtDestination(resource))
            {
                MoveVehicleTowardsTarget(resource);
                return ;
            }


            // have we arrived at the incident ?
            switch (resource.Status)
            {
                case ResourceStatus.Enroute:
                    VehicleArrivedAtIncident(resource);
                    break;
                case ResourceStatus.Convey:
                    VehicleArrivedAtHospital(resource);
                    break;
                case ResourceStatus.OnScene:
                    VehicleArrivedAtIncident(resource);
                    break;
                case ResourceStatus.Available:
                    VehicleArrivedAtStandby(resource);
                    break;
                default:
                    Logger.Write(string.Format("NextNavigationEvent: Unrecognised status and at destination {0} Status='{1}' Destination={2}", resource.ResourceId, resource.Status, resource.Destination.Name));

                    break;
            }
            return ;
        }

        /// <summary>
        /// calc position of new veicle
        /// </summary>
        /// <param name="resource"></param>
        private void CalcNewLocation(SimResource resource)
        {
            double dx = resource.Destination.X- resource.Position.X;
            double dy = resource.Destination.Y - resource.Position.Y;

            if (dx < 50 && dy < 50)
            {
                resource.Position = new Coordinate(resource.Destination.X, resource.Destination.Y); 
            }
            else
            {
                if (dx != 0)
                    resource.Position.X += (int)(dx / Math.Abs(dx) * 25.0);

                if (dy != 0)
                    resource.Position.Y += (int)(dy / Math.Abs(dy) * 25.0);
            }
            resource.LastChanged = DateTime.Now;
        }

        /// <summary>
        /// Calculate the next location.. 
        /// 1) builds the route if one has not been calculated yet 
        /// 2) waits 5 seconds if the routing engine is not yet initialised
        /// 3) We have a list of connections, each contaiing a list of points. We iterate through
        /// this list until we have a movement lasting around "GPSFrequency" seconds
        /// </summary>
        /// <returns></returns>
        private double MoveToNextWayPoint(SimResource Resource)
        {
            double seconds = 0;

            // do we have a route - if not we may need to calculate one
            if (!_routes.ContainsKey(Resource.ResourceId))
            {
                // no route.. is there a destination?
                if (Resource.Destination != null)
                    return MakeRouteToDestination(Resource);   // yes - make a request to calculate the route
                else
                    return Resource.GPSFrequency;                // no destinate and no route - check again in 15 seconds
            }

            // move through the existing route for GPSFrequency seconds
            RoutingResult route = _routes[Resource.ResourceId];
            var conn = route.Connections[0];

            while (seconds < Resource.GPSFrequency)
            {
                while (true)
                {
                    // no more Connections? 
                    if (route.Connections.Count == 0)
                    {
                        Resource.LastChanged = DateTime.Now;
                        return Resource.GPSFrequency;              // check again in 15 seconds
                    }

                    Resource.RoutingPoint = route.EndEdge.Coord;

                    // we have a connection, do we have any trackpoints left? if not get rid of this/
                    // connection and move onto the next one.
                    if (route.Connections[0].Edge.Geometry.Count <= 0)
                        // pop off the top one
                        route.Connections.RemoveAt(0);
                    else
                        break;  // out of the loop...as we have trackpoints
                }

                conn = route.Connections[0];
                var rp = conn.Edge.Geometry.GetCoordinateN(0);

                // calculate distance along the route
                int meters = (int)Math.Sqrt(
                    (Resource.Position.X - rp.X) * (Resource.Position.X - rp.X)
                            +
                    (Resource.Position.Y - rp.Y) * (Resource.Position.Y - rp.Y)
                    );

                // cartesian to polar 
                double dx = Resource.Position.X - rp.X;
                double dy = Resource.Position.Y - rp.Y;
                double r = Math.Sqrt(dx * dx + dy * dy);
                double theta;

                if (dx == 0)
                    theta = 0;
                else
                    if (dx > 0)
                    theta = Math.Asin(dy / r);
                else
                    theta = -Math.Asin(dy / r) + Math.PI;

                // set this new location
                Resource.Heading = theta * 360 / (2 * Math.PI);
                Resource.Position.X = rp.X;
                Resource.Position.Y = rp.Y;
                Resource.LastChanged = DateTime.Now;
                Resource.Location = conn.Edge.RoadName;

                // 
                seconds += r / conn.Vector.DistanceMeters * conn.Vector.DurationSecs;

                // and pop off this track point
                //TODO!!!!
                //conn.Edge.Geometry.RemoveAt(0);
            }

            // calc ETA's
            Resource.DTG = route.Connections.Sum(x => x.Vector.DistanceMeters);
            Resource.TTG = route.Connections.Sum(x => x.Vector.DurationSecs);
            Resource.Speed = Constants.Constant.ms2mph * conn.Vector.DistanceMeters / conn.Vector.DurationSecs;

            return seconds;
        }

        /// <summary>
        /// set the route this vehicle is going to take. This method caches the route in the
        /// _routes table
        /// </summary>
        /// <param name="Resource"></param>
        /// <param name="result"></param>
        void SetRoute(SimResource Resource, RoutingResponse routes)
        {
            var result = routes.Items[0];
            if (_routes.ContainsKey(Resource.ResourceId))
                _routes.Remove(Resource.ResourceId);

            if (result != null)
            {
                _routes.Add(Resource.ResourceId, result);
                if (result.Connections.Count() == 0)
                {
                    SetSysMessage(Resource, string.Format("Route complete         :  {0} no nodes  ", Resource.Callsign));
                }
                else
                {
                    SetSysMessage(Resource, string.Format("Route complete {0} Nodes  ", result.Connections.Count()));
                }
            }
            else
            {
                SetSysMessage(Resource, string.Format("Route not found - using Pythag"));
            }
        }

        void SetSysMessage(SimResource res, string txt)
        {
            res.SysMessage = txt;
            res.LastSysMessage = EventQueue.Now.ToString(DATETIMEFORMAT);
            Logger.Write(res.Callsign + " OnDuty=" + res.OnDuty.ToString() + " " + txt);
        }

        void SetTxtMessage(SimResource res, string txt)
        {
            res.TxtMessage = txt;
            res.LastTxtMessage = EventQueue.Now.ToString(DATETIMEFORMAT);

        }

        void SetMdtMessage(SimResource res, string txt)
        {
            res.MdtMessage = txt;
            res.LastMdtMessage = EventQueue.Now.ToString(DATETIMEFORMAT);
        }

        /// <summary>
        /// moves the vehicle to the next waypoint toword its target
        /// </summary>
        /// <param name="vehicle"></param>
        private void MoveVehicleTowardsTarget(SimResource resource)
        {
            // no destination? go to standby point
            if (resource.Destination == null)
            {
                // go to default standby point
                GotoStandbyPoint(resource, null, "MoveVehicleTowardsTarget .. no destination");
                return;
            }

            // get next waypoint and then set up a timer that fires when we should have got there
            double nextreportseconds = MoveToNextWayPoint(resource);

            // keep moving towards the target

            //NextNavigationEvent data = new NextNavigationEvent();
            //TimeSpan ts = new TimeSpan(0, 0, (int)nextreportseconds);
            //new TaskEntry(EventQueue, new TaskKey("Res-" + Resource.ResourceId.ToString()), NextNavigationEventHandler, data, ts);

            SetTimedEvent($"Res-{resource.ResourceId}", EventQueue.Now.AddSeconds(nextreportseconds), () => NextNavigationEventHandler(resource));

            SendCurrentPosition(resource);

            Logger.Write(string.Format("{0} MoveVehicleTowardsTarget: {1}/{2} {3} ({4}/{5}) Next in {6} seconds ETA={7} distance={8}", resource.Callsign, resource.Position.X.ToString("000000"), resource.Position.Y.ToString("000000"), resource.Destination.Name, resource.Destination.X.ToString("000000"), resource.Destination.Y.ToString("000000"), nextreportseconds, resource.TTG, resource.DTG));
        }

        /// <summary>
        /// send the position of the Vehicle to C&C
        /// </summary>
        /// <param name="vehicle"></param>
        void SendCurrentPosition(SimResource resource)
        {
            // RJP: TODO Can't see what this is doing
            //string hospital = FindNearestDestination(vehicle, _parent.Config.Hospitals);

            // send current position
            SATNAVLocation details = new SATNAVLocation()
            {
                ResourceId = resource.ResourceId,
                Position = resource.Position,
                EtaDistance = resource.DTG,
                EtaSeconds = resource.TTG,
                Direction = resource.Heading / 45,
                Speed = resource.Speed
            };

            try
            {
                SetMdtMessage(resource, $"<< SATNAVLocation {resource.Position.X} {resource.Position.Y}");

                ServiceBusClient.Broadcast(details);
            }
            catch (Exception ex)
            {
                Logger.Write($"SATNAVLocation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Crew have arrived at the incident, decide how long to stay
        /// </summary>
        /// <param name="resource"></param>
        private void VehicleArrivedAtIncident(SimResource resource)
        {
            Debug.Assert(resource.Incident != null);

            var msg = resource.Callsign + " Vehicle Arrived At Inc: " + resource.Incident.IncidentId.ToString() + " " + resource.Incident.Position.ToString() + " Current location " + resource.Position.X.ToString() + "/" + resource.Position.Y.ToString();
            Logger.Write(msg);

            // record action in database
            ServiceBusClient.Broadcast(new UpdateAssignmentRecord { ResourceId = resource.ResourceId, IncidentId = resource.Incident.IncidentId, Message = "At Incident", Status= ResourceStatus.OnScene });

            SetSysMessage(resource, "Arrived on Scene");

            resource.LastChanged = DateTime.Now;
            resource.TTG = 0;
            resource.DTG = 0;
            resource.Speed = 0;

            SendCurrentPosition(resource);
            SendAtDestinationToCad(resource, DestType.Incident);
            SendStatusToCad(resource, ResourceStatus.OnScene, resource.NonConveyCode, null);

            TaskKey tk = new TaskKey("Res-" + resource.ResourceId.ToString());

            {
                // if this is the first vehicle then determine
                int secondsOnScene = _OSprobability.GetAtSceneTime(resource.Incident.AMPDS, EventQueue.Now, resource.VehicleType);

                if (secondsOnScene == 0)
                {
                    secondsOnScene = (int)(_randomOnSceneRangeStart + (_rnd.NextDouble() * (_randomOnSceneRangeStop - _randomOnSceneRangeStart)));
                    SetSysMessage(resource, "AMPDS Code probably unknown so random onscene time of " + secondsOnScene.ToString() + " seconds will be used");
                }

                SetTimedEvent($"Res-{resource.ResourceId}", EventQueue.Now.AddSeconds(secondsOnScene), () => OnsceneComplete(resource));

                SetSysMessage(resource, $"first on scene, will leave in {secondsOnScene} seconds ");
            }
        }

        /// <summary>
        /// crew have dealt with the incident and now either go GM or to Hospital
        /// </summary>
        /// <param name="te"></param>
        private void OnsceneComplete(SimResource resource)
        {
            Logger.Write(resource.Callsign + " Onscene completed:        " + resource.Incident.IncidentId.ToString());

            /// the incident has probably been conveyed or cancelled . go to standby point
            /// TODO: Check this in full simulator
            if (resource.Incident.Status > ResourceStatus.OnScene)
            {
                resource.NonConveyCode = 9001;
                // go to default standby point
                GotoStandbyPoint(resource, null, "OnsceneComplete but not onscene");

                return ;
            }

            // If vehicle is an Ambulance then see if we are going to convey
            if (resource.VehicleType == "AEU")
            {
                if (resource.Incident.WillConvey == false)                                   //TODO: nationally this is about right
                {
                    // we wont take this patient to hospital
                    // TODO: RJP This is where we want a non convey code.
                    resource.NonConveyCode = 9001;
                    // go to default standby point
                    GotoStandbyPoint(resource, null, "OnsceneComplete but wont convey");
                }
                else
                    GotoNearestHospital(resource);
            }
            else
            {
                // We are not going to convey the patient
                // go to default standby point
                GotoStandbyPoint(resource, null, "OnsceneComplete I'm an FRU");
            }
            return ;
        }

        /// <summary>
        /// Crew have arrived at the incident, decide how long to stay
        /// </summary>
        /// <param name="resource"></param>
        private void VehicleArrivedAtHospital(SimResource resource)
        {
            Logger.Write(resource.Callsign + " Vehicle Arrived At Hosp: " + resource.Incident.IncidentId.ToString() + " " + resource.Incident.Position + " Current location " + resource.Position.X.ToString() + "/" + resource.Position.Y.ToString());

            // record action in database
            ServiceBusClient.Broadcast(new UpdateAssignmentRecord { ResourceId = resource.ResourceId, IncidentId = resource.Incident.IncidentId, Status = ResourceStatus.Hospital });

            resource.LastChanged = DateTime.Now;
            SetSysMessage(resource, "At hospital");
            resource.TTG = 0;
            resource.DTG = 0;
            resource.Speed = 0;

            SendCurrentPosition(resource);
            SendAtDestinationToCad(resource, DestType.Hospital);
            SendStatusToCad(resource, ResourceStatus.Hospital, resource.NonConveyCode, null);

            // how long to remain at scene? queue up a task to fire when done.
            int secondsOnScene = _OSprobability.GetHospitalTime(resource.Incident.AMPDS, EventQueue.Now, resource.VehicleType);

            if (secondsOnScene == 0)
            {
                secondsOnScene = (int)(_randomOnSceneRangeStart + (_rnd.NextDouble() * (_randomAtHospRangeStop - _randomAtHospRangeStart)));
            }

            DateTime finishedAt = EventQueue.Now.Add(new TimeSpan(0, 0, secondsOnScene));

            SetSysMessage(resource, "will leave hospital at " + finishedAt.ToString());

            
            SetTimedEvent($"Res-{resource.ResourceId}", EventQueue.Now.AddSeconds(secondsOnScene), () => HospitalFinished(resource));


        }

        /// <summary>
        /// Crew have arrived at the incident, decide how long to stay
        /// </summary>
        /// <param name="resource"></param>
        private void VehicleArrivedAtStandby(SimResource resource)
        {
            Logger.Write(resource.Callsign + " VehicleArrivedAtStandby");
            resource.TTG = 0;
            resource.DTG = 0;
            resource.Speed = 0;

            resource.LastChanged = DateTime.Now;
            SetSysMessage(resource, "Arrived @ standby");

            // record action in database
            ServiceBusClient.Broadcast(new UpdateAssignmentRecord { ResourceId = resource.ResourceId, IncidentId = resource.Incident.IncidentId, Status = ResourceStatus.Available, Message="Standby" });

            SendCurrentPosition(resource);
            SendAtDestinationToCad(resource, DestType.Other);
            SendStatusToCad(resource, ResourceStatus.Available, resource.NonConveyCode, null);

            // if we are implementing random movement then find another standby point to go to
            if (_randomMovementRange > 0)
            {
                String msg = "";
                // select a list of candidate destination standby points within the specified range
                var candidates = from dest in _destinationStore.GetDestinations(false, false, true)
                                 where 
                                 Math.Sqrt(Math.Pow(dest.Y - resource.Position.Y, 2) + Math.Pow(dest.X - resource.Position.X, 2)) < _randomMovementRange
                                 select dest;

                if (candidates.Count() > 0)
                {
                    SimDestination d = candidates.ToArray()[(int)(_rnd.NextDouble() * candidates.Count())];

                    msg = "VehicleArrivedAtStandby:  " + resource.Callsign + " will move to random SBP " + d.Name;

                    NewDestination(resource, d);
                    MoveVehicleTowardsTarget(resource);
                }
                else
                {
                    msg = "VehicleArrivedAtStandby:  " + resource.Callsign + " would move but no nearby standby points";
                }

                SetSysMessage(resource, msg);

                Logger.Write(msg);
            }

        }

        /// <summary>
        /// crew have dropped off the patient, go back to standby point
        /// </summary>
        /// <param name="te"></param>
        private void HospitalFinished(SimResource resource)
        {
            Logger.Write(resource.Callsign + " HospitalFinished:         " + resource.DestHospital);

            SetSysMessage(resource, string.Format("Leaving hospital"));

            resource.DestHospital = "";
            resource.TTG = 0;
            resource.DTG = 0;

            // go to default standby point
            GotoStandbyPoint(resource, null, "HospitalFinished");
            return;
        }

        /// <summary>
        /// directs a resource to the given standby point by calculating a route. If 
        /// no standbypoint is given it will pick one.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="standbyPoint"></param>
        /// <param name="Description"></param>
        private void GotoStandbyPoint(SimResource resource, SimDestination standbyPoint, String Description)
        {
            Logger.Write(resource.Callsign + " GotoStandbyPoint:         because " + Description);

            if (resource.StandbyPoint == null)
            {
                resource.StandbyPoint = FindNearestDestination(resource, false, true);

                if (resource.StandbyPoint == null)
                {
                    Logger.Write(resource.Callsign + " GotoStandbyPoint:         failed to find suitable standby point");
                    return;
                }

                standbyPoint = resource.StandbyPoint;

            }

            Logger.Write(resource.Callsign + " GotoStandbyPoint:         Selected " + resource.StandbyPoint.Name);

            if (standbyPoint != null)
                NewDestination(resource, standbyPoint);
            else
                // go Green Mobile and go to nominated standby Point name
                NewDestination(resource, resource.StandbyPoint);

            if (resource.Destination != null)
                MoveVehicleTowardsTarget(resource);

            // clearout the incident
            SetSysMessage(resource, "To standby " + resource.StandbyPoint.Name);

            resource.LastChanged = DateTime.Now;

            // clear down any incident he was working on
            SendStatusToCad(resource, ResourceStatus.Available, resource.NonConveyCode, null);

            // the vehicle has gone off duty to cleanup.
            if (resource.OnDuty == false && _noShutdown == false)
            {
                Logger.Write(resource.Callsign + String.Format(" GotoStandbyPoint:         {0} marked as off duty..shutting down", resource.Callsign));
                ShutdownMDT(resource.ResourceId, "Resource is marked as off duty");
                return;
            }
        }

        /// <summary>
        /// go to the nearest hospital. This does not take into consideration the type of illness.
        /// </summary>
        /// <param name="resource"></param>
        private void GotoNearestHospital(SimResource resource)
        {
            Logger.Write(resource.Callsign + " Goto nearest Hospital:    " + resource.Incident.IncidentId.ToString() + " " + resource.Incident.Position.ToString() + " Current location " + resource.Position.X.ToString() + "/" + resource.Position.Y.ToString());

            // pick a hospital
            SimDestination hospital = FindNearestDestination(resource, true, false);

            SetSysMessage(resource, "To hospital " + hospital.Name);
            resource.DestHospital = hospital.Name;
            NewDestination(resource, hospital);
            MoveVehicleTowardsTarget(resource);
            SendStatusToCad(resource, ResourceStatus.Hospital, resource.NonConveyCode, hospital);
        }

        /// <summary>
        /// find the nearest destination in the list to this Vehicle 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="destinations"></param>
        /// <returns></returns>
        private SimDestination FindNearestDestination(SimResource resource, Boolean includeHospitals, Boolean includeStandbys)
        {
            SimDestination nearest = null;
            string sysMessage = string.Format("{0} Calculating nearest destination", resource.Callsign);

            Logger.Write(sysMessage);

            // build up an array of RoutingLocations
            List<EdgeWithOffset> destinations = new List<EdgeWithOffset>();

            foreach (SimDestination d in _destinationStore.GetDestinations(true, false, true)
                .Where(x => (x.IsHospital == true && includeHospitals == true))
                .Where(x => (x.IsStandby == true && includeStandbys == true))
                )
                destinations.Add(d.RoadPosition);

            var start = _data.GetEdgeFromPoint(resource.Position);

            RouteRequestMultiple request = new RouteRequestMultiple()
            {
                StartLocation = start,
                EndLocations = destinations,
                DistanceMax = double.MaxValue,
                DurationMax = double.MaxValue,
                InstanceMax = 1,
                VehicleType = resource.VehicleType,
                SearchType = RouteSearchType.Quickest,
                HourOfWeek = EventQueue.Now.HourOfWeek(),
                Map = null
            };

            RoutingResponse result = _router.CalculateRouteMultiple(request);

            if (result == null || result.Items.Count() == 0)
                return null;

            nearest = _destinationStore.GetDestinations(true, true, true).FirstOrDefault(x=>x.RoadPosition== result.Items[0].EndEdge);

            if (nearest != null)
                Logger.Write("FindNearestDestination:   " + resource.Callsign + " - selected '" + nearest.Name + "'");
            else
                Logger.Write("FindNearestDestination:   " + resource.Callsign + " - none selected ");
            return nearest;
        }

        /// <summary>
        /// return true if the current vehicle location is within the allowed range of the destination
        /// </summary>
        /// <param name="Resource"></param>
        /// <returns></returns>
        private bool IsAtDestination(SimResource Resource)
        {
            if (_routes.ContainsKey(Resource.ResourceId) == false)
                return false;

            if (Resource.Destination == null)
                return false;

            // vehicle has tracked to its destination (or had no destination
            if (_routes[Resource.ResourceId] == null)
                return true;

            if (_routes[Resource.ResourceId].Connections.Count == 0)
                return true;

            int distance = (int)Math.Sqrt(
                ((Resource.Destination.X - Resource.Position.X) * (Resource.Position.X - Resource.Position.X)) +
                ((Resource.Destination.Y - Resource.Position.Y) * (Resource.Position.Y - Resource.Position.Y))
                );

            return (Resource.AtDestinationRange >= distance);
        }

        private Response RespondToNewIncident(NewMessageArgs te)
        {
            SimResource resource = ((RespondToNewIncident)te.Payload).Resource;
            RespondToNewIncident(resource);
            return null;
        }

        /// <summary>
        /// sets the destination to the new incident location and starts navigating toward it.
        /// </summary>
        /// <param name="resource"></param>
        private void RespondToNewIncident(SimResource resource)
        {

            if (resource.Incident == null)
            {
                Logger.Write(resource.Callsign + " RespondToNewIncident:     Incident=NULL");
                GotoStandbyPoint(resource, null, "RespondToNewIncident but no incident");       // go to default standby point
                return;
            }

            // change status to Amber to Scene
            if (resource.IncidentAccept == true)
            {
                Logger.Write(resource.Callsign + " RespondToNewIncident:     Accepted " + resource.Incident.IncidentId);

                // record action in database
                ServiceBusClient.Broadcast(new UpdateAssignmentRecord { ResourceId = resource.ResourceId, IncidentId = resource.Incident.IncidentId, Status = ResourceStatus.Dispatched });

                SendStatusToCad(resource, ResourceStatus.Enroute, resource.NonConveyCode, null);
                NewDestination(resource, resource.Destination);
                MoveVehicleTowardsTarget(resource);
            }
            else
            {
                Logger.Write("RespondToNewIncident: " + resource.ResourceId.ToString() + " Rejected");
                resource.LastChanged = DateTime.Now;

                // start driving to standby point
                NewDestination(resource, resource.StandbyPoint);

                SendStatusToCad(resource, ResourceStatus.Available, resource.NonConveyCode, null);
            }

        }

        /// <summary>
        /// general routing for a new destination (of any kind).
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="d"></param>
        private void NewDestination(SimResource resource, SimDestination d)
        {
            // this causes the engine to calculate a new route
            if (_routes.ContainsKey(resource.ResourceId))
                _routes.Remove(resource.ResourceId);

            if (d != null)
                resource.Destination = d;

            if (resource.Position.X == 0 || resource.Position.Y == 0)
                SetRandomPosition(resource);

            MoveVehicleTowardsTarget(resource);

            resource.LastChanged = DateTime.Now;
        }

        /// <summary>
        ///  set a new destination for this vehicle
        /// </summary>
        /// <param name="Resource"></param>
        /// <param name="name"></param>
        private void NewDestination(SimResource Resource, string name)
        {
            Logger.Write(Resource.Callsign + " SetDestination:           " + name);

            SimDestination d = _destinationStore.GetDestinations(true,true,true).Where(x => x.Name == name).FirstOrDefault();
            NewDestination(Resource, d);
        }

        private void SendStatusToCad(SimResource resource, ResourceStatus status, int nonConveyCode, SimDestination destHospital)
        {
            if (destHospital != null)
                Logger.Write(resource.Callsign + " SetStatus:                " + status + " " + destHospital.Name);
            else
                Logger.Write(resource.Callsign + " SetStatus:                " + status);

            resource.Status = status;

            if (destHospital != null)
                resource.DestHospital = destHospital.Name;

            StatusChange change = new StatusChange();
            change.Status = status;
            change.NonConveyCode = nonConveyCode;

            if (destHospital != null)
                change.DestHospital = destHospital.Name;

            change.ResourceId = resource.ResourceId;
            change.Position = resource.Position;

            resource.NonConveyCode = 0;

            SetMdtMessage(resource, "<< StatusChange");

            ServiceBusClient.Broadcast(change);

        }

        private void SendAtDestinationToCad(SimResource resource, DestType destType)
        {
            resource.LastChanged = DateTime.Now;
            SetSysMessage(resource, "At Destination");

            AtDestination atDestinationDetails = new AtDestination();
            atDestinationDetails.ResourceId = resource.ResourceId;
            atDestinationDetails.ConveyCode = resource.NonConveyCode;
            atDestinationDetails.DestType = destType;    //TODO: wrong type, should be int (Now fixed RJP)
            atDestinationDetails.Position = resource.Position;
            atDestinationDetails.EventCode = "";

            SetMdtMessage(resource, "<< AtDestination");

            // send to cad
            ServiceBusClient.Broadcast(atDestinationDetails);
        }

        private void SendLogonToCad(SimResource resource)
        {
            Login loginDetails = new Login();
            loginDetails.ResourceId = resource.ResourceId;
            loginDetails.TimeStamp = DateTime.Now;

            SetMdtMessage(resource, "<< Login");
            ServiceBusClient.Broadcast(loginDetails);
        }

        private void SendLogoutToCad(SimResource resource)
        {

            Logout details = new Logout();
            details.ResourceId = resource.ResourceId;
            details.TimeStamp = DateTime.Now;

            try
            {
                SetMdtMessage(resource, "<< Logout");
                ServiceBusClient.Broadcast(details);
            }
            catch (Exception ex)
            {
                Logger.Write("SendLogoutToCad failed: " + ex.Message);
            }

        }

        private void SendSkillLevelToCad(SimResource resource)
        {

            resource.LastChanged = DateTime.Now;
            SkillLevel SkillLevel = new SkillLevel();

            SkillLevel.ResourceId = resource.ResourceId;
            SkillLevel.Skill  = resource.Skill;

            try
            {
                SetMdtMessage(resource, "<< SkillLevel");
                ServiceBusClient.Broadcast(SkillLevel);
            }
            catch (Exception ex)
            {
                Logger.Write("SendSkillLevelToCad failed: " + ex.Message);
            }

        }

        #region ICadOut Members

        public Response StartMDT(NewMessageArgs args)
        {
            StartMDT(((StartMDT)args.Payload).Resource);
            return null;
        }


        public Response Incident(NewMessageArgs args)
        {
            Incident((MDTIncident)args.Payload);
            return null;
        }

        public void Incident(MDTIncident incidentDetails)
        {

            SimResource resource = _resManager.Resources.Where(x => x.ResourceId == incidentDetails.ResourceId).First();

            String msg = String.Format(resource.Callsign + "{0} Assigning resource:       {1} {2}/{3}->{4}/{5}",
                resource.Incident.IncidentId,
                resource.Position.X, resource.Position.Y,
                resource.Incident.Position.X, resource.Incident.Position.Y,
                resource.Incident.Category);

            Logger.Write(msg);

            if (resource != null)
            {
                SetMdtMessage(resource, ">> Incident");
                SetSysMessage(resource, "Got incident");

                EventQueue.Remove(new TaskKey("Res-" + resource.ResourceId.ToString()));

                // clear out movements tasks.

                // create a new destination object
                SimDestination d = new SimDestination();
                d.Name = "Incident " + resource.Incident.IncidentId.ToString();
                //d.Position = incidentDetails.IncidentDetails.Position;
                resource.Destination = d;

                RespondToNewIncident(resource);

                resource.LastChanged = DateTime.Now;
            }


        }

        public Response CancelIncident(NewMessageArgs args)
        {
            CancelIncident((CancelIncident)args.Payload);
            return null;
        }

        public void CancelIncident(CancelIncident CancelIncidentdetails)
        {

            SimResource resource = _resManager.Resources.Where(x => x.ResourceId == CancelIncidentdetails.ResourceId).First();
            if (resource != null)
            {
                String msg = " CancelIncident - current status is " + resource.Status.ToString();
                SetMdtMessage(resource, msg);

                // record action in database
                ServiceBusClient.Broadcast(new UpdateAssignmentRecord { ResourceId = resource.ResourceId, IncidentId = resource.Incident.IncidentId, Status = ResourceStatus.Available, Message="Cancelled" });

                Logger.Write(resource.Callsign + msg);
                resource.Incident = null;

                EventQueue.Remove(new TaskKey("Res-" + resource.ResourceId.ToString()));
                GotoStandbyPoint(resource, null, "CancelIncident - current status is " + resource.Status.ToString());       // go to default standby point

            }

        }

        public Response ResMessage(NewMessageArgs args)
        {
            ResMessage((ResMessage)args.Payload);
            return null;
        }

        public void ResMessage(ResMessage Messagedetails)
        {

            SimResource resource = _resManager.Resources.Where(x => x.ResourceId == Messagedetails.ResourceId).FirstOrDefault();
            if (resource != null)
            {
                SetMdtMessage(resource, ">> SendMessage");
                SetTxtMessage(resource, Messagedetails.Text);
                resource.LastMessagePriority = Messagedetails.Priority;
                resource.LastChanged = DateTime.Now;
            }

        }

        public Response CallsignUpdate(NewMessageArgs args)
        {
            CallsignUpdate((CallsignUpdate)args.Payload);
            return null;
        }

        public void CallsignUpdate(CallsignUpdate callsignDetails)
        {

            SimResource resource = _resManager.Resources.Where(x => x.ResourceId == callsignDetails.ResourceId).FirstOrDefault();
            if (resource != null)
            {
                SetMdtMessage(resource, ">> CallsignUpdate");
                resource.Callsign = callsignDetails.Callsign;
                // If the callsign is 4 chars then we need to send a Skill Level
                if (resource.Callsign.Length == 4)
                {
                    SendSkillLevelToCad(resource);
                }
                resource.LastChanged = DateTime.Now;

            }

        }

        public Response SetStatus(NewMessageArgs args)
        {
            SetStatus((SetStatus)args.Payload);
            return null;
        }

        public void SetStatus(SetStatus setStatusdetails)
        {
            SimResource resource = _resManager.Resources.Where(x => x.ResourceId == setStatusdetails.ResourceId).FirstOrDefault();
            if (resource != null)
            {
                SetMdtMessage(resource, ">> SetStatus");
                resource.Status = setStatusdetails.Status;
                resource.LastChanged = DateTime.Now;
            }
        }

        public Response NavigateTo(NewMessageArgs args)
        {
            NavigateTo((NavigateTo)args.Payload);
            return null;
        }

        /// <summary>
        /// Make an Vehicle start navigating toward a predefined destination (listed in Destinations array)
        /// </summary>
        /// <param name="ResourceId">The name of the Vehicle to start navigation on. If this is null or empty then 
        /// all MDT's will be navigated to the same destination
        /// </param>
        /// <param name="name"></param>
        public void NavigateTo(NavigateTo details)
        {
            SimDestination dest = _destinationStore.GetDestinations(false, false, true).Where(d => d.ID == details.DestinationId.ToString()).FirstOrDefault();

            Logger.Write(String.Format("IUserOut::NavigateTo: {0}-->{1} {2}", details.ResourceId, dest.Name, details.Reason));

            var vehicles = _resManager.Resources.Where(x => x.ResourceId == 0 || x.ResourceId == details.ResourceId);
            foreach (SimResource resource in vehicles)
            {
                if (resource.Status == ResourceStatus.Available)
                {
                    // record action in database
                    ServiceBusClient.Broadcast(new UpdateAssignmentRecord { ResourceId = resource.ResourceId, IncidentId = resource.Incident.IncidentId, Status = resource.Status, Message = "Navigate to " + dest.Name + " " + details.Reason });

                    NewDestination(resource, dest);
                    SetSysMessage(resource, string.Format("Navigate to {0}", dest.Name));
                }
            }
        }

        #endregion

        #region IUserOut Members

        /// <summary>
        /// Randomise the positions of all the vehicles
        /// </summary>
        public void RandomisePositions()
        {

            Logger.Write("RandomisePositions");

            foreach (SimResource config in _resManager.Resources)
                SetRandomPosition(config);


        }

        /// <summary>
        /// starts the Vehicle given or starts all Resources if no name is provided.
        /// </summary>
        /// <param name="ResourceId"></param>
        public void StartMDT(int ResourceId)
        {
            int milliseconds = 0;
            var Resource = _resManager.Resources.Where(x => ResourceId == 0 || x.ResourceId == ResourceId);
            foreach (SimResource vehicle in Resource)
            {
                if (vehicle.Enabled == true)
                {
                    // keep moving towards the target
                    TimeSpan ts = new TimeSpan(0, 0, 0, 0, milliseconds);
                    TaskEntry newte = new TaskEntry(EventQueue, new TaskKey(vehicle.ResourceId, "Logon"), StartMDT, vehicle, ts);
                    milliseconds += 50;
                }
            }

        }

        private void StartMDT(TaskEntry te)
        {
            SimResource resource = ((StartMDT)te.DataTag).Resource;
            StartMDT(resource);
       }

        /// <summary>
        /// startup routing for the MDT.. logon on to the cad system and start moving to the
        /// nominated nearest standby point.
        /// </summary>
        /// <param name="resource"></param>
        private void StartMDT(SimResource resource)
        {

            resource.PoweredOn = true;
            resource.Enabled = true;
            resource.LastChanged = DateTime.Now;
            Logger.Write(String.Format("IUserOut::StartMDT: {0} ", resource.ResourceId));

            SendLogonToCad(resource);

            SendStatusToCad(resource, ResourceStatus.Available, resource.NonConveyCode, null);
            NewDestination(resource, resource.StandbyPoint);

            if (_routes.ContainsKey(resource.ResourceId))
                _routes.Remove(resource.ResourceId);

            SendCurrentPosition(resource);
            MoveVehicleTowardsTarget(resource);

            SetSysMessage(resource, "Started");

            resource.LastChanged = DateTime.Now;

        }

        public void SetAcceptCancellation(int ResourceId, bool value)
        {

            Logger.Write(String.Format("IUserOut::SetAcceptCancellation: {0} ", ResourceId));

            var vehicles = _resManager.Resources.Where(x => ResourceId == 0 || x.ResourceId == ResourceId);
            foreach (SimResource Resource in vehicles)
            {
                Resource.AcceptCancellation = value;
                Resource.LastChanged = DateTime.Now;
            }

        }

        public void SetStandbyPoint(int ResourceId, string name)
        {
            /// find the destination
            SimDestination dest = _destinationStore.GetDestinations(false, false, true).FirstOrDefault(d => d.Name == name && d.IsStandby == true);

            Logger.Write(String.Format("IUserOut::SetStandbyPoint: {0} ", ResourceId));

            var vehicles = _resManager.Resources.Where(x => ResourceId == 0 || x.ResourceId == ResourceId);
            foreach (SimResource vehicle in vehicles)
            {
                vehicle.StandbyPoint = dest;
                vehicle.Destination = null;
                vehicle.LastChanged = DateTime.Now;
            }


        }

        /// <summary>
        /// Shutdown on or more MDT's
        /// </summary>
        /// <param name="resourceId">The name of the Vehicle . If this is null or empty then 
        /// all MDT's will be affected
        /// </param>
        public void ShutdownMDT(int resourceId, string reason)
        {

            if (_resManager.Resources == null)
                return;

            var vehicles = _resManager.Resources.Where(x => resourceId == 0 || x.ResourceId == resourceId);
            foreach (SimResource resource in vehicles)
            {
                if (resource.PoweredOn == true)
                {
                    Logger.Write(String.Format("IUserOut::ShutdownMDT: id={0} {1} {2}", resourceId, resource.Callsign, reason));

                    resource.PoweredOn = false;
                    resource.OnDuty = false;
                    resource.LastChanged = DateTime.Now;
                    SendLogoutToCad(resource);
                    SetSysMessage(resource, "Shutdown");
                }
            }


        }

        /// <summary>
        /// User has pressed the At Destination button (if there is one)
        /// </summary>
        /// <param name="ResourceId">The name of the Vehicle . If this is null or empty then 
        /// all MDT's will be affected
        /// </param>
        /// <param name="destType">The type of destination reached</param>
        public void AtDestination(int ResourceId, DestType destType)
        {
            Logger.Write(String.Format("IUserOut::AtDestination: {0}-->{1}", ResourceId, destType.ToString()));

            var Resources = _resManager.Resources.Where(x => ResourceId == 0 || x.ResourceId == ResourceId);
            foreach (SimResource Resource in Resources)
                SendAtDestinationToCad(Resource, destType);

        }

        /// <summary>
        /// Make an Vehicle start navigating toward a predefined destination (listed in Destinations array)
        /// </summary>
        /// <param name="ResourceId">The name of the Vehicle to start navigation on. If this is null or empty then 
        /// all MDT's will be navigated to the same destination
        /// </param>
        public void UserStatusChange(int ResourceId, ResourceStatus status, int nonConveyCode, String name)
        {
            SimDestination destHospital = _destinationStore.GetDestinations(false, false, true).FirstOrDefault(x => x.Name == name);

            //if (destHospital == null)
            //    throw new Exception("Destination not found: " + name);

            Logger.Write(String.Format("IUserOut::UserStatusChange: {0}-->status={1} convey={2} dest={3}", ResourceId, status, nonConveyCode, destHospital));

            var vehicles = _resManager.Resources.Where(x => ResourceId == 0 || x.ResourceId == ResourceId);
            foreach (SimResource Resource in vehicles)
                SendStatusToCad(Resource, status, nonConveyCode, destHospital);
        }
        
        /// <summary>
        /// Get a list of vehicles that have changed from the time given in the parameter
        /// </summary>
        /// <param name="since">Return MDT's that have changed after this time</param>
        /// <returns></returns>
        public SimResource[] GetVehicles(DateTime since)
        {
            if (_resManager.Resources == null)
                return null;

            if (since == null)
                since = DateTime.MinValue;

            return (from v in _resManager.Resources where v.LastChanged > since select v).ToArray();

        }

        /// <summary>
        /// return the version of the simulator
        /// </summary>
        /// <returns></returns>
        public String Version()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        

        #endregion


    }
}