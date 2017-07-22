using System;
using Apache.NMS;
using Quest.Lib.Trace;
using Quest.Common.Messages;
using Quest.Lib.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quest.Lib.Processor;
using Quest.Common.ServiceBus;

namespace Quest.Lib.ServiceBus
{

    public class ActiveMqClient : IServiceBusClient, IOptionalComponent, IModule
    {
        public event EventHandler<NewMessageArgs> NewMessage;

        /// <summary>
        /// use a shared (static) unerlying service bus engine
        /// </summary>
        public string format { get; set; } = "json";
        public string topic { get; set; } = "quest.common";
        private string[] validFormats = { "json", "binary" };
        public string server { get; set; } = "activemq:tcp://127.0.0.1:61616";
        public int TTL = 10;
        public bool Persistent = false;
        private ISession _session;
        private AutoResetEvent _senderFailed = new AutoResetEvent(false);

        //public ServiceStatus Status
        //{
        //    get { return _status; }
        //    set
        //    {
        //        _status = value;
        //        Broadcast(_status);
        //    }
        //}

        /// <summary>
        ///     initialise 
        /// </summary>
        /// <param name="queueName">The queue name on which we will listen</param>
        public void Initialise(string queueName)
        {
            QueueName = queueName;

            Task.Factory.StartNew(() => ProcessMessages());
        }

        public string QueueName { get; set; }

        //http://activemq.apache.org/nms/nms-simple-asynchronous-consumer-example.html

        /// <summary>
        /// connect and dispatch outbound messages - long running
        /// </summary>
        private void ProcessMessages()
        {
            do
            {
                try
                {
                    var env = Environment.GetEnvironmentVariable("ActiveMQ");
                    if (env != null)
                    {
                        Logger.Write($"Overriding ActiveMQ address with {env}", GetType().Name);
                        server = env;
                    }

                    Logger.Write($"Connecting to ActiveMq topic {topic} on {server}", GetType().Name);

                    Uri connecturi = new Uri(server);

                    ConnectionFactory amqfactory = new ConnectionFactory(connecturi);

                    TimeSpan receiveTimeout = new TimeSpan(0, 0, 0, 30);

                    using (IConnection connection = amqfactory.CreateConnection())
                    using (ISession session = connection.CreateSession( AcknowledgementMode.ClientAcknowledge ))
                    {
                        _senderFailed.Reset();

                        _session = session;

                        // Start the connection so that messages will be processed.
                        connection.Start();

                        List<Task> tasks = new List<Task>();

                        // read topic 
                        tasks.Add(ListenOnTopic(session, topic));

                        // read queue
                        tasks.Add(ListenOnQueue(session));

                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            _senderFailed.WaitOne();
                            throw new Exception("");
                        }));

                            // wait for tasks to complete
                            Task.WaitAll(tasks.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write($"Message queue error: {ex} ", TraceEventType.Error, "ActiveMq");
                }
                Thread.Sleep(5000);
            } while (true);
            // ReSharper disable once FunctionNeverReturns
        }

        private Task ListenOnTopic(ISession session, string topic)
        {
            var t = Task.Factory.StartNew(() =>
            {
                var queue = new ActiveMQQueue(QueueName);
                IDestination destination = session.GetTopic(topic);
                // Create a consumer and producer
                using (IMessageConsumer topic_consumer = session.CreateConsumer(destination))
                {
                    topic_consumer.Listener += OnTopicMessage;
                    while (true)
                        Thread.Sleep(10000);
                }
            });

            return t;
        }

        private Task ListenOnQueue(ISession session)
        {
            var t2 = Task.Factory.StartNew(() =>
            {
                var queue = new ActiveMQQueue(QueueName);
                // Create a consumer and producer
                using (IMessageConsumer queue_consumer = session.CreateConsumer(queue))
                {
                    // clear existing messages
                    while (queue_consumer.ReceiveNoWait() != null) ;

                    queue_consumer.Listener += OnQueueMessage;
                    while (true)
                        Thread.Sleep(10000);
                }
            });

            return t2;
        }

