using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Specialized;
using Quest.Lib.Utils;
using Quest.Lib.Net;
using Quest.Lib.Trace;
using Quest.Lib.Processor;
using Quest.Lib.Resource;
using Quest.Lib.Incident;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Notifier;
using Quest.Common.ServiceBus;
using Quest.Lib.ServiceBus;
using Quest.Common.Messages;
using Quest.Lib.Coords;
using Quest.Lib.Device;
using Quest.Common.Messages.CAD;
using Quest.Common.Messages.Incident;
using Quest.Common.Messages.Resource;
using Quest.Common.Messages.System;

namespace Quest.Lib.Northgate
{

    public class XCConnector : ServiceBusProcessor, IXCMessageParser
    {
        public enum StatusCode
        {
            Disabled,
            Disconnected,
            Connected,
            ValidData,
            Active
        }

        public string Commands { get; set; } = "";
        public string Subscriptions { get; set; } = "";
        public string CliFormat { get; set; } = "";
        public string IncFormat { get; set; } = "";
        public string ResFormat { get; set; } = "";
        public string SpaFormat { get; set; } = "";
        public string SpbFormat { get; set; } = "";
        public string Lvm { get; set; } = "";
        public string DrFormat { get; set; } = "";
        public string RscFormat { get; set; } = "";
        public int HBTReceiveDelay { get; set; } = 60;
        public int HBTSendDelay { get; set; } = 30;
        public string Connection { get; set; } = "";
        public string Name { get; set; } = "XC0";
        public bool Enabled { get; set; } = true;

        public event System.EventHandler<DataEventArgs> IncomingData;

        private Dictionary<string, int> _incformat = new Dictionary<string, int>();
        private Dictionary<string, int> _resformat = new Dictionary<string, int>();
        private Dictionary<string, int> _cliformat = new Dictionary<string, int>();
        private Dictionary<string, int> _drformat = new Dictionary<string, int>();
        private Dictionary<string, int> _rscformat = new Dictionary<string, int>();
        
        private Dictionary<string, XCMessageHandler> _handlers;

        private delegate void XCMessageHandler(string[] parts);

        private int _HBTReceiveDelay = 60;
        private int _HBTSendDelay = 30;
        private DateTime _lastDataReceived;
        private DateTime _lastHeartbeatSent;
        private Thread worker;
        private StatusCode _status = StatusCode.Disconnected;
        private int _retries = 5;
        private String _subscriptions = "";

        private DataChannel _channel;
        private ManualResetEvent _quiting = new ManualResetEvent(false);
        private NotificationSettings _notificationSettings = new NotificationSettings();

        /// <summary>
        /// is this channel connected to the remote end?
        /// </summary>
        private StatusCode _connStatus;

        public XCConnector(
            NotificationSettings notificationSettings,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _notificationSettings = notificationSettings;
        }

        protected override void OnPrepare()
        {
            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler("XCOutbound", XCOutboundHandler);
        }

        /// <summary>
        /// initialise the channel
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ConnectionString">database connection string to obtain settings</param>
        /// <param name="host">target host</param>
        /// <param name="port">target port</param>
        /// <param name="backupFor">act as backup for this channel</param>
        /// <param name="includeMessages">only process these messages</param>
        public void Initialise()
        {
            _handlers = new Dictionary<string, XCMessageHandler>();

            StringCollection codes = new StringCollection();

            codes.AddRange(Commands.Split('|'));

            if (codes.Contains("CLI"))
                _handlers.Add("CLI", CLIHandler);

            if (codes.Contains("HBT"))
                _handlers.Add("HBT", HBTHandler);

            if (codes.Contains("DI"))
                _handlers.Add("DI", CLIHandler);

            if (codes.Contains("PII"))
                _handlers.Add("PII", PIIHandler);

            if (codes.Contains("PRI"))
                _handlers.Add("PRI", URHandler);

            if (codes.Contains("UR"))
                _handlers.Add("UR", URHandler);

            if (codes.Contains("DR"))
                _handlers.Add("DR", DRHandler);

            if (codes.Contains("CCC"))
                _handlers.Add("CCC", GenericHandler);

            if (codes.Contains("OSB"))
                _handlers.Add("OSB", OSBHandler);

            if (codes.Contains("RSC"))
                _handlers.Add("RSC", RSCHandler);

            SplitOut(ref _cliformat, CliFormat);
            SplitOut(ref _incformat, IncFormat);
            SplitOut(ref _resformat, ResFormat);
            SplitOut(ref _drformat, DrFormat);
            SplitOut(ref _rscformat, RscFormat);

            Logger.Write("Inc format is " + IncFormat, TraceEventType.Information, "Quest Channel " + Name);
            Logger.Write("Res format is " + ResFormat, TraceEventType.Information, "Quest Channel " + Name);
            Logger.Write("Cli format is " + CliFormat, TraceEventType.Information, "Quest Channel " + Name);
            Logger.Write("Dr  format is " + DrFormat,  TraceEventType.Information, "Quest Channel " + Name);
            Logger.Write("Rsc format is " + RscFormat, TraceEventType.Information, "Quest Channel " + Name);
        }

