﻿using System;
using Microsoft.AspNetCore.Mvc;
using Quest.Common.Messages;
using Quest.Lib.ServiceBus;
using System.Threading.Tasks;
using Quest.Common.Messages.Device;
using Quest.Common.Messages.GIS;

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

        /// <summary>
        /// Logon to the device manager in order to receive alerts and perform device queries
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("Logon")]
        public async Task<LoginResponse> Logon([FromBody] LoginRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<LoginResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new LoginResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
        }

        /// <summary>
        /// Log out of Quest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("Logout")]
        public async Task<LogoutResponse> Logout([FromBody]LogoutRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<LogoutResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new LogoutResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
        }

        /// <summary>
        /// Request a callsign change. If the device is being managed by Quest then
        /// the request is always granted. If the device is being managed by a CAD then
        /// the request is forwarded on to the CAD for processing.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("CallsignChange")]
        public async Task<CallsignChangeResponse> CallsignChange([FromBody]CallsignChangeRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<CallsignChangeResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new CallsignChangeResponse() { Success = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// Get the status of the device. can be used at startup of the device so it has the right details.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("RefreshState")]
        public async Task<RefreshStateResponse> RefreshState([FromBody]RefreshStateRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<RefreshStateResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new RefreshStateResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
        }

        /// <summary>
        /// Acknowledge event assignment
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("AckAssignedEvent")]
        public async Task<AckAssignedEventResponse> AckAssignedEvent([FromBody]AckAssignedEventRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<AckAssignedEventResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new AckAssignedEventResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
        }

        /// <summary>
        /// send location update to the CAD/Quest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("PositionUpdate")]
        public async Task<PositionUpdateResponse> PositionUpdate([FromBody]PositionUpdateRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<PositionUpdateResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new PositionUpdateResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
        }

        /// <summary>
        /// Make a paient observation
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("MakePatientObservation")]
        public async Task<MakePatientObservationResponse> MakePatientObservation([FromBody]MakePatientObservationRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<MakePatientObservationResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new MakePatientObservationResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
        }

        /// <summary>
        /// Request patient details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("PatientDetails")]
        public async Task<PatientDetailsResponse> PatientDetails([FromBody]PatientDetailsRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<PatientDetailsResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new PatientDetailsResponse() { Success = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// Request a status change
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SetStatus")]
        public async Task<SetStatusResponse> SetStatus([FromBody]SetStatusRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<SetStatusResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new SetStatusResponse() { Success = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// get a list of valid status codes
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetStatusCodes")]
        public async Task<GetStatusCodesResponse> GetStatusCodes([FromBody]GetStatusCodesRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<GetStatusCodesResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new GetStatusCodesResponse() { Success = false, Message = ex.Message };
            }
        }

        [HttpPost("GetEntityTypes")]
        public async Task<GetEntityTypesResponse> GetEntityTypes([FromBody]GetEntityTypesRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<GetEntityTypesResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new GetEntityTypesResponse() { Success = false, Message = ex.Message };

            }
        }

        [HttpPost("GetMapItems")]
        [HttpPost]
        public async Task<MapItemsResponse> GetMapItems([FromBody]MapItemsRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<MapItemsResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new MapItemsResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
        }

        [HttpPost("GetHistory")]
        public async Task<GetHistoryResponse> GetHistory([FromBody]GetHistoryRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<GetHistoryResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new GetHistoryResponse() { Success = false, Message = ex.Message };

            }
            finally
            {
            }
        }
    }
}
