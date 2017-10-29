using Autofac;
using Quest.Common.Messages;
using Quest.Common.Messages.Notification;
using Quest.Common.ServiceBus;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
using Quest.Lib.Utils;
using System;

namespace Quest.Lib.Notifier
{
    public class NotificationManager : ServiceBusProcessor
    {
        private NotificationSettings _notificationSettings = new NotificationSettings();
        ILifetimeScope _scope;

        public NotificationManager(
            NotificationSettings notificationSettings,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            ILifetimeScope scope,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _notificationSettings = notificationSettings;
            _scope = scope;
        }

        protected override void OnPrepare()
        {
            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler<Notification>(NotifyHandler);
        }

        protected override void OnStart()
        {
            Initialise();
            Logger.Write("NotificationManager initialised", GetType().Name);
        }

        /// <summary>
        ///     "Quest.ResourceTracker"
        /// </summary>
        private void Initialise()
        {
        }

        /// <summary>
        ///     Send a message to one or more targets
        /// </summary>
        /// <param name="scope">Composition container</param>
        /// <param name="addresses">target addresses in the form address[,address...] where address is like mailto:someone@home.com</param>
        /// <param name="replyto">a return address if the target wishes to respond</param>
        /// <param name="message"></param>
        private Response NotifyHandler(NewMessageArgs t)
        {
            var message = t.Payload as Notification;

            try
            {
                if (message == null)
                    return new NotificationResponse { Message = "Null Message", Success = false };

                if (string.IsNullOrEmpty(message.Method))
                    return new NotificationResponse { Message = "No 'Method' defined", Success = false, RequestId = message.RequestId };

                if (!_scope.IsRegisteredWithKey<INotifier>(message.Method))
                    return new NotificationResponse { Message = $"Method {message.Method} unrecognised.", Success = false, RequestId = message.RequestId };

                var processor = _scope.ResolveNamed<INotifier>(message.Method);
                if (processor != null)
                    return processor.Send(message);
                else
                    return new NotificationResponse { Message = $"Could not load method {message.Method}", Success = false, RequestId = message.RequestId };
            }
            catch(Exception ex)
            {
                Logger.Write(ex);
                return new NotificationResponse { Message = $"Failed - see server logs for the reason", Success = false};
            }
        }
    }
}