using System;
using System.Diagnostics;
using www.aspect.com.unifiedip.edk.ctipsapi._2009._08;
using www.aspect.com.unifiedip.edk.commondata._2009._08;
using System.Timers;
using Quest.Lib.Trace;
using Quest.Common.Messages;
using System.ServiceModel;

namespace Quest.Lib.Telephony.AspectCTIPS
{
    public class CTIPSChannel
    {
        public event System.EventHandler StatusChanged;

        #region Private Variables
        
        /// <summary>
        /// configuration of this channel
        /// </summary>
        public CTIChannelConfig Config;

        /// <summary>
        /// Host service that receives events
        /// </summary>
        private ServiceHost _host;

        private CTIEventHandler _eventHandler;

        /// <summary>
        /// Outbound connection
        /// </summary>
        private AgentCallService ClientProxy { get; set; }
        private IClientChannel ClientChannel { get; set; }
        private string SessionId { get; set; }

        private ICADChannel _cadChannel;

        private int _dialRequestId = 1;

        private bool _running = false;
       
        private Timer _heatbeattimer = new Timer();

        private Timer _connecttimer = new Timer();

        #endregion

        public bool IsPrimary { get; set; }

        public bool IsRunning 
        { 
            get
            {
                return _running;
            }
            set
            {
                if (_running!=value)
                {
                    _running=value;
                    if (StatusChanged!=null)
                        StatusChanged(this, null);
                }
            }
        }


        public override string ToString()
        {
            return Config == null ? "unconfigured channel" : Config.Name;
        }

        public void Initialise(CTIChannelConfig cfg, ICADChannel cadChannel)
        {
            Config = cfg;

            IsPrimary = false;

            _cadChannel = cadChannel;

            if (Config.HeartbeatTimeout == 0)
                Config.HeartbeatTimeout = 120;

            _heatbeattimer.Interval = Config.HeartbeatTimeout * 1000;
            _heatbeattimer.Elapsed += timer_Elapsed;
            _heatbeattimer.Stop();

            IsRunning = false;

            Logger.Write(string.Format("Channel {0} initialising ", this.ToString()), TraceEventType.Information, "CTIPSChannel");
        }

