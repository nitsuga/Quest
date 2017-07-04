using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Quest.Lib.Utils;
using Quest.Lib.Net;

namespace Quest.XC
{
    class XCReflector
    {
        private TcpipListener _channel;
        private String _name;
        private DateTime _lastHeartbeatReceived;
        private DateTime _lastHeartbeatSent;
        private Thread _worker;
        private int _HBTReceiveDelay = 60;
        private int _HBTSendDelay = 30;
        private ManualResetEvent _quiting = new ManualResetEvent(false);

        public void Initialise(ChannelController parent, string name)
        {
            Logger.Write(string.Format("Starting"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "XCReflector");
            _name = name;
            parent.IncomingData += new EventHandler<DataEventArgs>(parent_IncomingData);

            _HBTReceiveDelay = SettingsHelper.GetVariable("XC.HBTReceiveDelay", 60);
            _HBTSendDelay = SettingsHelper.GetVariable("XC.HBTSendDelay", 30);

            Worker();
            Logger.Write(string.Format("Started"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "XCReflector");
        }

        public void Stop()
        {
            try
            {
                Logger.Write(string.Format("Stopping"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "XCReflector");

                if (_channel != null)
                {
                    _quiting.Set();

                    _channel.CloseChannel();
                    _channel = null;
                }
                Logger.Write(string.Format("Stopped"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "XCReflector");

            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, LoggingPolicy.Policy.TracePolicy.ToString()))
                    throw;
            }
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

        /// <summary>
        /// parse incoming message - only record heartbeats
        /// </summary>
        /// <param name="message"></param>
        private void ParseMessage(string message)
        {
            try
            {
                string[] parts = message.Split('|');

                if (parts.Length < 3)
                    return;

                if (parts[3] == "HBT")
                {
                    _lastHeartbeatReceived = DateTime.Now;
                    return;
                }

            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, LoggingPolicy.Policy.TracePolicy.ToString()))
                    throw;
            }

        }

        /// <summary>
        /// channel got disconnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _channel_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Logger.Write(string.Format("XC Reflector channel disconnected {0}", e.Remoteconnection.Socket.ToString()), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
        }

        /// <summary>
        /// channel got connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _channel_Connected(object sender, ConnectedEventArgs e)
        {
            Logger.Write(string.Format("XC Reflector channel Connected {0}", e.RemoteTcpipConnection.Socket.ToString()), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
        }

        /// <summary>
        /// start heartbeat thread
        /// </summary>
        private void Worker()
        {
            _worker = new Thread(new ThreadStart(HeartbeatWorker));
            _worker.IsBackground = true;
            _worker.Name = "XC Reflector Worker - " + _name;
            _worker.Start();
        }

        /// <summary>
        /// send heartbeats periodically
        /// </summary>
        private void HeartbeatWorker()
        {
            bool enabled;

            while (true)
            {
                try
                {
                    // commanded to stop?
                    if (_quiting.WaitOne(1000))
                        return;

                    enabled = SettingsHelper.GetVariable( "XCReflector.Enabled", false);
                    int port = SettingsHelper.GetVariable( "XCReflector.Port", 3080);

                    // shut down?
                    if (!enabled && _channel != null)
                    {
                        Logger.Write(string.Format("XC Reflector disabled", port), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
                        _channel.CloseChannel();
                        _channel = null;
                        continue;
                    }

                    // start up
                    if (enabled && _channel == null)
                    {
                        _channel = new TcpipListener();
                        // start the master listener
                        STXStreamCodec codec = new STXStreamCodec();
                        _channel.Connected += new EventHandler<ConnectedEventArgs>(_channel_Connected);
                        _channel.Disconnected += new EventHandler<DisconnectedEventArgs>(_channel_Disconnected);
                        _channel.Data += new EventHandler<DataEventArgs>(_channel_Data);
                        _channel.StartListening(codec.GetType(), port);
                        Logger.Write(string.Format("XC Reflector started on port {0}", port), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _name);
                        continue;
                    }
                    
                    // send heartbeats
                    if (_channel!=null && DateTime.Now.Subtract(_lastHeartbeatSent).TotalSeconds > _HBTSendDelay)
                    {
                        _channel.Send("0|||HBT|");
                        _lastHeartbeatSent = DateTime.Now;
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