        private Response XCOutboundHandler(NewMessageArgs t)
        {
            XCOutbound instance1 = t.Payload as XCOutbound;

            if (instance1 != null && _channel != null)
            {
                // output to this channel?
                if (instance1.channel.Split(',').Contains(Name))
                {
                    Logger.Write(string.Format("XCOutbound {1} - sent {0}", instance1.command, Name), TraceEventType.Information, "Quest Channel " + Name);
                    _channel.Send(instance1.command);
                }
            }
            return null;
        }

        protected override void OnStart()
        {
            Logger.Write("XC Connector initialised", "Device");

            Initialise();

            worker = new Thread(new ThreadStart(Work));
            worker.IsBackground = true;
            worker.Name = "XC Worker - " + Name;
            worker.Start();
        }

        /// <summary>
        /// stop the channel
        /// </summary>
        public void Stop()
        {
            try
            {
                _quiting.Set();
                Thread.Sleep(500);
                Disconnect();

                if (worker == null)
                    return;

                if (!worker.IsAlive)
                    return;

                worker.Abort();
                worker.Join();

                _connStatus = XCConnector.StatusCode.Disconnected;

            }
            catch 
            {
            }
        }


        /// <summary>
        /// connect to remote data source
        /// </summary>
        /// <returns></returns>
        private bool Connect()
        {
            // get connection settings.. these decide whether this is a server or client

            _channel = ChannelFactory.MakeTcPchannel(Connection);
            _channel.Data += new EventHandler<DataEventArgs>(connection_Data);
            _channel.ActionOnDisconnect = DisconnectAction.RaiseDisconnectEvent;

            // Host=cad-hq-diba,port=2080,Codec=STXStreamCodec

            _lastDataReceived = DateTime.Now;
            _lastHeartbeatSent = DateTime.Now;

            return _channel.IsConnected;
        }

        /// <summary>
        /// disconnect from remote data source 
        /// </summary>
        private void Disconnect()
        {
            if (_channel != null)
            {
                if (_channel.IsConnected)
                    _channel.CloseChannel();
                
                _channel = null;
            }
            _lastDataReceived = DateTime.MinValue;
            _lastHeartbeatSent = DateTime.MinValue;
        }

