#define NO_APPLE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Utils;
using Quest.Lib.Incident;
using Quest.Lib.Resource;
using Quest.Lib.Data;
using Quest.Common.ServiceBus;
using Quest.Lib.Search.Elastic;
using Quest.Common.Utils;
using Quest.Common.Messages.Device;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.CAD;
using Quest.Common.Messages.Resource;

namespace Quest.Lib.Device
{
    public class DeviceHandler
    {
        private ElasticSettings _elastic;
        private BuildIndexSettings _config;
        private IDeviceStore _devStore;
        private IResourceStore _resStore;
        private IIncidentStore _incStore;
        private ResourceHandler _resHandler;
        private IDatabaseFactory _dbFactory;

        public string deletedStatus { get; set; } = "";

        public string triggerStatus { get; set; } = "";

        /// <summary>
        /// maximum delta between current time and message time
        /// </summary>
        public long MaxMessageTimeDelta { get; set; } = 60*60;

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

    public DeviceHandler(IDatabaseFactory dbFactory, ElasticSettings elastic,
            IDeviceStore devStore,
            IResourceStore resStore,
            ResourceHandler resHandler,
            IIncidentStore incStore)
        {
            _resHandler = resHandler;
            _devStore = devStore;
            _resStore = resStore;
            _incStore = incStore;
            _elastic = elastic;
            _dbFactory = dbFactory;

            var syns = IndexBuilder.LoadSynsFromFile(_elastic.SynonymsFile);
            _config = new BuildIndexSettings(_elastic, _elastic.DefaultIndex, null);
            _config.RestrictToMaster = false;
        }

        /// <summary>
        ///     Class uses by the Quest server to process messages arriving from devices. In most cases the
        ///     device manager processes a xRequest and returns an xResponse.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public LoginResponse Login(LoginRequest request, IServiceBusClient msgSource)
        {
            // check the timestamp
            var failedTimecheck = ValidateTime<LoginResponse>(request);
            if (failedTimecheck != null)
                return failedTimecheck;
            var timestamp = Time.UnixTime(request.Timestamp);

            // get the device record
            var devrecord = _devStore.Get(request.DeviceIdentity);

            if (devrecord != null)
            {
                // update core details
                devrecord.OwnerId = request.Username;
                devrecord.LoggedOnTime = DateTime.UtcNow;
                devrecord.NotificationTypeId = request.NotificationTypeId;
                devrecord.NotificationId = request.NotificationId;
                devrecord.DeviceIdentity = request.DeviceIdentity;
                devrecord.Osversion = request.OSVersion;
                devrecord.DeviceMake = request.DeviceMake;
                devrecord.DeviceModel = request.DeviceModel;
                devrecord.FleetNo = request.FleetNo;
            }
            else
            {
                var offRoadStatus = _resStore.GetOffroadStatusId();
                    
                // new record
                devrecord = new QuestDevice
                {
                    OwnerId = request.Username,
                    DeviceIdentity = request.DeviceIdentity,
                    LoggedOnTime = DateTime.UtcNow,
                    DeviceRoleId = 3, //TODO: This is the default role that the new login will play. This should come from a setting 
                    NotificationTypeId = request.NotificationTypeId,
                    NotificationId = request.NotificationId,
                    IsEnabled = true,
                    Osversion = request.OSVersion,
                    DeviceMake = request.DeviceMake,
                    DeviceModel = request.DeviceModel,
                    FleetNo = request.FleetNo
            };
            }

            // make a new token. this becomes a claim (jti unique identitier
            devrecord.AuthToken = Guid.NewGuid().ToString();

            _devStore.Update(devrecord, timestamp);

            QuestResource resource = null;

            // try and use the provided fleetnumber to associate with a resource
            if (!string.IsNullOrEmpty(request.FleetNo))
            {
                // update associated resource record
                ResourceUpdate newresource = new ResourceUpdate
                {
                    Resource = new QuestResource
                    {
                        FleetNo = request.FleetNo,
                    },
                    UpdateTime = DateTime.UtcNow
                };

                var updateResult = _resHandler.ResourceUpdate(newresource, msgSource, _config);

                resource = updateResult.NewResource;
            }

            return new LoginResponse
            {
                QuestApi = Version,
                RequestId = request.RequestId,
                RequiresCallsign = false,
                Success = true,
                SessionId = devrecord.AuthToken,
                Resource = resource,
                Message = "Successfully logged on"
            };
        }

