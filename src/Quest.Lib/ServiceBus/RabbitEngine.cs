using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Threading;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace Quest.Lib.ServiceBus
{

    public class RabbitEngine : IDisposable, IServiceBus
    {
        public event System.EventHandler<NewMessageArgs> NewMessage;

        private System.Timers.Timer _timer;
        private IConnection _connection = null;
        private IModel _channel = null;
        private QueueingBasicConsumer _consumer;
        private Queue<PublishRequest> _queue = new Queue<PublishRequest>();
        private ReaderWriterLock _readlocker = new System.Threading.ReaderWriterLock();
        private AutoResetEvent _are = new AutoResetEvent(false);
        private ManualResetEvent _quiting = new ManualResetEvent(false);
        private Thread _pushWorkerThread;
        private Thread _pullWorkerThread;
        private int _timeOut = 10000;
        private String _connectionString;
        private string _exchange;
        private string _queueName;
        private String[] _routingList;
        private Objects.ServiceStatus _status;
        private int _TTL;

        public Objects.ServiceStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                Broadcast(new PublishRequest() { message = _status, routingKey = "System.Status", source = _queueName });
            }
        }

        /// <summary>
        /// initialise the RabbitMQ with a URL for the Rabbit server
        /// </summary>
        /// <param name="connectionString">a Rabbit connection string e.g. amqp://guest:guest@localhost:5672</param>
        /// <param name="exchange">An exchange to use e.g. CAC</param>
        /// <param name="queueName">The queue name on which we will listen</param>
        /// <param name="Instance">An instance name of the calling process to be used </param>
        public RabbitEngine(String connectionString, String exchange, String queueName, String Instance, String[] routingList, int TTL)
        {
            _connectionString = connectionString;
            _exchange = exchange;
            _queueName = queueName;
            _routingList = routingList;
            _TTL = TTL;

            Logger.Write("Connecting to Rabbit queue '" + queueName + "'");

            SetupChannel();

            Status = new Objects.ServiceStatus() { Reason = "Starting", Server = System.Net.Dns.GetHostName(), ServiceName = queueName, Instance = Instance, Status = "Runnning" };
        }

        private void StartPushWorker()
        {
            _pushWorkerThread = new Thread(new ThreadStart(pushworker));
            _pushWorkerThread.IsBackground = true;
            _pushWorkerThread.Name = "Rabbit Push Worker";
            _pushWorkerThread.Start();
        }

        private void StartPullWorker()
        {
            _pullWorkerThread = new Thread(new ThreadStart(pullworker));
            _pullWorkerThread.IsBackground = true;
            _pullWorkerThread.Name = "Rabbit Pull Worker";
            _pullWorkerThread.Start();
        }

        /// <summary>
        /// publish a message by placing it in the queue
        /// </summary>
        /// <param name="request"></param>
        public void Broadcast(PublishRequest request)
        {
            lock (_queue)
            {
                try
                {
                    _readlocker.AcquireWriterLock(_timeOut);
                    _queue.Enqueue(request);
                }
                finally
                {
                    _readlocker.ReleaseLock();
                    _are.Set();
                }

            }
        }

        public void Stop()
        {
            try
            {
                if (_timer != null)
                    _timer.Stop();

                _quiting.Set();

                if (_connection != null)
                {
                    try
                    {
                        _connection.Close(500);
                        _connection = null;
                    }
                    catch { }
                }

                if (_channel != null)
                {
                    try
                    {
                        _channel.Close();
                        _channel = null;
                    }
                    catch { }
                }


                if (_pushWorkerThread != null)
                {
                    if (_pushWorkerThread.IsAlive)
                    {
                        _pushWorkerThread.Abort();
                    }
                    _pushWorkerThread.Join();      // wait for thread to stop
                }
                if (_pullWorkerThread != null)
                {
                    if (_pullWorkerThread.IsAlive)
                    {
                        _pullWorkerThread.Abort();
                    }
                    _pullWorkerThread.Join();      // wait for thread to stop
                }
            }
            catch { }
        }

        ~RabbitEngine()
        {
            Stop();
        }

        /// <summary>
        /// start a connect to the Rabbit server
        /// </summary>
        private void SetupChannel()
        {
            if (_connection == null)
            {
                ConnectionFactory factory = new ConnectionFactory();
                factory.Uri = _connectionString;
                _connection = factory.CreateConnection();
            }

            if (_connection != null && _channel == null)
            {
                // open up a channel to talk down
                _channel = _connection.CreateModel();

                StartPushWorker();

                // make sure our exchange queue exist
                try
                {
                    _channel.ExchangeDeclarePassive(_exchange);

                    Dictionary<String, Object> args = new Dictionary<string, object>();

                    try
                    {
                        _channel.QueueDeclarePassive(_queueName);
                    }
                    catch
                    {
                        if (_TTL > 0)
                            args.Add("x-message-ttl", (long)_TTL);
                        _channel.QueueDeclare(_queueName, true, false, false, args);
                    }


                    foreach (String r in _routingList)
                    {
                        if (r.ToLower() == "all")
                            _channel.QueueBind(_queueName, _exchange, "");
                        else
                            if (r != "")
                                _channel.QueueBind(_queueName, _exchange, r);
                    }

                    //_channel.QueueDeclarePassive(_queueName);
                }
                catch (Exception ex)
                {
                    Logger.Write("Error binding to queue '" + _queueName + "' " + ex.ToString() );
                }
                // start the dequeuer
                _consumer = new QueueingBasicConsumer(_channel);
                String consumerTag = _channel.BasicConsume(_queueName, false, _consumer);
                if (_pullWorkerThread == null)
                    StartPullWorker();

                _timer = new System.Timers.Timer(30000);
                _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
                _timer.Start();
            }
        }

        static public void MakeRabbitExchange(String connectionString, String exchange)
        {
            IConnection connection = null;
            IModel channel = null;
            ConnectionFactory factory = new ConnectionFactory();
            Dictionary<String, Object> args = new Dictionary<string, object>();

            if (connectionString == "local")
                connectionString = "amqp://guest:guest@localhost:5672";

            factory.Uri = connectionString;
            using (connection = factory.CreateConnection())
            {
                if (connection != null && channel == null)
                {
                    // open up a channel to talk down
                    using (channel = connection.CreateModel())
                    {

                        // make sure our exchange queue exist
                        try
                        {
                            channel.ExchangeDeclarePassive(exchange);

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                    }
                }
            }
        }

        static public void DeleteRabbitQueue(String connectionString, String queueName)
        {
            IConnection connection = null;
            IModel channel = null;
            ConnectionFactory factory = new ConnectionFactory();
            Dictionary<String, Object> args = new Dictionary<string, object>();

            if (connectionString == "local")
                connectionString = "amqp://guest:guest@localhost:5672";

            factory.Uri = connectionString;
            using (connection = factory.CreateConnection())
            {
                if (connection != null && channel == null)
                {
                    // open up a channel to talk down
                    using (channel = connection.CreateModel())
                    {

                        // make sure our exchange queue exist
                        try
                        {
                            // delete the queue if it exists
                            channel.QueueDelete(queueName);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                    }
                }
            }
        }

        static public void MakeRabbitQueue(String connectionString, String queueName, String[] bindings, int TTL)
        {
            IConnection connection = null;
            IModel channel = null;
            ConnectionFactory factory = new ConnectionFactory();
            Dictionary<String, Object> args = new Dictionary<string, object>();

            if (connectionString == "local")
                connectionString = "amqp://guest:guest@localhost:5672";

            factory.Uri = connectionString;

            using (connection = factory.CreateConnection())
            {
                if (connection != null && channel == null)
                {
                    // open up a channel to talk down
                    using (channel = connection.CreateModel())
                    {
                        // make sure our exchange queue exist
                        try
                        {
                            if (TTL > 0)
                                args.Add("x-message-ttl", (long)TTL);

                            channel.QueueDeclare(queueName, true, false, false, args);

                            foreach (String r in bindings)
                            {
                                if (r.ToLower() != "none")
                                {
                                    string[] parts = r.Split(':');
                                    if (parts.Length == 2)
                                        channel.QueueBind(queueName, parts[0], parts[1]);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                    }
                }
            }
        }        
        

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Status = _status;
        }

        /// <summary>
        /// Use this method to broadcast an event object
        /// </summary>
        /// <param name="message"></param>
        private void BroadcastBytes(String uri, String exchange, String routingKey, String ContentType, byte[] message, String CorrelationId, String ReplyTo)
        {
            IModel channel = null;
            IConnection connection = null;
            try
            {
                // we're sending to a specific server
                ConnectionFactory factory = new ConnectionFactory();
                if (uri == null || uri == "")
                    factory.Uri = _connectionString;
                else
                    factory.Uri = uri;

                using (connection = factory.CreateConnection())
                {
                    using (channel = connection.CreateModel())
                    {
                        if (channel != null)
                        {
                            // publish the message
                            IBasicProperties props = channel.CreateBasicProperties();
                            props.ContentType = ContentType;
                            if (CorrelationId != null)
                            {
                                props.CorrelationId = CorrelationId ?? "";
                            }

                            if (ReplyTo != null)
                            {
                                props.ReplyTo = ReplyTo ?? "";
                            }

                            channel.BasicPublish(exchange ?? "", routingKey ?? "", props, message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("BroadcastBytes failed: {0}", ex.ToString()), "Trace", 0, 0, TraceEventType.Information, "RabbitEngine");
                
                System.Diagnostics.Trace.TraceError(ex.ToString());

                if (channel != null)
                {
                    try
                    {
                        channel.Close();
                        channel.Dispose();
                    }
                    catch { }

                }

                if (connection != null)
                {
                    try
                    {
                        _connection.Close();
                        _connection.Dispose();
                    }
                    catch { }

                }
                connection = null;
            }

        }

        /// <summary>
        /// Use this method to broadcast an event object
        /// </summary>
        /// <param name="message"></param>
        private void BroadcastBytes(String exchange, String routingKey, String ContentType, byte[] message, String CorrelationId, String ReplyTo)
        {
            try
            {
                if (_channel != null)
                {
                    // publish the message
                    IBasicProperties props = _channel.CreateBasicProperties();
                    props.ContentType = ContentType;
                    if (CorrelationId != null)
                    {
                        props.CorrelationId = CorrelationId ?? "";
                    }

                    if (ReplyTo != null)
                    {
                        props.ReplyTo = ReplyTo ?? "";
                    }

                    _channel.BasicPublish(exchange ?? "", routingKey ?? "", props, message);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("BroadcastBytes failed: {0}", ex.ToString()), "Trace", 0, 0, TraceEventType.Information, "RabbitEngine");

                if (_channel != null)
                {
                    try
                    {
                        _channel.Close();
                        _channel.Dispose();
                    }
                    catch { }

                }

                _channel = null;
                if (_connection != null)
                {
                    try
                    {
                        _connection.Close();
                        _connection.Dispose();
                    }
                    catch { }

                }
                _connection = null;
            }

        }

        /// <summary>
        /// Use this method to broadcast an event object
        /// </summary>
        /// <param name="message"></param>
        private void PushMessage(PublishRequest request)
        {
            try
            {
                //String msg = String.Format("Sending event via Rabbit to exchange={0} routing={1} msg={2}", _exchange, request.routingKey, request.message.ToString());
                //System.Diagnostics.Trace.TraceInformation(msg);
                String ContentType = request.message.GetType().FullName;
                BinaryFormatter formatter = new BinaryFormatter(); //request.message.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    formatter.Serialize(stream, request.message);
                    byte[] messageBodyBytes = stream.ToArray();

                    if (request.uri==null || request.uri.Length==0)
                        BroadcastBytes(request.exchange, request.routingKey, ContentType, messageBodyBytes, request.CorrelationId, request.ReplyTo);
                    else
                        BroadcastBytes(request.uri, request.exchange, request.routingKey, ContentType, messageBodyBytes, request.CorrelationId, request.ReplyTo);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("PushMessage failed: {0}", ex.ToString()), "Trace", 0, 0, TraceEventType.Information, "RabbitEngine");
            }
        }

        private void pushworker()
        {
            WaitHandle[] signals = new WaitHandle[] { _are, _quiting };
            try
            {
                do
                {
                    try
                    {
                        int index = System.Threading.WaitHandle.WaitAny(signals);

                        // "quite" signal?
                        if (index == 1)
                            return;

                        // must have been that ARE
                        _readlocker.AcquireReaderLock(_timeOut);
                        while (_queue.Count > 0)
                        {
                            PushMessage(_queue.Dequeue());
                        }
                    }
                    finally
                    {
                        try
                        {
                            // Ensure that the lock is released.
                            if (_readlocker.IsReaderLockHeld)
                                _readlocker.ReleaseReaderLock();
                        }
                        catch { }
                    }

                } while (true);
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("pushworker failed: {0}", ex.ToString()), "Trace", 0, 0, TraceEventType.Information, "RabbitEngine");
            }

        }

        private void pullworker()
        {
            try
            {
                do
                {
                    object o = _consumer.Queue.Dequeue();

                    //Logger.Write(string.Format("dequeued message"), "Trace", 0, 0, TraceEventType.Information, "RabbitEngine");

                    BasicDeliverEventArgs e = o as BasicDeliverEventArgs;

                    if (e != null)
                    {
                        try
                        {
                            if (NewMessage != null)
                            {
                                // we have a content type so attempt to decode it
                                String ContentType = e.BasicProperties.ContentType;
                                using (MemoryStream s = new MemoryStream(e.Body))
                                {
                                    //Logger.Write(string.Format("dequeued message - deserialising"), "Trace", 0, 0, TraceEventType.Information, "RabbitEngine");
                                    BinaryFormatter formatter = new BinaryFormatter(); // Type.GetType(ContentType));
                                    Object decodedMessage = formatter.UnsafeDeserialize(s, null);
                                    //Logger.Write(string.Format("dequeued message - raising event"), "Trace", 0, 0, TraceEventType.Information, "RabbitEngine");
                                    NewMessage(this, new NewMessageArgs() { Metadata = new PublishRequest() { CorrelationId = e.BasicProperties.CorrelationId, ReplyTo = e.BasicProperties.ReplyTo }, Payload = decodedMessage });
                                }
                            }
                            else
                                Logger.Write(string.Format("dequeued message - no-one listening to my events"), "Trace", 0, 0, TraceEventType.Information, "RabbitEngine");

                        }
                        catch (Exception ex)
                        {
                            WriteError(ex);
                            Logger.Write(string.Format("dequeued message"), "Trace", 0, 0, TraceEventType.Information, "RabbitEngine");
                            System.Diagnostics.Trace.TraceError(ex.ToString());
                        }
                        _channel.BasicAck(e.DeliveryTag, false);
                    }
                    else
                        Logger.Write(string.Format("dequeued message - couldn't convert to BasicDeliverEventArgs"), "Trace", 0, 0, TraceEventType.Information, "RabbitEngine");
                } while (true);
            }
            catch (Exception ex)
            {
                WriteError(ex);

                Logger.Write(string.Format("pullworker failed: {0}", ex.ToString()), "Trace", 0, 0, TraceEventType.Information, "RabbitEngine");
            }

        }

        public Object Rpc(Object request, string routingKey)
        {
            Object decodedMessage = null;

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = _connectionString;

            using (IConnection _connection = factory.CreateConnection())
            {
                using (IModel channel = _connection.CreateModel())
                {
                    String ContentType = request.GetType().FullName;
                    BinaryFormatter formatter = new BinaryFormatter();
                    using (MemoryStream stream = new MemoryStream())
                    {
                        formatter.Serialize(stream, request);
                        byte[] messageBodyBytes = stream.ToArray();

                        //SimpleRpcClient client = new SimpleRpcClient(channel, address);

                        // this one works locally
                        //SimpleRpcClient client = new SimpleRpcClient(channel, targetQueue);

                        PublicationAddress address = new PublicationAddress(ExchangeType.Direct, _exchange, routingKey);

                        using (SimpleRpcClient client = new SimpleRpcClient(channel, address))
                        {
                            client.TimeoutMilliseconds = 5000; // defaults to infinity
                            //client.TimedOut += new EventHandler(TimedOutHandler);
                            //client.Disconnected += new EventHandler(DisconnectedHandler);
                            byte[] replyMessageBytes = client.Call(messageBodyBytes);

                            // other useful overloads of Call() and Cast() are
                            // available. See the code documentation of SimpleRpcClient
                            // for full details.
                            if (replyMessageBytes != null)
                            {
                                using (MemoryStream s = new MemoryStream(replyMessageBytes))
                                {
                                    decodedMessage = formatter.UnsafeDeserialize(s, null);
                                }
                            }
                        }
                    }
                }
            }

            return decodedMessage;
        }

        static private void WriteError(Exception ex)
        {
            EventLog.WriteEntry("Application", ex.ToString());
        }


        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
