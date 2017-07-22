////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2014 Marcus Poulton. Copying is only allowed with the express permission of Marcus Poulton
//   
//   Use of this code is not permitted without a valid license from Marcus Poulton
//   
////////////////////////////////////////////////////////////////////////////////////////////////

namespace Quest.Lib.HEMS
{
#if HEMS
    public class HEMSLinkServer : ProcessorBase
    {
        private DataChannel _channel;
        private Thread _worker;
        private ManualResetEvent _quiting = new ManualResetEvent(false);
        private int _port;
        private Type[] knownMessageTypes = new Type[] { typeof(EventUpdate), typeof(Logon), typeof(CancelNotifications) };
        private bool _singleDevicebyCallsign;

        public event System.EventHandler<HEMSEventArgs> NewMessage;

        
        public HEMSLinkServer(
            TimedEventQueue eventQueue,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler) : base(serviceBusClient, msgHandler)
        {
        }


        /// <summary>
        /// list of cached event updates
        /// </summary>
        private List<EventUpdate> eventCache = new List<EventUpdate>();

        /// <summary>
        /// list of logged on users
        /// </summary>
        private Dictionary<Guid, LogonRecord> loggedOn = new Dictionary<Guid, LogonRecord>();

        /// <summary>
        /// a list of device ids and their last callsign
        /// </summary>
        private List<NotificationRecord> notificationList = new List<NotificationRecord>();

        private double _MaxAge = 5 * 24 * 60.0;  // hold 5 day of events
        private bool Listen { get; set; }

        /// <summary>
        /// the application id that should be received in logon packets
        /// </summary>
        private string _AppId;

        /// <summary>
        /// certificate for push serices
        /// </summary>
        private string _appleP12Certificate;
        private string _appleP12Password;

        private bool Enabled { get; set; }

        private String Host { get; set; }
        private const string QueueName = "Quest.HEMS";
        private bool _IsProduction;
        private string _eventPath;
        private string _notifyPath;
        private string _targetSound;
        private string _adminSound;

        public void Initialise(String host, int port, bool enabled, double MaxAge, string AppId, string RabbitQueue, string RabbitConnection, String RabbitExchange, String RabbitRouting, int RabbitTTL, String appleP12Certificate, string appleP12Password, bool IsProduction, string eventPath, string notifyPath, bool singleDevicebyCallsign, string targetSound, string adminSound)
        {
            _targetSound = targetSound;
            _adminSound = adminSound;
            _singleDevicebyCallsign = singleDevicebyCallsign;
            _eventPath = eventPath;
            _notifyPath = notifyPath;
            _IsProduction = IsProduction;
            _AppId = AppId;
            _MaxAge = MaxAge;
            _appleP12Certificate = appleP12Certificate;
            _appleP12Password = appleP12Password;
            Host = host;
            Listen = host == null;
            Enabled = enabled;
            _port = port;
            Logger.Write(string.Format("Starting"),  TraceEventType.Information, "HEMS Link");
            Worker();
            Logger.Write(string.Format("Started"), TraceEventType.Information, "HEMS Link");

            //
            Logon testmsg = new Logon() { AppId = "LAALAS", LastUpdate = new DateTime(2013, 9, 1), MaxEvents = 20 };
            HEMSMessage msg = new HEMSMessage() { MessageBody = testmsg };
            
            Load();
        }

        protected override void OnPrepare()
        {
            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler<EventUpdate>(EventUpdateHandler);
        }

        protected override void OnStart()
        {
            Initialise();
        }

        protected override void OnStop()
        {
            _channel.CloseChannel();
            _channel = null;
        }

        void Save()
        {

            try
            {
                String events = Serialiser.SerializeToString(eventCache);
                System.IO.File.WriteAllText(_eventPath, events);

                String notifications = Serialiser.SerializeToString(notificationList);
                System.IO.File.WriteAllText(_notifyPath, notifications);
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Failed to save system state: {0}", ex.ToString()), TraceEventType.Error, "HEMS Link");
            }
        }
        