        private void Work()
        {

            try
            {
                while (true)
                {
                    try
                    {
                        // commanded to stop?
                        if (_quiting.WaitOne(2000))
                            return;

                        XCConnector primary = null;

                        if (Enabled == false && _connStatus == StatusCode.Disconnected)
                        {
                            continue;
                        }

                        if (Enabled == false && _connStatus != StatusCode.Disabled)
                        {
                            String msg = string.Format("Disabled. Disconnecting");
                            ChangeStatus(msg, StatusCode.Disabled);
                            Disconnect();
                            continue;
                        }

                        if (_channel == null)
                            _connStatus = StatusCode.Disconnected;


                        // did we get disconnected?
                        if (_channel != null && _channel.IsConnected == false)
                        {
                            Disconnect();
                            ChangeStatus("Disconnected", StatusCode.Disconnected);
                        }

                        switch (_connStatus)
                        {
                            case StatusCode.Disabled:
                                if (Enabled == true)
                                {
                                    ChangeStatus("Enabled. Connecting..", StatusCode.Disconnected);
                                }
                                break;

                            case StatusCode.Disconnected:

                                if (Enabled)
                                {
                                    if (Connect())
                                    {
                                        String msg = string.Format("Primary {0} channel now ACTIVE", Name);
                                        ChangeStatus(msg, StatusCode.Active);
                                    }
                                    else
                                        _connStatus = StatusCode.Disconnected;
                                }
                                break;

                            case StatusCode.Connected:
                                // detect proper feed of data
                                if (DateTime.Now.Subtract(_lastDataReceived).TotalSeconds < _HBTReceiveDelay)
                                {
                                    String msg = string.Format("Receiving data");
                                    ChangeStatus(msg, StatusCode.ValidData);
                                }
                                break;

                            case StatusCode.ValidData:
                                // detect loss of proper feed of data
                                if (DateTime.Now.Subtract(_lastDataReceived).TotalSeconds >= _HBTReceiveDelay)
                                {
                                    String msg = string.Format("Not receiving data");
                                    ChangeStatus(msg, StatusCode.Connected);
                                    break;

                                }

                                // secondary channel check - should we take over?
                                if (primary != null && primary._connStatus != StatusCode.Active)
                                {
                                    String msg = string.Format("Primary channel {0} is not recieving valid data, channel {1} taking over.", primary.Name, Name);
                                    ChangeStatus(msg, StatusCode.Active);
                                }

                                // primary channel check - should we take over?
                                if (primary == null)
                                {
                                    String msg = string.Format("Primary channel {0} becoming active.", Name);
                                    ChangeStatus(msg, StatusCode.Active);
                                }

                                break;

                            case StatusCode.Active:
                                // detect loss of proper feed of data
                                if (DateTime.Now.Subtract(_lastDataReceived).TotalSeconds >= _HBTReceiveDelay)
                                {
                                    String msg = string.Format("Not receiving data");
                                    ChangeStatus(msg, StatusCode.Connected);
                                    break;
                                }

                                // secondary channel check, should we relinquish?
                                if (primary != null && primary._connStatus == StatusCode.Active)
                                {
                                    String msg = string.Format("Primary channel {0} is not recieving valid data, channel {1} taking over.", primary.Name, Name);
                                    ChangeStatus(msg, StatusCode.ValidData);
                                }

                                break;
                        }

                        // send heartbeats
                        if (_channel != null && _connStatus >= StatusCode.Connected && DateTime.Now.Subtract(_lastHeartbeatSent).TotalSeconds > _HBTSendDelay)
                        {
                            try
                            {
                                if (_channel.IsConnected)
                                {
                                    _channel.Send("0|||HBT|");
                                    _lastHeartbeatSent = DateTime.Now;
                                }
                            }
                            catch
                            {
                                Disconnect();
                                _connStatus = StatusCode.Disconnected;
                            }
                        }
                    }
                    catch 
                    {
                    }
                }
            }
            catch 
            {
            }
        }

        void ChangeStatus(String reason, StatusCode newStatus)
        {
            Logger.Write(reason, TraceEventType.Information, "Quest Channel " + Name);
            _connStatus = newStatus;
            ServiceStatus status = new ServiceStatus() { Instance = Name, ServiceName = "Quest", Status = _connStatus.ToString(), Reason = reason, Server = System.Net.Dns.GetHostName() };
            base.ServiceBusClient.Broadcast(status);
        }

        void connection_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Logger.Write(string.Format("Got disconnected"), TraceEventType.Information, "Quest Channel " + Name);
            Disconnect();
            _connStatus = StatusCode.Disconnected;
        }

        /// <summary>
        /// data has arrived fro the XC connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void connection_Data(object sender, DataEventArgs e)
        {
            Debug.Print(e.Data);
            ///reflect data if this channel is active
            if (IncomingData != null && _connStatus == StatusCode.Active && _isPrimary == true)
                IncomingData(this, e);

            ParseMessage(e.Data);

            XCInbound data = new XCInbound(e.Data, Name);

            base.ServiceBusClient.Broadcast(data);
        }

        private void SplitOut(ref Dictionary<string, int> target, string source)
        {
            string[] parts = source.Split('|');
            int i = 0;
            target = new Dictionary<string, int>();
            foreach (string p in parts)
            {
                if (!target.ContainsKey(p))
                    target.Add(p.ToLower(), i);
                i += 1;
            }
        }

