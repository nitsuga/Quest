#define NO_APPLE
#define XCAN_SEND_NOTIFICATIONS_TO_RESOURCES

//TODO: Implement resource notifications using a Notification service
#define xCAN_SEND_NOTIFICATIONS_TO_RESOURCES

using System;
using System.Linq;
using Nest;
using Quest.Lib.Search.Elastic;
using Quest.Common.ServiceBus;
using Quest.Lib.Incident;
using Quest.Lib.Data;
using Quest.Lib.Trace;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using Quest.Lib.Device;
using Quest.Common.Messages.CAD;
using Quest.Common.Messages.Gazetteer;
using Quest.Common.Messages.Device;
using Quest.Common.Messages.Incident;
using Quest.Common.Messages.Resource;
using Quest.Common.Messages.Notification;
using Quest.Lib.Destinations;
using Quest.Lib.Coords;

namespace Quest.Lib.Resource
{
    public class ResourceHandler
    {
        public string deletedStatus { get; set; } = "OOS";
        public string triggerStatus { get; set; } = "DSP";

        private IDatabaseFactory _dbFactory;
        private const string Version = "1.0.0";
        private IResourceStore _resStore;
        private IIncidentStore _incStore;
        private IDeviceStore _devStore;
        private IDestinationStore _denStore;

        public ResourceHandler(IDatabaseFactory dbFactory, IResourceStore resStore, IIncidentStore incStore, IDeviceStore devStore, IDestinationStore denStore)
        {
            _dbFactory = dbFactory;
            _resStore = resStore;
            _incStore = incStore;
            _devStore = devStore;
            _denStore = denStore;
        }

        /// <summary>
        ///     Detects resource changing to DSP (Dispatched) and sends incident details to callsign
        ///     Detects resource changing to status and sends status update to callsign
        /// </summary>
        /// <param name="resourceUpdate"></param>
        /// <param name="settings"></param>
        /// <param name="client"></param>
        /// <param name="msgSource"></param>
        public ResourceUpdateResult ResourceUpdate(ResourceUpdateRequest resourceUpdate, IServiceBusClient msgSource, BuildIndexSettings config)
        {
            // update the resource record
            var resupdate = _resStore.Update(resourceUpdate);

            var res = resupdate.NewResource;

            // create a notification to say that resource details are persisted

            //TODO: turn on/off elastic search saving
            // save details to elastic if we have location info
            // UpdateElastic(config, resupdate.NewResource);

            // send out message that the resource has changed
            msgSource.Broadcast(new ResourceUpdate() { Callsign = resourceUpdate.Resource.Callsign, Item = res });

            // detect change in status
            if (resupdate.OldResource.Status != resupdate.NewResource.Status)
            {
                msgSource.Broadcast(new ResourceStatusChange()
                {
                    Callsign = resupdate.NewResource.Callsign,
                    FleetNo = resupdate.NewResource.FleetNo,
                    OldStatus= resupdate.OldResource.Status,
                    NewStatus= resupdate.NewResource.Status,
                    OldStatusCategory = resupdate.OldResource.StatusCategory,
                    NewStatusCategory = resupdate.NewResource.StatusCategory
                });

                SendStatusNotification(resourceUpdate.Resource.FleetNo, "Update", msgSource);
            }

            // detect change in event
            if (resupdate.OldResource.EventId != resupdate.NewResource.EventId)
            {
                SendEventNotification(resourceUpdate.Resource.FleetNo, resourceUpdate.Resource.EventId, "C&C Assigned", msgSource);
            }


            return resupdate;
        }

