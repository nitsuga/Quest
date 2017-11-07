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
using Quest.Common.Messages.Destination;
using Quest.Common.Messages.Incident;

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
                ResourceUpdateRequest newresource = new ResourceUpdateRequest
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
            var resrecord = _devStore.GetByToken(request.SessionId);
            if (resrecord == null)
                return new GetStatusCodesResponse
                    {
                        RequestId = request.RequestId,
                        Items = new List<StatusCode>(),
                        Success = false,
                        Message = "unknown device"
                    };

            //TODO: get from resource handler
            List<StatusCode> results = new List<StatusCode>();

            return new GetStatusCodesResponse
            {
                RequestId = request.RequestId,
                Items = results,
                Success = true,
                Message = "successful"
            };
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

            deviceRecord.Latitude = (float?)request.Vector.Coord.Latitude ?? 0;
            deviceRecord.Longitude = (float?)request.Vector.Coord.Longitude ?? 0;
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
                        Position = new LatLongCoord(request.Vector.Coord.Longitude, request.Vector.Coord.Latitude),
                        HDoP = (float)request.Vector.HDoP,
                        Speed = (float)request.Vector.Speed,
                        Course = (float)request.Vector.Course
                    };

                    ResourceUpdateRequest resupdate = new ResourceUpdateRequest
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

        public GetHistoryResponse GetHistory(GetHistoryRequest request)
        {
            return null;
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