        private void ParseMessage(string message)
        {
            try
            {
                string[] parts = message.Split('|');

                if (parts.Length < 3)
                    return;

                XCMessageHandler f = null;

                _handlers.TryGetValue(parts[3], out f);

                if (parts[3] != "HBT")
                {
                    _lastDataReceived = DateTime.Now;
                }

                if (_connStatus != StatusCode.Active)
                {
                    Logger.Write(string.Format("{0} (ignored): {1}", _connStatus, message), TraceEventType.Information, "Quest Channel " + Name);
                    return;
                }
                else
                {
                    Logger.Write(string.Format("{0} : {1}", _connStatus, message), TraceEventType.Information, "Quest Channel " + Name);
                }

                if (f != null)
                {
                    bool failed = false;
                    int i = 0;
                    for (i = 0; i < _retries; i++)
                    {
                        try
                        {
                            Logger.Write(string.Format("Processing: #{0} {1}", i, message), TraceEventType.Information, "Quest Channel " + Name);
                            f(parts);
                            break;
                        }
                        catch (Exception e)
                        {
                            Logger.Write(string.Format("#{0} {1}", i, e.ToString()), TraceEventType.Information, "Quest Channel " + Name);
                            failed = true;
                        }
                    }
                    if (failed)
                        Logger.Write(string.Format("Exit @ try #{0}/{1}", i, _retries), TraceEventType.Information, "Quest Channel " + Name);

                }
                else
                    Logger.Write(string.Format("Ignoring  : {0}", message), TraceEventType.Information, "Quest Channel " + Name);

            }
            catch 
            {
            }

        }

        private void HBTHandler(string[] parts)
        {
            //_lastDataReceived = DateTime.Now;
        }

        /// <summary>
        /// Just recieved OSB - reset the status
        /// </summary>
        /// <param name="parts"></param>
        private void OSBHandler(string[] parts)
        {
            // subscribe to messages
            Logger.Write(string.Format("Requesting subscription: " + _subscriptions), TraceEventType.Information, "Quest Channel " + Name);
            _channel.Send("0|||STM|" + _subscriptions);

            Logger.Write(string.Format("Resetting current status"), TraceEventType.Information, "Quest Channel " + Name);

            BeginDump update = new BeginDump();
            base.ServiceBusClient.Broadcast(update);

            Logger.Write(string.Format("Reset complete"), TraceEventType.Information, "Quest Channel " + Name);

        }


        private void DRHandler(string[] parts)
        {
            DeleteResource update = new DeleteResource() { Callsign = GetValueString("Callsign", _resformat, parts) };
            base.ServiceBusClient.Broadcast(update);
        }

        private void RSCHandler(string[] parts)
        {
            String callsign = GetValueString("Callsign", _rscformat, parts);
            String[] tm = GetValueString("Logoff", _rscformat, parts).Split(':');
            if (tm.Length == 6)
            {
                int[] n = new int[6];
                for (int i = 0; i < 6; i++)
                    int.TryParse(tm[i], out n[i]);

                DateTime logoff = new DateTime(n[0], n[1], n[2], n[3], n[4], n[5]);

                ResourceLogon update = new ResourceLogon() { Callsign = callsign, Logoff = logoff, Logon=DateTime.Now };
                base.ServiceBusClient.Broadcast(update);
            }
        }

        

        private void CLIHandler(string[] parts)
        {
            string serial = "";

            object o = GetValue("Serial", _cliformat, parts);
            if (o != null)
            {
                serial = Makeserial(o.ToString());
            }

            CloseIncident update = new CloseIncident() { Serial = serial };
            base.ServiceBusClient.Broadcast(update);

        }

        private void URHandler(string[] parts)
        {
            // "MsgId|Workstation||Callsign|Status|Type|Group|Easting|Northing|Speed|Direction|Skill|LastUpdate|MinsNotMoved|OffDuty|XRay|FleetNo|Sector|Station|Incident"

            //  MsgId|Workstation|||Callsign|ResourceType|Status|Easting|Northing||Incident||Speed|Direction|LastUpdate|FleetNo|Skill||Sector|Emergency|Target|Agency|Class|EventType
            //                         x         x          x       x       x         x         x      x         x        x        x      x     
            string serial = "";
            object o = GetValue("Incident", _resformat, parts);
            if (o != null)
            {
                serial = Makeserial(o.ToString());
            }

            var pos = GetPosition(_resformat, parts);

            ResourceUpdateRequest update = new ResourceUpdateRequest()
            {
                Resource = new QuestResource()
                { 
                    Callsign = GetValueString("Callsign", _resformat, parts),
                    ResourceType = GetValueString("ResourceType", _resformat, parts),
                    Status = GetValueString("Status", _resformat, parts),
                    Position = new  Common.Messages.GIS.LatLongCoord(pos.Longitude, pos.Latitude),
                    Speed = GetValueInt("Speed", _resformat, parts),
                    Course = GetValueInt("Direction", _resformat, parts),
                    Skill = GetValueString("Skill", _resformat, parts),
                    //LastUpdate = GetValueDate("LastUpdate", _resformat, parts),
                    FleetNo = GetValueString("FleetNo", _resformat, parts),
                    //Sector = GetValueString("Sector", _resformat, parts),
                    EventId = GetValueString("Incident", _resformat, parts),
                    Destination = GetValueString("Destination", _resformat, parts),
                    Agency = GetValueString("Agency", _resformat, parts),
                    EventType = GetValueString("EventType", _resformat, parts),
                },
                UpdateTime = DateTime.UtcNow
            };

            base.ServiceBusClient.Broadcast(update);

        }

