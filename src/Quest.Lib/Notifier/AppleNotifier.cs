using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mail;
using Quest.Lib.Trace;
using PushSharp.Apple;

namespace Quest.Lib.Notifier
{
    public class AppleNotifier : INotifier
    {
        private ApnsServiceBroker _apnsBroker;

        public void Setup()
        {
            //if (settings.AppleP12Certificate.Length > 0)
            //{
            //    //Registering the Apple Service and sending an iOS Notification
            //    var appleCert = File.ReadAllBytes(settings.AppleP12Certificate);
            //    var apnsConfig = new ApnsConfiguration(settings.AppleIsProduction ? ApnsConfiguration.ApnsServerEnvironment.Production : ApnsConfiguration.ApnsServerEnvironment.Sandbox, appleCert, settings.AppleP12Password);
            //    _apnsBroker = new ApnsServiceBroker(apnsConfig);
            //    _apnsBroker.Start();
            //}

        }

        public void Send(INotificationMessage message)
        {
            Logger.Write($"Sending via {message.Method} to {message.Address} {message.Subject}", TraceEventType.Information, this.GetType().Name);
        }

        //private void PushToApple(string deviceToken, IDeviceNotification notification)
        //{
        //    var evt = notification as EventNotification;
        //    if (evt != null)
        //    {
        //        var sound = "default";
        //        Logger.Write(
        //            $"Sending Apple notification: {evt.EventId} token {deviceToken} sound '{sound}'", TraceEventType.Information, "Quest");
        //        if (sound.Length == 0)
        //            push.QueueNotification(new AppleNotification()
        //                .ForDeviceToken(deviceToken)
        //                .WithAlert("New event"));
        //        else
        //            push.QueueNotification(new AppleNotification()
        //                .ForDeviceToken(deviceToken)
        //                .WithAlert("New event")
        //                .WithSound(sound));

        //        return;
        //    }

        //    var status = notification as StatusNotification;
        //    if (status != null)
        //    {
        //        var sound = "default";
        //        //Logger.Write(string.Format("Sending Apple notification: {0} event callsign {1} token code {2} token callsign {3} sound '{4}'", evt.EventId, evt.Callsign, DeviceToken, evt.Callsign, sound), TraceEventType.Information, "Quest");
        //        if (sound.Length == 0)
        //            push.QueueNotification(new AppleNotification()
        //                .ForDeviceToken(deviceToken)
        //                .WithAlert("Status now " + status.Status.Description));
        //        else
        //            push.QueueNotification(new AppleNotification()
        //                .ForDeviceToken(deviceToken)
        //                .WithAlert("Status now " + status.Status.Description)
        //                .WithSound(sound));
        //    }
        //}

    }
}