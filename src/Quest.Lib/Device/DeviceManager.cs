#define USE_ELASTIC
using Quest.Lib.Search.Elastic;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Quest.Lib.Processor;
using Quest.Lib.Trace;
using Quest.Common.ServiceBus;
using Quest.Lib.Notifier;
using Quest.Lib.Incident;
using Quest.Lib.Resource;

namespace Quest.Lib.Device
{
    public class DeviceManager : ServiceBusProcessor
    {
#if USE_ELASTIC
        private ElasticSettings _elastic;
        private BuildIndexSettings _config;
        private DeviceHandler _deviceHandler;
        private IDeviceStore _devStore;
        private IResourceStore _resStore;
        private IIncidentStore _incStore;
#endif
        private NotificationSettings _notificationSettings = new NotificationSettings();

        public DeviceManager(
            IDeviceStore devStore,
            IResourceStore resStore,
            IIncidentStore incStore,
            DeviceHandler deviceHandler,
            ElasticSettings elastic,
            NotificationSettings notificationSettings,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _devStore = devStore;
            _resStore = resStore;
            _incStore = incStore;
            _elastic = elastic;
            _deviceHandler = deviceHandler;
            _notificationSettings = notificationSettings;
        }

        protected override void OnPrepare()
        {
            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler<AssignDeviceRequest>(AssignDeviceHandler);
            MsgHandler.AddHandler<LoginRequest>(LoginRequestHandler);
            MsgHandler.AddHandler<LogoutRequest>(LogoutRequestHandler);
            MsgHandler.AddHandler<AckAssignedEventRequest>(AckAssignedEventRequestHandler);
            MsgHandler.AddHandler<CallsignChangeRequest>(CallsignChangeRequestHandler);
            MsgHandler.AddHandler<RefreshStateRequest>(RefreshStateHandler);
            MsgHandler.AddHandler<GetEntityTypesRequest>(GetEntityTypesRequestHandler);
            MsgHandler.AddHandler<GetHistoryRequest>(GetHistoryRequestHandler);
            MsgHandler.AddHandler<GetStatusCodesRequest>(GetStatusCodesRequestHandler);
            MsgHandler.AddHandler<MakePatientObservationRequest>(MakePatientObservationRequestHandler);
            MsgHandler.AddHandler<MapItemsRequest>(GetMapItemsRequestHandler);
            MsgHandler.AddHandler<PatientDetailsRequest>(PatientDetailsRequestHandler);
            MsgHandler.AddHandler<PositionUpdateRequest>(PositionUpdateRequestHandler);
            MsgHandler.AddHandler<SetStatusRequest>(SetStatusRequestHandler);
        }

        protected override void OnStart()
        {
            Initialise();
            Logger.Write("DeviceManager initialised","Device");
        }
       

        /// <summary>
        ///     "Quest.ResourceTracker"
        /// </summary>
        private void Initialise()
        {
            var syns = IndexBuilder.LoadSynsFromFile(_elastic.SynonymsFile);
            _config = new BuildIndexSettings(_elastic, _elastic.DefaultIndex, null);
            _config.RestrictToMaster = false;
        }

        private Response LoginRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as LoginRequest;
            if (request != null)
            {
                return _deviceHandler.Login(request,_resStore,_devStore);
            }
            return null;
        }

        private Response LogoutRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as LogoutRequest;
            if (request != null)
            {
                return _deviceHandler.Logout(request,_devStore);
            }
            return null;
        }

        private Response AckAssignedEventRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as AckAssignedEventRequest;
            if (request != null)
            {
                return _deviceHandler.AckAssignedEvent(request);
            }
            return null;
        }

        private Response CallsignChangeRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as CallsignChangeRequest;
            if (request != null)
            {
                return _deviceHandler.CallsignChange(request);
            }
            return null;
        }

        private Response RefreshStateHandler(NewMessageArgs t)
        {
            var request = t.Payload as RefreshStateRequest;
            if (request != null)
            {
                return _deviceHandler.RefreshState(request, _notificationSettings,_incStore);
            }
            return null;
        }

        private Response GetEntityTypesRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as GetEntityTypesRequest;
            if (request != null)
            {
                return _deviceHandler.GetEntityTypes(request);
            }
            return null;
        }

        private Response GetHistoryRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as GetHistoryRequest;
            if (request != null)
            {
                return _deviceHandler.GetHistory(request);
            }
            return null;
        }

        private Response GetStatusCodesRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as GetStatusCodesRequest;
            if (request != null)
            {
                return _deviceHandler.GetStatusCodes(request);
            }
            return null;
        }

        private Response MakePatientObservationRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as MakePatientObservationRequest;
            if (request != null)
            {
                return _deviceHandler.MakePatientObservation(request);
            }
            return null;
        }

        private Response GetMapItemsRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as MapItemsRequest;
            if (request != null)
            {
                return _deviceHandler.GetMapItems(request);
            }
            return null;
        }

        private Response PatientDetailsRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as PatientDetailsRequest;
            if (request != null)
            {
                return _deviceHandler.PatientDetails(request);
            }
            return null;
        }

        private Response PositionUpdateRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as PositionUpdateRequest;
            if (request != null)
            {
                return _deviceHandler.PositionUpdate(request);
            }
            return null;
        }

        private Response SetStatusRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as SetStatusRequest;
            if (request != null)
            {
                return _deviceHandler.SetStatusRequest(request, _notificationSettings);
            }
            return null;
        }

        private Response AssignDeviceHandler(NewMessageArgs t)
        {
            var request = t.Payload as AssignDeviceRequest;
            if (request != null)
            {
                return _deviceHandler.AssignDevice(request, _notificationSettings,_incStore);
            }
            return null;
        }

        /// <summary>
        ///     Detects resource changing to DSP (Dispatched) and sends incident details to callsign
        ///     Detects resource changing to status and sends status update to callsign
        /// </summary>
        /// <param name="t"></param>
      


    }
}