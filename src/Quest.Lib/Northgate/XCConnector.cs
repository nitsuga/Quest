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
using Autofac;

namespace Quest.Lib.Northgate
{

    /// <summary>
    /// Connects to XCConnect and broadcasts inbound XC traffic onto the ESB
    /// Also listens for XC Outbound ESB messages and sends on to CAD
    /// </summary>
    public partial class XCConnector : ServiceBusProcessor
    {
        /// <summary>
        /// Name of this channel configuration
        /// </summary>
        public string Channel { get; set; }

        private XCConfig _config;
        private ILifetimeScope _scope;

        private Dictionary<string, int> _incformat = new Dictionary<string, int>();
        private Dictionary<string, int> _resformat = new Dictionary<string, int>();
        private Dictionary<string, int> _cliformat = new Dictionary<string, int>();
        private Dictionary<string, int> _drformat = new Dictionary<string, int>();
        private Dictionary<string, int> _rscformat = new Dictionary<string, int>();
        
        private Dictionary<string, XCMessageHandler> _handlers;

        private delegate void XCMessageHandler(string[] parts);

        private DateTime _lastDataReceived;
        private DateTime _lastHeartbeatSent;
        private Thread worker;
        private ChannelStatus _status = ChannelStatus.Disconnected;
        private int _retries = 5;
        private DataChannel _datachannel;
        private ManualResetEvent _quiting = new ManualResetEvent(false);

        /// <summary>
        /// is this channel connected to the remote end?
        /// </summary>
        private ChannelStatus _connStatus;

        public XCConnector(
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
        }

        protected override void OnPrepare()
        {
            try
            {
                Logger.Write($"Loading configuration {Channel}", TraceEventType.Information);
                // get configuration
                _config = _scope.ResolveNamed<XCConfig>(Channel);
            }
            catch(Exception ex)
            {

            }

            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler<XCChannelControl>(XCChannelControlHandler);
            MsgHandler.AddHandler<XCOutbound>(XCOutboundHandler);


        }

        private Response XCChannelControlHandler(NewMessageArgs arg)
        {
            var msg = arg.Payload as XCChannelControl;
            if (msg!=null)
            {
                switch(msg.Action)
                {
                    case XCChannelControl.Command.Disable:
                        _config.ConnectionEnabled = false;
                        break;
                    case XCChannelControl.Command.EnableAsPrimary:
                        _config.ConnectionEnabled = true;
                        _config.EmitEnabled = true;
                        break;
                    case XCChannelControl.Command.EnableAsBackup:
                        _config.ConnectionEnabled = true;
                        _config.EmitEnabled = false;
                        break;
                }
            }
            return null;
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

            codes.AddRange(_config.Commands.Split('|'));

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

            SplitOut(ref _cliformat, _config.CliFormat);
            SplitOut(ref _incformat, _config.IncFormat);
            SplitOut(ref _resformat, _config.ResFormat);
            SplitOut(ref _drformat, _config.DrFormat);
            SplitOut(ref _rscformat, _config.RscFormat);

            Logger.Write("Inc format is " + _config.IncFormat, TraceEventType.Information, "Quest Channel " + _config.Name);
            Logger.Write("Res format is " + _config.ResFormat, TraceEventType.Information, "Quest Channel " + _config.Name);
            Logger.Write("Cli format is " + _config.CliFormat, TraceEventType.Information, "Quest Channel " + _config.Name);
            Logger.Write("Dr  format is " + _config.DrFormat,  TraceEventType.Information, "Quest Channel " + _config.Name);
            Logger.Write("Rsc format is " + _config.RscFormat, TraceEventType.Information, "Quest Channel " + _config.Name);
        }