        /// <summary>
        /// MsgId|Workstation|||Serial|IncidentType|Status|Easting|Northing|Complaint|Determinant|JobType|Location|Priority|Sector|Resources
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="parts"></param>
        /// <remarks></remarks>
        private void PIIHandler(string[] parts)
        {
            if (parts.Length < _incformat.Count)
            {
                return;
            }

            var pos = GetPosition(_incformat, parts);

            IncidentUpdateRequest update = new IncidentUpdateRequest()
            {
                Serial = GetValueString("Serial", _incformat, parts),
                Status = GetValueString("Status", _incformat, parts),
                Latitude = pos.Latitude,
                Longitude = pos.Longitude,
                IncidentType = GetValueString("IncidentType", _incformat, parts),
                Complaint = GetValueString("Complaint", _incformat, parts),
                Determinant = GetValueString("Determinant", _incformat, parts),
                Location = GetValueString("Location", _incformat, parts),
                Priority = GetValueString("Priority", _incformat, parts),
                Sector = GetValueString("Sector", _incformat, parts),
                Description = GetValueString("Description", _incformat, parts),
            };

            base.ServiceBusClient.Broadcast(update);
        }

        private string Makeserial(string serial)
        {

            string[] serialparts = serial.Split('-');

            if (serialparts.Length == 3)
            {
                System.DateTime dt = new System.DateTime(DateTime.Now.Year, Convert.ToInt32(serialparts[2]), Convert.ToInt32(serialparts[1]));
                if (dt > DateTime.Now)
                {
                    dt = new System.DateTime(DateTime.Now.Year - 1, Convert.ToInt32(serialparts[2]), Convert.ToInt32(serialparts[1]));
                }

                int serialno = 0;
                int.TryParse(serialparts[0], out serialno);


                serial = dt.ToString("ddMMyyyy") + "-" + serialno.ToString("0000");
            }


            if (serialparts.Length == 2)
            {
                int serialno = 0;
                int.TryParse(serialparts[1], out serialno);
                serial = serialparts[0] + "-" + serialno.ToString("0000");
            }

            return serial;

        }

        /// <summary>
        /// get E/N from the parameter stream and convert to a geometry. If E/N are not legal then
        /// return empty string
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        private string GetPositionGeometry(Dictionary<string, int> dict, string[] parts)
        {
            int eastingordinal = -1;
            int northingordinal = -1;
            dict.TryGetValue("easting", out eastingordinal);
            dict.TryGetValue("northing", out northingordinal);
            if (eastingordinal != -1 & northingordinal != -1)
            {
                String e = parts[eastingordinal];
                String n = parts[northingordinal];

                double ev = 0;
                double nv = 0;

                double.TryParse(e, out ev);
                double.TryParse(n, out nv);

                if (ev == 0 || nv == 0)
                    return "";
                else
                    return string.Format("POINT( {0} {1} )", e, n);
            }
            else
            {
                return "";
            }
        }

        private LatLng GetPosition(Dictionary<string, int> dict, string[] parts)
        {
            int eastingordinal = -1;
            int northingordinal = -1;
            dict.TryGetValue("easting", out eastingordinal);
            dict.TryGetValue("northing", out northingordinal);
            if (eastingordinal != -1 & northingordinal != -1)
            {
                String e = parts[eastingordinal];
                String n = parts[northingordinal];

                double ev = 0;
                double nv = 0;

                double.TryParse(e, out ev);
                double.TryParse(n, out nv);

                if (ev == 0 || nv == 0)
                    return new LatLng(0, 0);
                else
                {
                    var ll = LatLongConverter.OSRefToWGS84(ev, nv);
                    return ll;
                }
            }
            else
            {
                return new LatLng(0, 0);
            }
        }

