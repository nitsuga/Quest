using System;
using System.Net;
using Quest.Common.Messages;
using Quest.Lib.Utils;
using Quest.Lib.Trace;
using Quest.Common.ServiceBus;

namespace Quest.Lib.ServiceBus
{
    public class LocalServiceBusClient : IServiceBusClient, IOptionalComponent
    {
        /// <summary>
        /// use a shared (static) underlying service bus engine
        /// </summary>
        private readonly LocalServiceBusEngine _engine = LocalServiceBusEngine.Bus();
        private ServiceStatus _status;

        /// <summary>
        ///     initialise 
        /// </summary>
        /// <param name="queueName">The queue name on which we will listen</param>
        public void Initialise(string queueName)
        {
            QueueName = queueName;

            _engine.NewMessage += _engine_NewMessage;

            Logger.Write("Connecting to LocalServiceBusClient '" + queueName + "'", GetType().Name);

            Status = new ServiceStatus
            {
                Reason = "Starting",
                Server = Dns.GetHostName(),
                ServiceName = queueName,
                Instance = "",
                Status = "Runnning"
            };
        }

        public string QueueName { get; set; }

        public event EventHandler<NewMessageArgs> NewMessage;

        public ServiceStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                Broadcast(_status );
            }
        }

        public void Broadcast(IServiceBusMessage message, PublishMetaData metadata)
        {
            _engine.Broadcast(this, message, metadata);
        }

        public void Broadcast(IServiceBusMessage message)
        {
            PublishMetaData metadata = new PublishMetaData()
            {
                CorrelationId="",
                ReplyTo = "",
                RoutingKey = "",
                Source = QueueName
            };

            Broadcast(message, metadata);
        }

        public void Stop()
        {
        }

        private void _engine_NewMessage(object sender, NewMessageArgs e)
        {
            NewMessage?.Invoke(this, e);
        }

        public void Send(IServiceBusMessage message, string queue)
        {
            Broadcast(message);            
        }
    }
}