using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System.Threading;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using HEMSLink.Message;
using Quest.Lib;
using Quest.Lib.Net;
using Quest.Lib.Utils;

namespace Quest.HEMSLinkTest
{
    public class HEMSLinkServer
    {
        private DataChannel _channel;
        private Thread _worker;
        private ManualResetEvent _quiting = new ManualResetEvent(false);
        private int _port;
        private Type[] knownMessageTypes = new Type[] { typeof(EventUpdate), typeof(Logon) };
        internal event System.EventHandler<HEMSEventArgs> NewMessage;

        public bool Enabled { get; set; }

        private bool Listen { get; set; }
        public String Host { get; set; }

        public void Initialise(int port, bool enabled, String host=null)
        {
            Host = host;
            Listen = host==null;
            Enabled = enabled;
            _port = port;
            Logger.Write(string.Format("Starting"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "HEMS Link");
            Worker();
            Logger.Write(string.Format("Started"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "HEMS Link");
        }

        public bool IsConnected
        { 
            get
            {
                if (_channel == null)
                    return false;
                else
                    return _channel.IsConnected;
            }
            set {}
        }



        /// <summary>
        /// recieved incoming data from the primary channel and send to connected clients
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void parent_IncomingData(object sender, DataEventArgs e)
        {
            if (_channel!=null)
                _channel.Send(e.Data);
        }

        /// <summary>
        /// processing incoming data from connected reflectors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _channel_Data(object sender, DataEventArgs e)
        {
            ParseMessage(e.Data);
        }

        String Object2JSON(MessageBase msg)
        {
            HEMSMessage obj = new HEMSMessage() {  MessageBody= msg  };
            using (MemoryStream stream1 = new MemoryStream())
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(HEMSMessage), knownMessageTypes);
                ser.WriteObject(stream1, obj);
                stream1.Position = 0;
                StreamReader sr = new StreamReader(stream1);
                return sr.ReadToEnd();
            }
        }

        HEMSMessage JSON2Object(String msg)
        {
            using (MemoryStream stream1 = new MemoryStream())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(msg);
                stream1.Write(bytes,0,bytes.Length);
                stream1.Position = 0;

                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(HEMSMessage), knownMessageTypes);
                return (HEMSMessage)ser.ReadObject(stream1);
            }
        }

        public void Send(MessageBase message)
        {
            _channel.Send(Object2JSON(message));
        }

        public void SendJSON(String message)
        {
            _channel.Send(message);
        }

        /// <summary>
        /// parse incoming message - only record heartbeats
        /// </summary>
        /// <param name="message"></param>
        private void ParseMessage(string message)
        {
            HEMSEventArgs evt = new HEMSEventArgs();
            try
            {
                evt.RawMessage=message;
                evt.HEMSMessage = JSON2Object(message);
            }
            catch (Exception ex)
            {
                evt.ErrorMessage = "Error decoding message: " + ex.Message;
            }
            
            if (NewMessage != null)
                NewMessage(this, evt);

        }

        /// <summary>
        /// channel got disconnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _channel_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Logger.Write(string.Format("HEMS link channel disconnected {0}", e.Remoteconnection.Socket.ToString()), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "HEMS Link");
        }

        /// <summary>
        /// channel got connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _channel_Connected(object sender, ConnectedEventArgs e)
        {
            Logger.Write(string.Format("HEMS link channel client connected "), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "HEMS Link");
        }

        /// <summary>
        /// start heartbeat thread
        /// </summary>
        private void Worker()
        {
            _worker = new Thread(new ThreadStart(HeartbeatWorker));
            _worker.IsBackground = true;
            _worker.Name = "HEMS Worker" ;
            _worker.Start();
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
                        Logger.Write(string.Format("HEMS link disabled", _port), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "HEMS Link");
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

                        Logger.Write(string.Format("HEMS Link started on port {0}", _port), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "HEMS Link");
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
}
