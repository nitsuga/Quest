#define NO_APPLE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Nest;
using Newtonsoft.Json.Linq;
using PushSharp.Google;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Lib.Trace;
using Quest.Common.ServiceBus;
using Quest.Lib.Notifier;
using Quest.Lib.Incident;
using Quest.Lib.Resource;

namespace Quest.Lib.Device
{
    public class DeviceHandler
    {
        private const string Version = "1.0.0";

#if APPLE_MSG
        private ApnsServiceBroker _apnsBroker;
#endif
        private GcmServiceBroker _gcmBroker;

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

        public void Update(QuestDevice device)
        {
            using (var db = new QuestEntities())
            {
                // locate record and create or update
                var resrecord = db.Devices.FirstOrDefault(x => x.DeviceIdentity == device.DeviceIdentity);
                if (resrecord != null)
                {
                    resrecord.OwnerID = device.OwnerID;
                    resrecord.LoggedOnTime = DateTime.UtcNow;
                    resrecord.LastUpdate = DateTime.UtcNow;
                    resrecord.NotificationTypeID = device.NotificationTypeID;
                    resrecord.NotificationID = device.NotificationID;
                    resrecord.AuthToken = device.Token;
                    resrecord.DeviceIdentity = device.DeviceIdentity;
                    resrecord.OSVersion = device.OSVersion;
                    resrecord.DeviceMake = device.DeviceMake;
                    resrecord.DeviceModel = device.DeviceModel;
                }
                else
                {
                    var status = db.ResourceStatus.FirstOrDefault(x => x.Offroad == true);
                    // new record
                    resrecord = new DataModel.Device
                    {
                        OwnerID = device.OwnerID,
                        DeviceIdentity = device.DeviceIdentity,
                        LoggedOnTime = DateTime.UtcNow,
                        LastUpdate = DateTime.UtcNow,
                        DeviceRoleID = 3, //TODO: This is the default role that the new login will play. This should come from a setting 
                        NotificationTypeID = device.NotificationTypeID,
                        NotificationID = device.NotificationID,
                        AuthToken = device.Token,
                        isEnabled = true,
                        LastStatusUpdate = DateTime.UtcNow,
                        LoggedOffTime = null,
                        OSVersion = device.OSVersion,
                        DeviceMake = device.DeviceMake,
                        DeviceModel = device.DeviceModel,
                        ResourceID = null,
                        PositionAccuracy = 0,
                        NearbyDistance = 0,
                    };

                    db.Devices.Add(resrecord);
                }

                db.SaveChanges();

            } // using
        }

        /// <summary>
        ///     Class uses by the Quest server to process messages arriving from devices. In most cases the
        ///     device manager processes a xRequest and returns an xResponse.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public LoginResponse Login(LoginRequest request, IResourceStore resStore, IDeviceStore devStore)
        {
            var token = Guid.NewGuid().ToString();
            var callsign = "";
            var sc = new StatusCode();

            var resrecord = devStore.Get(request.DeviceIdentity);
            if (resrecord != null) {
                resrecord.OwnerID = request.Username;
                resrecord.LoggedOnTime = DateTime.UtcNow;
                resrecord.LastUpdate = DateTime.UtcNow;
                resrecord.NotificationTypeID = request.NotificationTypeId;
                resrecord.NotificationID = request.NotificationId;
                resrecord.AuthToken = token;
                resrecord.DeviceIdentity = request.DeviceIdentity;
                resrecord.OSVersion = request.OSVersion;
                resrecord.DeviceMake = request.DeviceMake;
                resrecord.DeviceModel = request.DeviceModel;
            }
            else
            {

                var offRoadStatus = resStore.GetOffroadStatusId();

                // new record
                resrecord = new QuestDevice
                {
                    OwnerID = request.Username,
                    DeviceIdentity = request.DeviceIdentity,
                    LoggedOnTime = DateTime.UtcNow,
                    LastUpdate = DateTime.UtcNow,
                    DeviceRoleID = 3, //TODO: This is the default role that the new login will play. This should come from a setting 
                    NotificationTypeID = request.NotificationTypeId,
                    NotificationID = request.NotificationId,
                    AuthToken = token,
                    isEnabled = true,
                    LastStatusUpdate = DateTime.UtcNow,
                    LoggedOffTime = null,
                    OSVersion = request.OSVersion,
                    DeviceMake = request.DeviceMake,
                    DeviceModel = request.DeviceModel,
                    ResourceID = null,
                    PositionAccuracy = 0,
                    NearbyDistance = 0,
                };
            }

            devStore.Update(resrecord);


#if false
                // make sure device is paired with a callsign
                resStore.Get()
                Resource resource = null;
                if (resrecord.ResourceID != null)
                    resource = db.Resources.FirstOrDefault(x => x.ResourceID == resrecord.ResourceID);

                if (resource == null)
                {
                    // the device is not linked to a resource so make up a callsign using the device id
                    // this might already exist in which case link to that resource using that callsign
                    // make a suitable callsign
                    callsign = "#" + resrecord.DeviceID.ToString("0000");
                    var cs = db.Callsigns.FirstOrDefault(x => x.Callsign1 == callsign);
                    if (cs == null)
                    {
                        cs = new Callsign {Callsign1 = callsign};
                        db.Callsigns.Add(cs);
                        db.SaveChanges();
                    }

                    resource = db.Resources.FirstOrDefault(x => x.Callsign.Callsign1 == callsign);

                    if (resource != null)
                    {
                        // a resource record already exists for this devices temporary callsign so use it
                        resrecord.ResourceID = resource.ResourceID;
                        db.SaveChanges();
                    }
                    else
                    {
                        // its a new device with a new callsign so create a resource for it

                        // get suitable status
                        var status = db.ResourceStatus.FirstOrDefault(x => x.Status == "OOS");
                        var type = db.ResourceTypes.FirstOrDefault(x => x.ResourceType1 == "HAND");

                        // create a resource record for this device
                        if (status != null)
                        {
                            resource = new Resource
                            {
                                Agency = "",
                                Destination = "",
                                CallsignID = cs.CallsignID,
                                Class = "",
                                Comment = "device for " + request.Username,
                                Direction = 0,
                                Emergency = "",
                                ETA = null,
                                FleetNo = 0,
                                EventType = "",
                                LastUpdated = DateTime.UtcNow,
                                ResourceTypeId = type.ResourceTypeId,
                                Road = "",
                                ResourceStatusID = status.ResourceStatusID,
                                Sector = "",
                                Serial = "",
                                Skill = "",
                                Speed = 0
                            };

                            db.Resources.Add(resource);
                            db.SaveChanges();

                            resrecord.ResourceID = resource.ResourceID;
                            db.SaveChanges();

                            //TODO: Save to Elastic
                        }
                        db.SaveChanges();
                    }
                }
                else
                {
                    callsign = resource.Callsign.Callsign1;
                }

                // return status back to device
                if (resource != null)
                {
                    sc = new StatusCode
                    {
                        Code = resource.ResourceStatu.Status,
                        Description = GetStatusDescription(resource.ResourceStatu),
                    };
                }
#endif

            return new LoginResponse
            {
                AuthToken = token,
                QuestApi = Version,
                RequestId = request.RequestId,
                RequiresCallsign = false,
                Callsign = callsign,
                Status = sc,
                Success = true,
                Message = "successfully logged on"
            };
        }