        private int GetValueInt(string field, Dictionary<string, int> dict, string[] parts)
        {
            object o = GetValue(field, dict, parts);
            int i = 0;
            int.TryParse(o.ToString(), out i);
            return i;
        }

        private String GetValueString(string field, Dictionary<string, int> dict, string[] parts)
        {
            object o = GetValue(field, dict, parts);
            return o.ToString();
        }

        private DateTime GetValueDate(string field, Dictionary<string, int> dict, string[] parts)
        {
            object o = GetValue(field, dict, parts);
            DateTime k = DateTime.Now;
            DateTime.TryParse(o.ToString(), out k);
            return k;
        }

        private bool GetValueBool(string field, Dictionary<string, int> dict, string[] parts)
        {
            object o = GetValue(field, dict, parts);
            bool j = false;
            bool.TryParse(o.ToString(), out j);
            return j;
        }

        private object GetValue(string field, Dictionary<string, int> dict, string[] parts)
        {
            //' try and find out which column the value is in
            int ordinal = -1;
            dict.TryGetValue(field.ToLower(), out ordinal);
            if (parts.GetLowerBound(0) <= ordinal & parts.GetUpperBound(0) > ordinal)
            {
                return parts[ordinal];
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// process generic commands.. toggle by subtype
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="parts"></param>
        /// <remarks></remarks>
        private void GenericHandler(string[] parts)
        {
            if (parts.Length < 7)
            {
                return;
            }

            string wksta = parts[1];
            string subtype = parts[4];
            double e = 0;
            double n = 0;

            double.TryParse(parts[6], out e);
            double.TryParse(parts[7], out e);

            string key = makeKey(wksta, subtype);

            if (Genericsubscriptions.ContainsKey(key))
            {
                GenericDeletions.Add(key, Genericsubscriptions[key]);
                Genericsubscriptions.Remove(key);
            }
            else
            {
                Genericsubscriptions.Add(key, new GenericSubscription
                {
                    wksta = wksta,
                    subtype = subtype,
                    e = e,
                    n = n
                });
            }

        }

        private string makeKey(string workstation, string subtype)
        {
            return workstation + "-" + subtype;
        }

        /// <summary>
        /// get a list of Generic subscriptions
        /// </summary>
        /// <param name="subtype"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public System.Collections.Generic.List<string> GetGenericSubscriptions(string subtype)
        {
            List<string> results = new List<string>();
            foreach (GenericSubscription cc in Genericsubscriptions.Values)
            {
                if (cc.subtype == subtype)
                {
                    results.Add(cc.wksta);
                }
            }
            return results;
        }

        public List<string> GetGenericDeletions(string subtype)
        {
            List<string> results = new List<string>();
            foreach (GenericSubscription cc in GenericDeletions.Values)
            {
                if (cc.subtype == subtype)
                {
                    results.Add(cc.wksta);
                }
            }

            GenericDeletions.Clear();

            return results;
        }

        /// <summary>
        /// get a single workstation subscription record
        /// </summary>
        /// <param name="workstation"></param>
        /// <param name="subtype"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public IGenericSubscription GetGenericSubscription(string workstation, string subtype)
        {
            string key = makeKey(workstation, subtype);
            if (Genericsubscriptions.ContainsKey(key))
            {
                return Genericsubscriptions[key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// holds a single generic subscription
        /// </summary>
        /// <remarks></remarks>
        public class GenericSubscription : IGenericSubscription
        {

            private string _wksta;
            private string _subtype;
            private double _e;

            private double _n;

            public double e
            {
                get { return _e; }
                set { _e = value; }
            }

            public double n
            {
                get { return _n; }
                set { _n = value; }
            }

            public string subtype
            {
                get { return _subtype; }
                set { _subtype = value; }
            }

            public string wksta
            {
                get { return _wksta; }
                set { _wksta = value; }
            }
        }

        /// <summary>
        /// Keep a list of registered workstations that want the heatmap
        /// </summary>
        /// <remarks></remarks>

        private Dictionary<string, IGenericSubscription> Genericsubscriptions = new Dictionary<string, IGenericSubscription>();
        /// <summary>
        /// A list of deleteted subscriptions
        /// </summary>
        /// <remarks></remarks>

        private Dictionary<string, IGenericSubscription> GenericDeletions = new Dictionary<string, IGenericSubscription>();

    }
}