        /// <summary>
        /// create or update a resource to Destination (not incident) assignment
        /// </summary>
        /// <param name="resassign"></param>
        /// <param name="serviceBusClient"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        internal AssignToDestinationResponse AssignToDestination(AssignToDestinationRequest resassign, IServiceBusClient serviceBusClient, BuildIndexSettings config)
        {
            var res = _resStore.GetByCallsign(resassign.Callsign);
            if (res == null)
            {
                return new AssignToDestinationResponse
                {
                    Message=$"Could not find resource '{resassign.Callsign}'",
                    Success = false,
                };
            }

            var dest = _denStore.GetDestination(resassign.DestinationCode);
            if (dest == null)
            {
                return new AssignToDestinationResponse
                {
                    Message = $"Could not find destination '{resassign.DestinationCode}'",
                    Success = false,
                };
            }

            ResourceAssignmentUpdate request = new ResourceAssignmentUpdate
            {
                Callsign = resassign.Callsign,
                Assigned = DateTime.UtcNow,
                StartPosition = res.Position,
                Status = ResourceAssignmentUpdate.StatusCode.InProgress
            };

            var assignment = _resStore.UpdateResourceAssign(request);
            AssignToDestinationResponse response = new AssignToDestinationResponse
            {
                Success=true,
            };

            var assignments = _resStore.GetAssignmentStatus();
            var allResources = _resStore.GetResources(0, null, true, false);

            DestinationStatus status = new DestinationStatus
            {
                Destination = dest,
                Nearby = allResources.Where(x => x.Position.Distance(dest.Position) < resassign.NearbyDistance).ToList(),
                Assignments = assignments.Where(x => x.DestinationCode == dest.Code).ToList()
            };

            var msg = new DestinationStatusChanged 
            {
                Item = status
            };

            serviceBusClient.Broadcast(msg);

            return response;
        
        }

        /// <summary>
        /// get a list of resource assignments
        /// </summary>
        /// <returns></returns>
        internal GetResourceAssignmentsResponse GetAssignmentStatus(GetResourceAssignmentsRequest request)
        {
            var destinations = _denStore.GetDestinations(false, false, true);
            var assignments = _resStore.GetAssignmentStatus();
            var allResources = _resStore.GetResources(0, null, true, false);

            var result = new GetResourceAssignmentsResponse
            {
                Destinations = new List<DestinationStatus>()
            };

            foreach(var dest in destinations)
            {
                var myassignments = assignments.Where(x => x.DestinationCode == dest.Code).ToList();
                DestinationStatus.StatusCode status = DestinationStatus.StatusCode.Uncovered;

                if (myassignments.Any(x => x.Status == ResourceAssignmentStatus.StatusCode.InProgress || x.Status == ResourceAssignmentStatus.StatusCode.Warning ))
                    status = DestinationStatus.StatusCode.InProgress;

                if (myassignments.Any(x => x.Status == ResourceAssignmentStatus.StatusCode.Arrived))
                    status = DestinationStatus.StatusCode.Covered;

                DestinationStatus desstatus = new DestinationStatus {
                    Status = status,
                    Destination = dest,
                    Nearby = allResources.Where(x => x.Position.Distance(dest.Position) < request.NearbyDistance)
                                .OrderBy(x => x.Position.Distance(dest.Position))
                                .ToList(),
                    Assignments = myassignments
                };

                result.Destinations.Add(desstatus);
                result.History = new List<DestinationHistory>();

            }
            return result;
        }

        /// <summary>
        /// Update Elastic store with the latest resource details
        /// </summary>
        /// <param name="config"></param>
        /// <param name="res"></param>
        void UpdateElastic(BuildIndexSettings config, QuestResource res)
        {
            if (res.Position != null)
            {
                var point = new PointGeoShape(new GeoCoordinate(res.Position.Latitude, res.Position.Longitude));
                var geo = new GeoLocation(res.Position.Latitude, res.Position.Longitude);

                if (config != null)
                {
                    // save in elastic
                    var indextext = res.Callsign + " " + res.FleetNo ?? "" + " " + res.ResourceType ?? "" + " " + res.Status ?? "";

                    var add = new LocationDocument
                    {
                        ID = "RES " + res.Callsign,
                        Type = "Res",
                        Source = "Quest",
                        indextext = indextext,
                        Description = indextext,
                        Location = geo,
                        Point = point,
                        Created = DateTime.UtcNow
                    };
                    var descriptor = ElasticIndexer.GetBulkRequest(config);
                    ElasticIndexer.AddIndexItem(add, descriptor);
                    ElasticIndexer.CommitBultRequest(config, descriptor);
                }
            }
        }
        

        /// <summary>
        ///     delete resource marks the resource as the status specificed in the "DeviceManager.DeletedStatus" setting
        /// </summary>
        /// <param name="item"></param>
        /// <param name="settings"></param>
        /// <param name="msgSource"></param>
        public void DeleteResource(DeleteResource item, IServiceBusClient msgSource)
        {
        }