        public LogoutResponse Logout(LogoutRequest request, IDeviceStore devStore)
        {
            var resrecord = devStore.GetByToken(request.AuthToken);

            if (resrecord != null)
            {
                resrecord.LoggedOffTime = DateTime.UtcNow;
                resrecord.LastUpdate = DateTime.UtcNow;
                resrecord.AuthToken = null;
                resrecord.NotificationID = "";
                resrecord.NotificationTypeID = 0;

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
    
        public CallsignChangeResponse CallsignChange(CallsignChangeRequest request)
        {
            var oldCallsign = "";
            using (var db = new QuestEntities())
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.AuthToken);
                if (deviceRecord == null)
                {
                    return new CallsignChangeResponse
                    {
                        Success = false,
                        Message = "invalid authentication token",
                        RequestId = request.RequestId
                    };
                }

                if (deviceRecord.Resource?.Callsign != null)
                    oldCallsign = deviceRecord.Resource.Callsign.Callsign1;

                if (request.Callsign != null && request.Callsign.Length >= 0)
                {
                    var resrecord = db.Resources.FirstOrDefault(x => x.Callsign.Callsign1 == request.Callsign);
                    if (resrecord == null)
                    {
                        return new CallsignChangeResponse
                        {
                            RequestId = request.RequestId,
                            Success = false,
                            Message = "unknown callsign"
                        };
                    }
                    deviceRecord.ResourceID = resrecord.ResourceID;
                }

                deviceRecord.LastUpdate = DateTime.UtcNow;

                //TODO: Save to Elastic
                db.SaveChanges();
            }

            return new CallsignChangeResponse
            {
                RequestId = request.RequestId,
                Success = true,
                OldCallsign = oldCallsign,
                NewCallsign = request.Callsign,
                Message = "successful"
            };
        }

        /// <summary>
        ///     get the status of the device. can be used at startup of the device so it has the right details.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public RefreshStateResponse RefreshState(RefreshStateRequest request, NotificationSettings settings, IIncidentStore incStore)
        {
            using (var db = new QuestEntities())
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.AuthToken);
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

                var inc = incStore.Get(deviceRecord.Resource.Serial);

                // send incident details if currently assigned
                if (inc != null)
                {
                    var devices = new List<DataModel.Device> {deviceRecord};
                    SendEventNotification(devices, inc, settings, "Refresh");
                }

                // also send the status
                SendStatusNotification(deviceRecord.Resource.Callsign.Callsign1, settings, "Refresh");
                SendCallsignNotification(deviceRecord.Resource.Callsign.Callsign1, settings, "Refresh");