        void _cadChannel_SetData(object sender, SetDataRequest e)
        {
            try
            {
                if (IsPrimary && ClientProxy != null)
                {
                    Logger.Write(string.Format("Channel {0} Set Data call{1} {2}", this, e.callid, e.station, e.udf, e.data), TraceEventType.Information, "CTIPSChannel");

                    ClientProxy.SetCallData(new SetCallDataArgs()
                    {
                        SessionId = SessionId,
                        TenantId = Config.TenantId,
                        AgentIdx = _eventHandler.GetAgentID(e.station),
                        CallId = e.callid,
                        RequestId = -1,
                        UserData = new string[] { }
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Channel {0} Set Data Failed {1}", this, ex), TraceEventType.Information, "CTIPSChannel");
            }
        }

        public void Dial(MakeCall request)
        {
            try
            {
                if (IsPrimary && ClientProxy != null)
                {
                    if (request.Callee.StartsWith("9"))
                        request.Callee.Substring(1);

                    Logger.Write(string.Format("Channel {0} Executing MakeCall using {1} ", this, request), TraceEventType.Information, "CTIPSChannel");

                    var agent = _eventHandler.GetAgentID(request.Caller);

                    Logger.Write(string.Format("Channel {0} Executing MakeCall using {1} agent {2} TenantId={3}", this, request, agent, Config.TenantId), TraceEventType.Information, "CTIPSChannel");

                    int result = ClientProxy.MakeCall(
                        new MakeCallArgs()
                        {
                            TenantId = Config.TenantId,
                            AgentIdx = agent,
                            CallId = -1,
                            DestinationType = CTIDestinationType.External,
                            ServiceId = Config.DialoutServiceId,
                            WrapState = false,
                            SessionId = SessionId,
                            RequestId = _dialRequestId++,
                            DestinationAddress = request.Callee
                        }
                        );

                    Logger.Write(string.Format("Channel {0} Executed MakeCall using {1} result={2}", this, agent, request, result), TraceEventType.Information, "CTIPSChannel");
                }
                else
                    Logger.Write(string.Format("Channel {0} Ignored MakeCall using {1} as this channel is not primary", this, request), TraceEventType.Information, "CTIPSChannel");

            }
            catch(Exception ex)
            {
                Logger.Write(string.Format("Channel {0} Make Call Failed {1} with {2}", this, ex, request), TraceEventType.Information, "CTIPSChannel");
            }
        }

        public void Start()
        {
            if (Config.StartupDelay == 0)
                Config.StartupDelay = 1;
            _connecttimer.Interval = Config.StartupDelay * 1000;
            _connecttimer.Elapsed += _connecttimer_Elapsed;
            _connecttimer.Start();
        }

        void _connecttimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _connecttimer.Stop();

            if (!IsRunning)
                Connect();

            if (!IsRunning)
                _connecttimer.Start();
        }

        private void Connect()
        {
            IsRunning = false;

            if (!Config.Enabled)
            {
                Logger.Write(string.Format("Channel {0} will not start. not enabled.", this.ToString()), TraceEventType.Information, "CTIPSChannel");
                return;
            }

           Logger.Write(string.Format("Channel {0} starting", this.ToString()), TraceEventType.Information, "CTIPSChannel");

            try
            {
                StartListeningService();
                StartOutbound();
                RegisterForEvents();

                ClientProxy.OnLine(new OnLineArgs() { SessionId = SessionId, TenantId = Config.TenantId });

                GetCurrentAssociations();
            }
            catch(Exception ex)
            {
                _heatbeattimer.Stop();
                Logger.Write(string.Format("Channel {0} failed to start: {1}", this, ex), TraceEventType.Information, "CTIPSChannel");
                IsRunning = false;
                Stop();
                return;
            }

            _heatbeattimer.Start();
            IsRunning = true;

            Logger.Write(string.Format("Channel {0} heartbeat timer set to {1} milliseconds", this.ToString(), _heatbeattimer.Interval), TraceEventType.Information, "CTIPSChannel");
            Logger.Write(string.Format("Channel {0} heartbeat started", this.ToString()), TraceEventType.Information, "CTIPSChannel");
        }

        void GetCurrentAssociations()
        {
            Logger.Write(string.Format("Channel {0} Getting current agent mapping", this.ToString()), TraceEventType.Information, "CTIPSChannel");
            _eventHandler.FlushAgents();

            ClientProxy.Snapshot(
                new SnapshotArgs()
                {
                    SessionId = SessionId,
                    TenantId = Config.TenantId,
                    RequestId = 0,
                    EntityType = new CTIEntityType[] 
                        {
                             CTIEntityType.AgentInfo
                        }
                });
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.Write(string.Format("Channel {0} heartbeat fired due to inactivity on channel!", this.ToString()), TraceEventType.Information, "CTIPSChannel");

            // oops - restart everything
            Stop();
            Start();
        }

        public void Stop()
        {
            Logger.Write(string.Format("Channel {0} stopping", this.ToString()), TraceEventType.Information, "CTIPSChannel");

            try
            {
                StopListeningService();
                StopOutbound();
            }
            catch(Exception ex)
            {
                Logger.Write(string.Format("Channel {0} failed to stopped: {1}", this.ToString(), ex), TraceEventType.Information, "CTIPSChannel");
            }
            finally
            {
                // this causes us to attempt to open the channel again.
                IsRunning = false;
            }
            Logger.Write(string.Format("Channel {0} stopped", this.ToString()), TraceEventType.Information, "CTIPSChannel");
        }

        internal int StartListeningService()
        {
            Logger.Write(string.Format("Channel {0} setting up listen service", this.ToString()), TraceEventType.Information, "CTIPSChannel");

            _eventHandler = new CTIEventHandler(this.ToString(), _cadChannel);
            _eventHandler.HeartbeatEvent += _eventHandler_HeartbeatEvent;

            string strURL = Config.ClientCallbackURL;
            Uri baseAddress1 = new Uri(strURL);
            //_host = new ServiceHost(_eventHandler, baseAddress1);
            //WSHttpBinding mybinding = new WSHttpBinding();
            _host = new ServiceHost(_eventHandler, baseAddress1);
            BasicHttpBinding mybinding = new BasicHttpBinding();
            //mybinding.Security.Mode = SecurityMode.None;
            //mybinding.ReliableSession.Enabled = false;
            //mybinding.ReliableSession.Ordered = false;
            //mybinding.ReliableSession.InactivityTimeout = TimeSpan.MaxValue;
            mybinding.ReceiveTimeout = TimeSpan.MaxValue;
            _host.AddServiceEndpoint(typeof(CTIEventService), mybinding, baseAddress1);

            try
            {
                _host.Open();

            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Channel {0} error starting channel {1}", this.ToString(), ex.ToString()), TraceEventType.Information, "CTIPSChannel");
                throw;
            }
            Logger.Write(string.Format("Channel {0} listen service established", this.ToString()), TraceEventType.Information, "CTIPSChannel");
            return 0;
        }

        void _eventHandler_HeartbeatEvent(object sender, EventArgs e)
        {
            Logger.Write(string.Format("Channel {0} got heartbeat from CTI Server, timer reset", this.ToString()), TraceEventType.Information, "CTIPSChannel");
            // reset our timer..
            _heatbeattimer.Stop();
            _heatbeattimer.Start();
        }

        internal int StopListeningService()
        {
            Logger.Write(string.Format("Channel {0} stopping client connection", this.ToString()), TraceEventType.Information, "CTIPSChannel");
            if (_host != null)
            {
                try
                {
                    _host.Close();
                }
                catch (Exception ex)
                {
                    _host.Abort();
                    Logger.Write(string.Format("Channel {0} error stopping channel {1}", this.ToString(), ex.ToString()), TraceEventType.Information, "CTIPSChannel");
                }
                _host = null;
                return 0;
            }
            return 0;
        }

        /// <summary>
        /// initialise an outbound link to the CTI server
        /// </summary>
        public void StartOutbound()
        {
            Logger.Write(string.Format("Channel {0} starting client connection to CTI server @ {1}", this.ToString(), Config.CTIPortalServiceAddress), TraceEventType.Information, "CTIPSChannel");

            // Setup the binding and address info to create a CTIPS client connection
            // for now we're modifing a standard WSHttpBinding so it will stay open 
            // for ever without any activity (timeout = System.TimeSpan.MaxValue)
            // and were using a Reliable Ordered session to preserve message ordering 
            // WSHttpBinding binding = new WSHttpBinding();
            BasicHttpBinding binding = new BasicHttpBinding();


            if (Config.CTIPortalServiceAddress.StartsWith("http:"))
            {
                binding.ReceiveTimeout = System.TimeSpan.MaxValue;         // d.hh:mm:ss.ff
                //binding.ReliableSession.Enabled = false;
                //binding.ReliableSession.Ordered = false;
                binding.Security.Mode =  BasicHttpSecurityMode.None;
                //binding.ReliableSession.InactivityTimeout = System.TimeSpan.MaxValue;
                binding.ReceiveTimeout = TimeSpan.MaxValue;
            }
            else if (Config.CTIPortalServiceAddress.StartsWith("https:"))
            {
                binding.Security.Mode = BasicHttpSecurityMode.Transport; // SecurityMode.Transport;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                //binding.Security.Transport.Realm = "";
                //binding.ReliableSession.Enabled = false;
                //binding.ReliableSession.Ordered = false;
                //binding.ReliableSession.InactivityTimeout = TimeSpan.MaxValue;
                binding.ReceiveTimeout = TimeSpan.MaxValue;
            }

            EndpointAddress remoteAddress = new EndpointAddress(Config.CTIPortalServiceAddress);

            Logger.Write(string.Format("Channel {0} Creating channel factory: remote address={1}", this, remoteAddress), TraceEventType.Information, "CTIPSChannel");
            var factory = new ChannelFactory<AgentCallService>(binding, remoteAddress);

            Logger.Write(string.Format("Channel {0} Using factory to create channel", this), TraceEventType.Information, "CTIPSChannel");
            ClientChannel = (IClientChannel)factory.CreateChannel();
            ClientProxy = (AgentCallService)ClientChannel;

            OpenConnectionArgs ocArgs = new OpenConnectionArgs();

            ocArgs.ClientCallbackURL = Config.ClientCallbackURL;
            ocArgs.TenantIds = new int[1];
            ocArgs.TenantIds[0] = Config.TenantId;
            ocArgs.UserCredentials = new UserCredentials();
            ocArgs.UserCredentials.UserName = Config.UserName;
            ocArgs.UserCredentials.Password = Config.Password;

            SessionObject so = new SessionObject();

            try
            {
                Logger.Write(string.Format("Channel {0} Opening connection via channel: user={1} password={2} tenant={3} callback={4}", this, Config.UserName, Config.Password, Config.TenantId, Config.ClientCallbackURL), TraceEventType.Information, "CTIPSChannel");
                so = ClientProxy.OpenConnection(ocArgs);
                Logger.Write(string.Format("Channel {0} Opened connection session id={1}", this, so.SessionId), TraceEventType.Information, "CTIPSChannel");
                SessionId = so.SessionId;

            }
            catch (FaultException<CTIPortalServiceFault> fe)
            {
                Logger.Write(string.Format("Channel {0} client connection failed: {1}", this.ToString(), fe.ToString()), TraceEventType.Information, "CTIPSChannel");

                throw new Exception(fe.Message);
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Channel {0} client connection failed: {1}", this.ToString(), ex.ToString()), TraceEventType.Information, "CTIPSChannel");

                ClientChannel = null;
                ClientProxy = null;

                throw new Exception(ex.InnerException.Message);
            }

            Logger.Write(string.Format("Channel {0} client connection established with session id {1}", this.ToString(), SessionId), TraceEventType.Information, "CTIPSChannel");

        }

        /// <summary>
        /// Tell the CTI system that we want to be sent various events
        /// </summary>
        public void RegisterForEvents()
        {
            Logger.Write(string.Format("Channel {0} registering for \"agent\" events", this.ToString()), TraceEventType.Information, "CTIPSChannel");
            AddMessageClassArgs msgArgs = new AddMessageClassArgs();
            msgArgs.SessionId = SessionId;
            msgArgs.TenantId = Config.TenantId;
            msgArgs.MessageClass = CTIMessageClass.EventCategoryAgent;
            ClientProxy.AddMessageClass(msgArgs);

            Logger.Write(string.Format("Channel {0} registering for \"call\" events", this.ToString()), TraceEventType.Information, "CTIPSChannel");
            msgArgs.MessageClass = CTIMessageClass.EventCategoryCall;
            ClientProxy.AddMessageClass(msgArgs);
        
            Logger.Write(string.Format("Channel {0} registering for \"route\" events", this.ToString()), TraceEventType.Information, "CTIPSChannel");
            msgArgs.MessageClass = CTIMessageClass.EventCategoryRoute;
            ClientProxy.AddMessageClass(msgArgs);
        
            Logger.Write(string.Format("Channel {0} registering for \"system\" events", this.ToString()), TraceEventType.Information, "CTIPSChannel");
            msgArgs.MessageClass = CTIMessageClass.EventCategorySystem;
            ClientProxy.AddMessageClass(msgArgs);
        }

        /// <summary>
        /// stop the link to the external CTI server
        /// </summary>
        public void StopOutbound()
        {
            try
            {
                if (ClientProxy != null)
                {
                    Logger.Write(string.Format("Channel {0} stopping client connection to CTI server", this.ToString()), TraceEventType.Information, "CTIPSChannel");
                    CloseConnectionArgs cca = new CloseConnectionArgs();
                    cca.SessionId = SessionId;
                    ClientProxy.CloseConnection(cca);
                }

                if (ClientChannel != null)
                    ClientChannel.Close();
            }
            catch
            {
                if (ClientChannel != null)
                    ClientChannel.Abort();
            }
            finally
            { 
                ClientChannel = null;
                ClientProxy = null;
            }
        }
    }
}
