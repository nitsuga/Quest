using System;
using System.Diagnostics;
using Quest.Lib.Routing;
using Quest.Common.Messages;
using Quest.Lib.ServiceBus;
using Quest.Lib.Processor;
using Autofac;
using Quest.Common.ServiceBus;
using Quest.Common.Simulation;
using Quest.Lib.Trace;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.Utils;
using Quest.Lib.Simulation.Resources;
using Quest.Lib.Simulation.Incidents;
using GeoAPI.Geometries;
using Quest.Common.Utils;
using Quest.Common.Messages.Routing;
using Quest.Common.Messages.Incident;
using Quest.Common.Messages.Resource;

namespace Quest.Lib.Simulation.Cad
{
    public class CadSimulator : ServiceBusProcessor
    {
        public string defaultengine { get; set; }
        public bool EnableCoverage { get; set; }
        public SimResourceManager _ResManager { get; set; }
        public IRouteEngine _Router { get; set; }

        private ILifetimeScope _scope;
        private RoutingData _data;
        private SimIncidentManager _incidentManager;

        public CadSimulator(
            TimedEventQueue eventQueue,
            SimIncidentManager incidentManager,
            RoutingData data,
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient, 
            MessageHandler msgHandler) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _data = data;
            _scope = scope;
            _incidentManager = incidentManager;
        }

        protected override void OnPrepare()
        {
            var _routingEngine = _scope.ResolveNamed<IRouteEngine>(defaultengine);

            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler<IncidentUpdate>(IncidentUpdateHandler);
            MsgHandler.AddHandler<AssignVehicle>(AssignVehicle);
            MsgHandler.AddHandler<CancelVehicle>(CancelVehicle);
            MsgHandler.AddHandler<StatusChange>(StatusChange);
            MsgHandler.AddHandler<Login>(Login);
            MsgHandler.AddHandler<Logout>(Logout);
            MsgHandler.AddHandler<SkillLevel>(SkillLevel);

            this.LogMessage($"Waiting for routing engine", TraceEventType.Warning);
            while (!_routingEngine.IsReady)
            {
                System.Threading.Thread.Sleep(100);
            }
            LogMessage($"Routing engine ready");

            //_incidentManager.LiveIncidents.CollectionChanged
        }

        protected override void OnStart()
        {            
        }

        protected override void OnStop()
        {
        }

        private Response IncidentUpdateHandler(NewMessageArgs t)
        {
            SimIncidentUpdate inc = (SimIncidentUpdate)t.Payload;
            SimIncident incident;
            switch (inc.UpdateType)
            {
                case SimIncidentUpdate.UpdateTypes.CallStart:
                    var geometry = new Coordinate(inc.Easting ?? 0, inc.Northing ?? 0);

                    // a new incident has arrived at the CAD.. record it as active and hook up event handlers to monitor its tstatus change
                    incident = new SimIncident() { CallStart = inc.CallStart, Position= geometry, IncidentId = inc.IncidentId, Location = "", Category = inc.Category, WillConvey = inc.WasConveyed ?? false };
                    _incidentManager.LiveIncidents.Add(incident);
                    if (inc.WasDispatched == false || inc.OutsideLAS == true)
                        CloseIncident(incident);
                    break;
                case SimIncidentUpdate.UpdateTypes.AMPDS:
                    incident = _incidentManager.FindIncident(inc.IncidentId);
                    if (incident != null)
                        incident.AMPDS = inc.AMPDSCode;
                    break;
            }

            return null;
        }

        void incident_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SimIncident inc = (SimIncident)sender;

