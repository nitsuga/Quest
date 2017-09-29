using System.Diagnostics;
using Quest.Lib.Trace;
using Quest.Common.Messages;

namespace Quest.Lib.Notifier
{
    public class HttpNotifier : INotifier
    {
        public void Setup()
        {
        }

        public NotificationResponse Send(Notification message)
        {
            Logger.Write($"Sending via {message.Method} to {message.Address} {message.Subject}", TraceEventType.Information, this.GetType().Name);
            return new NotificationResponse { Message = $"Method {GetType().Name} not implemented", Success = false, RequestId = message.RequestId };
        }
    }
}