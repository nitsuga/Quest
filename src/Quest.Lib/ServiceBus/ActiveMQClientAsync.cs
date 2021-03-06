﻿using System;
using System.Net;
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
using Quest.Common.ServiceBus;
using Quest.Common.Messages.System;

namespace Quest.Lib.ServiceBus
{

    public class ActiveMqClientAsync : IServiceBusClient
    {
        public event EventHandler<NewMessageArgs> NewMessage;

        /// <summary>
        /// use a shared (static) unerlying service bus engine
        /// </summary>
        private ServiceStatus _status;

        /// <summary>
        /// flag indicates that a message is ready to be sent by the writer
        /// </summary>
        private readonly AutoResetEvent _writeSemaphore = new AutoResetEvent(false);

        /// <summary>
        /// flag indicates a fault on the listeners or write
        /// </summary>
        private readonly AutoResetEvent _errorSemaphore = new AutoResetEvent(false);

        /// <summary>
        /// flag to signal listeners and writer to quit
        /// </summary>
        private readonly ManualResetEvent _quitTasksSemaphore = new ManualResetEvent(false);

        private readonly Queue<MessageQueueItem> _outputQueue = new Queue<MessageQueueItem>(32768);
        public string format { get; set; } = "json";
        public string topic { get; set; } = "quest.common";
        private string[] validFormats = { "json", "binary" };
        public string server { get; set; } = "activemq:tcp://127.0.0.1:61616";
        public int TTL = 10;
        public bool Persistent = false;
        public string Environment { get; set; } = "";

