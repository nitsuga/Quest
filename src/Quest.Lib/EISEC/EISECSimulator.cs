using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using Quest.Common.Messages;
using Quest.Lib.Trace;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Net;
using Quest.Common.ServiceBus;
using Quest.Lib.Utils;

// Simulates an EISEC server for testing purposes. It opens up a socket and listens for requests. When a client
// connects, it queues the request using the thread pool. The Connection Worker method gets executed by a pool thread
// The connection worker waits for requests and sends back responses until the client disconnects. 

namespace Quest.Lib.EISEC
{

    /// <summary>
    /// The main EISECSimulator class. It opens a socket and responds to EISEC requests.
    /// </summary>
    public class EisecSimulator : ServiceBusProcessor
    {
        private SimulatorConfig _config;
        private TcpListener _listener;
        private readonly List<TcpClient> _clients = new List<TcpClient>();
        private readonly StxStreamCodec _streamProcessor = new StxStreamCodec();
        private const string _name = "EISECSim";
        private int _pollTimeout = 0;
        private DateTime _lastMobile = DateTime.MinValue;

        public string EisecConfigFile { get; set; }


        public EisecSimulator(
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _streamProcessor.DataReceived += _streamProcessor_DataReceived;
            _streamProcessor.DataToSend += streamProcessor_DataToSend;
        }

        protected override void OnPrepare()
        {
            // ensure EisecConfigFile is set
            if (EisecConfigFile == null || EisecConfigFile.Length == 0)
                throw new ApplicationException("EisecConfigFile is not set");

            _config = Config.LoadConfig<SimulatorConfig>(EisecConfigFile);

            if (_config == null)
                throw new ApplicationException($"Could not load config file: {EisecConfigFile}");
        }

        protected override void OnStart()
        {
            Initialise();
        }

        private void Initialise() 
        {
            SetMessage("EISEC Simulator initialised");

            System.Threading.Thread worker = new System.Threading.Thread(ListenWorker);
            worker.IsBackground = true;
            worker.Start();
        }

