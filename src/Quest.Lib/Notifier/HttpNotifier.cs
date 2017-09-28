using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mail;
using Quest.Lib.Trace;
using PushSharp.Apple;

namespace Quest.Lib.Notifier
{
    public class HttpNotifier : INotifier
    {
        public void Setup()
        {
        }

        public void Send(INotificationMessage message)
        {
            Logger.Write($"Sending via {message.Method} to {message.Address} {message.Subject}", TraceEventType.Information, this.GetType().Name);
        }
    }
}