        public ServiceStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                Broadcast(_status);
            }
        }

        /// <summary>
        ///     initialise 
        /// </summary>
        /// <param name="queueName">The queue name on which we will listen</param>
        public void Initialise(string queueName)
        {
            var env = System.Environment.GetEnvironmentVariable("Session");
            if (string.IsNullOrEmpty(env))
                env = "0";
            Environment = env;
            QueueName = Environment + "-" + queueName;
            topic = Environment + "-" + topic;

            Task.Factory.StartNew(() => ProcessMessages());

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
                    var env = System.Environment.GetEnvironmentVariable("ActiveMQ");
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
                    using (ISession qsess = connection.CreateSession( AcknowledgementMode.ClientAcknowledge ))
                    {
                        Logger.Write($"Connected successfully to {server}", GetType().Name);

                        // Start the connection so that messages will be processed.
                        connection.Start();

                        List<Task> tasks = new List<Task>();

                        // read topic 
                        tasks.Add(ListenOnTopic(qsess));

                        // read queue
                        tasks.Add(ListenOnQueue(qsess));

                        //writer..
                        tasks.Add(Writer(qsess, receiveTimeout, topic));

                        int index = WaitHandle.WaitAny(new WaitHandle[] { _quitTasksSemaphore, _errorSemaphore });

                        // tell others to stop
                        _quitTasksSemaphore.Set();

                        // wait for tasks to complete
                        Task.WaitAll(tasks.ToArray());

                        _quitTasksSemaphore.Reset();

                        // 
                        Logger.Write($"Listeners and write task stopped on ActiveMq topic {topic} on {server}", GetType().Name);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write($"Message queue error: {ex.Message} ", TraceEventType.Error, "ActiveMq");
                }
                Thread.Sleep(5000);
            } while (true);
            // ReSharper disable once FunctionNeverReturns
        }

        private Task Writer(ISession session, TimeSpan receiveTimeout, string topic)
        {
            var t2 = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        int index = WaitHandle.WaitAny(new WaitHandle[] { _quitTasksSemaphore, _writeSemaphore });

                        // got a quit signal
                        if (index == 0)
                            return;

                        while (_outputQueue.Count > 0)
                        {
                            var objectToSend = _outputQueue.Dequeue();
                            if (objectToSend == null)
                            {
                                Logger.Write("Object is NULL", TraceEventType.Error, GetType().Name);
                            }

                            SendMessage(objectToSend, session, receiveTimeout, topic);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Write($"Message queue error: {ex} ", TraceEventType.Error, "ActiveMq::Writer");
                        _errorSemaphore.Set();
                        break;
                    }
                }
            });

            return t2;
        }


        private Task ListenOnTopic(ISession qsess)
        {
            var t = Task.Factory.StartNew(() =>
            {
                try
                {
                    IDestination destination = qsess.GetTopic(topic);
                    // Create a consumer and producer
                    using (IMessageConsumer topic_consumer = qsess.CreateConsumer(destination))
                    {
                        Logger.Write($"Listening for messages on {topic}", GetType().Name);

                        // dispatch waiting messages
                        IMessage msg = null;
                        while ((msg = topic_consumer.ReceiveNoWait()) != null)
                            OnTopicMessage(msg);

                        topic_consumer.Listener += OnTopicMessage;

                        _quitTasksSemaphore.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write($"Message queue error: {ex} ", TraceEventType.Error, "ActiveMq::ListenOnTopic");
                    _errorSemaphore.Set();
                    return;
                }
            });

            return t;
        }

        public T SendMessageAndWait<T>(IServiceBusMessage message, string destqueue, string srcqueue = null) where T: class
        {
            if (srcqueue == null)
                srcqueue = Guid.NewGuid().ToString();

            MessageQueueItem objectToSend = new MessageQueueItem
            {
                Message = message,
                Metadata = new PublishMetaData()
                {
                    CorrelationId = "",
                    ReplyTo = srcqueue,
                    RoutingKey = "",
                    Source = srcqueue,
                    Destination = destqueue,
                    MsgType = message.GetType().Name
                }
            };

            var env = System.Environment.GetEnvironmentVariable("ActiveMQ");
            if (env != null)
            {
                Logger.Write($"Overriding ActiveMQ address with {env}", GetType().Name);
                server = env;
            }

            Logger.Write($"Connecting to ActiveMq on {server}", GetType().Name);

            Uri connecturi = new Uri(server);
            ConnectionFactory amqfactory = new ConnectionFactory(connecturi);
            TimeSpan receiveTimeout = new TimeSpan(0, 0, 0, 30);
            using (IConnection connection = amqfactory.CreateConnection())
            {
                using (ISession session = connection.CreateSession(AcknowledgementMode.ClientAcknowledge))
                {
                    var queue = new ActiveMQQueue(QueueName);
                    // Create a consumer and producer
                    using (IMessageConsumer queue_consumer = session.CreateConsumer(queue))
                    {
                        // clear existing messages
                        while (queue_consumer.ReceiveNoWait() != null) ;

                        SendMessage(objectToSend, session, receiveTimeout, destqueue);

                        var msg = queue_consumer.Receive();

                        NewMessageArgs reply = ParseMessage(msg);

                        T result = reply.Payload as T;
                        return result;
                    }
                }
            }
        }

        private Task ListenOnQueue(ISession qsess)
        {
            var t2 = Task.Factory.StartNew(() =>
            {
                try
                {

                    var queue = new ActiveMQQueue(QueueName);
                    // Create a consumer and producer
                    using (IMessageConsumer queue_consumer = qsess.CreateConsumer(queue))
                    {
                        Logger.Write($"Listening for messages on {QueueName}", GetType().Name);

                        // dispatch waiting messages
                        IMessage msg = null;
                        while ((msg = queue_consumer.ReceiveNoWait()) != null)
                            OnQueueMessage(msg);

                        queue_consumer.Listener += OnQueueMessage;

                        _quitTasksSemaphore.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write($"Message queue error: {ex} ", TraceEventType.Error, "ActiveMq::ListenOnQueue");
                    _errorSemaphore.Set();
                    return;
                }
            });

            return t2;
        }

        private void SendMessage(MessageQueueItem objectToSend, ISession session, TimeSpan receiveTimeout, string topic)
        {
            IMessage msg = null;

            switch (format)
            {
                case "binary":
                    var bytes = objectToSend.Message.SerializeBinary();

                    if (bytes == null)
                    {
                        Logger.Write("Object could not be serialised", TraceEventType.Error, GetType().Name);
                    }

                    // Send a message
                    msg = session.CreateBytesMessage(bytes);
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
                    msg = session.CreateTextMessage(json);
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

                    IDestination destination = session.GetTopic(topic);
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        producer.DeliveryMode = Persistent ? MsgDeliveryMode.Persistent : MsgDeliveryMode.NonPersistent;
                        producer.RequestTimeout = receiveTimeout;
                        producer.TimeToLive = new TimeSpan(0, 0, TTL);
                        producer.Send(msg);
                    }
                }
                else
                {
                    Logger.Write($"Sending message {objectToSend.Message.GetType()} to queue {objectToSend.Metadata.Destination}", TraceEventType.Information, GetType().Name);
                    var queue = new ActiveMQQueue(objectToSend.Metadata.Destination);
                    using (IMessageProducer producer = session.CreateProducer(queue))
                    {
                        producer.DeliveryMode = Persistent ? MsgDeliveryMode.Persistent : MsgDeliveryMode.NonPersistent;
                        producer.RequestTimeout = receiveTimeout;
                        producer.Send(queue, msg);
                    }
                }
            }
        }
       

        protected void OnTopicMessage(IMessage receivedMsg)
        {
            receivedMsg.Acknowledge();
            OnMessage(receivedMsg);
        }

        protected void OnQueueMessage(IMessage receivedMsg)
        {
            receivedMsg.Acknowledge();
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
                var msg = ParseMessage(receivedMsg);
                if (msg != null)
                    NewMessage?.Invoke(this, msg);
            }
            catch(Exception ex)
            {
                Logger.Write($"OnMessage error: {ex} ", TraceEventType.Error, "ActiveMq::OnMessage");
            }
        }

        protected NewMessageArgs ParseMessage(IMessage receivedMsg)
        {
            try
            {
                object obj = null;

                var textmessage = receivedMsg as ITextMessage;
                if (textmessage != null)
                {
                    //Debug.WriteLine(textmessage.Text);
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

                    return(args);
                }

            }
            catch (Exception ex)
            {
                Logger.Write($"Message queue error: {ex} ", TraceEventType.Error, "ActiveMq");
            }
            return null;
        }

        /// <summary>
        /// broadcast a message on the Quest.Common topic with additional metadata
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="metadata">meta data to accompany the message</param>
        public void Broadcast(IServiceBusMessage message, PublishMetaData metadata)
        {
            Logger.Write($"Queuing {message.GetType()} {metadata}", TraceEventType.Information, GetType().Name);
            // add into local outbound queue
            MessageQueueItem item = new MessageQueueItem() { Message = message, Metadata = metadata };
            _outputQueue.Enqueue(item);
            _writeSemaphore.Set();
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
                Destination = "",
                MsgType = message.GetType().Name
            };

            Broadcast(message, metadata);
        }

        public void Stop()
        {
        }

    }
}