        /// <summary>
        /// Log out of Quest
        /// </summary>
        /// <param name="request"></param>
        /// <param name="devStore"></param>
        /// <returns></returns>
        public LogoutResponse Logout(LogoutRequest request)
        {
            // check the timestamp
            var failedTimecheck = ValidateTime<LogoutResponse>(request);
            if (failedTimecheck != null)
                return failedTimecheck;
            var timestamp = Time.UnixTime(request.Timestamp);

            var resrecord = _devStore.GetByToken(request.SessionId);

            if (resrecord != null)
            {
                resrecord.LoggedOffTime = DateTime.UtcNow;
                resrecord.AuthToken = null;
                resrecord.NotificationId = "";
                resrecord.NotificationTypeId = "";

                _devStore.Update(resrecord, timestamp);

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

        public GetStatusCodesResponse GetStatusCodes(GetStatusCodesRequest request)
        {
            // check the timestamp
            var failedTimecheck = ValidateTime<GetStatusCodesResponse>(request);
            if (failedTimecheck != null)
                return failedTimecheck;
            var timestamp = Time.UnixTime(request.Timestamp);

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
            // check the timestamp
            var failedTimecheck = ValidateTime<SetStatusResponse>(request);
            if (failedTimecheck != null)
                return failedTimecheck;
            var timestamp = Time.UnixTime(request.Timestamp);

            QuestDevice deviceRecord = _devStore.GetByToken(request.SessionId);

            if (deviceRecord == null)
                return new SetStatusResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = "unknown device"
                };

            //TODO: make changes here..

            var updated = _devStore.Update(deviceRecord, timestamp);

            //TODO: Save to Elastic
            return new SetStatusResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "successful"
            };

        }

        /// <summary>
        /// Get the status of the device. can be used at startup of the device so it has the right details.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public RefreshStateResponse RefreshState(RefreshStateRequest request, IServiceBusClient serviceBusClient)
        {
            // check the timestamp
            var failedTimecheck = ValidateTime<RefreshStateResponse>(request);
            if (failedTimecheck != null)
                return failedTimecheck;
            var timestamp = Time.UnixTime(request.Timestamp);

            return new RefreshStateResponse() {  Message="Not Implemented"};
        }

        public AckAssignedEventResponse AckAssignedEvent(AckAssignedEventRequest request)
        {
            // check the timestamp
            var failedTimecheck = ValidateTime<AckAssignedEventResponse>(request);
            if (failedTimecheck != null)
                return failedTimecheck;
            var timestamp = Time.UnixTime(request.Timestamp);

            var devrecord = _devStore.GetByToken(request.SessionId);

            if (devrecord == null)
            {
                return new AckAssignedEventResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = "unknown device"
                };
            }

            if ( string.IsNullOrEmpty(request.EventId))
            {
                return new AckAssignedEventResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = "Empty event id sent"
                };
            }

