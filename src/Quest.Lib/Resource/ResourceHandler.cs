#define NO_APPLE
#define XCAN_SEND_NOTIFICATIONS_TO_RESOURCES

//TODO: Implement resource notifications using a Notification service
#define xCAN_SEND_NOTIFICATIONS_TO_RESOURCES

using System;
using System.Linq;
using Nest;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Search.Elastic;
using Quest.Common.ServiceBus;
using Quest.Lib.Notifier;
using Quest.Lib.Incident;
using Quest.Lib.Data;
using Quest.Lib.Trace;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;

namespace Quest.Lib.Resource
{
    public class ResourceHandler
    {
        public string deletedStatus { get; set; } = "OOS";
        public string triggerStatus { get; set; } = "DSP";

        private IDatabaseFactory _dbFactory;
        private const string Version = "1.0.0";
        private int _deleteStatusId;        

        public ResourceHandler(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        private int DeleteStatusId
        {
            get
            {
                if (_deleteStatusId == 0)
                    GetDeleteStatusId();
                return _deleteStatusId;

            }
        }

        /// <summary>
        ///     Detects resource changing to DSP (Dispatched) and sends incident details to callsign
        ///     Detects resource changing to status and sends status update to callsign
        /// </summary>
        /// <param name="resourceUpdate"></param>
        /// <param name="settings"></param>
        /// <param name="client"></param>
        /// <param name="msgSource"></param>
        public int ResourceUpdate(ResourceUpdate resourceUpdate, IServiceBusClient msgSource, BuildIndexSettings config, IIncidentStore incStore)
        {
            return _dbFactory.Execute<QuestContext, int>((db) =>
            {
                // make sure callsign exists
                var callsign = db.Callsign.FirstOrDefault(x => x.Callsign1 == resourceUpdate.Callsign);
                if (callsign == null)
                {
                    callsign = new Callsign { Callsign1 = resourceUpdate.Callsign };
                    db.Callsign.Add(callsign);
                    db.SaveChanges();
                }

                // find corresponding status;
                var status = db.ResourceStatus.FirstOrDefault(x => x.Status == resourceUpdate.Status);
                if (status == null)
                {
                    status = new DataModel.ResourceStatus
                    {
                        Status = resourceUpdate.Status,
                        Rest = false,
                        Offroad = false,
                        NoSignal = false,
                        BusyEnroute = false,
                        Busy = false,
                        Available = false
                    };
                    db.ResourceStatus.Add(status);
                    db.SaveChanges();
                }

                int? originalStatusId = null;
                int? originalTypeId = null;
                long? originalRevision = null;

                // find the most up-to-date resource record;
                var oldres = db.Resource.FirstOrDefault(x => x.CallsignId == callsign.CallsignId && x.EndDate == null);

                if (oldres == null)
                {
                    // resource doesn't exist so use current status as the last status
                    originalTypeId = db.ResourceType.FirstOrDefault(x => x.ResourceType1 == resourceUpdate.ResourceType).ResourceTypeId;
                }
                else
                {
                    originalTypeId = oldres.ResourceTypeId;
                    originalStatusId = oldres.ResourceStatusId;
                    originalRevision = oldres.Revision;

                    // mark record as old
                    oldres.EndDate = resourceUpdate.UpdateTime;
                }

                //create a new record for this moving dimension
                var newres = new DataModel.Resource
                {
                    // save the new resource record
                    Agency = resourceUpdate.Agency,
                    CallsignId = callsign.CallsignId,
                    Destination = resourceUpdate.Destination,
                    Direction = 0, //resourceUpdate.Direction;
                    Eta = null, // resourceUpdate.ETA;
                    EventType = resourceUpdate.EventType,
                    FleetNo = resourceUpdate.FleetNo,
                    Latitude = (float?)resourceUpdate.Latitude,
                    Longitude = (float?)resourceUpdate.Longitude,
                    Serial = resourceUpdate.Incident,
                    Speed = resourceUpdate.Speed,
                    ResourceTypeId = originalTypeId,
                    Skill = resourceUpdate.Skill,
                    Sector = "",
                    EndDate = null,
                    StartDate = resourceUpdate.UpdateTime,
                    ResourceStatusId = status.ResourceStatusId,
                    // copy across previous values not contained in the resource update
                    Comment = oldres?.Comment,
                    DestLatitude = oldres?.DestLatitude,
                    DestLongitude =oldres?.DestLongitude,
                };

                db.Resource.Add(newres);

                // save the old and new records
                db.SaveChanges();

                ResourceItem ri = GetResourceItemFromView(newres);

                var point = new PointGeoShape(new GeoCoordinate(newres.Latitude ?? 0, newres.Longitude ?? 0));

                if (config != null)
                {
                    // save in elastic
                    var indextext = resourceUpdate.Callsign + " " + newres.FleetNo + " " + resourceUpdate.ResourceType + " " +
                                    resourceUpdate.Status;
                    var add = new LocationDocument
                    {
                        ID = "RES " + resourceUpdate.FleetNo,
                        Type = "Res",
                        Source = "Quest",
                        indextext = indextext,
                        Description = indextext,
                        Location = new GeoLocation(newres.Latitude ?? 0, newres.Longitude ?? 0),
                        Point = point,
                        Created = DateTime.UtcNow
                    };
                    var descriptor = ElasticIndexer.GetBulkRequest(config);
                    ElasticIndexer.AddIndexItem(add, descriptor);
                    ElasticIndexer.CommitBultRequest(config, descriptor);
                }

                SendStatusNotification(resourceUpdate.Callsign, "Update", msgSource);

                SendEventNotification(resourceUpdate.Callsign, resourceUpdate.Incident, "C&C Assigned", incStore, msgSource);

                msgSource.Broadcast(new ResourceDatabaseUpdate() { Callsign = resourceUpdate.Callsign, Item = ri });

                return newres.ResourceId;

            });
        }

        private ResourceItem GetResourceItemFromView( DataModel.Resource res)
        {
            return new ResourceItem
            {
                ID = res.ResourceId.ToString(),
                revision = res.Revision ?? 0,
                X = res.Longitude ?? 0,
                Y = res.Latitude ?? 0,
                Callsign = res.Callsign.Callsign1,
                StatusCategory = GetStatusDescription(res.ResourceStatus),
                Status = res.ResourceStatus.Status,
                VehicleType = res.ResourceType.ResourceType1 ?? "VEH",
                Destination = res.Destination,
                Eta = res.Eta,
                FleetNo = res.FleetNo,
                Comment = res.Comment,
                Skill = res.Skill,
                Speed = res.Speed,
                Direction = res.Direction,
                Incident = res.Serial,
                Available = res.ResourceStatus.Available ?? false,
                Busy = res.ResourceStatus.Busy ?? false,
                BusyEnroute = res.ResourceStatus.BusyEnroute ?? false,
                //ResourceTypeGroup = res.ResourceTypeGroup,
            };
        }

        private void GetDeleteStatusId()
        {
            _dbFactory.Execute<QuestContext>((db) =>
            {
                var ds = db.ResourceStatus.FirstOrDefault(x => x.Status == deletedStatus);
                if (ds != null)
                {
                    _deleteStatusId = ds.ResourceStatusId;
                }
            });
        }

        /// <summary>
        ///     delete resource marks the resource as the status specificed in the "DeviceManager.DeletedStatus" setting
        /// </summary>
        /// <param name="item"></param>
        /// <param name="settings"></param>
        /// <param name="msgSource"></param>
        public void DeleteResource(DeleteResource item, IServiceBusClient msgSource)
        {
            _dbFactory.Execute<QuestContext>((db) =>
            {
                var oldres = db.Resource.Where(x => x.Callsign.Callsign1 == item.Callsign && x.EndDate==null).FirstOrDefault();
                if (oldres==null) return;

                oldres.EndDate = DateTime.UtcNow;

                var newres = new DataModel.Resource
                {
                    ResourceStatusId = DeleteStatusId,

                    // save the new resource record
                    Agency = oldres?.Agency,
                    CallsignId = oldres?.CallsignId,
                    Destination = oldres?.Destination,
                    Direction = oldres?.Direction,
                    Eta = oldres?.Eta,
                    EventType = oldres?.EventType,
                    FleetNo = oldres?.FleetNo,
                    Latitude = oldres?.Latitude,
                    Longitude = oldres?.Longitude,
                    Serial = oldres?.Serial,
                    Speed = oldres?.Speed,
                    ResourceTypeId = oldres?.ResourceTypeId,
                    Skill = oldres?.Skill,
                    Sector = "",
                    EndDate = null,
                    StartDate = oldres.EndDate,
                    Comment = oldres?.Comment,
                    DestLatitude = oldres?.DestLatitude,
                    DestLongitude = oldres?.DestLongitude,
                };

                db.Resource.Add(newres);

                db.SaveChanges();

                msgSource.Broadcast(new ResourceDatabaseUpdate() { Callsign = item.Callsign });
            });
        }

        public void ResourceLogon(ResourceLogon item)
        {
        }

        private string GetStatusDescription(bool available, bool busy, bool enroute, bool rest)
        {
            if (available == true)
                return "Available";
            if (enroute == true)
                return "Enroute";
            if (busy == true)
                return "Busy";
            if (rest == true)
                return "Rest";
            return "Offroad";
        }

        private string GetStatusDescription(DataModel.ResourceStatus status)
        {
            return GetStatusDescription(status.Available ?? false, status.Busy ?? false, status.BusyEnroute ?? false, status.Rest ?? false);
        }

        public void BeginDump(BeginDump item)
        {
            Logger.Write(string.Format("System reset commanded from " + item.From), TraceEventType.Information, "XReplayPlayer");
            _dbFactory.Execute<QuestContext>((db) =>
            {
                // remove all incidents
                db.Incident.RemoveRange(db.Incident);

                db.SaveChanges();

                // set all resource
                db.Resource.RemoveRange(db.Resource.Where(x => !x.Devices.Any()));

                db.SaveChanges();
            });
        }

        public void SendCallsignNotification(string callsign, string reason, IServiceBusClient serviceBusClient)
        {
            _dbFactory.Execute<QuestContext>((db) =>
            {
                // find associated devices with this resource callsign
                var devices = db
                    .Devices
                    .ToList()
                    .Where(x => x.Resource != null && x.Resource.Callsign.Callsign1 == callsign)
                    .ToList();

                foreach (var deviceRecord in devices)
                {
                    // save audit record
                    //TODO: Save to Elastic
                    db.SaveChanges();
                }

                SendCallsignNotification(devices, callsign, reason, serviceBusClient);
            });
        }

        private void SendCallsignNotification(List<Devices> devices, string callsign, string reason, IServiceBusClient serviceBusClient)
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
        ///     send an assignment to this callsign. Also sends to corresponding unmanaged callsigns if within range
        /// </summary>
        /// <param name="callsign"></param>
        /// <param name="serial"></param>
        /// <param name="settings"></param>
        /// <param name="reason"></param>
        public void SendEventNotification(string callsign, string serial, string reason, IIncidentStore incStore, IServiceBusClient serviceBusClient)
        {
            _dbFactory.Execute<QuestContext>((db) =>
            {
                // make sure callsign exists
                var callsignRec = db.Callsign.FirstOrDefault(x => x.Callsign1 == callsign);
                if (callsignRec == null)
                {
                    callsignRec = new Callsign { Callsign1 = callsign };
                    db.Callsign.Add(callsignRec);
                    db.SaveChanges();
                }

                // find corresponding resource;
                var res = db.Resource.FirstOrDefault(x => x.Callsign.Callsign1 == callsign);
                if (res == null)
                    throw new ApplicationException(
                        $"Unable to send notification, no resource exists with callsign {callsignRec}");

                // create a event message to send to devices if necessary
                var inc = incStore.Get(serial);

                // find associated devices with this resource callsign
                var devices = db
                    .Devices
                    .ToList()
                    .Where(x => x.ResourceId == res.ResourceId)
                    .ToList();

                foreach (var deviceRecord in devices)
                {
                    // save audit record
                    //TODO: Save to Elastic
                    db.SaveChanges();
                }

                SendEventNotification(devices, inc, reason, serviceBusClient);
            });
        }

        private void SendEventNotification(List<Devices> devices, QuestIncident inc, string reason, IServiceBusClient serviceBusClient)
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
                        AZGrid = inc.AZ ?? "",
                        CallOrigin = inc.Created == null ? "" : inc.Created.ToString(),
                        Determinant = inc.DeterminantDescription ?? inc.Determinant,
                        Created = inc.Created == null ? "" : inc.Created.ToString(),
                        EventId = inc.Serial,
                        Latitude = (float)inc.Latitude,
                        Longitude = (float)inc.Longitude,
                        Sex = inc.PatientSex ?? "",
                        Notes = inc.ProblemDescription,
                        Reason = reason,
                        Updated = DateTime.Now.ToString(CultureInfo.InvariantCulture)
                    };