            if (e.PropertyName == "Status")
            {
                Logger.Write("Incident Status:          " + inc.IncidentId.ToString() + " " + inc.Status.ToString());

                // the incident is reporting that there are no more resources active
                if (inc.Status == ResourceStatus.Off)
                    CloseIncident(inc);


            }
        }

        /// <summary>
        /// Send in a change of Status to CAD 
        /// </summary>
        /// <param name="newStatus"></param>
        public Response StatusChange(NewMessageArgs t)
        {
            StatusChange newStatus = (StatusChange)t.Payload;
            SimResource res = _ResManager.FindResource(newStatus.Callsign);

            if (res != null)
                ProcessVehicleStatus(res.Incident, res, EventQueue.Now, newStatus.Status);
            return null;
        }

        /// <summary>
        /// Notify CAD that an Vehicle is starting up and requesting operating Params and Callsign
        /// From SIM point of view switches on looking for outbound messages and processing of all inbound messages.
        /// Inbound AVLS are always sent irrespective of whether Vehicle is 'Logged On' or not.
        /// </summary>
        /// <param name="loginDetails"></param>
        public Response Login(NewMessageArgs t)
        {
            Login loginDetails = (Login)t.Payload;
            // As the Vehicle is Logging Off remove it from the list
            SimResource res = _ResManager.FindResource(loginDetails.Callsign);

            if (res != null)
            {
                res.Status = ResourceStatus.Available;
                res.OnDuty = true;
            }

            // actually, make one up.
            CallsignUpdate cu = new CallsignUpdate();
            cu.Callsign = res.Callsign;
            cu.Callsign = loginDetails.Callsign;

            ServiceBusClient.Broadcast(cu);
            return null;
        }

        /// <summary>
        /// Notify CAD that Vehicle is shutting down and from Sim point of view to stop processing messages in and Out
        /// </summary>
        /// <param name="logoutDetails"></param>
        public Response Logout(NewMessageArgs t)
        {
            Logout logoutDetails = (Logout)t.Payload;
            // As the Vehicle is Logging Off remove it from the list
            SimResource res = _ResManager.FindResource(logoutDetails.Callsign);

            if (res != null)
            {
                Logger.Write(string.Format("C&C Got logout request {0}", res.Callsign));
                res.Status = ResourceStatus.Off;
                res.OnDuty = false;
            }
            else
                Logger.Write(string.Format("C&C Got logout request for unknown Callsign {0}", res.Callsign));

            ServiceBusClient.Broadcast(logoutDetails);
            return null;
        }

        /// <summary>
        /// Sends in selected crew skill level. CAD will respond with new Callsign with Skill level as suffix.
        /// </summary>
        /// <param name="skillLevelDetails"></param>
        public Response SkillLevel(NewMessageArgs t)
        {
            SkillLevel skillLevelDetails = (SkillLevel)t.Payload;
            // If the Vehicle is logged in then Send the Message
            SimResource res = _ResManager.FindResource(skillLevelDetails.Callsign);
            // If this Vehicle has logged in then send in this message
            if (res == null)
            {
                // Logger.Write(string.Format("ExpressQ:  <-- ICadIn::SkillLevel - {0}/{1}", skillLevelDetails.Callsign, skillLevelDetails.SkillLevel1));
            }
            return null;
        }

        public void CloseIncident(SimIncident incident)
        {
            SimIncident inc = _incidentManager.FindIncident(incident.IncidentId);

            if (inc != null)
            {
                Logger.Write("Incident Closed:          " + incident.IncidentId.ToString());

                // update the final turnaround time
                incident.TurnaroundTime = (int)(EventQueue.Now.Subtract(incident.CallStart).TotalSeconds);

                _incidentManager.LiveIncidents.Remove(inc);
            }
        }

   

        /// <summary>
        /// assign a vehicle to the resource.. adds the vehicle to the activation list and sends a message
        /// to the mdt.
        /// </summary>
        /// <param name="IncidentId"></param>
        /// <param name="Callsign"></param>
        public Response AssignVehicle(NewMessageArgs t)
        {
            AssignVehicle assignVehicle = (AssignVehicle)t.Payload;
            SimResource res = _ResManager.FindResource(assignVehicle.Callsign);
            SimIncident inc = _incidentManager.FindIncident(assignVehicle.IncidentId);

            // find the resource
            if (res != null && inc != null)
            {
                ProcessVehicleStatus(inc, res, EventQueue.Now, ResourceStatus.Dispatched);

                // this prevents it being considered for other incs in this loop.
                res.Status = ResourceStatus.Dispatched;

                // send message to MDT
                MDTIncident mdtinc = new MDTIncident();
                mdtinc.Callsign = assignVehicle.Callsign;
                mdtinc.IncidentDetails = new SimIncident();
                mdtinc.IncidentDetails.Category = inc.Category;
                mdtinc.IncidentDetails.Date = (DateTime)inc.CallStart;
                mdtinc.IncidentDetails.Position = inc.Position;
                mdtinc.IncidentDetails.Incomplete = true;
                mdtinc.IncidentDetails.Location = "";

                res.Incident = inc;
                ServiceBusClient.Broadcast(mdtinc);
            }
            else
            {
                Logger.Write("Assign vehicle failed:          ");
            }
            return null;
        }


        /// <summary>
        /// cancels a vehicle from the incident
        /// </summary>
        /// <param name="IncidentId"></param>
        /// <param name="Callsign"></param>
        public Response CancelVehicle(NewMessageArgs t)
        {
            CancelVehicle cancelVehicle = (CancelVehicle)t.Payload;
            SimResource res = _ResManager.FindResource(cancelVehicle.Callsign);
            SimIncident inc = _incidentManager.FindIncident(cancelVehicle.IncidentId);

            if (res != null && inc != null)
            {
                RemoveResource(inc, res);

                Logger.Write(res.Callsign + " Cancelling vehicle from " + inc.IncidentId.ToString());
                CancelIncident details = new CancelIncident() { Callsign = res.Callsign };
                ServiceBusClient.Broadcast(details);
            }
            else
            {
            }
            return null;
        }

        private Response CalcCoverage()
        {
            if (EnableCoverage)
            {
                // get all green vehicles
                var resources = from v in _ResManager.Resources where v.Status == ResourceStatus.Available && v.VehicleType == "AEU" select v;

                var endPoints = resources.Select(x => x.Position).ToArray();

                String vehicleType = "AEU";

                double distanceMax = int.MaxValue, durationMax = 480;

                string sysMessage = string.Format("Calculating coverage map for {0} vehicles", endPoints.Count());
                Logger.Write(sysMessage);

                RouteRequestCoverage request = new RouteRequestCoverage()
                {
                    Name = "Coverage",
                    StartPoints = endPoints,
                    VehicleType = vehicleType,
                    HourOfWeek = EventQueue.Now.HourOfWeek(),
                    DistanceMax = distanceMax,
                    DurationMax = durationMax,
                    SearchType = RouteSearchType.Quickest,
                    TileSize = 500
                };

                CoverageMap result = _Router.CalculateCoverage(request);
            }

            SetTimedEvent($"CALCRES", EventQueue.Now.AddMinutes(1), () => CalcCoverage());

            return null;
        }

        /// <summary>
        /// add a new vehicle to the list of vehicles activated on this incident. It also updates the 
        /// current incident status
        /// </summary>
        /// <param name="Callsign"></param>
        private static AssignedResource AddResourceRecord(SimIncident incident, SimResource resource, DateTime now)
        {
            Debug.Assert(incident != null && resource != null);

            AssignedResource resRecord = null;

            if (incident.AssignedResources != null)
                resRecord = (from r in incident.AssignedResources where r.Callsign == resource.Callsign select r).FirstOrDefault();

            // no record? add one
            if (resRecord == null)
            {
                resRecord = new AssignedResource() { Callsign = resource.Callsign, Dispatched = null, OnSceneTime = null, Convey = null, Enroute = null, Hospital = null, Released = null };

                // convert to a list and add the new entry onto the end
                List<AssignedResource> list = incident.AssignedResources != null ? new List<AssignedResource>(incident.AssignedResources) : new List<AssignedResource>();

                // create a new resource record
                list.Add(new AssignedResource() { Callsign = resource.Callsign, Dispatched = null, OnSceneTime = null, Convey = null, Enroute = null, Hospital = null, Released = null });

                incident.AssignedResources = list;

                if (resource.VehicleType == "AEU")
                    incident.AmbAssigned--;

                if (resource.VehicleType == "FRU")
                    incident.FruAssigned--;

            }

            return resRecord;

        }

        public void ProcessVehicleStatus(SimIncident incident, SimResource resource, DateTime now, ResourceStatus newstatus)
        {
            AssignedResource record = null;

            // get incident assignedResource record or make one if appropriate
            if (incident != null)
                record = AddResourceRecord(incident, resource, now);

            // change status from Waiting to enroute.. other vehicles may be on scene already
            switch (newstatus)
            {
                case ResourceStatus.Dispatched:
                    Debug.Assert(incident != null);

                    if (incident == null)
                        break;

                    resource.Incident = incident;
                    record.Dispatched = now;
                    break;

                case ResourceStatus.Enroute:

                    Debug.Assert(incident != null);

                    if (incident == null)
                        break;

                    resource.Incident = incident;
                    record.Enroute = now;
                    break;

                case ResourceStatus.OnScene:

                    Debug.Assert(incident != null);

                    if (incident == null)
                        break;

                    resource.Incident = incident;

                    //record first vehicle on scene                    
                    if (incident.FirstResponderArrivalDelay == 0)
                    {
                        incident.FirstResponderArrivalDelay = (int)(now.Subtract(incident.CallStart).TotalSeconds);
                        incident.FirstResponderArrival = resource.Callsign;
                    }

                    // record the ambulance on scene time
                    if (incident.AmbulanceArrivalDelay == 0 && resource.VehicleType == "AEU")  // 1=ambulance 2=FRU
                    {
                        incident.AmbulanceArrivalDelay = (int)(now.Subtract(incident.CallStart).TotalSeconds);
                        incident.AmbulanceArrival = resource.Callsign;
                    }

                    incident.AmbulanceArrival = resource.Callsign;

                    if (incident.OnSceneTime == DateTime.MinValue)
                        incident.OnSceneTime = now;

                    if (record != null)
                        record.OnSceneTime = now;
                    break;

                case ResourceStatus.Conveying:

                    Debug.Assert(incident != null);

                    if (incident == null)
                        break;

                    resource.Incident = incident;
                    record.Dispatched = now;

                    // cancel any other vehicle that is dispatched or enroute
                    if (incident != null)
                    {
                        List<string> resourcesToCancel = new List<string>();

                        // remove the assigned resource from the list
                        foreach (AssignedResource ar in incident.AssignedResources)
                            if (ar.Callsign != resource.Callsign)
                                resourcesToCancel.Add(ar.Callsign);

                        resourcesToCancel.ForEach(
                            x =>
                            {
                                CancelVehicle( new NewMessageArgs { Payload = (IServiceBusMessage)new CancelVehicle { IncidentId = incident.IncidentId, Callsign = x } });
                            }
                        );
                    }

                    break;

                case ResourceStatus.Arrived:
                    Debug.Assert(incident != null);

                    if (incident == null)
                        break;

                    resource.Incident = incident;
                    record.Hospital = now;
                    break;

                case ResourceStatus.Off:
                    Logger.Write(resource.Callsign + " turned off.. incident set to null");
                    resource.Incident = null;
                    if (record != null)
                        record.Released = now;
                    break;

                case ResourceStatus.Available:
                    if (incident != null)
                    {
                        RemoveResource(incident, resource);
                    }

                    resource.Incident = null;
                    if (record != null)
                        record.Released = now;
                    break;
            }

            if (incident != null)
            {
                if (incident.Status == ResourceStatus.OnScene)
                    if (incident.OnSceneDelay == 0)
                        incident.OnSceneDelay = (int)(now.Subtract(incident.OnSceneTime).TotalSeconds);

                // update incident status to the latest resource status
                if (incident.Status < newstatus)
                    incident.Status = newstatus;

                // if there are no active resources on this incident then it is closed
                int active_count = (from x in incident.AssignedResources
                                    join r in _ResManager.Resources on x.Callsign equals r.Callsign
                                    where r.Status == ResourceStatus.Enroute ||
                                     r.Status == ResourceStatus.OnScene ||
                                     r.Status == ResourceStatus.Conveying ||
                                     r.Status == ResourceStatus.Arrived
                                    select r).Count();

                if (active_count == 0)
                {

                    // if the incident status is more advanced than enroute and the active resource count is now zero
                    // then all resources have cleared from the incident.. record the total turnaround time
                    if (incident.Status >= ResourceStatus.OnScene)
                        incident.Status = ResourceStatus.Off;
                    else
                    {
                        // if the resource count is now 0 and the inc statue was enroute then the vehicle was cancelled from this job.
                        // reset back to Waiting so that the job gets picked up again.
                        if (incident.Status >= ResourceStatus.Enroute)
                            incident.Status = ResourceStatus.Available;
                    }

                }

                UpdateIncident incUpdate = new UpdateIncident { IncidentDetails = incident };

                ServiceBusClient.Broadcast(incUpdate);
            }
        }

        void RemoveResource(SimIncident incident, SimResource resource)
        {
            // remove the assigned resource from the list
            AssignedResource ar = (from a in incident.AssignedResources where a.Callsign == resource.Callsign select a).FirstOrDefault();
            List<AssignedResource> list = new List<AssignedResource>(incident.AssignedResources);
            list.Remove(ar);
            incident.AssignedResources = list;

            if (resource.VehicleType == "AEU")
                incident.AmbAssigned--;

            if (resource.VehicleType == "FRU")
                incident.FruAssigned--;
        }


    } // End of Class

} //End of Namespace
