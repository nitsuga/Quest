using System;
using Microsoft.AspNetCore.Mvc;
using Quest.Lib.ServiceBus;
using System.Threading.Tasks;
using Quest.Common.Messages.Device;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Entities;

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

        [HttpPost("GetEntities")]
        public async Task<GetEntitiesResponse> GetEntityTypes([FromBody]GetEntitiesRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<GetEntitiesResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new GetEntitiesResponse() { Success = false, Message = ex.Message };

            }
        }

        [HttpPost("GetEntity")]
        public async Task<GetEntityResponse> GetEntityTypes([FromBody]GetEntityRequest request)
        {
            try
            {
                return await _messageCache.SendAndWaitAsync<GetEntityResponse>(request, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new GetEntityResponse() { Success = false, Message = ex.Message };

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

    }
}