        void streamProcessor_DataToSend(object sender, DataToSendEventArgs e)
        {
            EisecSimulator sim = (EisecSimulator)sender;
            foreach (TcpClient client in sim._clients)
            {
                NetworkStream stream = client.GetStream();
                byte[] bytes = e.GetMessage();
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private void _streamProcessor_DataReceived(object sender, Net.DataReceivedEventArgs e)
        {
            // Process the data sent by the client, a response packet is generated.
            string response = ProcessMessage(e.Message);

            if (!string.IsNullOrEmpty(response))
            {
                // convert response packet to bytes that we can send.
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(response);

                // Send back a response.
                _streamProcessor.Send(this, buffer);
            }
        }

        public bool Running { get; private set; }

        public bool IsDead { get; private set; }

        public static string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// stop listening.
        /// </summary>
        protected override void OnStop()
        {
            Stop();
        }

        void Stop()
        {
            Running = false;

            _listener?.Stop();

            foreach (TcpClient c in _clients)
            {
                try
                {
                    c.Close();
                }
                catch
                {
                    // ignored
                }
            }
            _clients.Clear();

            _listener = null;
        }

        /// <summary>
        /// worker thread that listens for connection requests
        /// </summary>
        private void ListenWorker()
        {
            try
            {
                Stop();

                if (_config==null)
                {
                    SetMessage($"No config loaded", TraceEventType.Error);
                }
                SetMessage($"Listening on port {_config.Port}");

                _listener = new TcpListener(System.Net.IPAddress.Any, _config.Port);
                _listener.Start();
                byte[] bytes = new byte[2048];

                Running = true;

                do
                {
                    // wait for and then accept incoming connections
                    TcpClient client = _listener.AcceptTcpClient();

                    SetMessage($"Accepted connection from {client.Client.RemoteEndPoint}");

                    // queue the connection request request
                    System.Threading.ThreadPool.QueueUserWorkItem(ConnectionWorker, client);

                } while (Running);

            }
            catch (SocketException ex1)
            {
                Running = false;
                Logger.Write("Simulator listening thread socket error: " + ex1.Message, TraceEventType.Information, "EISEC.Simulator " + _config.Port.ToString());
                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                // Standard application exeption handling
                Logger.Write("Simulator listening thread general error: " + ex.Message, TraceEventType.Information, "EISEC.Simulator " + _config?.Port.ToString());
                throw;
            }
            finally
            {
                Running = false;
            }
        }

        /// <summary>
        /// connection worker thread processes requests
        /// </summary>
        private void ConnectionWorker(Object stateInfo)
        {
            var client = (TcpClient)stateInfo;

            try
            {

                _clients.Add(client);

                byte[] bytes = new byte[2048];

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                SetMessage($"Waiting for data from {client.Client.RemoteEndPoint}");
                // Loop to receive all the data sent by the client.
                int i = stream.Read(bytes, 0, bytes.Length);

                while (i != 0)
                {
                    SetMessage($"Got {i} bytes from {client.Client.RemoteEndPoint}");

                    // process data
                    _streamProcessor.Receive(client, bytes, i);

                    // get the next packet
                    i = stream.Read(bytes, 0, bytes.Length);
                }

            }
            catch(Exception ex)
            {
                SetMessage($"Error in ConnectionWorker from {client.Client.RemoteEndPoint}: {ex}");
                // Standard application exeption handling
                //if (ExceptionPolicy.HandleException(ex, Policy.TracePolicy.ToString()))
                //    throw;
            }

            SetMessage($"ConnectionWorker from {client.Client.RemoteEndPoint} closed");
            client.Close();
            _clients.Remove(client);
        }

        /// <summary>
        /// test routine for processing a query request from an EISEC client
        /// </summary>
        /// <returns></returns>
        private string ProcessQuery(string request)
        {
            EisecAddressQueryReq reqPkt = new EisecAddressQueryReq();
            reqPkt.Deserialize(request);

            EisecAddressQueryResp resPkt;

            System.Threading.Thread.Sleep(2000);

            switch (reqPkt.Number)
            {
                case "02086691650":
                case "01":

                    resPkt = new EisecAddressQueryResp
                    {
                        Details = new CallLookupResponse
                        {
                            TelephoneNumber = reqPkt.Number,
                            Name = "BRITISH TELECOMMUNICATIONS PLC PCO",
                            Address =new[] { "KIOSK 673083 OPP THE DALE", "HIGH BEECHES",  "ORPINGTON", "",  "", "BR6 6EF"}
                        },
                        Request = reqPkt.Request
                    };

                    return resPkt.Serialize();

                case "07525626059":
                case "02":
                    var delay = (DateTime.Now - _lastMobile).TotalSeconds;

                    if (delay > 60)
                    {
                        // new call as last call was > 60 secs
                        resPkt = new EisecAddressQueryResp
                        {
                            Details = new CallLookupResponse
                            {
                                TelephoneNumber = reqPkt.Number,
                                Name = "*MOB* Vodafone  99 200912030023833 0 Searching        5 ",
                                Address  = new string[6]
                            },
                            Request = reqPkt.Request
                        };
                        _lastMobile = DateTime.Now;
                        return resPkt.Serialize();
                    }

                    if (delay < 10)
                    {
                        // request
                        resPkt = new EisecAddressQueryResp
                        {
                            Details = new CallLookupResponse
                            {
                                TelephoneNumber = reqPkt.Number,
                                Name = "*MOB* Vodafone  99 200912030023833 0 Data Available  10 ",
                                Address = new [] {
                                    "545447     163234     1500   1800  ",
                                    "66  112.6  OSGB36  2000  230 2   ",
                                    "Extent Headquarters",
                                    "24 HIGH BEECHES",
                                    "ORPINGTON",
                                    "BR6 6EF"
                                }
                            },
                            Request = reqPkt.Request
                        };
                        _lastMobile = DateTime.Now;
                        return resPkt.Serialize();
                    }


                    resPkt = new EisecAddressQueryResp
                        {
                            Details = new CallLookupResponse
                            {
                                TelephoneNumber = reqPkt.Number,
                                Name = "*MOB* Vodafone  99 200912030023833 0 Data Available    ",
                                Address = new [] {
                                    "545467     163364     0015   0015  ",
                                    "66  112.6  OSGB36  2000  230 2   ",
                                    "Extent Headquarters",
                                    "24 HIGH BEECHES",
                                    "ORPINGTON",
                                    "BR6 6EF"
                                }
                            },
                            Request = reqPkt.Request
                        };
                    _lastMobile = DateTime.MinValue;
                    return resPkt.Serialize();
            }
            return null;
        }

        // ReSharper disable once UnusedParameter.Local
        private static string ProcessLogoff(string request)
        {
            EisecLogoff resPkt = new EisecLogoff();
            return resPkt.Serialize();
        }

        public enum LogonMode
        {
            Accept,
            Reject,
            Grace
        }

        public LogonMode Logonmode= LogonMode.Accept;

        private string ProcessLogon(string request)
        {
            switch (Logonmode)
            {
                case LogonMode.Accept:
                    EisecLogonAccept resPkt = new EisecLogonAccept();
                    return resPkt.Serialize();
                case LogonMode.Grace:
                    EisecGraceLogon resPkt1 = new EisecGraceLogon();
                    resPkt1.GraceLogons = 5;
                    return resPkt1.Serialize();
                case LogonMode.Reject:
                    EisecLogonReject resPkt2 = new EisecLogonReject();
                    resPkt2.RejectCode = 5;
                    return resPkt2.Serialize();
            }
            return null;
        }

        private static string ProcessPasswordChange(string request)
        {
            EisecPasswordChgAccept resPkt = new EisecPasswordChgAccept();
            return resPkt.Serialize();
        }

        private string ProcessPoll(string request)
        {
            // send a poll straight back
            if (_pollTimeout > 0)
            {
                EisecPoll resPkt = new EisecPoll();
                return resPkt.Serialize();
            }
            else
            {
                return null;
            }
        }

        private string ProcessSetTimeout(string request)
        {
            EisecSettimeoutRequest reqPkt = new EisecSettimeoutRequest();
            reqPkt.Deserialize(request);
            _pollTimeout = reqPkt.Timeout;

            if (_pollTimeout > 32000 || _pollTimeout == 0)
            {
                EisecSettimeoutReject resPkt = new EisecSettimeoutReject();
                return resPkt.Serialize();
            }
            else
            {
                EisecSettimeoutAccept resPkt = new EisecSettimeoutAccept();
                return resPkt.Serialize();
            }
        }

        /// <summary>
        /// process a message and return a response
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private string ProcessMessage(string request)
        {
            string response = "";

            Logger.Write("Sim received '" + request + "'", TraceEventType.Information, "EISEC.Simulator " + _config.Port);

            if (IsDead)
            {
                Logger.Write("Sim ignoring request as appearing dead", TraceEventType.Information, "EISEC.Simulator " + _config.Port);
                return null;
            }

            switch (EisecPacket.GetPduType(request))
            {
                case constants.EisecQueryRequest:
                    response = ProcessQuery(request);
                    break;
                case constants.EisecLogoffRequest:
                    response = ProcessLogoff(request);
                    break;
                case constants.EisecLogonRequest:
                    response = ProcessLogon(request);
                    break;
                case constants.EisecPasswordChangeRequest:
                    response = ProcessPasswordChange(request);
                    break;
                case constants.EisecSettimeout:
                    response = ProcessSetTimeout(request);
                    break;
                case constants.EisecPoll:
                    response = ProcessPoll(request);
                    break;
            }

            Logger.Write("Sim responded '" + response + "'", TraceEventType.Information, "EISEC.Simulator " + _config.Port);

            return response;
        }
    }
}
