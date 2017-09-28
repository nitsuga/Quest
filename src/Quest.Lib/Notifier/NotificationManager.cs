using Autofac;
using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
using Quest.Lib.Utils;
using System;
using System.Threading.Tasks;

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
            Logger.Write("NotificationManager initialised", "Device");
        }

        /// <summary>
        ///     "Quest.ResourceTracker"
        /// </summary>
        private void Initialise()
        {
        }

        private Response NotifyHandler(NewMessageArgs t)
        {
            var notification = t.Payload as Notification;

            if (notification != null)
                Send(notification);
            return null;
        }

        /// <summary>
        ///     Send a message to one or more targets
        /// </summary>
        /// <param name="scope">Composition container</param>
        /// <param name="addresses">target addresses in the form address[,address...] where address is like mailto:someone@home.com</param>
        /// <param name="replyto">a return address if the target wishes to respond</param>
        /// <param name="message"></param>
        public void Send(Notification message)
        {
            if (message == null)
                return;

            Task.Run(() => SendAsync(message));
        }

        /// <summary>
        ///     Send a message to a target
        /// </summary>
        /// <param name="container">MEF Composition container</param>
        /// <param name="address">target address in the form mailto:someone@home.com</param>
        /// <param name="replyto">a return address if the target wishes to respond</param>
        /// <param name="message"></param>
        public async Task SendAsync(Notification message)
        {
            if (message == null)
                return;

            var processor = _scope.ResolveNamed<INotifier>(message.Method);
            if (processor != null)
            {
                await Task.Run(() => processor.Send(message));
            }
        }
    }
}