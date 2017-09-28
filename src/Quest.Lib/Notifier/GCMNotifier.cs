using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mail;
using Quest.Lib.Trace;
using PushSharp.Google;

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

        public void Send(INotificationMessage message)
        {
            Logger.Write($"Sending via {message.Method} to {message.Address} {message.Subject}", TraceEventType.Information, this.GetType().Name);
        }

        //private void PushToGoogle(string deviceToken, IDeviceNotification notification, string reason)
        //{
        //    var evt = notification as EventNotification;

        //    var ticks = DateTime.UtcNow.Ticks;

        //    var unixtime = (ticks / 1000L - 62135596800000L).ToString();

        //    Logger.Write($"Sending via {message.Method} to {message.Address} {message.Subject}", TraceEventType.Information, this.GetType().Name);

        //    if (evt != null)
        //    {
        //        //https://console.developers.google.com/project/865069987651/apiui/credential?authuser=0
        //        //OPS project AIzaSyC9WT1cTt4uQqfatIpSVxPYq6zvopjX1yo   for 86.29.75.151 & 194.223.243.235 (HEMS server)

        //        var data = new JObject
        //        {
        //            {"Reason", reason},
        //            {"Timestamp", unixtime},
        //            {"ContentType", "EventNotification"},
        //            {"Priority", evt.Priority},
        //            {"AZGrid", evt.AZGrid},
        //            {"CallOrigin", evt.CallOrigin ?? ""},
        //            {"Determinant", evt.Determinant},
        //            {"Dispatched", evt.Created ?? ""},
        //            {"EventId", evt.EventId},
        //            {"Latitude", evt.Latitude.ToString()},
        //            {"Location", Crypto.Encrypt(evt.Location)},
        //            {"LocationComment", Crypto.Encrypt(evt.LocationComment)},
        //            {"Longitude", evt.Longitude.ToString()},
        //            {"Notes", Crypto.Encrypt(evt.Notes)},
        //            {"PatientAge", (evt.PatientAge ?? "").Trim()},
        //            {"Sex", (evt.Sex ?? "").Trim()},
        //            {"Updated", evt.Updated ?? ""}
        //        };

        //        _gcmBroker.QueueNotification(new GcmNotification { RegistrationIds = new List<string> { deviceToken }, Data = data });
        //        return;
        //    }

        //    var status = notification as StatusNotification;
        //    if (status != null)
        //    {
        //        //https://console.developers.google.com/project/865069987651/apiui/credential?authuser=0
        //        //OPS project AIzaSyC9WT1cTt4uQqfatIpSVxPYq6zvopjX1yo   for 86.29.75.151 & 194.223.243.235 (HEMS server)

        //        var data = new JObject
        //        {
        //            {"Reason", reason},
        //            {"Timestamp", unixtime},
        //            {"ContentType", "StatusNotification"},
        //            {"Code", status.Status.Code},
        //            {"Description", status.Status.Description},
        //        };

        //        _gcmBroker.QueueNotification(new GcmNotification { RegistrationIds = new List<string> { deviceToken }, Data = data });

        //        return;
        //    }

        //    var csn = notification as CallsignNotification;
        //    if (csn != null)
        //    {
        //        var data = new JObject
        //        {
        //            {"Reason", reason},
        //            {"Timestamp", unixtime},
        //            {"ContentType", "CallsignNotification"},
        //            {"Callsign", csn.Callsign},
        //        };

        //        _gcmBroker.QueueNotification(new GcmNotification { RegistrationIds = new List<string> { deviceToken }, Data = data });
        //    }
        //}

    }
}