using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using QuestXC.Properties;
using Microsoft.VisualBasic;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Collections.Specialized;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System.Data.SqlClient;
using MessageBroker.Objects;
using Quest.Lib.Utils;
using Quest.Lib.Net;

namespace Quest.XC
{
    public interface IXCMessageParser
    {
        List<string> GetGenericSubscriptions(string subtype);
        List<string> GetGenericDeletions(string subtype);
    }

    public interface IGenericSubscription
    {
        string wksta { get; set; }
        string subtype { get; set; }
        double e { get; set; }
        double n { get; set; }
    }

    public class XCConnector : IXCMessageParser
    {
        public enum StatusCode
        {
            Disabled,
            Disconnected,
            Connected,
            ValidData,
            Active
        }

        public event System.EventHandler<DataEventArgs> IncomingData;

        private System.Collections.Generic.Dictionary<string, int> _incformat = new System.Collections.Generic.Dictionary<string, int>();
        private System.Collections.Generic.Dictionary<string, int> _resformat = new System.Collections.Generic.Dictionary<string, int>();
        private System.Collections.Generic.Dictionary<string, int> _cliformat = new System.Collections.Generic.Dictionary<string, int>();
        private System.Collections.Generic.Dictionary<string, int> _drformat = new System.Collections.Generic.Dictionary<string, int>();
        private System.Collections.Generic.Dictionary<string, int> _rscformat = new System.Collections.Generic.Dictionary<string, int>();
        
        private System.Collections.Generic.Dictionary<string, MessageHandler> _handlers;

        private delegate void MessageHandler(string[] parts);

        private int _HBTReceiveDelay = 60;
        private int _HBTSendDelay = 30;
        private DateTime _lastDataReceived;
        private DateTime _lastHeartbeatSent;
        private Thread worker;
        private string _name;
        private StatusCode _status = StatusCode.Disconnected;
        private ChannelController _parent;
        private int _timeout = 3;
        private int _retries = 5;
        private String _subscriptions = "";

        private DataChannel _channel;

        private bool _isPrimary = false;
        private ManualResetEvent _quiting = new ManualResetEvent(false);
        private MessageHelper _msgSource;

