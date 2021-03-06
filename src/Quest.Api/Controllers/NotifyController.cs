﻿using System;
using Microsoft.AspNetCore.Mvc;
using Quest.Lib.ServiceBus;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Quest.Common.Messages.Device;
using Quest.Common.Messages.Notification;

namespace Quest.Api.Controllers
{
    [Route("api/[controller]")]
    public class NotifyController : Controller
    {
        AsyncMessageCache _messageCache;

        public NotifyController(AsyncMessageCache messageCache)
        {
            _messageCache = messageCache;
        }

        /// <summary>
        /// Send a message from the Quest app to a device or system.
        /// </summary>
        /// <param name="message">the json representing a message derived from INotificationMessage </param>
        /// <param name="method">The method to send the message via : e.g. GCM, ANS, Win, Email, SMS, HTTP</param>
        /// <param name="address">Target address specific to the method being used</param>
        /// <param name="subject">Subject line, might not used in all methods</param>
        /// <param name="silent">send the message silently</param>
        /// <returns></returns>
        [HttpPost("Send")]
        [AllowAnonymous]
        public async Task<NotificationResponse> Send([FromBody] string message, [FromQuery] string method, [FromQuery]string address, [FromQuery]string subject, [FromQuery]bool silent)
        {
            try
            {
                //TODO: parse the message to find out what type it is..
                MessageNotification msg = new MessageNotification { Message = message, Priority =0, Silent=silent, Title=subject };
                Notification n = new Notification { Address = address, Body = msg, Method = method, Subject = subject };
                return await _messageCache.SendAndWaitAsync<NotificationResponse>(n, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new NotificationResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
        }

        /// <summary>
        /// Send a standard text-based message from the Quest app to a device or system.
        /// </summary>
        /// <param name="message">the json representing a message derived from INotificationMessage </param>
        /// <param name="method">The method to send the message via : e.g. GCM, ANS, Win, Email, SMS, HTTP</param>
        /// <param name="address">Target address specific to the method being used</param>
        /// <param name="subject">Subject line, might not used in all methods</param>
        /// <returns></returns>
        [HttpPost("MessageNotification")]
        [AllowAnonymous]
        public async Task<NotificationResponse> SendMessage([FromBody] MessageNotification message, [FromQuery] string method, [FromQuery]string address, [FromQuery]string subject)
        {
            try
            {
                Notification n = new Notification { Address = address, Body = message, Method = method, Subject = subject };
                return await _messageCache.SendAndWaitAsync<NotificationResponse>(n, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new NotificationResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
        }

        [HttpPost("CallsignNotification")]
        [AllowAnonymous]
        public async Task<NotificationResponse> SendCall([FromBody] CallsignNotification message, [FromQuery] string method, [FromQuery]string address, [FromQuery]string subject)
        {
            try
            {
                Notification n = new Notification { Address = address, Body = message, Method = method, Subject = subject };
                return await _messageCache.SendAndWaitAsync<NotificationResponse>(n, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new NotificationResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
        }

        [HttpPost("StatusNotification")]
        [AllowAnonymous]
        public async Task<NotificationResponse> SendStatusNotification([FromBody] StatusNotification message, [FromQuery] string method, [FromQuery]string address, [FromQuery]string subject)
        {
            try
            {
                Notification n = new Notification { Address = address, Body = message, Method = method, Subject = subject };
                return await _messageCache.SendAndWaitAsync<NotificationResponse>(n, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new NotificationResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
        }

        [HttpPost("CancellationNotification")]
        [AllowAnonymous]
        public async Task<NotificationResponse> SendCancellationNotification([FromBody] CancellationNotification message, [FromQuery] string method, [FromQuery]string address, [FromQuery]string subject)
        {
            try
            {
                Notification n = new Notification { Address = address, Body = message, Method = method, Subject = subject };
                return await _messageCache.SendAndWaitAsync<NotificationResponse>(n, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new NotificationResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
        }

        [HttpPost("EventNotification")]
        [AllowAnonymous]
        public async Task<NotificationResponse> SendEventNotification([FromBody] EventNotification message, [FromQuery] string method, [FromQuery]string address, [FromQuery]string subject)
        {
            try
            {
                Notification n = new Notification { Address = address, Body = message, Method = method, Subject = subject };
                return await _messageCache.SendAndWaitAsync<NotificationResponse>(n, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return new NotificationResponse() { Success = false, Message = ex.Message };
            }
            finally
            {
            }
        }



    }
}
