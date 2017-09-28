using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Quest.Lib.ServiceBus;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using Quest.Api.Options;
using Quest.Common.Messages;
using Quest.Api.Extensions;

namespace Quest.Api.Controllers
{
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        AsyncMessageCache _messageCache;

        public NotificationController(AsyncMessageCache messageCache)
        {
            _messageCache = messageCache;
        }

        [HttpPost("Logon")]
        public NotificationResponse Notify([FromForm] INotificationMessage message, [FromQuery] string method, [FromQuery]string address, [FromQuery]string subject)
        {
            NotificationResponse result;
            try
            {
                Notification n = new Notification { Address = address, Body = message, Method = method, Subject = subject };
                result = n.Submit<NotificationResponse>(_messageCache);
            }
            catch (Exception ex)
            {
                return new NotificationResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
            return result;
        }

    }
}
