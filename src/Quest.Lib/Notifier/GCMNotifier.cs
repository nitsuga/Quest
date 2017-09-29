using System;
using System.Diagnostics;
using Quest.Lib.Trace;
using PushSharp.Google;
using Quest.Common.Messages;
using System.Collections.Generic;

namespace Quest.Lib.Notifier
{
    public class GCMNotifier : INotifier
    {
        public string GCMKey { get; set; }
        private GcmServiceBroker _gcmBroker;

        public GCMNotifier()
        {
            var gcmConfig = new GcmConfiguration("GCM-SENDER-Id", GCMKey, null);

            // Create a new broker
            _gcmBroker = new GcmServiceBroker(gcmConfig);
            _gcmBroker.Start();
        }

        public NotificationResponse Send(Notification message)
        {
            Logger.Write($"Sending via {message.Method} to {message.Address} {message.Subject}", TraceEventType.Information, this.GetType().Name);

            var ticks = DateTime.UtcNow.Ticks;
            var unixtime = (ticks / 1000L - 62135596800000L).ToString();
            var data = Newtonsoft.Json.Linq.JObject.FromObject(message.Body);            
            _gcmBroker.QueueNotification(new GcmNotification { RegistrationIds = new List<string> { message.Address }, Data = data });
            return new NotificationResponse { Message = $"GCM Message queued", Success = true, RequestId = message.RequestId };
        }
    }
}