            var resource = _resStore.GetByFleetNo(devrecord.FleetNo);
            if (resource==null)
            {
                return new AckAssignedEventResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = "device not linked to a resource"
                };
            }

            if (resource.EventId != request.EventId)
            {
                return new AckAssignedEventResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"Current event for your device is {resource.EventId}, not {request.EventId}"
                };
            }

            // action here to say the device has confirmed the assignment,
            return new AckAssignedEventResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "successful"
            };
        }

        public PositionUpdateResponse PositionUpdate(PositionUpdateRequest request, IServiceBusClient msgSource)
        {
            // check the timestamp
            var failedTimecheck = ValidateTime<PositionUpdateResponse>(request);
            if (failedTimecheck != null)
                return failedTimecheck;
            var timestamp = Time.UnixTime(request.Timestamp);

            QuestDevice deviceRecord = _devStore.GetByToken(request.SessionId);

            if (deviceRecord == null)
                return new PositionUpdateResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = "unknown device"
                };

            deviceRecord.Latitude = (float?)request.Vector.Latitude ?? 0;
            deviceRecord.Longitude = (float?)request.Vector.Longitude ?? 0;
            deviceRecord.HDoP = (float)request.Vector.HDoP;
            deviceRecord.Speed = (float)request.Vector.Speed;
            deviceRecord.Course = (float)request.Vector.Course;

            var updated = _devStore.Update(deviceRecord, timestamp);

            // Primary device is used to update the position  of the Resource record
            if (deviceRecord.IsPrimary==true)
            {
                // try and use the provided fleetnumber to associate with a resource
                if (!string.IsNullOrEmpty(deviceRecord.FleetNo))
                {
                    QuestResource resource = new QuestResource
                    {
                        FleetNo = deviceRecord.FleetNo,
                        Position = new GeoAPI.Geometries.Coordinate(request.Vector.Longitude, request.Vector.Latitude),
                        HDoP = (float)request.Vector.HDoP,
                        Speed = (float)request.Vector.Speed,
                        Course = (float)request.Vector.Course
                    };

                    ResourceUpdate resupdate = new ResourceUpdate
                    {
                        Resource = resource,
                        UpdateTime = DateTime.UtcNow
                    };

                    var updateResult = _resHandler.ResourceUpdate(resupdate, msgSource, _config);
                }
            }

            //TODO: Save to Elastic
            return new PositionUpdateResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "successful"
            };
        }

        public MakePatientObservationResponse MakePatientObservation(MakePatientObservationRequest request)
        {
            // check the timestamp
            var failedTimecheck = ValidateTime<MakePatientObservationResponse>(request);
            if (failedTimecheck != null)
                return failedTimecheck;
            var timestamp = Time.UnixTime(request.Timestamp);

            QuestDevice deviceRecord = _devStore.GetByToken(request.SessionId);

            if (deviceRecord == null)
                return new MakePatientObservationResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = "unknown device"
                };

            //TODO: make changes here..

            var updated = _devStore.Update(deviceRecord, timestamp);

            //TODO: Save to Elastic
            return new MakePatientObservationResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "successful"
            };
        }

        public PatientDetailsResponse PatientDetails(PatientDetailsRequest request)
        {
            // check the timestamp
            var failedTimecheck = ValidateTime<PatientDetailsResponse>(request);
            if (failedTimecheck != null)
                return failedTimecheck;
            var timestamp = Time.UnixTime(request.Timestamp);

            return new PatientDetailsResponse
            {
                RequestId = request.RequestId,
                Success = false,
                Message = "Not Implemented"
            };
        }

        public GetEntityTypesResponse GetEntityTypes(GetEntityTypesRequest request)
        {
            // check the timestamp
            var failedTimecheck = ValidateTime<GetEntityTypesResponse>(request);
            if (failedTimecheck != null)
                return failedTimecheck;
            var timestamp = Time.UnixTime(request.Timestamp);

            QuestDevice deviceRecord = _devStore.GetByToken(request.SessionId);

            if (deviceRecord == null)
                return new GetEntityTypesResponse
                {
                    RequestId = request.RequestId,
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

        public MapItemsResponse GetMapItems(MapItemsRequest request)
        {
            const int maxres = 20;
            const int maxdev = 20;
            const int maxinc = 20;

            // check the timestamp
            var failedTimecheck = ValidateTime<MapItemsResponse>(request);
            if (failedTimecheck != null)
                return failedTimecheck;
            var timestamp = Time.UnixTime(request.Timestamp);


            var response = new MapItemsResponse
            {
                RequestId = request.RequestId,
                Resources = new List<ResourceItem>(),
                Destinations = new List<QuestDestination>(),
                Events = new List<EventMapItem>(),
                //Devices = new List<ResourceItem>(),
                Success = true,
                Message = "successful"
            };

            _dbFactory.Execute<QuestContext>((db) =>
            {
                // make a note of the current revision
                var resmax = db.Resource.Max(x => x.Revision);

                response.CurrRevision = (long)(resmax ?? 0) + 1;
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
                //response.Devices.AddRange(GetDeviceResources(request.Revision, request.ResourcesAvailable,
                //    request.ResourcesBusy));

                // work out which have been deleted - these are in not in new_resource but are in original_resources                

                // truncate for efficiency                
                response.Resources.Sort((x, y) => (int)(x.revision - y.revision));
                response.Resources = response.Resources.Take(maxres).ToList();

                //response.Devices.Sort((x, y) => (int) (x.revision - y.revision));
                //response.Devices = response.Devices.Take(maxdev).ToList();
            }

            if (request.IncidentsImmediate || request.IncidentsOther)
            {
                response.Events.AddRange(GetIncidents(request.Revision, request.IncidentsImmediate,
                    request.IncidentsOther));
                response.Events.Sort((x, y) => (int)(x.revision - y.revision));
                response.Events = response.Events.Take(maxinc).ToList();
            }

            // calculate the maximum revision being returned.
            long c1 = 0, c2 = 0, c3 = 0;
            if (response.Resources.Count > 0)
                c1 = response.Resources.Max(x => x.revision);


            if (response.Events.Count > 0)
                c2 = response.Events.Max(x => x.revision);

            //if (response.Devices.Count > 0)
            //    c3 = response.Devices.Max(x => x.revision);

            response.Revision = Math.Max(Math.Max(c1, c2), c3);

            if (response.Revision == 0)
                response.Revision = response.CurrRevision;

            //Debug.Print($"Map rev in={request.Revision} rev out={response.Revision} rev cur = {response.CurrRevision} count inc={response.Events.Count} res={response.Resources.Count} dev={response.Devices.Count} deleted={response.DeleteResources.Count}");
            Debug.Print($"Map rev in={request.Revision} rev out={response.Revision} rev cur = {response.CurrRevision} count inc={response.Events.Count} res={response.Resources.Count}");

            return response;
        }

        public GetHistoryResponse GetHistory(GetHistoryRequest request)
        {
            // check the timestamp
            var failedTimecheck = ValidateTime<GetHistoryResponse>(request);
            if (failedTimecheck != null)
                return failedTimecheck;
            var timestamp = Time.UnixTime(request.Timestamp);

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

        private List<EventMapItem> GetIncidents(long revision, bool includeCatA = false, bool includeCatB = false)
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

        private IEnumerable<DataModel.Resource> ResourceAtRevision(QuestContext db, long revision)
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

        //private List<ResourceItem> GetDeviceResources(long revision, bool avail = false, bool busy = false)
        //{
        //    return _dbFactory.Execute<QuestContext, List<ResourceItem>>((db) =>
        //    {
        //        var features = new List<ResourceItem>();

        //        if (avail)
        //            features.AddRange(

        //                    db.Devices
        //                    .Where(
        //                        x =>
        //                            x.ResourceStatus.Available == true &&
        //                            x.Latitude != null && x.Longitude != null)
        //                    .Where(x => x.Revision > revision)
        //                    .ToList()
        //                    .Select(
        //                        res => new ResourceItem
        //                        {
        //                            ID = res.DeviceId.ToString(),
        //                            revision = res.Revision ?? 0,
        //                            X = res.Longitude ?? 0,
        //                            Y = res.Latitude ?? 0,
        //                            Callsign = res.DeviceCallsign ?? $"DV{res.DeviceId}",
        //                            lastUpdate = res.LastUpdate,
        //                            StatusCategory = GetStatusDescription(res.ResourceStatus.Available ?? false, res.ResourceStatus.Busy ?? false, res.ResourceStatus.BusyEnroute ?? false, res.ResourceStatus.Rest ?? false),
        //                            Status = res.ResourceStatus.Status,
        //                            PrevStatus = res.PrevStatus,
        //                            VehicleType = "Device",
        //                            Destination = res.Destination,
        //                            Eta = null,
        //                            FleetNo = "",
        //                            Road = res.Road,
        //                            Comment = "",
        //                            Skill = res.Skill,
        //                            Speed = res.Speed,
        //                            Direction = res.Direction,
        //                            Incident = res.Event,
        //                            Available = res.ResourceStatus.Available ?? false,
        //                            Busy = res.ResourceStatus.Busy ?? false,
        //                            BusyEnroute = res.ResourceStatus.BusyEnroute ?? false,
        //                            ResourceTypeGroup = "HAND"
        //                        }
        //                    )

        //                );

        //        if (busy)
        //            features.AddRange(
        //                db.Devices
        //                    .Where(
        //                        x =>
        //                            x.ResourceStatus.Busy == true &&
        //                            x.Latitude != null && x.Longitude != null)
        //                    .Where(x => x.Revision > revision)
        //                    .ToList()
        //                    .Select(
        //                               res => new ResourceItem
        //                               {
        //                                   ID = res.DeviceId.ToString(),
        //                                   revision = res.Revision ?? 0,
        //                                   X = res.Longitude ?? 0,
        //                                   Y = res.Latitude ?? 0,
        //                                   Callsign = res.DeviceCallsign ?? $"DV{res.DeviceId}",
        //                                   lastUpdate = res.LastUpdate,
        //                                   StatusCategory = GetStatusDescription(res.ResourceStatus.Available ?? false, res.ResourceStatus.Busy ?? false, res.ResourceStatus.BusyEnroute ?? false, res.ResourceStatus.Rest ?? false),
        //                                   Status = res.ResourceStatus.Status,
        //                                   PrevStatus = res.PrevStatus,
        //                                   VehicleType = "Device",
        //                                   Destination = res.Destination,
        //                                   Eta = null,
        //                                   FleetNo = "",
        //                                   Road = res.Road,
        //                                   Comment = "",
        //                                   Skill = res.Skill,
        //                                   Speed = res.Speed,
        //                                   Direction = res.Direction,
        //                                   Incident = res.Event,
        //                                   Available = res.ResourceStatus.Available ?? false,
        //                                   Busy = res.ResourceStatus.Busy ?? false,
        //                                   BusyEnroute = res.ResourceStatus.BusyEnroute ?? false,
        //                                   ResourceTypeGroup = "HAND"
        //                               }

        //                    )
        //                );

        //        var lastCallsign = "";
        //        var callsignCount = 0;
        //        foreach (var res in features.OrderBy(x => x.Callsign))
        //        {
        //            var latlng = LatLongConverter.OSRefToWGS84(res.X, res.Y);
        //            res.X = latlng.Longitude;
        //            res.Y = latlng.Latitude;

        //            if (lastCallsign == res.Callsign)
        //            {
        //                callsignCount++;
        //                res.Callsign += "/" + callsignCount;
        //            }
        //            else
        //                callsignCount = 0;
        //        }

        //        return features;
        //    });
        //}

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

        private void SendAlertMessage(string callsign, string message)
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
                Resource = new QuestResource
                {
                    Callsign = res.Callsign.Callsign1,
                    StatusCategory = GetStatusDescription(res),
                    Status = res.ResourceStatus.Status,
                    ResourceType = res.ResourceType.ResourceType1 ?? "VEH",
                    Destination = res.Destination,
                    Eta = res.Eta,
                    FleetNo = res.FleetNo,
                    Comment = res.Comment,
                    Skill = res.Skill,
                    Speed = res.Speed,
                    Course = res.Course,
                    EventId = res.EventId,
                    ResourceTypeGroup = res.ResourceType.ResourceTypeGroup,
                }
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

        private T ValidateTime<T>(Request request) where T : Response, new()
        {
            var current = Time.CurrentUnixTime();
            if (request.Timestamp == 0)
                return new T
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"You need to set the Timestamp in the request to the current Unix Epoch Time. The current Unix Epoch Time is {current}"
                };

            if (Math.Abs(request.Timestamp - current) > MaxMessageTimeDelta)
                return new T
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"The message timestamp is outside limits. The current Unix Epoch Time is {current} and the message was {request.Timestamp}. The limit is {MaxMessageTimeDelta} difference between the two."
                };

            return null;
        }

    }
}