#define NO_APPLE
//TODO: Implement resource notifications using a Notification service
#define xCAN_SEND_NOTIFICATIONS_TO_RESOURCES

using System;
using System.Linq;
using Nest;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Common.ServiceBus;
using Quest.Lib.Notifier;
using Quest.Lib.Incident;

namespace Quest.Lib.Resource
{
    public class ResourceHandler
    {
        private const string Version = "1.0.0";
        private int _deleteStatusId;

        public string triggerStatus { get; set; }

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
        public void ResourceUpdate(ResourceUpdate resourceUpdate, NotificationSettings settings,
            IServiceBusClient msgSource, BuildIndexSettings config, IIncidentStore incStore)
        {
            using (var db = new QuestContext())
            {
                // make sure callsign exists
                var callsign = db.Callsign.FirstOrDefault(x => x.Callsign1 == resourceUpdate.Callsign);
                if (callsign == null)
                {
                    callsign = new Callsign {Callsign1 = resourceUpdate.Callsign};
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

                // find corresponding resource;
                var res = db.Resource.FirstOrDefault(x => x.CallsignId == callsign.CallsignId);
                if (res == null)
                {
                    originalTypeId = db.ResourceType.FirstOrDefault(x => x.ResourceType1 == resourceUpdate.ResourceType).ResourceTypeId;
                    res = new DataModel.Resource();
                    db.Resource.Add(res);
                }
                else
                {
                    originalTypeId = res.ResourceTypeId;
                    originalStatusId = res.ResourceStatusId;
                    originalRevision = res.Revision;
                }

                // save the new resource record
                res.Agency = resourceUpdate.Agency;
                res.CallsignId = callsign.CallsignId;
                res.Class = resourceUpdate.Class;
                //res.Comment = resourceUpdate.Comment;
                res.Destination = resourceUpdate.Destination;
                res.Direction = 0; //resourceUpdate.Direction;
                res.Emergency = resourceUpdate.Emergency?"Y":"N";
                res.Eta = null; // resourceUpdate.ETA;
                res.EventType = resourceUpdate.EventType;
                res.FleetNo = resourceUpdate.FleetNo;
                res.Latitude = (float?)resourceUpdate.Latitude;
                res.Longitude = (float?)resourceUpdate.Longitude;
                res.LastUpdated = resourceUpdate.UpdateTime;
                res.Agency = resourceUpdate.Agency;
                res.Serial = resourceUpdate.Incident;
                res.Speed = resourceUpdate.Speed;
                res.ResourceTypeId = originalTypeId;
                res.Skill = resourceUpdate.Skill;
                res.Sector = "";
                res.ResourceStatusPrevId = res.ResourceStatusId;
                res.ResourceStatusId = status.ResourceStatusId;

#if CAN_SEND_NOTIFICATIONS_TO_RESOURCES
                var requireEventNotification = false;
                var requireStatusNotification = false;
#endif

                // detect change in status
                if (originalStatusId != status.ResourceStatusId)
                {
#if CAN_SEND_NOTIFICATIONS_TO_RESOURCES
                    requireStatusNotification = true;
                    if (status.Status == triggerStatus)
                        requireEventNotification = true;
#endif

                        // save a status history if the status has changed
                        var history = new ResourceStatusHistory
                    {
                        ResourceId = res.ResourceId,
                        ResourceStatusId = originalStatusId ?? status.ResourceStatusId,
                        // use current status if status not known
                        Revision = originalRevision ?? 0
                    };

                    db.ResourceStatusHistory.Add(history);
                }
                db.SaveChanges();

                //var rv = db.Resource.FirstOrDefault(x => x.ResourceId == res.ResourceId);
                ResourceItem ri = GetResourceItemFromView(res);

                var point = new PointGeoShape(new GeoCoordinate(res.Latitude ?? 0, res.Longitude??0));

                if (config != null)
                {
                    // save in elastic
                    var indextext = resourceUpdate.Callsign + " " + res.FleetNo + " " + resourceUpdate.ResourceType + " " +
                                    resourceUpdate.Status;
                    var add = new LocationDocument
                    {
                        ID = "RES " + resourceUpdate.FleetNo,
                        Type = "Res",
                        Source = "Quest",
                        indextext = indextext,
                        Description = indextext,
                        Location = new GeoLocation(res.Latitude ?? 0, res.Longitude ?? 0),
                        Point = point,
                        Created = DateTime.UtcNow
                    };
                    var descriptor = ElasticIndexer.GetBulkRequest(config);
                    ElasticIndexer.AddIndexItem(add, descriptor);
                    ElasticIndexer.CommitBultRequest(config, descriptor);
                }

#if CAN_SEND_NOTIFICATIONS_TO_RESOURCES
                if (requireStatusNotification)
                    SendStatusNotification(resourceUpdate.Callsign, settings, "Update");

                if (requireEventNotification)
                    SendEventNotification(resourceUpdate.Callsign, resourceUpdate.Incident, settings, "C&C Assigned", incStore);
#endif

                msgSource.Broadcast(new ResourceDatabaseUpdate() { ResourceId = res.ResourceId, Item=ri });

            }
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
                lastUpdate = res.LastUpdated,
                StatusCategory = GetStatusDescription(res.ResourceStatus),
                Status = res.ResourceStatus.Status,
                PrevStatus = res.ResourceStatusPrev.Status,
                VehicleType = res.ResourceType.ResourceType1 ?? "VEH",
                Destination = res.Destination,
                Eta = res.Eta,
                FleetNo = res.FleetNo,
                Road = res.Road,
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
            var deletedStatus = SettingsHelper.GetVariable("DeviceManager.DeletedStatus", "OOS");
            using (var db = new QuestContext())
            {
                var ds = db.ResourceStatus.FirstOrDefault(x => x.Status == deletedStatus);
                if (ds != null)
                {
                    _deleteStatusId = ds.ResourceStatusId;
                }
            }
        }

        /// <summary>
        ///     delete resource marks the resource as the status specificed in the "DeviceManager.DeletedStatus" setting
        /// </summary>
        /// <param name="item"></param>
        /// <param name="settings"></param>
        /// <param name="msgSource"></param>
        public void DeleteResource(DeleteResource item, NotificationSettings settings, IServiceBusClient msgSource)
        {
            using (var db = new QuestContext())
            {
                var i = db.Resource.Where(x => x.Callsign.Callsign1 == item.Callsign).ToList();
                if (!i.Any()) return;
                foreach (var x in i)
                {
                    x.ResourceStatusId = DeleteStatusId;
                    x.LastUpdated = DateTime.UtcNow;
                    db.SaveChanges();
                    msgSource.Broadcast(new ResourceDatabaseUpdate() {ResourceId = x.ResourceId});
                }
            }
        }

        public void ResourceLogon(ResourceLogon item, NotificationSettings settings)
        {
            using (var db = new QuestContext())
            {
            }
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

    }

}