        /// <summary>
        /// initialise the channel
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ConnectionString">database connection string to obtain settings</param>
        /// <param name="host">target host</param>
        /// <param name="port">target port</param>
        /// <param name="backupFor">act as backup for this channel</param>
        /// <param name="includeMessages">only process these messages</param>
        public void Initialise(ChannelController parent, string name, MessageHelper msgSource)
        {
            _msgSource = msgSource;
            _msgSource.NewMessage += new EventHandler<MessageBroker.NewMessageArgs>(_msgSource_NewMessage);
            _name = name;
            _parent = parent;
            _handlers = new System.Collections.Generic.Dictionary<string, MessageHandler>();

            StringCollection codes = new StringCollection();

            String cmdsString = SettingsHelper.GetVariable( _name + ".Commands", "");

            _subscriptions = SettingsHelper.GetVariable( _name + ".Subscriptions", "");
            _timeout = SettingsHelper.GetVariable( _name + ".SQLTimeout", 3);
            _retries = SettingsHelper.GetVariable( _name + ".SQLRetries", 5);

            codes.AddRange(cmdsString.Split('|'));


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

            string cliformat = SettingsHelper.GetVariable("XC.CliFormat", "");
            string incformat = SettingsHelper.GetVariable("XC.IncFormat", "");
            string resformat = SettingsHelper.GetVariable("XC.ResFormat", "");
            string spaformat = SettingsHelper.GetVariable("XC.SpaFormat", "");
            string spbformat = SettingsHelper.GetVariable("XC.SpbFormat", "");
            string lvmformat = SettingsHelper.GetVariable("XC.Lvm", "");
            string drformat =  SettingsHelper.GetVariable("XC.DrFormat", "");
            string rscformat = SettingsHelper.GetVariable("XC.RscFormat", "");

            _HBTReceiveDelay = SettingsHelper.GetVariable("XC.HBTReceiveDelay", 60);
            _HBTSendDelay = SettingsHelper.GetVariable("XC.HBTSendDelay", 30);

            SplitOut(ref _cliformat, incformat);
            SplitOut(ref _incformat, incformat);
            SplitOut(ref _resformat, resformat);
            SplitOut(ref _drformat, drformat);
            SplitOut(ref _rscformat, rscformat);

            Logger.Write("Inc format is " + incformat, LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
            Logger.Write("Res format is " + resformat, LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
            Logger.Write("Cli format is " + cliformat, LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
            Logger.Write("Dr  format is " + drformat,  LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
            Logger.Write("Rsc format is " + rscformat, LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
        }

        void _msgSource_NewMessage(object sender, MessageBroker.NewMessageArgs e)
        {
            try
            {
                // message from Quest Processor to write to XC
                XCOutbound instance1 = e.Payload as XCOutbound;
                
                // message from another XCReader 
                XCInbound instance2 = e.Payload as XCInbound;

                if (instance1 != null && _channel != null)
                {
                    // output to this channel?
                    if (instance1.channel.Split(',').Contains(_name))
                    {
                        Logger.Write(string.Format("XCOutbound {1} - sent {0}", instance1.command, _name), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
                        _channel.Send(instance1.command);
                    }
                }

                if (instance2 != null)
                { 
                }
            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, LoggingPolicy.Policy.TracePolicy.ToString()))
                    throw;
            }
        }

        /// <summary>
        /// start the channel
        /// </summary>
        public void Start()
        {
            worker = new Thread(new ThreadStart(Work));
            worker.IsBackground = true;
            worker.Name = "XC Worker - " + _name;
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

                Status = XCConnector.StatusCode.Disconnected;

            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, LoggingPolicy.Policy.TracePolicy.ToString()))
                    throw;
            }
        }

        /// <summary>
        /// is this channel connected to the remote end?
        /// </summary>
        public StatusCode Status
        {
            get
            {
                return _status;
            }
            private set
            {
                _status = value;
            }
        }

        /// <summary>
        /// connect to remote data source
        /// </summary>
        /// <returns></returns>
        private bool Connect()
        {
            // get connection settings.. these decide whether this is a server or client
            String connsettings = SettingsHelper.GetVariable( _name + ".Parameters", "");

            _channel = ChannelFactory.MakeTCPchannel(connsettings);
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

                        bool enabled = SettingsHelper.GetVariable( _name + ".Enabled", false);
                        String backupFor = SettingsHelper.GetVariable( _name + ".BackupFor", "");
                        _isPrimary = (backupFor == null || backupFor.Length == 0);
                        XCConnector primary = null;

                        if (enabled == false && Status == StatusCode.Disconnected)
                        {
                            continue;
                        }

                        if (enabled == false && Status != StatusCode.Disabled)
                        {
                            String msg = string.Format("Disabled. Disconnecting");
                            ChangeStatus(msg, StatusCode.Disabled);
                            Disconnect();
                            continue;
                        }

                        if (_channel == null)
                            Status = StatusCode.Disconnected;

                        // get backup details.
                        if (!_isPrimary)
                        {
                            // get the status of the primary from the parent                            
                            _parent.Channels.TryGetValue(backupFor, out primary);
                            if (primary == null)
                            {
                                Logger.Write(string.Format("Warning: No primary channel found called {0} that {1} is to act as backup for.", backupFor, _name), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
                                continue;
                            }
                        }

                        // did we get disconnected?
                        if (_channel != null && _channel.IsConnected == false)
                        {
                            Disconnect();
                            ChangeStatus("Disconnected", StatusCode.Disconnected);
                        }

                        switch (Status)
                        {
                            case StatusCode.Disabled:
                                if (enabled == true)
                                {
                                    ChangeStatus("Enabled. Connecting..", StatusCode.Disconnected);
                                }
                                break;

                            case StatusCode.Disconnected:

                                if (enabled)
                                {
                                    if (Connect())
                                    {
                                        if (_isPrimary)
                                        {
                                            String msg = string.Format("Primary {0} channel now ACTIVE", _name);
                                            ChangeStatus(msg, StatusCode.Active);
                                        }
                                        else
                                        {
                                            String msg = string.Format("Secondary {0} channel now PASSIVE", _name);
                                            ChangeStatus(msg, StatusCode.Connected);
                                        }
                                    }
                                    else
                                        Status = StatusCode.Disconnected;
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
                                if (primary != null && primary.Status != StatusCode.Active)
                                {
                                    String msg = string.Format("Primary channel {0} is not recieving valid data, channel {1} taking over.", primary._name, _name);
                                    ChangeStatus(msg, StatusCode.Active);
                                }

                                // primary channel check - should we take over?
                                if (primary == null)
                                {
                                    String msg = string.Format("Primary channel {0} becoming active.", _name);
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
                                if (primary != null && primary.Status == StatusCode.Active)
                                {
                                    String msg = string.Format("Primary channel {0} is not recieving valid data, channel {1} taking over.", primary._name, _name);
                                    ChangeStatus(msg, StatusCode.ValidData);
                                }

                                break;
                        }

                        // send heartbeats
                        if (_channel != null && Status >= StatusCode.Connected && DateTime.Now.Subtract(_lastHeartbeatSent).TotalSeconds > _HBTSendDelay)
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
                                Status = StatusCode.Disconnected;
                            }
                        }



                    }
                    catch (Exception ex2)
                    {
                        if (ExceptionPolicy.HandleException(ex2, LoggingPolicy.Policy.TracePolicy.ToString()))
                            throw;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, LoggingPolicy.Policy.TracePolicy.ToString()))
                    throw;
            }
        }