        /// <summary>
        /// process XCOutbound command
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private Response XCOutboundHandler(NewMessageArgs t)
        {
            if (!_config.OutboundEnabled)
                return null;

            if (!_config.EmitEnabled)
                return null;

            XCOutbound instance1 = t.Payload as XCOutbound;

            if (instance1 != null && _datachannel != null)
            {
                // output to this channel?
                if (instance1.channel.Split(',').Contains(_config.Name))
                {
                    Logger.Write($"XCOutbound {instance1.command} - sent {_config.Name}", TraceEventType.Information, "Quest Channel " + _config.Name);
                    _datachannel.Send(instance1.command);
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
            worker.Name = $"XC Worker - {_config.Name}";
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

                _connStatus = ChannelStatus.Disconnected;

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

            _datachannel = ChannelFactory.MakeTcPchannel(_config.Connection);
            _datachannel.Data += new EventHandler<DataEventArgs>(connection_Data);
            _datachannel.ActionOnDisconnect = DisconnectAction.RaiseDisconnectEvent;
            _datachannel.Disconnected += connection_Disconnected;

            _lastDataReceived = DateTime.Now;
            _lastHeartbeatSent = DateTime.Now;

            return _datachannel.IsConnected;
        }

        /// <summary>
        /// disconnect from remote data source 
        /// </summary>
        private void Disconnect()
        {
            if (_datachannel != null)
            {
                if (_datachannel.IsConnected)
                    _datachannel.CloseChannel();
                
                _datachannel = null;
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

                        if (_config.ConnectionEnabled == false)
                        {
                            if (_connStatus == ChannelStatus.Disconnected)
                            {
                                String msg = string.Format("Disconnected");
                                ChangeStatus(msg, ChannelStatus.Disconnected);
                                continue;
                            }

                            if (_connStatus != ChannelStatus.Disabled)
                            {
                                String msg = string.Format("Disabled. Disconnecting");
                                ChangeStatus(msg, ChannelStatus.Disabled);
                                Disconnect();
                                continue;
                            }
                        }

                        if (_datachannel == null)
                            _connStatus = ChannelStatus.Disconnected;


                        // did we get disconnected?
                        if (_datachannel != null && _datachannel.IsConnected == false)
                        {
                            Disconnect();
                            ChangeStatus("Disconnected", ChannelStatus.Disconnected);
                        }

                        // channel is enabled..

                        switch (_connStatus)
                        {

                            case ChannelStatus.Disconnected:
                                // attempt to connect
                                if (Connect())
                                {
                                    String msg = string.Format("Primary {0} channel now CONNECTED", _config.Name);
                                    ChangeStatus(msg, ChannelStatus.Connected);
                                }
                                else
                                    _connStatus = ChannelStatus.Disconnected;
                                break;

                            case ChannelStatus.Connected:
                                // detect proper feed of data
                                if (DateTime.Now.Subtract(_lastDataReceived).TotalSeconds < _config.HBTReceiveDelay)
                                {
                                    String msg = string.Format("Receiving data");
                                    ChangeStatus(msg, ChannelStatus.Active);
                                }
                                break;

                            case ChannelStatus.Active:
                                // detect loss of proper feed of data
                                if (DateTime.Now.Subtract(_lastDataReceived).TotalSeconds >= _config.HBTReceiveDelay)
                                {
                                    String msg = string.Format("Not receiving data");
                                    ChangeStatus(msg, ChannelStatus.Connected);
                                    break;
                                }
                                break;
                        }

                        // send heartbeats
                        if (_datachannel != null && _connStatus >= ChannelStatus.Connected && DateTime.Now.Subtract(_lastHeartbeatSent).TotalSeconds > _config.HBTSendDelay)
                        {
                            try
                            {
                                if (_datachannel.IsConnected)
                                {
                                    _datachannel.Send("0|||HBT|");
                                    _lastHeartbeatSent = DateTime.Now;
                                }
                            }
                            catch
                            {
                                Disconnect();
                                _connStatus = ChannelStatus.Disconnected;
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

        void ChangeStatus(String reason, ChannelStatus newStatus)
        {
            Logger.Write(reason, TraceEventType.Information, "Quest Channel " + _config.Name);
            _connStatus = newStatus;

            XCChannelStatus xcstatus = new XCChannelStatus() { Channel = _config.Name, Status = _connStatus };
            ServiceBusClient.Broadcast(xcstatus);
        }

        void connection_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Logger.Write(string.Format("Got disconnected"), TraceEventType.Information, "Quest Channel " + _config.Name);
            Disconnect();
            _connStatus = ChannelStatus.Disconnected;
        }

        /// <summary>
        /// data has arrived fro the XC connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void connection_Data(object sender, DataEventArgs e)
        {
            ParseMessage(e.Data);

            if (!_config.EmitEnabled)
                return;

            XCInbound data = new XCInbound(e.Data, _config.Name);
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

                if (_connStatus != ChannelStatus.Active)
                {
                    Logger.Write(string.Format("{0} (ignored): {1}", _connStatus, message), TraceEventType.Information, "Quest Channel " + _config.Name);
                    return;
                }
                else
                {
                    Logger.Write(string.Format("{0} : {1}", _connStatus, message), TraceEventType.Information, "Quest Channel " + _config.Name);
                }

                if (f != null)
                {
                    bool failed = false;
                    int i = 0;
                    for (i = 0; i < _retries; i++)
                    {
                        try
                        {
                            Logger.Write(string.Format("Processing: #{0} {1}", i, message), TraceEventType.Information, "Quest Channel " + _config.Name);
                            f(parts);
                            break;
                        }
                        catch (Exception e)
                        {
                            Logger.Write(string.Format("#{0} {1}", i, e.ToString()), TraceEventType.Information, "Quest Channel " + _config.Name);
                            failed = true;
                        }
                    }
                    if (failed)
                        Logger.Write(string.Format("Exit @ try #{0}/{1}", i, _retries), TraceEventType.Information, "Quest Channel " + _config.Name);

                }
                else
                    Logger.Write(string.Format("Ignoring  : {0}", message), TraceEventType.Information, "Quest Channel " + _config.Name);

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
            Logger.Write(string.Format("Requesting subscription: " + _config.Subscriptions), TraceEventType.Information, "Quest Channel " + _config.Name);
            _datachannel.Send("0|||STM|" + _config.Subscriptions);

            Logger.Write(string.Format("Resetting current status"), TraceEventType.Information, "Quest Channel " + _config.Name);

            if (_config.EmitEnabled)
                ServiceBusClient.Broadcast(new BeginDump());

            Logger.Write(string.Format("Reset complete"), TraceEventType.Information, "Quest Channel " + _config.Name);

        }

        private void DRHandler(string[] parts)
        {
            if (!_config.EmitEnabled)
                return;

            DeleteResource update = new DeleteResource() { Callsign = GetValueString("Callsign", _resformat, parts) };
            base.ServiceBusClient.Broadcast(update);
        }

        private void RSCHandler(string[] parts)
        {
            if (!_config.EmitEnabled)
                return;

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
            if (!_config.EmitEnabled)
                return;

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
            if (!_config.EmitEnabled)
                return;

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
            if (!_config.EmitEnabled)
                return;

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
        private System.Collections.Generic.List<string> GetGenericSubscriptions(string subtype)
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

        private List<string> GetGenericDeletions(string subtype)
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
        private IGenericSubscription GetGenericSubscription(string workstation, string subtype)
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
