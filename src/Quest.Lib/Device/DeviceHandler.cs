#define NO_APPLE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Utils;
using Quest.Lib.Trace;
using Quest.Lib.Notifier;
using Quest.Lib.Incident;
using Quest.Lib.Resource;
using Quest.Lib.Data;
using Quest.Common.ServiceBus;
using Quest.Lib.Search.Elastic;

namespace Quest.Lib.Device
{
    public class DeviceHandler
    {
        IDatabaseFactory _dbFactory;

        public string deletedStatus { get; set; } = "";

        public string triggerStatus { get; set; } = "";

        private const int Version = 1;

        private int _deleteStatusId;

        private int DeleteStatusId
        {
            get
            {
                if (_deleteStatusId == 0)
                    GetDeleteStatusId();
                return _deleteStatusId;
            }
        }

        public DeviceHandler(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        /// <summary>
        ///     Class uses by the Quest server to process messages arriving from devices. In most cases the
        ///     device manager processes a xRequest and returns an xResponse.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public LoginResponse Login(LoginRequest request, IResourceStore resStore, IDeviceStore devStore, ResourceHandler resHandler, ResourceUpdate resourceUpdate, IServiceBusClient msgSource, BuildIndexSettings config, IIncidentStore incStore)
        {
            var callsign = "";
            var sc = new StatusCode() { Code="???", Description = "Pending" };

            var devrecord = devStore.Get(request.DeviceIdentity);

            if (devrecord != null) {
                devrecord.OwnerId = request.Username;
                devrecord.LoggedOnTime = DateTime.UtcNow;
                devrecord.LastUpdate = DateTime.UtcNow;
                devrecord.NotificationTypeId = request.NotificationTypeId;
                devrecord.NotificationId = request.NotificationId;
                devrecord.DeviceIdentity = request.DeviceIdentity;
                devrecord.OSVersion = request.OSVersion;
                devrecord.DeviceMake = request.DeviceMake;
                devrecord.DeviceModel = request.DeviceModel;
            }
            else
            {
                var offRoadStatus = resStore.GetOffroadStatusId();

                // new record
                devrecord = new QuestDevice
                {
                    OwnerId = request.Username,
                    DeviceIdentity = request.DeviceIdentity,
                    LoggedOnTime = DateTime.UtcNow,
                    LastUpdate = DateTime.UtcNow,
                    DeviceRoleId = 3, //TODO: This is the default role that the new login will play. This should come from a setting 
                    NotificationTypeId = request.NotificationTypeId,
                    NotificationId = request.NotificationId,
                    IsEnabled = true,
                    LastStatusUpdate = DateTime.UtcNow,
                    LoggedOffTime = null,
                    OSVersion = request.OSVersion,
                    DeviceMake = request.DeviceMake,
                    DeviceModel = request.DeviceModel,
                    ResourceId = null,
                    PositionAccuracy = 0,
                    NearbyDistance = 0,
                };
            }

            // make a new token. this becomes a claim (jti unique identitier
            devrecord.AuthToken = Guid.NewGuid().ToString();

            devStore.Update(devrecord);

            QuestResource resource=null;

            // try and use the provided fleetnumber to associate with a resource
            if (!string.IsNullOrEmpty(request.FleetNo))
                resource = resStore.GetByFleetNo(request.FleetNo);
            else
                resource = resStore.GetByResourceId(devrecord.ResourceId??0);

            // the device is not associated with a resource
            if (resource == null)
            {
                ResourceUpdate newresource = new ResourceUpdate
                {
                    Agency = "",
                    Destination = "",
                    Callsign = $"#{devrecord.DeviceID.ToString("0000")}",
                    Class = "",
                    Direction = 0,
                    FleetNo = $"DEV-{devrecord.DeviceID}",
                    EventType = "",
                    Skill = "",
                    Speed = 0
                };
                var resourceid = resHandler.ResourceUpdate(newresource, msgSource, config, incStore);
                resource = resStore.GetByResourceId(devrecord.ResourceId ?? 0);
            }

            // look up the resource if the id is set
            if (devrecord.ResourceId != resource.ResourceId)
            {
                devrecord.ResourceId = resource.ResourceId;
                devStore.Update(devrecord);
            }

            return new LoginResponse
            {
                QuestApi = Version,
                RequestId = request.RequestId,
                RequiresCallsign = false,
                Callsign = callsign,
                Status = sc,
                Success = true, 
                SessionId = devrecord.AuthToken,
                Message = "successfully logged on"
            };
        }

        /// <summary>
        /// Log out of Quest
        /// </summary>
        /// <param name="request"></param>
        /// <param name="devStore"></param>
        /// <returns></returns>
        public LogoutResponse Logout(LogoutRequest request, IDeviceStore devStore)
        {
            var resrecord = devStore.GetByToken(request.SessionId);

            if (resrecord != null)
            {
                resrecord.LoggedOffTime = DateTime.UtcNow;
                resrecord.LastUpdate = DateTime.UtcNow;
                resrecord.AuthToken = null;
                resrecord.NotificationId = "";
                resrecord.NotificationTypeId = "";

                devStore.Update(resrecord);

                return new LogoutResponse
                {
                    Success = true,
                    Message = "successful",
                    RequestId = request.RequestId
                };
            }
            return new LogoutResponse
            {
                Success = false,
                Message = "invalid authentication token",
                RequestId = request.RequestId
            };
        }
    
        /// <summary>
        /// Request a callsign change
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public CallsignChangeResponse CallsignChange(CallsignChangeRequest request, IDeviceStore devStore, IResourceStore resStore)
        {
            var oldCallsign = "";

            QuestDevice deviceRecord = devStore.GetByToken(request.SessionId);

            if (deviceRecord == null)
            {
                return new CallsignChangeResponse
                {
                    Success = false,
                    Message = "invalid authentication token",
                    RequestId = request.RequestId
                };
            }

            if (deviceRecord?.ResourceId==null)
            {
                return new CallsignChangeResponse
                {
                    Success = false,
                    Message = "device is not associated with a resource",
                    RequestId = request.RequestId
                };
            }

            var resource = resStore.GetByResourceId((int)deviceRecord.ResourceId);
                            
            return new CallsignChangeResponse
            {
                RequestId = request.RequestId,
                Success = true,
                OldCallsign = oldCallsign,
                NewCallsign = request.Callsign,
                Message = "requested"
            };
        }

        /// <summary>
        /// Get the status of the device. can be used at startup of the device so it has the right details.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public RefreshStateResponse RefreshState(RefreshStateRequest request, IIncidentStore incStore, IServiceBusClient serviceBusClient)
        {
            return _dbFactory.Execute<QuestContext, RefreshStateResponse>((db) =>
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.SessionId);
                if (deviceRecord == null)
                    return new RefreshStateResponse
                    {
                        RequestId = request.RequestId,
                        Success = false,
                        Message = "unknown device"
                    };

                if (deviceRecord.Resource == null)
                    return new RefreshStateResponse
                    {
                        RequestId = request.RequestId,
                        Success = false,
                        Message = "device not linked to a resource"
                    };

                //TODO: trigger message sending
#if false
                var inc = incStore.Get(deviceRecord.Resource.Serial);

                // send incident details if currently assigned
                if (inc != null)
                {
                    var devices = new List<DataModel.Devices> { deviceRecord };
                    SendEventNotification(devices, inc, "Refresh", serviceBusClient);
                }

                // also send the status
                SendStatusNotification(deviceRecord.Resource.Callsign.Callsign1, "Refresh", serviceBusClient);
                SendCallsignNotification(deviceRecord.Resource.Callsign.Callsign1, "Refresh", serviceBusClient);
#endif
                return new RefreshStateResponse();
            });
        }