                    Logger.Write($"Sending incident {inc.Serial} ", TraceEventType.Information, "DeviceTracker");

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
        public void SendStatusNotification(string callsign, string reason, IServiceBusClient serviceBusClient)
        {
            _dbFactory.Execute<QuestContext>((db) =>
            {
                // find corresponding resource;
                var resource = db.Resource.FirstOrDefault(x => x.Callsign.Callsign1 == callsign);
                if (resource == null)
                    throw new ApplicationException(
                        $"Unable to send notification, no resource exists with callsign {callsign}");

                var request = new StatusNotification
                {
                    Status = new StatusCode
                    {
                        Code = resource.ResourceStatus.Status,
                        Description = GetStatusDescription(resource.ResourceStatus),
                    }
                };

                Logger.Write($"Sending status to devices for callsign {callsign} ",
                    TraceEventType.Information, "DeviceTracker");

                var devices = db
                    .Devices
                    .ToList()
                    .Where(x => x.ResourceId == resource.ResourceId)
                    .ToList();

                foreach (var deviceRecord in devices)
                {
                    // save audit record
                    //TODO: Save to Elastic
                }

                if (devices.Count > 0)
                    NotifyDevices(devices, request, reason, serviceBusClient);
            });
        }

        /// <summary>
        /// This notifies the device of an event
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="evt"></param>
        /// <param name="settings"></param>
        /// <param name="reason"></param>
        private void NotifyDevices(IEnumerable<Devices> devices, INotificationMessage evt, string reason, IServiceBusClient serviceBusClient)
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