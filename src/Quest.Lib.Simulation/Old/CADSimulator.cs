using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Quest.Lib.Processor;

namespace Emulator.Lib.Simulators
{
    
    /// <summary>
    /// This class is a concrete implementation of a command and control system core.
    /// The class receives events from resources and maintains state of resources and
    /// incidents. The class also emits events to all other interests modules.
    /// </summary>
    [Export(typeof(CADSimulator))]
    public class CADSimulator : Processor
    {
        
        private DateTime _lastResourceReport;

        /// All the available message type calls that the Vehicle Simm can make to CAD
        #region ICadIn Members

        public event EventHandler<Incident> NewIncidentEvent;

        public event EventHandler<Incident> UpdateIncidentEvent;

        public event EventHandler<Incident> UpdateLocationEvent;

        public event EventHandler<Incident> UpdateAMPDSEvent;

        public event EventHandler<Incident> CloseIncidentEvent;

        public event EventHandler<StatusChange> StatusChangeEvent;

        public event EventHandler<AtDestination> AtDestinationEvent;

        public event EventHandler<Login> LoginEvent;

        public event EventHandler<Logout> LogoutEvent;

        public event EventHandler<RejectCancelIncident> RejectCancelIncidentEvent;

        public event EventHandler<SkillLevel> SkillLevelEvent;

        public event EventHandler<SATNAVLocation> SATNAVLocationEvent;

        /// <summary>
        /// Send in a change of Status to CAD 
        /// </summary>
        /// <param name="newStatus"></param>
        public void StatusChange(StatusChange newStatus)
        {
            Resource res = _ResManager.FindResource(newStatus.ResourceId);

            if (res != null)
                ProcessVehicleStatus( res.Incident, res, EventQueue.Now, newStatus.Status, _simEngine);

            DumpResourceStatistics();

            if (StatusChangeEvent != null)
                StatusChangeEvent(this, newStatus);
            
        }


        /// <summary>
        /// Notify CAD that Vehicle has arrived at or estimated to have arrived at a destination it was being navigated to 
        /// </summary>
        /// <param name="destinationDetails"></param>
        public void AtDestination(AtDestination destinationDetails)
        {
            if (AtDestinationEvent != null)
                AtDestinationEvent(this, destinationDetails);
        }

        /// <summary>
        /// Notify CAD that an Vehicle is starting up and requesting operating Params and Callsign
        /// From SIM point of view switches on looking for outbound messages and processing of all inbound messages.
        /// Inbound AVLS are always sent irrespective of whether Vehicle is 'Logged On' or not.
        /// </summary>
        /// <param name="loginDetails"></param>
        public void Login(Login loginDetails)
        {
            // As the Vehicle is Logging Off remove it from the list
            Resource res = _ResManager.FindResource(loginDetails.ResourceId);

            if (res != null)
            {
                res.Status = ResourceStatus.Waiting;
                res.OnDuty = true;
            }

            // actually, make one up.
            CallsignUpdate cu = new CallsignUpdate();
            cu.Callsign = res.Callsign ;
            cu.ResourceId = loginDetails.ResourceId;

            _CadOut.CallsignUpdate(cu);

            if (LoginEvent != null)
                LoginEvent(this, loginDetails);

        }

        /// <summary>
        /// Find an incident record by incident ID
        /// </summary>
        /// <param name="incidentId"></param>
        /// <returns></returns>
        public Incident FindIncident(long incidentId)
        {
            return _simEngine.LiveIncidents.Where(x => x.IncidentId == incidentId).FirstOrDefault();
        }

        /// <summary>
        /// Notify CAD that Vehicle is shutting down and from Sim point of view to stop processing messages in and Out
        /// </summary>
        /// <param name="logoutDetails"></param>
        public void Logout(Logout logoutDetails)
        {
            // As the Vehicle is Logging Off remove it from the list
            Resource res = _ResManager.FindResource(logoutDetails.ResourceId);

            if (res!=null)
            {
                Logger.Write(string.Format("C&C Got logout request {0}", res.Callsign));
                res.Status = ResourceStatus.Off;
                res.OnDuty = false;
            }
            else
                Logger.Write(string.Format("C&C Got logout request for unknown resourceid {0}", res.ResourceId));

            if (LogoutEvent != null)
                LogoutEvent(this, logoutDetails);
        }