                return new RefreshStateResponse();
            }
        }

        public AckAssignedEventResponse AckAssignedEvent(AckAssignedEventRequest request)
        {
            using (var db = new QuestEntities())
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.AuthToken);
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
            }
        }

        public PositionUpdateResponse PositionUpdate(PositionUpdateRequest request)
        {
            using (var db = new QuestEntities())
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.AuthToken);
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
                deviceRecord.PositionAccuracy = (float) request.Vector.HDoP;

                //TODO: Save to Elastic
                db.SaveChanges();
            }
            return new PositionUpdateResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "successful"
            };
        }

        public MakePatientObservationResponse MakePatientObservation(MakePatientObservationRequest request)
        {
            using (var db = new QuestEntities())
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.AuthToken);
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
            }
            return new MakePatientObservationResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "successful"
            };
        }

        public PatientDetailsResponse PatientDetails(PatientDetailsRequest request)
        {
            using (var db = new QuestEntities())
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.AuthToken);
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
            }
        }

        public GetEntityTypesResponse GetEntityTypes(GetEntityTypesRequest request)
        {
            using (var db = new QuestEntities())
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.AuthToken);
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
            }
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

            using (var db = new QuestEntities())
            {
                // make a note of the current revision
                var firstOrDefault = db.GetRevision().FirstOrDefault();
                if (firstOrDefault != null)
                    response.CurrRevision = (long) firstOrDefault;
            }

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
                    if (i1 != null)
                    {
                        var i = (int) i1;
                        if (!newResource.Contains(i))
                            response.DeleteResources.Add(i);
                    }
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
            using (var db = new QuestEntities())
            {
                var d = db.DestinationViews
                    .Where(x => ((hospitals == true && x.IsHospital == true))
                             || ((stations == true && x.IsStation == true))
                             || ((standby == true && x.IsStandby == true))
                              )
                    .ToList()
                    .Select(x => new QuestDestination
                    {
                        DestinationId = x.DestinationID,
                        IsHospital = x.IsHospital ?? false,
                        IsAandE = x.IsAandE ?? false,
                        IsRoad = x.IsRoad ?? false,
                        IsStandby = x.IsStandby ?? false,
                        IsStation = x.IsStation ?? false,
                        Name = x.Destination,
                        Position = new GeoAPI.Geometries.Coordinate(x.X ?? 0, x.Y ?? 0)
                    })
                    .ToList();

                foreach (var res in d)
                {
                    var latlng = LatLongConverter.OSRefToWGS84(res.Position);
                    res.Position = new GeoAPI.Geometries.Coordinate(  latlng.Longitude, latlng.Latitude);
                }
                return d;
            }
        }

        private List<int?> GetResourcesAtRevision(long revision, bool avail = false, bool busy = false)
        {
            using (var db = new QuestEntities())
            {
                var results = new List<int?>();

                // work out which ones were on display at the original revision
                if (avail && busy)
                {
                    results.AddRange(
                        db.ResourceAtRevision(revision)
                            .Where(x => x.Busy == true || x.Available == true)
                            .Select(x => x.ResourceID)
                        );
                }
                else
                {
                    if (avail)
                        results.AddRange(
                            db.ResourceAtRevision(revision)
                                .Where(x => x.Available == true)
                                .Select(x => x.ResourceID)
                            );

                    if (busy)
                        results.AddRange(
                            db.ResourceAtRevision(revision)
                                .Where(x => x.Busy == true)
                                .Select(x => x.ResourceID)
                            );
                }

                return results;
            }
        }

        public List<EventMapItem> GetIncidents(long revision, bool includeCatA = false, bool includeCatB = false)
        {
            using (var db = new QuestEntities())
            {
                var results = new List<IncidentView>();

                if (includeCatA)
                    results.AddRange(db.IncidentViews.Where(x => x.Priority.StartsWith("R") && x.Revision > revision));

                if (includeCatB)
                    results.AddRange(db.IncidentViews.Where(x => !x.Priority.StartsWith("R") && x.Revision > revision));

                var features = new List<EventMapItem>();

                foreach (var inc in results)
                {
                    if (inc.Longitude != null)
                    {
                        if (inc.Latitude != null)
                        {
                            var incsFeature = new EventMapItem
                            {
                                ID = inc.IncidentID,
                                revision = inc.Revision ?? 0,
                                X = inc.Longitude ?? 0,
                                Y = inc.Latitude ?? 0,
                                EventId = inc.Serial,
                                Notes = inc.Determinant,
                                Priority = inc.Priority,
                                Status = inc.Status,
                                Created = inc.Created?.ToString("hh:MM") ?? "?",
                                LastUpdated =inc.LastUpdated,
                                //AssignedResources = inc.AssignedResources,
                                AZ = inc.AZ,
                                Determinant = inc.Determinant,
                                DeterminantDescription = inc.DeterminantDescription,
                                Location= inc.Location,
                                LocationComment = inc.LocationComment,
                                PatientAge =inc.PatientAge,
                                PatientSex =inc.PatientSex,
                                ProblemDescription = inc.ProblemDescription
                            };

                            features.Add(incsFeature);
                        }
                    }
                }

                return features;
            }
        }

        private List<ResourceItem> GetDeviceResources(long revision, bool avail = false, bool busy = false)
        {
            using (var db = new QuestEntities())
            {
                var features = new List<ResourceItem>();

                if (avail)
                    features.AddRange(

                            db.DeviceViews
                            .AsNoTracking()
                            .Where(
                                x =>
                                    x.Available == true &&
                                    x.Latitude != null && x.Longitude != null)
                            .Where(x => x.Revision > revision)
                            .ToList()
                            .Select(
                                res => new ResourceItem
                                {
                                    ID = res.DeviceID,
                                    revision = res.Revision ?? 0,
                                    X = res.Longitude??0,
                                    Y = res.Latitude??0,
                                    Callsign = res.DeviceCallsign??$"DV{res.DeviceID}",
                                    lastUpdate = res.LastUpdate,
                                    StatusCategory = GetStatusDescription(res.Available ?? false, res.Busy ?? false, res.BusyEnroute ?? false, res.Rest ?? false),
                                    Status = res.Status,
                                    PrevStatus = res.PrevStatus,
                                    VehicleType = "Device",
                                    Destination = res.Destination,
                                    Eta = null,
                                    FleetNo = 0,
                                    Road = res.Road,
                                    Comment = "",
                                    Skill = res.Skill,
                                    Speed = res.Speed,
                                    Direction = res.Direction,
                                    Incident = res.Event,
                                    Available = res.Available ?? false,
                                    Busy = res.Busy ?? false,
                                    BusyEnroute = res.BusyEnroute ?? false,
                                    ResourceTypeGroup = "HAND"
                                }
                            )

                        );

                if (busy)
                    features.AddRange(
                        db.DeviceViews
                            .AsNoTracking()
                            .Where(
                                x =>
                                    x.Busy == true && 
                                    x.Latitude != null && x.Longitude != null)
                            .Where(x => x.Revision > revision)
                            .ToList()
                            .Select(
                                       res => new ResourceItem
                                       {
                                           ID = res.DeviceID,
                                           revision = res.Revision ?? 0,
                                           X = res.Longitude ?? 0,
                                           Y = res.Latitude ?? 0,
                                           Callsign = res.DeviceCallsign ?? $"DV{res.DeviceID}",
                                           lastUpdate = res.LastUpdate,
                                           StatusCategory = GetStatusDescription(res.Available ?? false, res.Busy ?? false, res.BusyEnroute ?? false, res.Rest ?? false),
                                           Status = res.Status,
                                           PrevStatus = res.PrevStatus,
                                           VehicleType = "Device",
                                           Destination = res.Destination,
                                           Eta = null,
                                           FleetNo = 0,
                                           Road = res.Road,
                                           Comment = "",
                                           Skill = res.Skill,
                                           Speed = res.Speed,
                                           Direction = res.Direction,
                                           Incident = res.Event,
                                           Available = res.Available ?? false,
                                           Busy = res.Busy ?? false,
                                           BusyEnroute = res.BusyEnroute ?? false,
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
            }
        }

        private List<ResourceItem> GetStandardResources(long revision, bool avail = false, bool busy = false)
        {
            using (var db = new QuestEntities())
            {
                var results = new List<ResourceView>();

                if (avail && busy)
                {
                    results.AddRange(
                        db.ResourceViews.Where(x => x.Busy == true || x.Available == true)
                            .Where(x => x.Revision > revision)
                        );
                }
                else
                {
                    if (avail)
                        results.AddRange(
                            db.ResourceViews
                                .Where(x => x.Available == true)
                                .Where(x => x.Revision > revision)
                            );

                    if (busy)
                        results.AddRange(
                            db.ResourceViews
                                .Where(x => x.Busy == true)
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
            }
        }

        public GetHistoryResponse GetHistory(GetHistoryRequest request)
        {
            using (var db = new QuestEntities())
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.AuthToken);
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
            }
        }

        public AssignDeviceResponse AssignDevice(AssignDeviceRequest request, NotificationSettings settings, IIncidentStore incStore)
        {
            var inc = incStore.Get(request.EventId);

            if (inc == null)
            {
                Logger.Write(string.Format("Resource assigned to unknown incident " + request.EventId),
                    TraceEventType.Information, "DeviceTracker");

                //Send incident alert message to Quest informing that the incident doesn't exist                    
                try
                {
                    SendAlertMessage(request.Callsign,
                        $"Device dispatched {request.Callsign} to unknown incident {request.EventId}");
                }
                catch (Exception ex)
                {
                    Logger.Write(
                        string.Format("ERROR: Cannot send alert for Device dispatch to unknown incident " + ex.Message),
                        TraceEventType.Error, "DeviceTracker");
                }

                return new AssignDeviceResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = "unknown incident"
                };
            }
            SendEventNotification(request.Callsign, request.EventId, settings, "Quest Assigned", incStore);

            return new AssignDeviceResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "successful"
            };
        }

        /// <summary>
        ///     check if a device is the target (by callsign) or is nearby
        /// </summary>
        /// <param name="device"></param>
        /// <param name="target"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        private bool IsNearbyDeviceOrAssigned(DataModel.Device device, float? Latitude, float? Longitude, string serial)
        {
            // enabled?
            if (device.isEnabled == false)
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


                // detect changes in status to DSP
                var requireEventNotification = false;
                var requireStatusNotification = false;

                // detect change in status
                if (originalStatusId != status.ResourceStatusID)
                {
                    requireStatusNotification = true;
                    if (status.Status == triggerStatus)
                        requireEventNotification = true;

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

                if (requireStatusNotification)
                    SendStatusNotification(resourceUpdate.Callsign, settings, "Update");

                if (requireEventNotification)
                    SendEventNotification(resourceUpdate.Callsign, resourceUpdate.Incident, settings, "C&C Assigned", incStore);
                
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


        public void CPEventStatusListHandler( CPEventStatusList item, NotificationSettings settings)
        {
            using (var db = new QuestEntities())
            {
                foreach (var i in item.Items)
                {
                    var inc = db.Incidents.FirstOrDefault(x => x.Serial == i.Serial);
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
            }
        }

        public void CallDisconnectStatusListHandler( CallDisconnectStatusList item,
            NotificationSettings settings)
        {
            using (var db = new QuestEntities())
            {
                foreach (var i in item.Items)
                {
                    var inc = db.Incidents.FirstOrDefault(x => x.Serial == i.Serial);
                    if (inc != null)
                    {
                        inc.DisconnectTime = i.DisconnectTime;
                    }
                    db.SaveChanges();
                }
            }
        }


        public void BeginDump( BeginDump item, NotificationSettings settings)
        {
            Logger.Write(string.Format("System reset commanded from " + item.From), TraceEventType.Information, "XReplayPlayer");
            using (var db = new QuestEntities())
            {
                // remove all incidents
                db.Incidents.RemoveRange(db.Incidents);

                db.SaveChanges();

                // set all resource
                db.Resources.RemoveRange(db.Resources.Where(x => !x.Devices.Any()));

                db.ResourceStatusHistories.RemoveRange(db.ResourceStatusHistories);

                db.SaveChanges();
            }
        }

#endregion

#region Status Handling


        public GetStatusCodesResponse GetStatusCodes(GetStatusCodesRequest request)
        {
            using (var db = new QuestEntities())
            {
                var deviceRecord = db.Devices.FirstOrDefault(x => x.AuthToken == request.AuthToken);
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
            }
        }

        /// <summary>
        ///     Status request by device
        /// </summary>
        /// <param name="request"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public SetStatusResponse SetStatusRequest(SetStatusRequest request, NotificationSettings settings)
        {
            using (var db = new QuestEntities())
            {
                var deviceRecord = db.DeviceViews.FirstOrDefault(x => x.AuthToken == request.AuthToken);
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
                if (deviceRecord.ResourceID != null)
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

                    StatusCode oldStatusCode = null;

                    if (deviceRecord.ResourceStatusID != null)
                    {
                        oldStatusCode = new StatusCode
                        {
                            Code = deviceRecord.Status,
                            Description = GetStatusDescription(deviceRecord.Available ?? false, deviceRecord.Busy ?? false, deviceRecord.BusyEnroute ?? false, deviceRecord.Rest ?? false),
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
                        Description = GetStatusDescription(deviceRecord.Available ?? false, deviceRecord.Busy ?? false, deviceRecord.BusyEnroute ?? false, deviceRecord.Rest ?? false)

                    };

                    // are we using the CAD to make status changes?
                    if (deviceRecord.UseExternalStatus.HasValue && deviceRecord.UseExternalStatus.Value)
                    {
                        // use the cad..
                    }
                    else
                    {
                        // set the device status
                        deviceRecord.ResourceStatusID = newStatusRecord.ResourceStatusID;
                        db.SaveChanges();
                    }

                    SendStatusNotification(deviceRecord.Callsign, settings, "Update");

                    return new SetStatusResponse
                    {
                        RequestId = request.RequestId,
                        NewStatusCode = newStatusCode,
                        OldStatusCode = oldStatusCode,
                        Callsign = deviceRecord.DeviceCallsign,
                        Success = true,
                        Message = "successful"
                    };
                }
            }
        }

        private string GetStatusDescription(ResourceStatu status)
        {
            return GetStatusDescription(status.Available ?? false, status.Busy ?? false, status.BusyEnroute ?? false, status.Rest ?? false);
        }

        private string GetStatusDescription(ResourceView status)
        {
            return GetStatusDescription(status.Available ?? false, status.Busy ?? false, status.BusyEnroute ?? false, status.Rest ?? false);
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

#endregion

#region GCM

        private void SendCallsignNotification(string callsign, NotificationSettings settings, string reason)
        {
            using (var db = new QuestEntities())
            {
                // find associated devices with this resource callsign
                var devices = db
                    .Devices
                    .AsNoTracking()
                    .ToList()
                    .Where(x => x.Resource != null && x.Resource.Callsign.Callsign1 == callsign)
                    .ToList();

                foreach (var deviceRecord in devices)
                {
                    // save audit record
                    //TODO: Save to Elastic
                    db.SaveChanges();
                }

                SendCallsignNotification(devices, callsign, settings, reason);
            }
        }

        private void SendCallsignNotification(List<DataModel.Device> devices, string callsign,
            NotificationSettings settings, string reason)
        {
            if (devices == null || !devices.Any())
                return;

            var request = new CallsignNotification
            {
                Callsign = callsign
            };
            NotifyDevices(devices, request, settings, reason);
        }


        /// <summary>
        ///     send an assignment to this callsign. Also sends to corresponding unmanaged callsigns if within range
        /// </summary>
        /// <param name="callsign"></param>
        /// <param name="serial"></param>
        /// <param name="settings"></param>
        /// <param name="reason"></param>
        private void SendEventNotification(string callsign, string serial, NotificationSettings settings,
            string reason, IIncidentStore incStore)
        {
            using (var db = new QuestEntities())
            {
                // make sure callsign exists
                var callsignRec = db.Callsigns.FirstOrDefault(x => x.Callsign1 == callsign);
                if (callsignRec == null)
                {
                    callsignRec = new Callsign {Callsign1 = callsign};
                    db.Callsigns.Add(callsignRec);
                    db.SaveChanges();
                }

                // find corresponding resource;
                var res = db.Resources.FirstOrDefault(x => x.Callsign.Callsign1 == callsign);
                if (res == null)
                    throw new ApplicationException(
                        $"Unable to send notification, no resource exists with callsign {callsignRec}");

                // create a event message to send to devices if necessary
                var inc = incStore.Get(serial);

                // find associated devices with this resource callsign
                var devices = db
                    .Devices
                    .AsNoTracking()
                    .ToList()
                    .Where(x => x.ResourceID == res.ResourceID)
                    .ToList();

                foreach (var deviceRecord in devices)
                {
                    // save audit record
                    //TODO: Save to Elastic
                    db.SaveChanges();
                }

                SendEventNotification(devices, inc, settings, reason);
            }
        }


        private void SendEventNotification(List<DataModel.Device> devices, QuestIncident inc,
            NotificationSettings settings, string reason)
        {
            if (devices == null || !devices.Any())
                return;

            if (inc == null)
                throw new ApplicationException("Unable to send notification, incident is NULL");

            if (inc.Latitude != null)
            {
                if (inc.Longitude!= null)
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

                    NotifyDevices(devices, request, settings, reason);
                }
            }
        }

        /// <summary>
        ///     send the status of this callsign to devices
        /// </summary>
        /// <param name="callsign"></param>
        /// <param name="settings"></param>
        /// <param name="reason"></param>
        private void SendStatusNotification(string callsign, NotificationSettings settings, string reason)
        {
            using (var db = new QuestEntities())
            {
                // find corresponding resource;
                var resource = db.Resources.FirstOrDefault(x => x.Callsign.Callsign1 == callsign);
                if (resource == null)
                    throw new ApplicationException(
                        $"Unable to send notification, no resource exists with callsign {callsign}");

                var request = new StatusNotification
                {
                    Status = new StatusCode
                    {
                        Code = resource.ResourceStatu.Status,
                        Description = GetStatusDescription(resource.ResourceStatu),
                    }
                };

                Logger.Write($"Sending status to devices for callsign {callsign} ",
                    TraceEventType.Information, "DeviceTracker");

                var devices = db
                    .Devices
                    .AsNoTracking()
                    .ToList()
                    .Where(x => x.ResourceID == resource.ResourceID)
                    .ToList();

                foreach (var deviceRecord in devices)
                {
                    // save audit record
                    //TODO: Save to Elastic
                }

                if (devices.Count > 0)
                    NotifyDevices(devices, request, settings, reason);
            }
        }

        //TODO: Implement device notifications using a Notification service

        /// <summary>
        /// This notifies the device of an event
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="evt"></param>
        /// <param name="settings"></param>
        /// <param name="reason"></param>
        private void NotifyDevices(IEnumerable<DataModel.Device> devices, IDeviceNotification evt,
            NotificationSettings settings, string reason)
        {
            try
            {
                RegisterWithPushServices(settings);

                // send push notifications
                foreach (var target in devices)
                    if (target.isEnabled == true && !string.IsNullOrEmpty(target.DeviceIdentity))
                    {
                        try
                        {
                            switch (target.NotificationTypeID)
                            {
                                case 0:
                                    break;

                                case 1:
#if APPLE_MSG
                                    PushToApple( target.NotificationID, evt);
#endif
                                    break;

                                case 2:
                                    PushToGoogle(target.NotificationID, evt, reason);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Write($"error sending notification: {ex}",
                                TraceEventType.Information, "Quest");
                        }
                    }
            }
            catch (Exception ex)
            {
                Logger.Write($"error sending notification: {ex}",
                    TraceEventType.Information, "Quest");
            }
            finally
            {
                //Thread.Sleep(1000);

                //StopPushServices(true);
                //Logger.Write(string.Format("finished closing notification channels"), TraceEventType.Information, "Quest");
            }
        }

        private void StopPushServices(bool wait)
        {
            try
            {
#if APPLE_MSG
                if (_apnsBroker != null)
                {
                    _apnsBroker.Stop(wait);
                    _apnsBroker = null;
                }
#endif
                if (_gcmBroker != null)
                {
                    _gcmBroker.Stop(wait);
                    _gcmBroker = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"Failed to stop push object: {ex}",
                    TraceEventType.Error, "Quest");
            }
        }

#if APPLE_MSG
        private void PushToApple(string deviceToken, IDeviceNotification notification)
        {
            var evt = notification as EventNotification;
            if (evt != null)
            {
                var sound = "default";
                Logger.Write(
                    $"Sending Apple notification: {evt.EventId} token {deviceToken} sound '{sound}'", TraceEventType.Information, "Quest");
                if (sound.Length == 0)
                    push.QueueNotification(new AppleNotification()
                        .ForDeviceToken(deviceToken)
                        .WithAlert("New event"));
                else
                    push.QueueNotification(new AppleNotification()
                        .ForDeviceToken(deviceToken)
                        .WithAlert("New event")
                        .WithSound(sound));

                return;
            }

            var status = notification as StatusNotification;
            if (status != null)
            {
                var sound = "default";
                //Logger.Write(string.Format("Sending Apple notification: {0} event callsign {1} token code {2} token callsign {3} sound '{4}'", evt.EventId, evt.Callsign, DeviceToken, evt.Callsign, sound), TraceEventType.Information, "Quest");
                if (sound.Length == 0)
                    push.QueueNotification(new AppleNotification()
                        .ForDeviceToken(deviceToken)
                        .WithAlert("Status now " + status.Status.Description));
                else
                    push.QueueNotification(new AppleNotification()
                        .ForDeviceToken(deviceToken)
                        .WithAlert("Status now " + status.Status.Description)
                        .WithSound(sound));
            }
        }
#endif

        private void PushToGoogle(string deviceToken, IDeviceNotification notification,
            string reason)
        {
            var evt = notification as EventNotification;

            var ticks = DateTime.UtcNow.Ticks;

            var unixtime = (ticks/1000L - 62135596800000L).ToString();

            var msg = $"Sending GCM notification: {notification} token {deviceToken}";
            Logger.Write(msg, TraceEventType.Information, "Quest");
            Debug.WriteLine(msg);


            if (evt != null)
            {
                //https://console.developers.google.com/project/865069987651/apiui/credential?authuser=0
                //OPS project AIzaSyC9WT1cTt4uQqfatIpSVxPYq6zvopjX1yo   for 86.29.75.151 & 194.223.243.235 (HEMS server)

#if false
                evt.PatientAge = "53";
                evt.Sex = "M";
                evt.LocationComment = "inside indian restaurant";
                evt.Notes = "patient colapsed w/ chest pains, difficulty breathing. previous inc. of chest pain known.";
#endif
                var data = new JObject
                {
                    {"Reason", reason},
                    {"Timestamp", unixtime},
                    {"ContentType", "EventNotification"},
                    {"Priority", evt.Priority},
                    {"AZGrid", evt.AZGrid},
                    {"CallOrigin", evt.CallOrigin ?? ""},
                    {"Determinant", evt.Determinant},
                    {"Dispatched", evt.Created ?? ""},
                    {"EventId", evt.EventId},
                    {"Latitude", evt.Latitude.ToString()},
                    {"Location", Crypto.Encrypt(evt.Location)},
                    {"LocationComment", Crypto.Encrypt(evt.LocationComment)},
                    {"Longitude", evt.Longitude.ToString()},
                    {"Notes", Crypto.Encrypt(evt.Notes)},
                    {"PatientAge", (evt.PatientAge ?? "").Trim()},
                    {"Sex", (evt.Sex ?? "").Trim()},
                    {"Updated", evt.Updated ?? ""}
                };

                _gcmBroker.QueueNotification(new GcmNotification { RegistrationIds = new List<string> {deviceToken}, Data = data });
                return;
            }

            var status = notification as StatusNotification;
            if (status != null)
            {
                //https://console.developers.google.com/project/865069987651/apiui/credential?authuser=0
                //OPS project AIzaSyC9WT1cTt4uQqfatIpSVxPYq6zvopjX1yo   for 86.29.75.151 & 194.223.243.235 (HEMS server)

                var data = new JObject
                {
                    {"Reason", reason},
                    {"Timestamp", unixtime},
                    {"ContentType", "StatusNotification"},
                    {"Code", status.Status.Code},
                    {"Description", status.Status.Description},
                };

                _gcmBroker.QueueNotification(new GcmNotification { RegistrationIds = new List<string> { deviceToken }, Data = data });

                return;
            }

            var csn = notification as CallsignNotification;
            if (csn != null)
            {
                var data = new JObject
                {
                    {"Reason", reason},
                    {"Timestamp", unixtime},
                    {"ContentType", "CallsignNotification"},
                    {"Callsign", csn.Callsign},
                };

                _gcmBroker.QueueNotification(new GcmNotification { RegistrationIds = new List<string> { deviceToken }, Data = data });
            }
        }

        private void RegisterWithPushServices(NotificationSettings settings)
        {
            try
            {
                Logger.Write("Registering with PUSH services", 
                    TraceEventType.Information, "Quest");

#if APPLE_MSG

                if (settings.AppleP12Certificate.Length > 0)
                {
                    //Registering the Apple Service and sending an iOS Notification
                    var appleCert = File.ReadAllBytes(settings.AppleP12Certificate);
                    var apnsConfig = new ApnsConfiguration(settings.AppleIsProduction? ApnsConfiguration.ApnsServerEnvironment.Production: ApnsConfiguration.ApnsServerEnvironment.Sandbox, appleCert, settings.AppleP12Password);
                    _apnsBroker = new ApnsServiceBroker(apnsConfig);
                    _apnsBroker.Start();
                }
#endif

                if (settings.GCMKey.Length > 0)
                {
                    var gcmConfig = new GcmConfiguration("GCM-SENDER-ID", settings.GCMKey, null);

                    // Create a new broker
                    _gcmBroker = new GcmServiceBroker(gcmConfig);
                    _gcmBroker.Start();
                }

                //push.OnChannelCreated += push_OnChannelCreated;
                //push.OnChannelDestroyed += push_OnChannelDestroyed;
                //push.OnChannelException += push_OnChannelException;
                //push.OnDeviceSubscriptionChanged += push_OnDeviceSubscriptionChanged;
                //push.OnDeviceSubscriptionExpired += push_OnDeviceSubscriptionExpired;
                //_gcmBroker.OnNotificationFailed += (notification, exception) => push_OnNotificationFailed(null,notification,exception);
                //push.OnNotificationRequeue += push_OnNotificationRequeue;
                //push.OnNotificationSent += push_OnNotificationSent;
                //push.OnServiceException += push_OnServiceException;


            }
            catch (Exception ex)
            {
                Logger.Write($"Apple registration failed: {ex}",
                    TraceEventType.Error, "Quest");
           }
        }

        //private void push_OnServiceException(object sender, Exception error)
        //{
        //    Logger.Write($"push_OnServiceException: {error}", LoggingPolicy.Category.Trace.ToString(),
        //        0, 0, TraceEventType.Error, "Quest");
        //    StopPushServices(true);
        //}

        //private void push_OnNotificationSent(object sender, INotification notification)
        //{
        //    Logger.Write(
        //        $"push_OnNotificationSent: {notification} IsValidDeviceRegistrationId {notification.IsValidDeviceRegistrationId()}", 
        //        TraceEventType.Error, "Quest");
        //}

        //private void push_OnNotificationRequeue(object sender, NotificationRequeueEventArgs e)
        //{
        //    Logger.Write($"push_OnNotificationRequeue: {e.Notification}",
        //        TraceEventType.Error, "Quest");
        //}

        //private void push_OnNotificationFailed(object sender, INotification notification, Exception error)
        //{
        //    Logger.Write($"push_OnNotificationFailed: {error} {notification}",
        //        TraceEventType.Error, "Quest");
        //    StopPushServices(true);
        //}

        //private void push_OnDeviceSubscriptionExpired(object sender, string expiredSubscriptionId,
        //    DateTime expirationDateUtc, INotification notification)
        //{
        //    Logger.Write(
        //        $"push_OnDeviceSubscriptionExpired: {expiredSubscriptionId} expired {expirationDateUtc}", TraceEventType.Error, "Quest");
        //    StopPushServices(true);
        //}

        //private void push_OnDeviceSubscriptionChanged(object sender, string oldSubscriptionId,
        //    string newSubscriptionId, INotification notification)
        //{
        //    Logger.Write(
        //        $"push_OnDeviceSubscriptionChanged: old {oldSubscriptionId} new {newSubscriptionId} notification {notification}", 
        //        TraceEventType.Error, "Quest");
        //}

        //private void push_OnChannelException(object sender, IPushChannel pushChannel, Exception error)
        //{
        //    Logger.Write($"push_OnChannelException: {error}", LoggingPolicy.Category.Trace.ToString(),
        //        0, 0, TraceEventType.Error, "Quest");
        //    StopPushServices(true);
        //}

        //private void push_OnChannelDestroyed(object sender)
        //{
        //    Logger.Write("push_OnChannelDestroyed", TraceEventType.Error,
        //        "Quest");
        //}

        //private void push_OnChannelCreated(object sender, IPushChannel pushChannel)
        //{
        //    Logger.Write($"push_OnChannelCreated: {pushChannel}",
        //        TraceEventType.Error, "Quest");
        //}

#endregion
    }
}