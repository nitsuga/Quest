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
            using (var db = new QuestEntities())
            {
                // make sure callsign exists
                var callsign = db.Callsigns.FirstOrDefault(x => x.Callsign1 == resourceUpdate.Callsign);
                if (callsign == null)
                {
                    callsign = new Callsign {Callsign1 = resourceUpdate.Callsign};
                    db.Callsigns.Add(callsign);
                    db.SaveChanges();
                }

                // find corresponding status;
                var status = db.ResourceStatus.FirstOrDefault(x => x.Status == resourceUpdate.Status);
                if (status == null)
                {
                    status = new ResourceStatu
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
                var res = db.Resources.FirstOrDefault(x => x.CallsignID == callsign.CallsignID);
                if (res == null)
                {
                    originalTypeId = db.ResourceTypes.FirstOrDefault(x => x.ResourceType1 == resourceUpdate.ResourceType).ResourceTypeId;
                    res = new DataModel.Resource();
                    db.Resources.Add(res);
                }
                else
                {
                    originalTypeId = res.ResourceTypeId;
                    originalStatusId = res.ResourceStatusID;
                    originalRevision = res.Revision;
                }

                // save the new resource record
                res.Agency = resourceUpdate.Agency;
                res.CallsignID = callsign.CallsignID;
                res.Class = resourceUpdate.Class;
                //res.Comment = resourceUpdate.Comment;
                res.Destination = resourceUpdate.Destination;
                res.Direction = 0; //resourceUpdate.Direction;
                res.Emergency = resourceUpdate.Emergency?"Y":"N";
                res.ETA = null; // resourceUpdate.ETA;
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
                res.ResourceStatusPrevID = res.ResourceStatusID;
                res.ResourceStatusID = status.ResourceStatusID;

#if CAN_SEND_NOTIFICATIONS_TO_RESOURCES
                var requireEventNotification = false;
                var requireStatusNotification = false;
#endif

                // detect change in status
                if (originalStatusId != status.ResourceStatusID)
                {
#if CAN_SEND_NOTIFICATIONS_TO_RESOURCES
                    requireStatusNotification = true;
                    if (status.Status == triggerStatus)
                        requireEventNotification = true;
#endif

                        // save a status history if the status has changed
                        var history = new ResourceStatusHistory
                    {
                        ResourceID = res.ResourceID,
                        ResourceStatusID = originalStatusId ?? status.ResourceStatusID,
                        // use current status if status not known
                        Revision = originalRevision ?? 0
                    };

                    db.ResourceStatusHistories.Add(history);
                }
                db.SaveChanges();

                var rv = db.ResourceViews.FirstOrDefault(x => x.ResourceID == res.ResourceID);
                ResourceItem ri = GetResourceItemFromView(rv);

                var point = new PointGeoShape(new GeoCoordinate(rv.Latitude ?? 0, rv.Longitude??0));

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
                        Location = new GeoLocation(rv.Latitude ?? 0, rv.Longitude ?? 0),
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

                msgSource.Broadcast(new ResourceDatabaseUpdate() { ResourceId = res.ResourceID, Item=ri });

            }
        }

        private ResourceItem GetResourceItemFromView(ResourceView res)
        {
            return new ResourceItem
            {
                ID = res.ResourceID,
                revision = res.Revision ?? 0,
                X = res.Longitude ?? 0,
                Y = res.Latitude ?? 0,
                Callsign = res.Callsign,
                lastUpdate = res.LastUpdated,
                StatusCategory = GetStatusDescription(res),
                Status = res.Status,
                PrevStatus = res.PrevStatus,
                VehicleType = res.ResourceType ?? "VEH",
                Destination = res.Destination,
                Eta = res.ETA,
                FleetNo = res.FleetNo,
                Road = res.Road,
                Comment = res.Comment,
                Skill = res.Skill,
                Speed = res.Speed,
                Direction = res.Direction,
                Incident = res.Serial,
                Available = res.Available ?? false,
                Busy = res.Busy ?? false,
                BusyEnroute = res.BusyEnroute ?? false,
                ResourceTypeGroup = res.ResourceTypeGroup,
            };
        }

        private void GetDeleteStatusId()
        {
            var deletedStatus = SettingsHelper.GetVariable("DeviceManager.DeletedStatus", "OOS");
            using (var db = new QuestEntities())
            {
                var ds = db.ResourceStatus.FirstOrDefault(x => x.Status == deletedStatus);
                if (ds != null)
                {
                    _deleteStatusId = ds.ResourceStatusID;
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
            using (var db = new QuestEntities())
            {
                var i = db.Resources.Where(x => x.Callsign.Callsign1 == item.Callsign).ToList();
                if (!i.Any()) return;
                foreach (var x in i)
                {
                    x.ResourceStatusID = DeleteStatusId;
                    x.LastUpdated = DateTime.UtcNow;
                    db.SaveChanges();
                    msgSource.Broadcast(new ResourceDatabaseUpdate() {ResourceId = x.ResourceID});
                }
            }
        }

        public void ResourceLogon(ResourceLogon item, NotificationSettings settings)
        {
            using (var db = new QuestEntities())
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
        private string GetStatusDescription(ResourceView status)
        {
            return GetStatusDescription(status.Available ?? false, status.Busy ?? false, status.BusyEnroute ?? false, status.Rest ?? false);
        }

    }

}