        /// <summary>
        /// Notify CAD of a refusal to stand down from Amber to scene.
        /// </summary>
        /// <param name="rejectCancellation"></param>
        public void RejectCancelIncident(RejectCancelIncident rejectCancellation)
        {
            Resource res = _ResManager.FindResource(rejectCancellation.ResourceId);
            // If this Vehicle has logged in then send in this message
            if (res==null)
            {
                // Logger.Write(string.Format("ExpressQ:  <-- ICadIn::RejectCancelIncident - {0}", rejectCancellation.ResourceId));
            }

            if (RejectCancelIncidentEvent != null)
                RejectCancelIncidentEvent(this, rejectCancellation);
        }

        /// <summary>
        /// Sends in selected crew skill level. CAD will respond with new Callsign with Skill level as suffix.
        /// </summary>
        /// <param name="skillLevelDetails"></param>
        public void SkillLevel(SkillLevel skillLevelDetails)
        {
            // If the Vehicle is logged in then Send the Message
            Resource res = _ResManager.FindResource(skillLevelDetails.ResourceId);
            // If this Vehicle has logged in then send in this message
            if (res == null)
            {
                // Logger.Write(string.Format("ExpressQ:  <-- ICadIn::SkillLevel - {0}/{1}", skillLevelDetails.ResourceId, skillLevelDetails.SkillLevel1));
            }
            if (SkillLevelEvent != null)
                SkillLevelEvent(this, skillLevelDetails);
        }

        /// <summary>
        /// Frequent Location speed and direction information from an Vehicle that is mobile.
        /// Message will be processed whether or not the Vehicle is 'Logged On' or not. We need to know where they are
        /// at all times.
        /// </summary>
        /// <param name="avlsDetails"></param>
        public void SATNAVLocation(SATNAVLocation avlsDetails)
        {
            if (SATNAVLocationEvent != null)
                SATNAVLocationEvent(this, avlsDetails);
        }

        /// <summary>
        /// a new incident has arrived at the CAD.. record it as active and hook up event handlers to monitor its tstatus change
        /// </summary>
        /// <param name="incident"></param>
        public void Newincident(Incident incident)
        {
            _simEngine.LiveIncidents.Add(incident);

            incident.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(incident_PropertyChanged);
            
            // add to the list of live incidents
            if (NewIncidentEvent != null)
                NewIncidentEvent(this, incident);
        }

        void incident_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Incident inc = (Incident)sender;

            if (e.PropertyName == "Status")
            {
                OnMessage("Incident Status:          " + inc.IncidentId.ToString() + " " + inc.Status.ToString());

                // the incident is reporting that there are no more resources active
                if (inc.Status == ResourceStatus.Off)
                    CloseIncident(inc);


            }
        }

        public void UpdateLocation(Incident incident)
        {
            // add to the list of live incidents
            if (UpdateLocationEvent != null)
                UpdateLocationEvent(this, incident);


            //if (inc.WasDispatched == false || inc.OutsideLAS == true)
            //    return;

            // get the live incident
            //var liveInc = (from i in _simEngine.LiveIncidents where i.IncidentId == inc.IncidentId select i).FirstOrDefault();

            //Debug.Assert(liveInc != null, "No corresponding incident" + te.ToString());

            // calculate its location
            //RoutingLocation loc = _Router.GetLocationFromPoint((int)inc.Easting, (int)inc.Northing, inc);

            //if (loc != null)
            //{
            //    liveInc.Location = loc.Name;
            //    _Cad.UpdateLocation(liveInc);
            // }

        }