        private void SendNextMessage(TimeSpan receiveTimeout, string topic, MessageQueueItem objectToSend)
        {
            try
            {
                IMessage msg = null;

                while (_session == null)
                    Thread.Sleep(1000);

                switch (format)
                {
                    case "binary":
                        var bytes = objectToSend.Message.SerializeBinary();

                        if (bytes == null)
                        {
                            Logger.Write("Object could not be serialised", TraceEventType.Error, GetType().Name);
                        }

                        // Send a message
                        msg = _session.CreateBytesMessage(bytes);
                        break;
                    case "json":
                        JsonSerializerSettings settings = new JsonSerializerSettings()
                        {
                            MaxDepth = 1000,
                            TypeNameHandling = TypeNameHandling.All,
                            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        };

                        var json = JsonConvert.SerializeObject(objectToSend, settings);
                        msg = _session.CreateTextMessage(json);
                        break;
                }

                if (msg != null)
                {
                    msg.NMSCorrelationID = objectToSend.Metadata.CorrelationId;
                    msg.Properties["ReplyTo"] = objectToSend.Metadata.ReplyTo;
                    msg.Properties["Source"] = objectToSend.Metadata.Source;
                    msg.Properties["CorrelationId"] = objectToSend.Metadata.CorrelationId;
                    msg.Properties["RoutingKey"] = objectToSend.Metadata.RoutingKey;
                    msg.Properties["MessageType"] = objectToSend.Message.GetType().Name;

                    if (string.IsNullOrEmpty(objectToSend.Metadata.Destination))
                    {
                        Logger.Write($"Sending message {objectToSend.Message.GetType()} to topic {topic}", TraceEventType.Information, GetType().Name);

                        IDestination destination = _session.GetTopic(topic);
                        using (IMessageProducer producer = _session.CreateProducer(destination))
                        {
                            producer.DeliveryMode = Persistent ? MsgDeliveryMode.Persistent : MsgDeliveryMode.NonPersistent;
                            producer.RequestTimeout = receiveTimeout;
                            producer.TimeToLive = new TimeSpan(0, 0, TTL);
                            producer.Send(msg, MsgDeliveryMode.NonPersistent, MsgPriority.Normal, receiveTimeout);
                        }
                    }
                    else
                    {
                        Logger.Write($"Sending message {objectToSend.Message.GetType()} to queue {objectToSend.Metadata.Destination}", TraceEventType.Information, GetType().Name);
                        var queue = new ActiveMQQueue(objectToSend.Metadata.Destination);
                        using (IMessageProducer producer = _session.CreateProducer(queue))
                        {
                            producer.DeliveryMode = Persistent ? MsgDeliveryMode.Persistent : MsgDeliveryMode.NonPersistent;
                            producer.RequestTimeout = receiveTimeout;

                            producer.Send(queue, msg);
                        }
                    }
                }
            }
            catch
            {
                _senderFailed.Set();
            }
        }

        protected void OnTopicMessage(IMessage receivedMsg)
        {
            OnMessage(receivedMsg);
        }

        protected void OnQueueMessage(IMessage receivedMsg)
        {
            OnMessage(receivedMsg);
        }

        /// <summary>
        /// process and dispatch incoming messages
        /// </summary>
        /// <param name="receivedMsg"></param>
        protected void OnMessage(IMessage receivedMsg)
        {
            try
            {
                object obj = null;

                var textmessage = receivedMsg as ITextMessage;
                
                if (textmessage != null)
                {
                    textmessage.Acknowledge();
                    JsonSerializerSettings settings = new JsonSerializerSettings()
                    {
                        MaxDepth = 1000,
                        TypeNameHandling = TypeNameHandling.All,
                        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    };
                    obj = JsonConvert.DeserializeObject(textmessage.Text, settings);
                }

                var binarymessage = receivedMsg as IBytesMessage;
                if (binarymessage != null)
                {
                    binarymessage.Acknowledge();
                    obj = Serialiser.DeserializeBinary<object>(binarymessage.Content);
                }

                var meta = new PublishMetaData()
                {
                    CorrelationId = receivedMsg.NMSCorrelationID,
                    ReplyTo = receivedMsg.Properties.GetString("ReplyTo"),
                    Source = receivedMsg.Properties.GetString("Source")
                };

                var mqi = obj as MessageQueueItem;

                if (mqi != null)
                {
                    var args = new NewMessageArgs()
                    {
                        Client = this,
                        Metadata = mqi.Metadata,
                        Payload = mqi.Message
                    };

                    NewMessage?.Invoke(this, args);
                }

            }
            catch (Exception ex)
            {
                Logger.Write($"Message queue error: {ex} ", TraceEventType.Error, "ActiveMq");
            }
        }

        /// <summary>
        /// broadcast a message on the Quest.Common topic with additional metadata
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="metadata">meta data to accompany the message</param>
        public void Broadcast(IServiceBusMessage message, PublishMetaData metadata)
        {
            // Logger.Write($"Queuing {message.GetType()} {metadata}", TraceEventType.Information, GetType().Name);
            // add into local outbound queue
            MessageQueueItem item = new MessageQueueItem() { Message = message, Metadata = metadata };

            SendNextMessage(new TimeSpan(0, 0, TTL), topic, item);
        }

        /// <summary>
        /// broadcast a message on the Quest.Common topic
        /// </summary>
        /// <param name="message"></param>
        public void Broadcast(IServiceBusMessage message)
        {
            PublishMetaData metadata = new PublishMetaData()
            {
                CorrelationId = "",
                ReplyTo = QueueName,
                RoutingKey = "",
                Source = QueueName,
                Destination = ""
            };

            Broadcast(message, metadata);
        }

        public void Send(IServiceBusMessage message, string queue)
        {
            PublishMetaData metadata = new PublishMetaData()
            {
                CorrelationId = "",
                ReplyTo = "",
                RoutingKey = "",
                Source = QueueName,
                Destination = queue
            };

            Broadcast(message, metadata);
        }

        public void Stop()
        {
        }

    }
}