        public AckAssignedEventResponse AckAssignedEvent(AckAssignedEventRequest request)
        {
            return _dbFactory.Execute<QuestContext, AckAssignedEventResponse>((db) =>
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.SessionId);
                if (deviceRecord == null)
                    return new AckAssignedEventResponse
                    {
                        RequestId = request.RequestId,
                        Success = false,
                        Message = "unknown device"
                    };

                if (deviceRecord.Resource == null)
                    return new AckAssignedEventResponse
                    {
                        RequestId = request.RequestId,
                        Success = false,
                        Message = "device not linked to a resource"
                    };

                if (deviceRecord.Resource.Serial == null)
                    return new AckAssignedEventResponse
                    {
                        RequestId = request.RequestId,
                        Success = false,
                        Message = "Not assigned to an event"
                    };

                // save audit record
                //TODO: Save to Elastic
                db.SaveChanges();

                return new AckAssignedEventResponse
                {
                    RequestId = request.RequestId,
                    Success = true,
                    Message = "successful"
                };
            });
        }

        public PositionUpdateResponse PositionUpdate(PositionUpdateRequest request)
        {
            return _dbFactory.Execute<QuestContext, PositionUpdateResponse>((db) =>
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.SessionId);
                if (deviceRecord == null)
                    return new PositionUpdateResponse
                    {
                        RequestId = request.RequestId,
                        Success = false,
                        Message = "unknown device"
                    };

                deviceRecord.Latitude = (float?)request.Vector.Latitude ?? 0;
                deviceRecord.Longitude = (float?)request.Vector.Longitude ?? 0;
                deviceRecord.LastUpdate = DateTime.UtcNow;
                deviceRecord.PositionAccuracy = (float)request.Vector.HDoP;

                //TODO: Save to Elastic
                db.SaveChanges();
                return new PositionUpdateResponse
                {
                    RequestId = request.RequestId,
                    Success = true,
                    Message = "successful"
                };

            });
        }

        public MakePatientObservationResponse MakePatientObservation(MakePatientObservationRequest request)
        {
            return _dbFactory.Execute<QuestContext, MakePatientObservationResponse>((db) =>
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.SessionId);
                if (deviceRecord == null)
                    return new MakePatientObservationResponse
                    {
                        RequestId = request.RequestId,
                        Success = false,
                        Message = "unknown device"
                    };

                // save audit record
                //TODO: Save to Elastic
                db.SaveChanges();
                return new MakePatientObservationResponse
                {
                    RequestId = request.RequestId,
                    Success = true,
                    Message = "successful"
                };
            });
        }

        public PatientDetailsResponse PatientDetails(PatientDetailsRequest request)
        {
            return _dbFactory.Execute<QuestContext, PatientDetailsResponse>((db) =>
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.SessionId);
                if (deviceRecord == null)
                    return new PatientDetailsResponse
                    {
                        RequestId = request.RequestId,
                        Success = false,
                        Message = "unknown device"
                    };

                // save audit record
                //TODO: Save to Elastic
                db.SaveChanges();

                var response = new PatientDetailsResponse
                {
                    RequestId = request.RequestId,
                    DoB = "03/03/1989",
                    FirstName = "siobhan",
                    LastName = "metcalfe-poulton",
                    NHSNumber = "NI72761982",
                    Notes =
                        new List<string>
                        {
                            "Some notes line 1",
                            "Some notes line 2",
                            "Some notes line 3",
                            "Some notes line 4"
                        },
                    Message = "success",
                    Success = true
                };

                return response;
            });
        }

        public GetEntityTypesResponse GetEntityTypes(GetEntityTypesRequest request)
        {
            return _dbFactory.Execute<QuestContext, GetEntityTypesResponse>((db) =>
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.SessionId);
                if (deviceRecord == null)
                    return new GetEntityTypesResponse
                    {
                        RequestId = request.RequestId,
                        Items = new List<string>(),
                        Success = false,
                        Message = "unknown device"
                    };

                var layers = new[]
                {
                    "Stations", "Hospitals (non-A&E)", "Hospital (A&E)", "Hospitals (Maternity)", "Fuel", "A-Z Grid", "CCG",
                    "Atoms"
                };
                return new GetEntityTypesResponse
                {
                    RequestId = request.RequestId,
                    Items = layers.ToList(),
                    Success = true,
                    Message = "successful"
                };
            });
        }

        public MapItemsResponse GetMapItems(MapItemsRequest request)
        {
            const int maxres = 20;
            const int maxdev = 20;
            const int maxinc = 20;

            var response = new MapItemsResponse
            {
                RequestId = request.RequestId,
                Resources = new List<ResourceItem>(),
                Destinations = new List<QuestDestination>(),
                Events = new List<EventMapItem>(),
                Devices = new List<ResourceItem>(),
                DeleteResources = new List<int>(),
                Success = true,
                Message = "successful"
            };

            _dbFactory.Execute<QuestContext>((db) =>
            {
                // make a note of the current revision
                var resmax = db.Resource.Max(x => x.Revision);
                
                response.CurrRevision = (long)(resmax??0) + 1;
            });

            response.Destinations = GetDestinations(request.Hospitals, request.Stations, request.Standby);

            // work out which ones were on display at the original revision
            var originalResource = GetResourcesAtRevision(request.Revision, request.ResourcesAvailable,
                request.ResourcesBusy);

            var newResource = GetResourcesAtRevision(response.CurrRevision, request.ResourcesAvailable,
                request.ResourcesBusy);

            if (request.ResourcesAvailable || request.ResourcesBusy)
            {
                // get resources
                response.Resources.AddRange(GetStandardResources(request.Revision, request.ResourcesAvailable,
                    request.ResourcesBusy));

                // get devices
                response.Devices.AddRange(GetDeviceResources(request.Revision, request.ResourcesAvailable,
                    request.ResourcesBusy));

                // work out which have been deleted - these are in not in new_resource but are in original_resources                
                foreach (var i1 in originalResource)
                {
                        var i = i1;
                        if (!newResource.Contains(i))
                            response.DeleteResources.Add(i);
                }

                // truncate for efficiency                
                response.Resources.Sort((x, y) => (int) (x.revision - y.revision));
                response.Resources = response.Resources.Take(maxres).ToList();

                response.Devices.Sort((x, y) => (int) (x.revision - y.revision));
                response.Devices = response.Devices.Take(maxdev).ToList();
            }

            if (request.IncidentsImmediate || request.IncidentsOther)
            {
                response.Events.AddRange(GetIncidents(request.Revision, request.IncidentsImmediate,
                    request.IncidentsOther));
                response.Events.Sort((x, y) => (int) (x.revision - y.revision));
                response.Events = response.Events.Take(maxinc).ToList();
            }

            // calculate the maximum revision being returned.
            long c1 = 0, c2 = 0, c3 = 0;
            if (response.Resources.Count > 0)
                c1 = response.Resources.Max(x => x.revision);


            if (response.Events.Count > 0)
                c2 = response.Events.Max(x => x.revision);

            if (response.Devices.Count > 0)
                c3 = response.Devices.Max(x => x.revision);

            response.Revision = Math.Max(Math.Max(c1, c2), c3);

            if (response.Revision == 0)
                response.Revision = response.CurrRevision;

            Debug.Print(
                $"Map rev in={request.Revision} rev out={response.Revision} rev cur = {response.CurrRevision} count inc={response.Events.Count} res={response.Resources.Count} dev={response.Devices.Count} deleted={response.DeleteResources.Count}");

            return response;
        }

        private List<QuestDestination> GetDestinations(bool hospitals, bool stations, bool standby)
        {
            return _dbFactory.Execute<QuestContext, List<QuestDestination>>((db) =>
            {
                var d = db.Destinations
                    .Where(x => ((hospitals == true && x.IsHospital == true))
                             || ((stations == true && x.IsStation == true))
                             || ((standby == true && x.IsStandby == true))
                              )
                    .ToList()
                    .Select(x =>
                    {
                        var point = Lib.Utils.GeomUtils.GetPointFromWkt(x.Wkt);
                        return new QuestDestination
                        {
                            ID = x.DestinationId.ToString(),
                            IsHospital = x.IsHospital ?? false,
                            IsAandE = x.IsAandE ?? false,
                            IsRoad = x.IsRoad ?? false,
                            IsStandby = x.IsStandby ?? false,
                            IsStation = x.IsStation ?? false,
                            Name = x.Destination,
                            X = point.X,
                            Y = point.Y
                        };

                    }
                    )
                    .ToList();

                foreach (var res in d)
                {
                    var latlng = LatLongConverter.OSRefToWGS84(res.X, res.Y);
                    res.X = latlng.Longitude;
                    res.Y = latlng.Latitude;
                }
                return d;
            });
        }

        IEnumerable<DataModel.Resource> ResourceAtRevision(QuestContext db, long revision)
        {
            //TODO: Net Core
            return new List<DataModel.Resource>();
        }

        private List<int> GetResourcesAtRevision(long revision, bool avail = false, bool busy = false)
        {
            return _dbFactory.Execute<QuestContext, List<int>>((db) =>
            {
                var results = new List<int>();

                // work out which ones were on display at the original revision
                if (avail && busy)
                {
                    var resources = ResourceAtRevision(db, revision)
                        .Where(x => x.ResourceStatus.Busy == true || x.ResourceStatus.Available == true)
                        .Select(x => x.ResourceId);
                    if (resources != null)
                        results.AddRange(resources);
                }
                else
                {
                    if (avail)
                    {
                        var resources = ResourceAtRevision(db, revision)
                                .Where(x => x.ResourceStatus.Available == true)
                                .Select(x => x.ResourceId);
                        if (resources != null)
                            results.AddRange(resources);
                    }

                    if (busy)
                    {
                        var resources = ResourceAtRevision(db, revision)
                            .Where(x => x.ResourceStatus.Busy == true)
                            .Select(x => x.ResourceId);
                        results.AddRange(resources);
                    }
                }

                return results;
            });
        }

        public List<EventMapItem> GetIncidents(long revision, bool includeCatA = false, bool includeCatB = false)
        {
            return _dbFactory.Execute<QuestContext, List<EventMapItem>>((db) =>
            {
                var results = new List<DataModel.Incident>();

                if (includeCatA)
                    results.AddRange(db.Incident.Where(x => x.Priority.StartsWith("R") && x.Revision > revision));

                if (includeCatB)
                    results.AddRange(db.Incident.Where(x => !x.Priority.StartsWith("R") && x.Revision > revision));

                var features = new List<EventMapItem>();

                foreach (var inc in results)
                {
                    if (inc.Longitude != null)
                    {
                        if (inc.Latitude != null)
                        {
                            var incsFeature = new EventMapItem
                            {
                                ID = inc.IncidentId.ToString(),
                                revision = inc.Revision ?? 0,
                                X = inc.Longitude ?? 0,
                                Y = inc.Latitude ?? 0,
                                EventId = inc.Serial,
                                Notes = inc.Determinant,
                                Priority = inc.Priority,
                                Status = inc.Status,
                                Created = inc.Created?.ToString("hh:MM") ?? "?",
                                LastUpdated = inc.LastUpdated,
                                //AssignedResources = inc.AssignedResources,
                                AZ = inc.Az,
                                Determinant = inc.Determinant,
                                DeterminantDescription = inc.DeterminantDescription,
                                Location = inc.Location,
                                LocationComment = inc.LocationComment,
                                PatientAge = inc.PatientAge,
                                PatientSex = inc.PatientSex,
                                ProblemDescription = inc.ProblemDescription
                            };

                            features.Add(incsFeature);
                        }
                    }
                }

                return features;
            });
        }

        private List<ResourceItem> GetDeviceResources(long revision, bool avail = false, bool busy = false)
        {
            return _dbFactory.Execute<QuestContext, List<ResourceItem>>((db) =>
            {
                var features = new List<ResourceItem>();

                if (avail)
                    features.AddRange(

                            db.Devices
                            .Where(
                                x =>
                                    x.ResourceStatus.Available == true &&
                                    x.Latitude != null && x.Longitude != null)
                            .Where(x => x.Revision > revision)
                            .ToList()
                            .Select(
                                res => new ResourceItem
                                {
                                    ID = res.DeviceId.ToString(),
                                    revision = res.Revision ?? 0,
                                    X = res.Longitude ?? 0,
                                    Y = res.Latitude ?? 0,
                                    Callsign = res.DeviceCallsign ?? $"DV{res.DeviceId}",
                                    lastUpdate = res.LastUpdate,
                                    StatusCategory = GetStatusDescription(res.ResourceStatus.Available ?? false, res.ResourceStatus.Busy ?? false, res.ResourceStatus.BusyEnroute ?? false, res.ResourceStatus.Rest ?? false),
                                    Status = res.ResourceStatus.Status,
                                    PrevStatus = res.PrevStatus,
                                    VehicleType = "Device",
                                    Destination = res.Destination,
                                    Eta = null,
                                    FleetNo = "",
                                    Road = res.Road,
                                    Comment = "",
                                    Skill = res.Skill,
                                    Speed = res.Speed,
                                    Direction = res.Direction,
                                    Incident = res.Event,
                                    Available = res.ResourceStatus.Available ?? false,
                                    Busy = res.ResourceStatus.Busy ?? false,
                                    BusyEnroute = res.ResourceStatus.BusyEnroute ?? false,
                                    ResourceTypeGroup = "HAND"
                                }
                            )

                        );

                if (busy)
                    features.AddRange(
                        db.Devices
                            .Where(
                                x =>
                                    x.ResourceStatus.Busy == true &&
                                    x.Latitude != null && x.Longitude != null)
                            .Where(x => x.Revision > revision)
                            .ToList()
                            .Select(
                                       res => new ResourceItem
                                       {
                                           ID = res.DeviceId.ToString(),
                                           revision = res.Revision ?? 0,
                                           X = res.Longitude ?? 0,
                                           Y = res.Latitude ?? 0,
                                           Callsign = res.DeviceCallsign ?? $"DV{res.DeviceId}",
                                           lastUpdate = res.LastUpdate,
                                           StatusCategory = GetStatusDescription(res.ResourceStatus.Available ?? false, res.ResourceStatus.Busy ?? false, res.ResourceStatus.BusyEnroute ?? false, res.ResourceStatus.Rest ?? false),
                                           Status = res.ResourceStatus.Status,
                                           PrevStatus = res.PrevStatus,
                                           VehicleType = "Device",
                                           Destination = res.Destination,
                                           Eta = null,
                                           FleetNo = "",
                                           Road = res.Road,
                                           Comment = "",
                                           Skill = res.Skill,
                                           Speed = res.Speed,
                                           Direction = res.Direction,
                                           Incident = res.Event,
                                           Available = res.ResourceStatus.Available ?? false,
                                           Busy = res.ResourceStatus.Busy ?? false,
                                           BusyEnroute = res.ResourceStatus.BusyEnroute ?? false,
                                           ResourceTypeGroup = "HAND"
                                       }

                            )
                        );

                var lastCallsign = "";
                var callsignCount = 0;
                foreach (var res in features.OrderBy(x => x.Callsign))
                {
                    var latlng = LatLongConverter.OSRefToWGS84(res.X, res.Y);
                    res.X = latlng.Longitude;
                    res.Y = latlng.Latitude;

                    if (lastCallsign == res.Callsign)
                    {
                        callsignCount++;
                        res.Callsign += "/" + callsignCount;
                    }
                    else
                        callsignCount = 0;
                }

                return features;
            });
        }

        private List<ResourceItem> GetStandardResources(long revision, bool avail = false, bool busy = false)
        {
            return _dbFactory.Execute<QuestContext, List<ResourceItem>>((db) =>
            {
                var results = new List<DataModel.Resource>();

                if (avail && busy)
                {
                    results.AddRange(
                        db.Resource.Where(x => x.ResourceStatus.Busy == true || x.ResourceStatus.Available == true)
                            .Where(x => x.Revision > revision)
                        );
                }
                else
                {
                    if (avail)
                        results.AddRange(
                            db.Resource
                                .Where(x => x.ResourceStatus.Available == true)
                                .Where(x => x.Revision > revision)
                            );

                    if (busy)
                        results.AddRange(
                            db.Resource
                                .Where(x => x.ResourceStatus.Busy == true)
                                .Where(x => x.Revision > revision)
                            );
                }

                var features = new List<ResourceItem>();


                foreach (var res in results)
                {
                    var resFeature = GetResourceItemFromView(res);
                    features.Add(resFeature);
                }
                return features;
            });
        }

        public GetHistoryResponse GetHistory(GetHistoryRequest request)
        {
            return _dbFactory.Execute<QuestContext, GetHistoryResponse>((db) =>
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.SessionId);
                if (deviceRecord == null)
                    return new GetHistoryResponse
                    {
                        RequestId = request.RequestId,
                        Items = new List<DeviceHistoryItem>(),
                        Success = false,
                        Message = "unknown device"
                    };

                var response = new GetHistoryResponse
                {
                    Items = new List<DeviceHistoryItem>(),
                    Success = true,
                    Message = "successful"
                };

                return response;
            });
        }

        /// <summary>
        ///     check if a device is the target (by callsign) or is nearby
        /// </summary>
        /// <param name="device"></param>
        /// <param name="target"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        private bool IsNearbyDeviceOrAssigned(DataModel.Devices device, float? Latitude, float? Longitude, string serial)
        {
            // enabled?
            if (device.IsEnabled == false)
                return false;

            // its linked to a resource - good, as it should always be anyway, and it has the right callsign?
            if (device.Resource?.Serial != null && string.Equals(device.Resource.Serial, serial, StringComparison.CurrentCultureIgnoreCase) && device.DeviceIdentity != null)
                return true;

            // check the nearby settings
            if (device.SendNearby == false)
                return false;

            if (device.NearbyDistance == 0)
                return false;

            // no position
            if (device.Latitude == null)
                return false;

            // no target
            if (Latitude == null)
                return false;

            // calculate the distance
            var distance = GeomUtils.Distance(device.Latitude ?? 0, device.Longitude ?? 0, Latitude ?? 0, Longitude ?? 0);

            if (distance <= device.NearbyDistance)
                return true;

            return false;
        }

        public void SendAlertMessage(string callsign, string message)
        {
            //using (QuestEntities db = new QuestEntities())
            //{
            //    db.NotifyResource(callsign, message, true, _warningSource, _warningGroup);
            //    db.SaveChanges();
            //}
        }

        private ResourceItem GetResourceItemFromView(DataModel.Resource res)
        {
            return new ResourceItem
            {
                ID = res.ResourceId.ToString(),
                revision = res.Revision ?? 0,
                X = res.Longitude ?? 0,
                Y = res.Latitude ?? 0,
                Callsign = res.Callsign.Callsign1,
                StatusCategory = GetStatusDescription(res),
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

        public GetStatusCodesResponse GetStatusCodes(GetStatusCodesRequest request)
        {
            return _dbFactory.Execute<QuestContext, GetStatusCodesResponse>((db) =>
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.SessionId);
                if (deviceRecord == null)
                    return new GetStatusCodesResponse
                    {
                        RequestId = request.RequestId,
                        Items = new List<StatusCode>(),
                        Success = false,
                        Message = "unknown device"
                    };

                List<StatusCode> results = null;

                switch (request.SearchMode)
                {
                    case GetStatusCodesRequest.Mode.AllCodes:
                        results = db.ResourceStatus.Where(x => x.NoSignal != true)
                            .Select(x => new StatusCode
                            {
                                Code = x.Status,
                                Description = GetStatusDescription(x.Available ?? false, x.Busy ?? false, x.BusyEnroute ?? false, x.Rest ?? false)
                            }
                            )
                            .ToList();
                        break;
                    case GetStatusCodesRequest.Mode.Context:
                        results = db.ResourceStatus.Where(x => x.NoSignal != true)
                            .Select(x => new StatusCode
                            {
                                Code = x.Status,
                                Description = GetStatusDescription(x.Available ?? false, x.Busy ?? false, x.BusyEnroute ?? false, x.Rest ?? false)
                            }
                            )
                            .ToList();
                        break;
                    case GetStatusCodesRequest.Mode.Specified:
                        results = db.ResourceStatus.Where(x => x.NoSignal != true)
                            .Select(x => new StatusCode
                            {
                                Code = x.Status,
                                Description = GetStatusDescription(x.Available ?? false, x.Busy ?? false, x.BusyEnroute ?? false, x.Rest ?? false)
                            }
                            )
                            .ToList();
                        break;
                }

                return new GetStatusCodesResponse
                {
                    RequestId = request.RequestId,
                    Items = results,
                    Success = true,
                    Message = "successful"
                };
            });
        }

        /// <summary>
        ///     Status request by device
        /// </summary>
        /// <param name="request"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public SetStatusResponse SetStatusRequest(SetStatusRequest request, IServiceBusClient serviceBusClient)
        {
            return _dbFactory.Execute<QuestContext, SetStatusResponse>((db) =>
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.SessionId);
                if (deviceRecord == null)
                    return new SetStatusResponse
                    {
                        RequestId = request.RequestId,
                        NewStatusCode = null,
                        OldStatusCode = null,
                        Success = false,
                        Message = "unknown device"
                    };

                // device is linked, set status on resource instead
                // the resource update will update all linked devices
                if (deviceRecord.ResourceId != null)
                {
                    //TODO:
                    return new SetStatusResponse
                    {
                        RequestId = request.RequestId,
                        NewStatusCode = null,
                        OldStatusCode = null,
                        Success = false,
                        Message = "not implemented - change status via MDT"
                    };
                }
                else
                {
                    return new SetStatusResponse
                    {
                        RequestId = request.RequestId,
                        NewStatusCode = null,
                        OldStatusCode = null,
                        Callsign = deviceRecord.DeviceCallsign,
                        Success = true,
                        Message = "successful"
                    };

                    //TODO: Migrate this code into the resource manager
#if false
                    StatusCode oldStatusCode = null;

                    if (deviceRecord.ResourceStatusId != null)
                    {
                        oldStatusCode = new StatusCode
                        {
                            Code = deviceRecord.ResourceStatus.Status,
                            Description = GetStatusDescription(deviceRecord.ResourceStatus.Available ?? false, deviceRecord.ResourceStatus.Busy ?? false, deviceRecord.ResourceStatus.BusyEnroute ?? false, deviceRecord.ResourceStatus.Rest ?? false),
                        };
                    }

                    var newStatusRecord = db.ResourceStatus.FirstOrDefault(x => x.Status == request.StatusCode);

                    if (newStatusRecord == null)
                        return new SetStatusResponse
                        {
                            RequestId = request.RequestId,
                            NewStatusCode = oldStatusCode,
                            OldStatusCode = oldStatusCode,
                            Success = false,
                            Message = "invalid status code"
                        };

                    var newStatusCode = new StatusCode
                    {
                        Code = newStatusRecord.Status,
                        Description = GetStatusDescription(deviceRecord.ResourceStatus.Available ?? false, deviceRecord.ResourceStatus.Busy ?? false, deviceRecord.ResourceStatus.BusyEnroute ?? false, deviceRecord.ResourceStatus.Rest ?? false)

                    };

                    // are we using the CAD to make status changes?
                    if (deviceRecord.UseExternalStatus.HasValue && deviceRecord.UseExternalStatus.Value)
                    {
                        // use the cad..
                    }
                    else
                    {
                        // set the device status
                        deviceRecord.ResourceStatusId = newStatusRecord.ResourceStatusId;
                        db.SaveChanges();
                    }

                    SendStatusNotification(deviceRecord.DeviceCallsign, "Update", serviceBusClient);

                    return new SetStatusResponse
                    {
                        RequestId = request.RequestId,
                        NewStatusCode = newStatusCode,
                        OldStatusCode = oldStatusCode,
                        Callsign = deviceRecord.DeviceCallsign,
                        Success = true,
                        Message = "successful"
                    };
#endif

                }

            });
        }

        private string GetStatusDescription(DataModel.ResourceStatus status)
        {
            return GetStatusDescription(status.Available ?? false, status.Busy ?? false, status.BusyEnroute ?? false, status.Rest ?? false);
        }

        private string GetStatusDescription(DataModel.Resource status)
        {
            return GetStatusDescription(status.ResourceStatus.Available ?? false, status.ResourceStatus.Busy ?? false, status.ResourceStatus.BusyEnroute ?? false, status.ResourceStatus.Rest ?? false);
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

    }
}