        public void UpdateAMPDS(Incident incident)
        {
            Incident inc = FindIncident(incident.IncidentId);
            if (inc !=null)
            {
                inc.AMPDS = incident.AMPDS;

                if (UpdateAMPDSEvent != null)
                    UpdateAMPDSEvent(this, inc);
            }
        }

        public void CloseIncident(Incident incident)
        {
            Incident inc = FindIncident(incident.IncidentId);

            if (inc != null)
            {
                OnMessage("Incident Closed:          " + incident.IncidentId.ToString());

                // update the final turnaround time
                incident.TurnaroundTime = (int)(EventQueue.Now.Subtract(incident.CallStart).TotalSeconds);

                _simEngine.LiveIncidents.Remove(inc);

                if (CloseIncidentEvent != null)
                    CloseIncidentEvent(this, incident);
            }
        }

        /// <summary>
        /// record the current number of resource status
        /// </summary>
        private void DumpResourceStatistics()
        {
            if (EventQueue.Now.Subtract(_lastResourceReport).TotalMinutes < 10)
                return;

            _lastResourceReport = EventQueue.Now;

            var resourceResults = _ResManager.Resources.GroupBy(r => new { r.Status, r.VehicleType }).Select(rs => new { Key = rs.Key, Count = rs.Count() });
            try
            {
                foreach (var r in resourceResults.ToArray())
                {
                    if (EventQueue.Now.Year > 2000)
                    {
                        SimulationStat stat = new SimulationStat() { Timestamp = EventQueue.Now, Quantity = r.Count, SimulationRunId = _simEngine.RunRecord.SimulationRunId, VehicleTypeId = r.Key.VehicleType, Status = r.Key.Status.ToString() };

                        using (SimDataDataContext context = new SimDataDataContext( connections.SimulatorDatabase))
                        {
                            context.SimulationStats.InsertOnSubmit(stat);
                            context.SubmitChanges();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// assign a vehicle to the resource.. adds the vehicle to the activation list and sends a message
        /// to the mdt.
        /// </summary>
        /// <param name="IncidentId"></param>
        /// <param name="ResourceId"></param>
        public void AssignVehicle(long IncidentId, int ResourceId)
        {
            Resource res = _ResManager.FindResource(ResourceId);
            Incident inc = FindIncident(IncidentId);

            // find the resource
            if (res != null && inc != null)
            {
                ProcessVehicleStatus(inc, res, EventQueue.Now, ResourceStatus.Dispatched, _simEngine);

                // this prevents it being considered for other incs in this loop.
                res.Status = ResourceStatus.Dispatched;

                // send message to MDT
                MDTIncident mdtinc = new MDTIncident();
                mdtinc.ResourceId = ResourceId;
                mdtinc.IncidentDetails = new IncidentDetails();
                mdtinc.IncidentDetails.Category = inc.Category;
                mdtinc.IncidentDetails.Date = (DateTime)inc.CallStart;
                mdtinc.IncidentDetails.IncEasting = (int)inc.Easting;
                mdtinc.IncidentDetails.IncNorthing = (int)inc.Northing;
                mdtinc.IncidentDetails.Incomplete = true;
                mdtinc.IncidentDetails.Location = "";

                res.Incident = inc;
                _CadOut.Incident(mdtinc);
            }
            else
            {
                OnMessage("Assign vehicle failed:          ");
            }
        }


        /// <summary>
        /// cancels a vehicle from the incident
        /// </summary>
        /// <param name="IncidentId"></param>
        /// <param name="ResourceId"></param>
        public void CancelVehicle(long IncidentId, int ResourceId)
        {
            Resource res = _ResManager.FindResource(ResourceId);
            Incident inc = FindIncident(IncidentId);

            if (res != null && inc != null)
            {
                RemoveResource(inc, res);

                OnMessage(res.Callsign + " Cancelling vehicle from " + inc.IncidentId.ToString());
                CancelIncident details = new CancelIncident() { ResourceId = res.ResourceId };

                _CadOut.CancelIncident(details);
            }
            else
            {
            }
        }

        #endregion

        public bool EnableCoverage { get; set; }

        #region ISimPart Members

        /// <summary>
        /// Initialise the processes that need to be operational for ExpressQ Sim Service to work
        /// </summary>
        public override void Initialise(CompositionContainer container)
        {
            PatchUpImports(container);
            IsInitialised = true;
            OnMessage("CAD: initialised");
        }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        public override void Prepare()
        {
            _lastResourceReport = DateTime.MinValue;
            QueueCalcCoverage();
        }

        #endregion

        private void QueueCalcCoverage()
        {
            // add trigger for the new incident
            TaskKey key2 = new TaskKey("CALCRES", null);
            new TaskEntry(EventQueue, key2, CalcCoverage, null, this.EventQueue.Now.AddMinutes(10));
        }

        private void CalcCoverage(TaskEntry te)
        {
            if (EnableCoverage)
            {
                // get all green vehicles
                var resources = from v in _ResManager.Resources where v.Status == ResourceStatus.Waiting && v.VehicleType == 1 select v;

                RoutingPoint[] endPoints = new RoutingPoint[resources.Count()];
                int vehicleType = 1, hour = 9;
                double distanceMax = int.MaxValue, durationMax = 480;

                int i = 0;
                foreach (Resource v in resources)
                    endPoints[i++] = new RoutingPoint() { X = (int)v.Easting, Y = (int)v.Northing };

                string sysMessage = string.Format("Calculating coverage map for {0} vehicles", endPoints.Count());
                OnMessage(sysMessage);

                RouteRequestCoverage request = new RouteRequestCoverage()
                {
                    Name = "Coverage",
                    StartPoints = endPoints,
                    VehicleType = vehicleType,
                    Hour = hour,
                    DistanceMax = distanceMax,
                    DurationMax = durationMax,
                    SearchType = SearchType.Quickest,
                    TileSize = 500
                };

                CoverageMap result = _Router.CalculateCoverage(request);
            }

            QueueCalcCoverage();
        }

        /// <summary>
        /// add a new vehicle to the list of vehicles activated on this incident. It also updates the 
        /// current incident status
        /// </summary>
        /// <param name="resourceId"></param>
        private static AssignedResource AddResourceRecord(Incident incident, Resource resource, DateTime now)
        {
            Debug.Assert(incident != null && resource != null);

            AssignedResource resRecord = null;

            if (incident.AssignedResources != null)
                resRecord = (from r in incident.AssignedResources where r.ResourceId == resource.ResourceId select r).FirstOrDefault();

            // no record? add one
            if (resRecord == null)
            {
                resRecord = new AssignedResource() { ResourceId = resource.ResourceId, Dispatched = null, Onscene = null, Convey = null, Enroute = null, Hospital = null, Released = null };

                // convert to a list and add the new entry onto the end
                List<AssignedResource> list = incident.AssignedResources != null ? new List<AssignedResource>(incident.AssignedResources) : new List<AssignedResource>();

                // create a new resource record
                list.Add(new AssignedResource() { ResourceId = resource.ResourceId, Dispatched = null, Onscene = null, Convey = null, Enroute = null, Hospital = null, Released = null });

                incident.AssignedResources = list.ToArray();

                if (resource.VehicleType == 1)
                    incident.AmbAssigned--;

                if (resource.VehicleType == 2)
                    incident.FruAssigned--;

            }

            return resRecord;

        }

        public void ProcessVehicleStatus(Incident incident, Resource resource, DateTime now, ResourceStatus newstatus, SimEngine simEngine)
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

                case ResourceStatus.Onscene:

                    Debug.Assert(incident != null);

                    if (incident == null)
                        break;

                    resource.Incident = incident;

                    //record first vehicle on scene                    
                    if (incident.FirstResponderArrivalDelay == 0)
                    {
                        incident.FirstResponderArrivalDelay = (int)(now.Subtract(incident.CallStart).TotalSeconds);
                        incident.FirstResponderArrivalId = resource.ResourceId;
                    }

                    // record the ambulance on scene time
                    if (incident.AmbulanceArrivalDelay == 0 && resource.VehicleType == 1)  // 1=ambulance 2=FRU
                    {
                        incident.AmbulanceArrivalDelay = (int)(now.Subtract(incident.CallStart).TotalSeconds);
                        incident.AmbulanceArrivalId = resource.ResourceId;
                    }

                    incident.AmbulanceArrivalId = resource.ResourceId;

                    if (incident.OnSceneTime == DateTime.MinValue)
                        incident.OnSceneTime = now;

                    if (record != null)
                        record.Onscene = now;
                    break;

                case ResourceStatus.Convey:

                    Debug.Assert(incident != null);

                    if (incident == null)
                        break;

                    resource.Incident = incident;
                    record.Dispatched = now;

                    // cancel any other vehicle that is dispatched or enroute
                    if (incident != null)
                    {
                        List<int> resourcesToCancel = new List<int>();

                        // remove the assigned resource from the list
                        foreach (AssignedResource ar in incident.AssignedResources)
                            if (ar.ResourceId != resource.ResourceId)
                                resourcesToCancel.Add(ar.ResourceId);

                        resourcesToCancel.ForEach(
                            x =>
                            {
                                CancelVehicle(incident.IncidentId, x);
                            }
                        );
                    }

                    break;

                case ResourceStatus.Hospital:
                    Debug.Assert(incident != null);

                    if (incident == null)
                        break;

                    resource.Incident = incident;
                    record.Hospital = now;
                    break;

                case ResourceStatus.Off:
                    OnMessage(resource.Callsign + " turned off.. incident set to null");
                    resource.Incident = null;
                    if (record != null)
                        record.Released = now;
                    break;

                case ResourceStatus.Waiting:
                    if (incident!=null)
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
                if (incident.Status == ResourceStatus.Onscene)
                    if (incident.OnSceneDelay == 0)
                        incident.OnSceneDelay = (int)(now.Subtract(incident.OnSceneTime).TotalSeconds);

                // update incident status to the latest resource status
                if (incident.Status < newstatus)
                    incident.Status = newstatus;

                // if there are no active resources on this incident then it is closed
                int active_count = (from x in incident.AssignedResources
                                    join r in _ResManager.Resources on x.ResourceId equals r.ResourceId
                                    where r.Status == ResourceStatus.Enroute ||
                                     r.Status == ResourceStatus.Onscene ||
                                     r.Status == ResourceStatus.Convey ||
                                     r.Status == ResourceStatus.Hospital
                                    select r).Count();

                if (active_count == 0)
                {

                    // if the incident status is more advanced than enroute and the active resource count is now zero
                    // then all resources have cleared from the incident.. record the total turnaround time
                    if (incident.Status >= ResourceStatus.Onscene)
                        incident.Status = ResourceStatus.Off;
                    else
                    {
                        // if the resource count is now 0 and the inc statue was enroute then the vehicle was cancelled from this job.
                        // reset back to Waiting so that the job gets picked up again.
                        if (incident.Status >= ResourceStatus.Enroute)
                            incident.Status = ResourceStatus.Waiting;
                    }

                }

                if (UpdateIncidentEvent!=null)
                    UpdateIncidentEvent(this, incident);
            }
        }

        void RemoveResource(Incident incident, Resource resource)
        {
            // remove the assigned resource from the list
            AssignedResource ar = (from a in incident.AssignedResources where a.ResourceId == resource.ResourceId select a).FirstOrDefault();
            List<AssignedResource> list = new List<AssignedResource>(incident.AssignedResources);
            list.Remove(ar);
            incident.AssignedResources = list.ToArray();

            if (resource.VehicleType == 1)
                incident.AmbAssigned--;

            if (resource.VehicleType == 2)
                incident.FruAssigned--;
        }

        void ICadIn.NavigateTo(NavigateTo details)
        {
            _CadOut.NavigateTo(details);
        }


    } // End of Class

} //End of Namespace
