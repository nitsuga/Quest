using System;
using System.Diagnostics;
using Quest.Lib.Trace;
using PushSharp.Google;
using Quest.Common.Messages;
using System.Collections.Generic;
using PushSharp.Common;

namespace Quest.Lib.Notifier
{
    public class GCMNotifier : INotifier
    {
        public string Key { get; set; }
        public string SenderId { get; set; }

        private GcmServiceBroker _gcmBroker=null;
        //fXKRAr-70Tw:APA91bGd243zux4AMipnTWxLfTqy5FYrO3tfUeMsBiNDe0H7FvAixcZAEV-EHhCeQdksV0KNW1qvg2J1QomscJI-6VEQDfEmhVjCnDpyMOMlcd0A-ojcNvtkb74im0xl31exwrLfhRYx
        public GCMNotifier()
        {
        }

        public void Initialise()
        {
            if (_gcmBroker != null)
                return;

            Logger.Write($"GCM Notifier starting with App={SenderId} Key={Key}", GetType().Name, TraceEventType.Start);
            var gcmConfig = new GcmConfiguration(SenderId, Key, null);

            // Create a new broker
            _gcmBroker = new GcmServiceBroker(gcmConfig);

            // Wire up events
            _gcmBroker.OnNotificationFailed += (notification, aggregateEx) =>
            {

                aggregateEx.Handle(ex =>
                {

                    // See what kind of exception it was to further diagnose
                    if (ex is GcmNotificationException)
                    {
                        var notificationException = (GcmNotificationException)ex;

                        // Deal with the failed notification
                        var gcmNotification = notificationException.Notification;
                        var description = notificationException.Description;

                        Logger.Write($"GCM Notification Failed: {ex.Message} ID={gcmNotification.MessageId}, Desc={description}", GetType().Name, TraceEventType.Error);
                    }
                    else if (ex is GcmMulticastResultException)
                    {
                        var multicastException = (GcmMulticastResultException)ex;

                        foreach (var succeededNotification in multicastException.Succeeded)
                        {
                            Logger.Write($"GCM Notification Succeeded: ID={succeededNotification.MessageId}", GetType().Name, TraceEventType.Information);
                        }

                        foreach (var failedKvp in multicastException.Failed)
                        {
                            var n = failedKvp.Key;
                            var e = failedKvp.Value;

                            Logger.Write($"GCM Notification Failed: ID={n.MessageId}, Desc={e.Message}", GetType().Name, TraceEventType.Error);
                        }

                    }
                    else if (ex is DeviceSubscriptionExpiredException)
                    {
                        var expiredException = (DeviceSubscriptionExpiredException)ex;

                        var oldId = expiredException.OldSubscriptionId;
                        var newId = expiredException.NewSubscriptionId;

                        Logger.Write($"Device RegistrationId Expired: {oldId}", GetType().Name, TraceEventType.Error);

                        if (!string.IsNullOrWhiteSpace(newId))
                        {
                            // If this value isn't null, our subscription changed and we should update our database
                            Logger.Write($"Device RegistrationId Changed To: {newId}", GetType().Name, TraceEventType.Error);
                        }
                    }
                    else if (ex is RetryAfterException)
                    {
                        var retryException = (RetryAfterException)ex;
                        // If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
                        Logger.Write($"GCM Rate Limited, don't send more until after {retryException.RetryAfterUtc}", GetType().Name, TraceEventType.Error);
                    }
                    else
                    {
                        Logger.Write($"GCM Notification Failed {ex.Message}", GetType().Name, TraceEventType.Error);
                    }

                    // Mark it as handled
                    return true;
                });
            };

            _gcmBroker.OnNotificationSucceeded += (notification) =>
            {
                Logger.Write("GCM Notification Sent!", GetType().Name, TraceEventType.Information);
            };

            _gcmBroker.Start();
        }

        public NotificationResponse Send(Notification message)
        {
            Initialise();

            Logger.Write($"Sending via {message.Method} to {message.Address} {message.Subject}", TraceEventType.Information, GetType().Name);

            var data = Newtonsoft.Json.Linq.JObject.FromObject(message.Body);            
            _gcmBroker.QueueNotification(new GcmNotification { RegistrationIds = new List<string> { message.Address }, Data = data, Notification = new Newtonsoft.Json.Linq.JObject});
            return new NotificationResponse { Message = $"GCM Message queued", Success = true, RequestId = message.RequestId };
        }
    }
}