        void ChangeStatus(String reason, StatusCode newStatus)
        {
            Logger.Write(reason, LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
            Status = newStatus;
            ServiceStatus status = new ServiceStatus() { Instance = _name, ServiceName = "Quest", Status = Status.ToString(), Reason = reason, Server = System.Net.Dns.GetHostName() };
            _msgSource.BroadcastMessage(status);
        }

        void connection_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Logger.Write(string.Format("Got disconnected"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
            Disconnect();
            Status = StatusCode.Disconnected;
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
            if (IncomingData != null && Status == StatusCode.Active && _isPrimary == true)
                IncomingData(this, e);

            ParseMessage(e.Data);

            XCInbound data = new XCInbound(e.Data, _name);

            _msgSource.BroadcastMessage(data);
        }

        private void SplitOut(ref System.Collections.Generic.Dictionary<string, int> target, string source)
        {
            string[] parts = source.Split('|');
            int i = 0;
            target = new System.Collections.Generic.Dictionary<string, int>();
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

                MessageHandler f = null;

                _handlers.TryGetValue(parts[3], out f);

                if (parts[3] != "HBT")
                {
                    _lastDataReceived = DateTime.Now;
                }

                if (Status != StatusCode.Active)
                {
                    Logger.Write(string.Format("{0} (ignored): {1}", Status, message), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
                    return;
                }
                else
                {
                    Logger.Write(string.Format("{0} : {1}", Status, message), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
                }

                if (f != null)
                {
                    bool failed = false;
                    int i = 0;
                    for (i = 0; i < _retries; i++)
                    {
                        try
                        {
                            Logger.Write(string.Format("Processing: #{0} {1}", i, message), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
                            f(parts);
                            break;
                        }
                        catch (Exception e)
                        {
                            Logger.Write(string.Format("#{0} {1}", i, e.ToString()), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
                            failed = true;
                        }
                    }
                    if (failed)
                        Logger.Write(string.Format("Exit @ try #{0}/{1}", i, _retries), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);

                }
                else
                    Logger.Write(string.Format("Ignoring  : {0}", message), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);

            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, LoggingPolicy.Policy.TracePolicy.ToString()))
                    throw;
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
            Logger.Write(string.Format("Requesting subscription: " + _subscriptions), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
            _channel.Send("0|||STM|" + _subscriptions);

            Logger.Write(string.Format("Resetting current status"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);

            BeginDump update = new BeginDump();
            _msgSource.BroadcastMessage(update);

            Logger.Write(string.Format("Reset complete"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);

        }


        private void DRHandler(string[] parts)
        {
            DeleteResource update = new DeleteResource() { Callsign = GetValueString("Callsign", _resformat, parts) };
            _msgSource.BroadcastMessage(update);
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
                _msgSource.BroadcastMessage(update);
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
            _msgSource.BroadcastMessage(update);

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

            String pos = GetPositionGeometry(_resformat, parts);

            // valid ?
            if (pos == "")
            {
                Logger.Write(string.Format("Resource had invalid E/N - ignored"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
                return;
            }

            ResourceUpdate update = new ResourceUpdate()
                {
                    Callsign = GetValueString("Callsign", _resformat, parts),
                    ResourceType = GetValueString("ResourceType", _resformat, parts),
                    Status = GetValueString("Status", _resformat, parts),
                    Geometry = pos,
                    Speed = GetValueInt("Speed", _resformat, parts),
                    Direction = GetValueString("Direction", _resformat, parts),
                    Skill = GetValueString("Skill", _resformat, parts),
                    LastUpdate = GetValueDate("LastUpdate", _resformat, parts),
                    FleetNo = GetValueInt("FleetNo", _resformat, parts),
                    Sector = GetValueString("Sector", _resformat, parts),
                    Incident = GetValueString("Incident", _resformat, parts),
                    Emergency = GetValueString("Emergency", _resformat, parts),
                    Destination = GetValueString("Destination", _resformat, parts),
                    Agency = GetValueString("Agency", _resformat, parts),
                    Class = GetValueString("Class", _resformat, parts),
                    EventType = GetValueString("EventType", _resformat, parts)
                };

            _msgSource.BroadcastMessage(update);

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

            String pos = GetPositionGeometry(_incformat, parts);

            // valid ?
            if (pos == "")
            {
                Logger.Write(string.Format("Incident had invalid E/N - ignored"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
                return;
            }

            IncidentUpdate update = new IncidentUpdate()
            {
                Serial = GetValueString("Serial", _incformat, parts),
                Status = GetValueString("Status", _incformat, parts),
                Geometry = pos,
                IncidentType = GetValueString("IncidentType", _incformat, parts),
                Complaint = GetValueString("Complaint", _incformat, parts),
                Determinant = GetValueString("Determinant", _incformat, parts),
                Location = GetValueString("Location", _incformat, parts),
                Priority = GetValueString("Priority", _incformat, parts),
                Sector = GetValueString("Sector", _incformat, parts),
                Description = GetValueString("Description", _incformat, parts),
            };

            _msgSource.BroadcastMessage(update);
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
        private string GetPositionGeometry(System.Collections.Generic.Dictionary<string, int> dict, string[] parts)
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

        private int GetValueInt(string field, System.Collections.Generic.Dictionary<string, int> dict, string[] parts)
        {
            object o = GetValue(field, dict, parts);
            int i = 0;
            int.TryParse(o.ToString(), out i);
            return i;
        }

        private String GetValueString(string field, System.Collections.Generic.Dictionary<string, int> dict, string[] parts)
        {
            object o = GetValue(field, dict, parts);
            return o.ToString();
        }

        private DateTime GetValueDate(string field, System.Collections.Generic.Dictionary<string, int> dict, string[] parts)
        {
            object o = GetValue(field, dict, parts);
            DateTime k = DateTime.Now;
            DateTime.TryParse(o.ToString(), out k);
            return k;
        }

        private bool GetValueBool(string field, System.Collections.Generic.Dictionary<string, int> dict, string[] parts)
        {
            object o = GetValue(field, dict, parts);
            bool j = false;
            bool.TryParse(o.ToString(), out j);
            return j;
        }

        private object GetValue(string field, System.Collections.Generic.Dictionary<string, int> dict, string[] parts)
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