        void Load()
        {

            try
            {
                if (System.IO.File.Exists(_eventPath))
                {
                    String events = System.IO.File.ReadAllText(_eventPath);
                    if (events.Length > 0)
                    {
                        eventCache = Serialiser.Deserialize<List<EventUpdate>>(events);
                    }
                    else
                        eventCache = new List<EventUpdate>();
                    Logger.Write(string.Format("Loaded {0} events", eventCache.Count), TraceEventType.Error, "HEMS Link");
                }
                else
                    Logger.Write(string.Format("Events save file does not exist: {0}", _eventPath), TraceEventType.Error, "HEMS Link");

                if (System.IO.File.Exists(_notifyPath))
                {
                    String notifications = System.IO.File.ReadAllText(_notifyPath);
                    if (notifications.Length > 0)
                        notificationList = Serialiser.Deserialize<List<NotificationRecord>>(notifications);
                    else
                        notificationList = new List<NotificationRecord>();

                    Logger.Write(string.Format("Loaded {0} notifications", notificationList.Count), TraceEventType.Error, "HEMS Link");
                }
                else
                    Logger.Write(string.Format("Notification save file does not exist: {0}", _notifyPath), TraceEventType.Error, "HEMS Link");

            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Failed to save system state: {0}", ex.ToString()), TraceEventType.Error, "HEMS Link");
            }
        }

