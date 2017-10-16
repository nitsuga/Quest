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
        private DeviceHandler _deviceHandler;
        
#endif
        public DeviceManager(

            DeviceHandler deviceHandler,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _deviceHandler = deviceHandler;
        }

        protected override void OnPrepare()
        {
            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler<LoginRequest>(LoginRequestHandler);
            MsgHandler.AddHandler<LogoutRequest>(LogoutRequestHandler);
            MsgHandler.AddHandler<AckAssignedEventRequest>(AckAssignedEventRequestHandler);
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
        }

        private Response LoginRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as LoginRequest;
            if (request != null)
            {
                return _deviceHandler.Login(request,ServiceBusClient);
            }
            return null;
        }

        private Response LogoutRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as LogoutRequest;
            if (request != null)
            {
                return _deviceHandler.Logout(request);
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

        private Response RefreshStateHandler(NewMessageArgs t)
        {
            var request = t.Payload as RefreshStateRequest;
            if (request != null)
            {
                return _deviceHandler.RefreshState(request, ServiceBusClient);
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
                return _deviceHandler.PositionUpdate(request, ServiceBusClient);
            }
            return null;
        }

        private Response SetStatusRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as SetStatusRequest;
            if (request != null)
            {
                return _deviceHandler.SetStatusRequest(request, ServiceBusClient);
            }
            return null;
        }

    }
}