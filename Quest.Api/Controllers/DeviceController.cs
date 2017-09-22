using System;
using Microsoft.AspNetCore.Mvc;
using Quest.Common.Messages;
using Quest.Lib.ServiceBus;
using Quest.Api.Extensions;

namespace Quest.Api.Controllers
{
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        AsyncMessageCache _messageCache;

        public DeviceController(AsyncMessageCache messageCache)
        {
            _messageCache = messageCache;
        }

        [HttpPost("Logout")]
        public LogoutResponse Logout([FromBody]LogoutRequest request)
        {
            LogoutResponse result;
            try
            {
                result = request.Submit<LogoutResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new LogoutResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
            return result;
        }

        [HttpPost("CallsignChange")]
        public CallsignChangeResponse CallsignChange([FromBody]CallsignChangeRequest request)
        {
            CallsignChangeResponse result;
            try
            {
                result = request.Submit<CallsignChangeResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new CallsignChangeResponse() { Success = false, Message = ex.Message };

            }
            return result;
        }

        [HttpPost("RefreshState")]
        public RefreshStateResponse RefreshState([FromBody]RefreshStateRequest request)
        {
            RefreshStateResponse result;
            try
            {
                result = request.Submit<RefreshStateResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new RefreshStateResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
            return result;
        }

        [HttpPost("AckAssignedEvent")]
        public AckAssignedEventResponse AckAssignedEvent([FromBody]AckAssignedEventRequest request)
        {
            AckAssignedEventResponse result;
            try
            {
                result = request.Submit<AckAssignedEventResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new AckAssignedEventResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
            return result;
        }

        [HttpPost("PositionUpdate")]
        public PositionUpdateResponse PositionUpdate([FromBody]PositionUpdateRequest request)
        {
            PositionUpdateResponse result;
            try
            {
                result = request.Submit<PositionUpdateResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new PositionUpdateResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
            return result;
        }

        [HttpPost("MakePatientObservation")]
        public MakePatientObservationResponse MakePatientObservation([FromBody]MakePatientObservationRequest request)
        {
            MakePatientObservationResponse result;
            try
            {
                result = request.Submit<MakePatientObservationResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new MakePatientObservationResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
            return result;
        }

        [HttpPost("PatientDetails")]
        public PatientDetailsResponse PatientDetails([FromBody]PatientDetailsRequest request)
        {
            PatientDetailsResponse result;
            try
            {
                result = request.Submit<PatientDetailsResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new PatientDetailsResponse() { Success = false, Message = ex.Message };

            }
            return result;
        }

        [HttpPost("SetStatus")]
        public SetStatusResponse SetStatus([FromBody]SetStatusRequest request)
        {
            SetStatusResponse result;
            try
            {
                result = request.Submit<SetStatusResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new SetStatusResponse() { Success = false, Message = ex.Message };
            }
            return result;
        }

        [HttpPost("GetStatusCodes")]
        public GetStatusCodesResponse GetStatusCodes([FromBody]GetStatusCodesRequest request)
        {
            GetStatusCodesResponse result;
            try
            {
                result = request.Submit<GetStatusCodesResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new GetStatusCodesResponse() { Success = false, Message = ex.Message };
            }
            return result;
        }

        [HttpPost("GetEntityTypes")]
        public GetEntityTypesResponse GetEntityTypes([FromBody]GetEntityTypesRequest request)
        {
            GetEntityTypesResponse result;
            try
            {
                result = request.Submit<GetEntityTypesResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new GetEntityTypesResponse() { Success = false, Message = ex.Message };

            }
            return result;
        }

        [HttpPost("GetMapItems")]
        [HttpPost]
        public MapItemsResponse GetMapItems([FromBody]MapItemsRequest request)
        {
            MapItemsResponse result;
            try
            {
                result = request.Submit<MapItemsResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new MapItemsResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
            return result;
        }

        [HttpPost("GetHistory")]
        public GetHistoryResponse GetHistory([FromBody]GetHistoryRequest request)
        {
            GetHistoryResponse result;
            try
            {
                result = request.Submit<GetHistoryResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new GetHistoryResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
            return result;
        }
    }
}