        public void ResourceLogon(ResourceLogon item)
        {
        }

        public void BeginDump(BeginDump item)
        {
            Logger.Write(string.Format("System reset commanded from " + item.From), TraceEventType.Information, "XReplayPlayer");
            _resStore.Clear();
        }

        public void SendCallsignNotification(string fleetNo, string callsign, string reason, IServiceBusClient serviceBusClient)
        {
            var devices = _devStore.GetByFleet(fleetNo);

            SendCallsignNotification(devices, callsign, reason, serviceBusClient);
        }

        private void SendCallsignNotification(List<QuestDevice> devices, string callsign, string reason, IServiceBusClient serviceBusClient)
        {
            if (devices == null || !devices.Any())
                return;

            var request = new CallsignNotification
            {
                Callsign = callsign
            };
            NotifyDevices(devices, request, reason, serviceBusClient);
        }

        /// <summary>
        ///     send an assignment to this FleetNo. Also sends to corresponding unmanaged callsigns if within range
        /// </summary>
        /// <param name="FleetNo"></param>
        /// <param name="serial"></param>
        /// <param name="settings"></param>
        /// <param name="reason"></param>
        public void SendEventNotification(string fleetNo, string serial, string reason, IServiceBusClient serviceBusClient)
        {
            var incident = _incStore.Get(serial);
            if (incident != null)
            {
                var devices = _devStore.GetByFleet(fleetNo);
                SendEventNotifications(devices, incident, reason, serviceBusClient);
            }
        }

        private void SendEventNotifications(List<QuestDevice> devices, QuestIncident inc, string reason, IServiceBusClient serviceBusClient)
        {
            if (devices == null || !devices.Any())
                return;

            if (inc == null)
                throw new ApplicationException("Unable to send notification, incident is NULL");

            if (inc.Latitude != null)
            {
                if (inc.Longitude != null)
                {
                    var request = new EventNotification
                    {
                        LocationComment = inc.LocationComment ?? "",
                        Location = inc.Location ?? "",
                        Priority = inc.Priority ?? "",
                        PatientAge = inc.PatientAge,
                        Determinant = inc.DeterminantDescription ?? inc.Determinant,
                        EventId = inc.EventId,
                        Latitude = (float)inc.Latitude,
                        Longitude = (float)inc.Longitude,
                        Sex = inc.PatientSex ?? "",
                        Notes = inc.ProblemDescription,
                        Reason = reason,
                        Updated = DateTime.Now.ToString(CultureInfo.InvariantCulture)
                    };

                    Logger.Write($"Sending incident {inc.EventId} ", TraceEventType.Information, "ResourceHandler");

                    NotifyDevices(devices, request, reason, serviceBusClient);
                }
            }
        }

        /// <summary>
        ///     send the status of this callsign to devices
        /// </summary>
        /// <param name="callsign"></param>
        /// <param name="settings"></param>
        /// <param name="reason"></param>
        public void SendStatusNotification(string fleetno, string reason, IServiceBusClient serviceBusClient)
        {
            var devices = _devStore.GetByFleet(fleetno);
            if (devices.Count > 0)
            {
                StatusNotification msg = new StatusNotification { };
                NotifyDevices(devices, msg, reason, serviceBusClient);
            }
        }

        /// <summary>
        /// This notifies the device of an event
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="evt"></param>
        /// <param name="settings"></param>
        /// <param name="reason"></param>
        private void NotifyDevices(IEnumerable<QuestDevice> devices, INotificationMessage evt, string reason, IServiceBusClient serviceBusClient)
        {
            try
            {
                // send push notifications
                foreach (var device in devices)
                    if (device.IsEnabled == true && !string.IsNullOrEmpty(device.DeviceIdentity))
                    {
                        Notification n = new Notification { Address = device.NotificationId, Body = evt, Method = device.NotificationTypeId, Subject = reason };
                        serviceBusClient.Broadcast(n);
                    }
            }
            catch (Exception ex)
            {
                Logger.Write($"error sending notification: {ex}",
                    TraceEventType.Information, "Quest");
            }
            finally
            {
            }
        }

    }

}