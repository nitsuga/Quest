using Quest.Common.Messages.Device;
using Quest.Lib.Notifier;
using Quest.Lib.Resource;

namespace Quest.Lib.Device
{
    public class MsgDeviceRelay
    {
        public MsgDeviceRelay()
        {
        }


        public void Login(LoginRequest request, IResourceStore resStore, IDeviceStore devStore)
        {
        }

        public void Logout(LogoutRequest request, IDeviceStore devStore)
        {
        }
    
        public void CallsignChange(CallsignChangeRequest request)
        {
        }

        public void AckAssignedEvent(AckAssignedEventRequest request)
        {
        }

        public void PositionUpdate(PositionUpdateRequest request)
        {
        }

        public void MakePatientObservation(MakePatientObservationRequest request)
        {
        }

        /// <summary>
        ///     Status request by device
        /// </summary>
        /// <param name="request"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public void SetStatusRequest(SetStatusRequest request, NotificationSettings settings)
        {
        }

#if false
#region CAD Message Handling

        /// <summary>
        ///     handle resource updates from GT.
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
            _dbFactory.Execute<QuestContext>((db) =>
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
                res.Emergency = resourceUpdate.Emergency ? "Y" : "N";
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


                // detect changes in status to DSP
                var requireEventNotification = false;
                var requireStatusNotification = false;

                // detect change in status
                if (originalStatusId != status.ResourceStatusId)
                {
                    requireStatusNotification = true;
                    if (status.Status == triggerStatus)
                        requireEventNotification = true;

                    // save a status history if the status has changed
                    var history = new ResourceStatusHistory
                    {
                        Callsign = res.Callsign,
                        ResourceStatusId = originalStatusId ?? status.ResourceStatusId,
                        // use current status if status not known
                        Revision = originalRevision ?? 0
                    };

                    db.ResourceStatusHistory.Add(history);
                }
                db.SaveChanges();

                var rv = db.Resource.FirstOrDefault(x => x.Callsign == res.Callsign);
                ResourceItem ri = GetResourceItemFromView(rv);

                var point = new PointGeoShape(new GeoCoordinate(rv.Latitude ?? 0, rv.Longitude ?? 0));

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

                if (requireStatusNotification)
                    SendStatusNotification(resourceUpdate.Callsign, settings, "Update");

                if (requireEventNotification)
                    SendEventNotification(resourceUpdate.Callsign, resourceUpdate.Incident, settings, "C&C Assigned", incStore);

                msgSource.Broadcast(new ResourceDatabaseUpdate() { Callsign = res.Callsign, Item = ri });

            });
        }

        private ResourceItem GetResourceItemFromView(DataModel.Resource res)
        {
            return new ResourceItem
            {
                ID = res.Callsign.ToString(),
                revision = res.Revision ?? 0,
                X = res.Longitude ?? 0,
                Y = res.Latitude ?? 0,
                Callsign = res.Callsign.Callsign1,
                lastUpdate = res.LastUpdated,
                StatusCategory = GetStatusDescription(res),
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
                ResourceTypeGroup = res.ResourceType.ResourceTypeGroup,
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
        public void DeleteResource(DeleteResource item, NotificationSettings settings, IServiceBusClient msgSource)
        {
            _dbFactory.Execute<QuestContext>((db) =>
            {
                var i = db.Resource.Where(x => x.Callsign.Callsign1 == item.Callsign).ToList();
                if (!i.Any()) return;
                foreach (var x in i)
                {
                    x.ResourceStatusId = DeleteStatusId;
                    x.LastUpdated = DateTime.UtcNow;
                    db.SaveChanges();
                    msgSource.Broadcast(new ResourceDatabaseUpdate() { Callsign = x.Callsign });
                }
            });
        }


        public void CPEventStatusListHandler( CPEventStatusList item, NotificationSettings settings)
        {
            _dbFactory.Execute<QuestContext>((db) =>
            {
                foreach (var i in item.Items)
                {
                    var inc = db.Incident.FirstOrDefault(x => x.Serial == i.Serial);
                    if (inc != null)
                    {
                        inc.PatientAge = i.Age;
                        inc.PatientSex = i.Sex;
                        inc.CallerTelephone = i.CallerTelephone;
                        inc.LocationComment = i.LocationComment;
                        inc.ProblemDescription = i.ProblemDescription;
                    }
                    db.SaveChanges();
                }
            });
        }

        public void CallDisconnectStatusListHandler( CallDisconnectStatusList item,
            NotificationSettings settings)
        {
            _dbFactory.Execute<QuestContext>((db) =>
            {
                foreach (var i in item.Items)
                {
                    var inc = db.Incident.FirstOrDefault(x => x.Serial == i.Serial);
                    if (inc != null)
                    {
                        inc.DisconnectTime = i.DisconnectTime;
                    }
                    db.SaveChanges();
                }
            });
        }


        public void BeginDump( BeginDump item, NotificationSettings settings)
        {
            Logger.Write(string.Format("System reset commanded from " + item.From), TraceEventType.Information, "XReplayPlayer");
            _dbFactory.Execute<QuestContext>((db) =>
            {
                // remove all incidents
                db.Incident.RemoveRange(db.Incident);

                db.SaveChanges();

                // set all resource
                db.Resource.RemoveRange(db.Resource.Where(x => !x.Devices.Any()));

                db.ResourceStatusHistory.RemoveRange(db.ResourceStatusHistory);

                db.SaveChanges();
            });
        }

#endregion
#endif

    }

}