        void StopPushServices(PushBroker push, bool wait)
        {
            try
            {
                if (push != null)
                {
                    push.StopAllServices(wait);
                    push = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Failed to stop push object: {0}", ex.ToString()), TraceEventType.Error, "HEMS Link");
            }
        }

        PushBroker RegisterWithPushServices()
        {
            PushBroker push = null;

            if (_appleP12Certificate.Length > 0)
            {
                try
                {
                    Logger.Write(string.Format("Registering with PUSH services"), TraceEventType.Information, "HEMS Link");

                    //Registering the Apple Service and sending an iOS Notification
                    var appleCert = File.ReadAllBytes(_appleP12Certificate);
                    push = new PushBroker();

                    push.OnChannelCreated += push_OnChannelCreated;
                    push.OnChannelDestroyed += push_OnChannelDestroyed;
                    push.OnChannelException += push_OnChannelException;
                    push.OnDeviceSubscriptionChanged += push_OnDeviceSubscriptionChanged;
                    push.OnDeviceSubscriptionExpired += push_OnDeviceSubscriptionExpired;
                    push.OnNotificationFailed += push_OnNotificationFailed;
                    push.OnNotificationRequeue += push_OnNotificationRequeue;
                    push.OnNotificationSent += push_OnNotificationSent;
                    push.OnServiceException += push_OnServiceException;

                    push.RegisterAppleService(new ApplePushChannelSettings(_IsProduction, appleCert, _appleP12Password));
                    push.RegisterGcmService(new GcmPushChannelSettings("AIzaSyC9WT1cTt4uQqfatIpSVxPYq6zvopjX1yo"));

                }
                catch (Exception ex)
                {
                    Logger.Write(string.Format("Apple registration failed: {0}", ex.ToString()), TraceEventType.Error, "HEMS Link");
                    return null;
                }


            }

            ////Registering the GCM Service and sending an Android Notification
            //push.RegisterGcmService(new GcmPushChannelSettings("theauthorizationtokenhere"));
            ////Fluent construction of an Android GCM Notification
            ////IMPORTANT: For Android you MUST use your own RegistrationId here that gets generated within your Android app itself!
            //push.QueueNotification(new GcmNotification().ForDeviceRegistrationId("DEVICE REGISTRATION ID HERE")
            //                      .WithJson("{\"alert\":\"Hello World!\",\"badge\":7,\"sound\":\"sound.caf\"}"));

            return push;
        }

        void push_OnServiceException(object sender, Exception error)
        {
            try
            {
                if (error != null)
                    Logger.Write(string.Format("push_OnServiceException: {0}", error.ToString()), TraceEventType.Error, "HEMS Link");
                else
                    Logger.Write(string.Format("push_OnServiceException: NULL"), TraceEventType.Error, "HEMS Link");
            }
            catch
            { }
        }

        void push_OnNotificationSent(object sender, PushSharp.Core.INotification notification)
        {
            try
            {
                Logger.Write(string.Format("push_OnNotificationSent: {0} IsValidDeviceRegistrationId {1}", notification.ToString(), notification.IsValidDeviceRegistrationId()), TraceEventType.Error, "HEMS Link");
            }
            catch
            { }
        }

        void push_OnNotificationRequeue(object sender, PushSharp.Core.NotificationRequeueEventArgs e)
        {
            try
            {
                Logger.Write(string.Format("push_OnNotificationRequeue: {0}", e.Notification.ToString()), TraceEventType.Error, "HEMS Link");

            }
            catch
            { }
        }

        void push_OnNotificationFailed(object sender, PushSharp.Core.INotification notification, Exception error)
        {
            try
            {
                Logger.Write(string.Format("push_OnNotificationFailed: {0} {1}", error.ToString(), notification), TraceEventType.Error, "HEMS Link");
            }
            catch
            { }

        }

        void push_OnDeviceSubscriptionExpired(object sender, string expiredSubscriptionId, DateTime expirationDateUtc, PushSharp.Core.INotification notification)
        {
            try
            {
                Logger.Write(string.Format("push_OnDeviceSubscriptionExpired: {0} expired {1}", expiredSubscriptionId, expirationDateUtc), TraceEventType.Error, "HEMS Link");
            }
            catch
            { }
        }

        void push_OnDeviceSubscriptionChanged(object sender, string oldSubscriptionId, string newSubscriptionId, PushSharp.Core.INotification notification)
        {
            try
            {
                Logger.Write(string.Format("push_OnDeviceSubscriptionChanged: old {0} new {1} notification {2}", oldSubscriptionId, newSubscriptionId, notification.ToString()), TraceEventType.Error, "HEMS Link");
            }
            catch
            { }
        }

        void push_OnChannelException(object sender, PushSharp.Core.IPushChannel pushChannel, Exception error)
        {
            try
            {
                Logger.Write(string.Format("push_OnChannelException: {0}", error.ToString()), TraceEventType.Error, "HEMS Link");
            }
            catch
            { }
        }

        void push_OnChannelDestroyed(object sender)
        {
            try
            {
                Logger.Write(string.Format("push_OnChannelDestroyed"), TraceEventType.Error, "HEMS Link");
            }
            catch
            { }
        }

        void push_OnChannelCreated(object sender, PushSharp.Core.IPushChannel pushChannel)
        {
            try
            {
                Logger.Write(string.Format("push_OnChannelCreated: {0}", pushChannel.ToString()), TraceEventType.Error, "HEMS Link");
            }
            catch
            { }
        }

        private Response EventUpdateHandler(NewMessageArgs t)
        {
            EventUpdate evt = t.Payload as EventUpdate;
            if (evt != null)
            {

                // remove old ones.
                var old = eventCache.Where(x => Math.Abs((DateTime.Now - x.Updated).TotalMinutes) > _MaxAge).ToList();
                foreach (var d in old)
                    eventCache.Remove(d);

                // remove those with same CAD
                var same = eventCache.Where(x => x.EventId == evt.EventId).ToList();
                foreach (var d in same)
                    eventCache.Remove(d);

                eventCache.Add(evt);

                Logger.Write(string.Format("Sending EventUpdate from Quest to HEMS : {0} {1} HEMS units logged on, {2} events in cache", evt.ToString(), loggedOn.Count(), eventCache.Count()), TraceEventType.Information, "HEMS Link");

                SendDataToDevices(evt);

                NotifyDevices(evt);

                // save system state
                Save();
            }
            return null;
        }

        void SendDataToDevices(EventUpdate evt)
        {
            var targets = loggedOn
                            .Where(x => x.Value.ReceiveAll == true || String.Compare(evt.Callsign, x.Value.Callsign, true) == 0);

            // send to selected logged on handsets
            Send(evt, targets.Select(x => x.Value.commsID).ToArray());
        }

        void NotifyDevices(EventUpdate evt)
        {
            PushBroker push = null;

            try
            {
                push = RegisterWithPushServices();

                if (push == null)
                    return;

                var targets = notificationList
                                .Where(x => x.ReceiveAll == true || String.Compare(evt.Callsign, x.Callsign, true) == 0);

                // send push notifications
                foreach (var target in targets)
                    if (target.DeviceToken != null && target.DeviceToken.Length > 0)
                    {
                        try
                        {
                            switch (target.DeviceType)
                            {
                                case 0:
                                    break;

                                case 1:
                                    PushToApple(push, target, evt);
                                    break;

                                case 2:
                                    PushToGoogle(push, target, evt);
                                    break;

                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Write(string.Format("error sending notification: {0}", ex.ToString()), TraceEventType.Information, "HEMS Link");
                        }
                    }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("error sending notification: {0}", ex.ToString()), TraceEventType.Information, "HEMS Link");
            }
            finally
            {
                Thread.Sleep(1000);
                StopPushServices(push, true);
                Logger.Write(string.Format("finished closing notification channels"), TraceEventType.Information, "HEMS Link");
            }
        }

        private void PushToApple(PushBroker push, NotificationRecord target, EventUpdate evt)
        {
            string sound = evt.Callsign == target.Callsign ? _targetSound : _adminSound;
            Logger.Write(string.Format("Sending Apple notification: {0} event callsign {1} token code {2} token callsign {3} sound '{4}'", evt.EventId, evt.Callsign, target.DeviceToken, target.Callsign, sound), TraceEventType.Information, "HEMS Link");
            if (sound.Length == 0)
                push.QueueNotification(new AppleNotification()
                   .ForDeviceToken(target.DeviceToken)
                   .WithAlert("New event for " + evt.Callsign));
            else
                push.QueueNotification(new AppleNotification()
                   .ForDeviceToken(target.DeviceToken)
                   .WithAlert("New event for " + evt.Callsign)
                   .WithSound(sound));
        }

        private void PushToGoogle(PushBroker push, NotificationRecord target, EventUpdate evt)
        {
            Logger.Write(string.Format("Sending GCM notification: {0} callsign {1} token {2}", evt.EventId, evt.Callsign, target.DeviceToken), TraceEventType.Information, "HEMS Link");
            //https://console.developers.google.com/project/865069987651/apiui/credential?authuser=0
            //OPS project AIzaSyC9WT1cTt4uQqfatIpSVxPYq6zvopjX1yo   for 86.29.75.151 & 194.223.243.235 (HEMS server)

            push.QueueNotification(new GcmNotification().ForDeviceRegistrationId("DEVICE REGISTRATION ID HERE")
                      .WithJson(@"{""alert"":""New event for " + evt.Callsign + @""",""sound"":""sound.caf""}"));

        }

        private bool IsConnected
        {
            get
            {
                if (_channel == null)
                    return false;
                else
                    return _channel.IsConnected;
            }
            set { }
        }

        /// <summary>
        /// processing incoming data from connected reflectors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _channel_Data(object sender, DataEventArgs e)
        {
            ParseMessage(e.Data, e);
        }

        /// <summary>
        /// parse incoming message - we should only get logon messages
        /// we record the guid of the connection as this will enable outbound messages..
        /// we wont send any data to a connection that hasn't sent us a logon packet
        /// </summary>
        /// <param name="message"></param>
        private void ParseMessage(string message, DataEventArgs e)
        {
            try
            {
                HEMSMessage msg = JSON2Object(message);

                Logon logon = msg.MessageBody as Logon;
                DataRequest datareq = msg.MessageBody as DataRequest;
                CancelNotifications cancelReq = msg.MessageBody as CancelNotifications;

                HEMSEventArgs nm = new HEMSEventArgs() { HEMSMessage = msg, RawMessage = e.Data, ErrorMessage = "" };

                if (cancelReq != null)
                {
                    int removed = notificationList.RemoveAll(x => x.DeviceToken == cancelReq.DeviceToken);
                    Logger.Write(String.Format("Cancel request for {0} - {1} items removed from notification list", cancelReq.DeviceToken, removed), TraceEventType.Warning, "HEMS Link");

                    // save system state
                    Save();

                    return;
                }

                if (logon != null)
                {
                    // failed 
                    if (logon.AppId != _AppId)
                    {
                        nm.ErrorMessage = string.Format("Logon rejected from callsign {0} id ={1} incorrect Appid {2}", logon.Callsign, e.ConnectionId, logon.AppId);

                        Logger.Write(nm.ErrorMessage, TraceEventType.Warning, "HEMS Link");
                        return;
                    }

                    if (logon.Callsign == null || logon.Callsign.Length == 0)
                    {
                        nm.ErrorMessage = string.Format("Logon rejected from callsign {0} id ={1} incorrect Appid {2}", logon.Callsign, e.ConnectionId, logon.AppId);
                        Logger.Write(nm.ErrorMessage, TraceEventType.Warning, "HEMS Link");
                        return;
                    }

                    LogonRecord record = new LogonRecord()
                            {
                                Callsign = logon.Callsign,
                                commsID = e.ConnectionId,
                                LoggedOn = DateTime.Now,
                                ReceiveAll = logon.ReceiveAll,
                            };


                    if (logon.DeviceToken.Length > 0)
                    {
                        // remove other entries with same device token
                        notificationList.RemoveAll(x => x.DeviceToken == logon.DeviceToken);

                        // remove other entries with same callsign
                        if (_singleDevicebyCallsign)
                            notificationList.RemoveAll(x => x.Callsign == logon.Callsign);

                        if (logon.DeviceType > 0)
                        {
                            NotificationRecord nrecord = new NotificationRecord()
                            {
                                Callsign = logon.Callsign,
                                DeviceToken = logon.DeviceToken,
                                ReceiveAll = logon.ReceiveAll,
                                DeviceType = logon.DeviceType
                            };
                            notificationList.Add(nrecord);
                        }
                    }

                    // add the connection to our list
                    if (!loggedOn.ContainsKey(e.ConnectionId))
                    {
                        Logger.Write(string.Format("Logon Accepted from callsign {0} appid={1} pushtoken={2} pushtype={3} since={4} receiveall={5}", logon.Callsign, logon.AppId, logon.DeviceToken, logon.DeviceType, logon.LastUpdate, logon.ReceiveAll), TraceEventType.Information, "HEMS Link");
                        loggedOn.Add(e.ConnectionId, record);
                    }

                    //broadcast userlist to be saved on db internally
                    BroadcastUserList();

                    Logger.Write(string.Format("checking cache, {0} items", eventCache.Count()), TraceEventType.Information, "HEMS Link");

                    // now send old events
                    var toSend = eventCache
                        .Where(x => x.Updated >= logon.LastUpdate && (record.ReceiveAll == true || String.Compare(record.Callsign, x.Callsign, true) == 0))
                        .Take(logon.MaxEvents)
                        .OrderBy(x => x.CallOrigin)
                        .Select(x => x);

                    if (toSend.Count() == 0)
                        Logger.Write(string.Format("No cached events to send to device"), TraceEventType.Information, "HEMS Link");
                    else
                        Logger.Write(string.Format("Sending {0} cached events to device", toSend.Count()), TraceEventType.Information, "HEMS Link");

                    foreach (EventUpdate evt in toSend)
                    {
                        Thread.Sleep(100);
                        Logger.Write(string.Format("Sending Cached EventUpdate from Quest to HEMS : {0} {1} HEMS units logged on", evt.ToString(), loggedOn.Count()), TraceEventType.Information, "HEMS Link");

                        Send(evt, e.ConnectionId);
                    }

                    // save system state
                    Save();
                }

                if (NewMessage != null)
                    NewMessage(this, nm);

            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Error processing Logon message: {0}", ex.ToString()), TraceEventType.Error, "HEMS Link");
            }
        }

        private String Object2JSON(MessageBase msg)
        {
            HEMSMessage obj = new HEMSMessage() { MessageBody = msg };
            using (MemoryStream stream1 = new MemoryStream())
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(HEMSMessage), knownMessageTypes);
                ser.WriteObject(stream1, obj);
                stream1.Position = 0;
                StreamReader sr = new StreamReader(stream1);
                String packet = sr.ReadToEnd();
                packet = packet.Replace("Quest.Lib.HEMS.Message", "HEMSLink.Message");
                return packet;
            }
        }

        private HEMSMessage JSON2Object(String msg)
        {
            msg = msg.Replace("HEMSLink.Message", "Quest.Lib.HEMS.Message");

            using (MemoryStream stream1 = new MemoryStream())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(msg);
                stream1.Write(bytes, 0, bytes.Length);
                stream1.Position = 0;

                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(HEMSMessage), knownMessageTypes);
                return (HEMSMessage)ser.ReadObject(stream1);
            }
        }

        public void Send(MessageBase message)
        {
            if (Listen)
                Send(message, loggedOn.Keys.ToArray());
            else
            {
                _channel.Send(Object2JSON(message));
            }
        }

        private void Send(MessageBase message, Guid client)
        {
            _channel.Send(Object2JSON(message), new Guid[] { client });
        }

        private void Send(MessageBase message, Guid[] clients)
        {
            _channel.Send(Object2JSON(message), clients);
        }

        public void SendJSON(String message, Guid[] clients)
        {
            _channel.Send(message, clients);
        }

        public void SendJSON(String message)
        {
            _channel.Send(message, loggedOn.Keys.ToArray());
        }

        /// <summary>
        /// channel got disconnected
        /// if disconnected client is in loggedon list, remove client from list and broadcast list to rabbit
        /// which will then be picked up internally and saved to DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _channel_Disconnected(object sender, DisconnectedEventArgs e)
        {

            Logger.Write(string.Format("HEMS link channel disconnected {0}", e.Remoteconnection.ConnectionId), TraceEventType.Information, "HEMS Link");
            var client = e.Remoteconnection.ConnectionId;
            if (loggedOn.ContainsKey(client))
            {
                loggedOn.Remove(client);
                BroadcastUserList();
            }
        }

        private void BroadcastUserList()
        {
            LoggedOnList userlist = new LoggedOnList();

            var currentusers = loggedOn.Select(i => i.Value).ToList();

            if (currentusers != null && currentusers.Count > 0)
                userlist.Users = currentusers;
            else
                userlist.Users = new List<LogonRecord>();

            base.ServiceBusClient.Broadcast(userlist);
            Logger.Write(string.Format("Broadcasting logged on list: {0}", userlist), TraceEventType.Information, "HEMS Link");

        }

        /// <summary>
        /// channel got connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _channel_Connected(object sender, ConnectedEventArgs e)
        {
            if (e.RemoteTcpipConnection != null)
                Logger.Write(string.Format("HEMS link channel client connected: id={0}", e.RemoteTcpipConnection.ConnectionId), TraceEventType.Information, "HEMS Link");

        }

        /// <summary>
        /// send heartbeats periodically
        /// </summary>
        private void HeartbeatWorker()
        {
            while (true)
            {
                try
                {
                    // commanded to stop?
                    if (_quiting.WaitOne(1000))
                        return;

                    // shut down?
                    if (!Enabled && _channel != null)
                    {
                        Logger.Write(string.Format("HEMS link disabled", _port), TraceEventType.Information, "HEMS Link");
                        _channel.CloseChannel();
                        _channel = null;
                        continue;
                    }

                    // start up
                    if (Enabled && _channel == null)
                    {
                        if (Listen)
                        {
                            _channel = new TcpipListener();
                            // start the master listener
                            STXStreamCodec codec = new STXStreamCodec();
                            _channel.Connected += new EventHandler<ConnectedEventArgs>(_channel_Connected);
                            _channel.Disconnected += new EventHandler<DisconnectedEventArgs>(_channel_Disconnected);
                            _channel.Data += new EventHandler<DataEventArgs>(_channel_Data);

                            ((TcpipListener)_channel).StartListening(codec.GetType(), _port);
                        }
                        else
                        {
                            _channel = new TcpipConnection();
                            // start the master listener
                            STXStreamCodec codec = new STXStreamCodec();
                            _channel.Connected += new EventHandler<ConnectedEventArgs>(_channel_Connected);
                            _channel.Disconnected += new EventHandler<DisconnectedEventArgs>(_channel_Disconnected);
                            _channel.Data += new EventHandler<DataEventArgs>(_channel_Data);

                            bool connected = ((TcpipConnection)_channel).Connect(codec.GetType(), new Guid(), Host, _port);

                            if (connected == false)
                            {
                                _channel.CloseChannel();
                                _channel = null;
                            }
                        }

                        Logger.Write(string.Format("HEMS Link started on port {0}", _port), TraceEventType.Information, "HEMS Link");
                        continue;
                    }

                }
                catch (Exception ex)
                {
                    if (ExceptionPolicy.HandleException(ex, LoggingPolicy.Policy.TracePolicy.ToString()))
                        throw;
                }
            }
        }
    }
#endif
}
