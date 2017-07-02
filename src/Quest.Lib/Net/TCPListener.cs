using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Quest.Lib.Trace;

namespace Quest.Lib.Net
{
    /// <summary>
    ///     Utilities for sending a command via tcp/ip to a remote service
    /// </summary>
    /// <remarks></remarks>
    public class TcpipListener : DataChannel, IDisposable
    {
        //** Half and hour in milliseconds
        private const int DEFAULTTIMEOUTMILLSECS = 1800000;
        //** Wait only a short while.
        private const int DEFAULT_RECEIVE_TIMEOUT = 0;

        private const int DEFAULT_SENDTIMEOUT = 5000;

        //** Defines a Codec to use to interpret the stream
        private Thread _listenThread;

        private TcpListener _masterSocket;

        private readonly Dictionary<Guid, RemoteTcpipConnection> ConnectionList =
            new Dictionary<Guid, RemoteTcpipConnection>();

        // To detect redundant calls
        private bool disposedValue;

        public Guid Queue { get; set; }

        public Type CodecType { get; set; }

        //** Raised when the data is recieved
        public event EventHandler<DataEventArgs> Data;

        //** Raised when the link is connected 
        public event EventHandler<ConnectedEventArgs> Connected;

        //** Raised when the link is disconnected 
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        ///     send data down all listening connections
        /// </summary>
        /// <param name="data"></param>
        public void Send(string data)
        {
            try
            {
                foreach (var conn in ConnectionList.Values)
                {
                    try
                    {
                        Logger.Write($"Sending to {conn.ConnectionId} data={data}",
                            TraceEventType.Information, "TCP/IP");
                        conn.Send(data);
                    }
                    catch
                    {
                        OnDisconnect(conn);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        ///     send to specific list of clients
        /// </summary>
        /// <param name="data"></param>
        /// <param name="clients">Guids to send to</param>
        public void Send(string data, Guid[] clients)
        {
            foreach (var g in clients)
            {
                try
                {
                    RemoteTcpipConnection conn = null;
                    ConnectionList.TryGetValue(g, out conn);
                    if (conn != null)
                    {
                        try
                        {
                            Logger.Write($"Sending to {conn.ConnectionId} data={data}",
                                TraceEventType.Information, "TCP/IP");
                            conn.Send(data);
                        }
                        catch
                        {
                        }
                    }
                    else
                        Logger.Write($"{g} not known", 
                            TraceEventType.Information, "TCP/IP");
                }
                catch
                {
                }
            }
        }

        public void Send(byte[] data)
        {
            try
            {
                foreach (var conn in ConnectionList.Values)
                {
                    try
                    {
                        conn.Send(data);
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

        /// <summary>
        ///     Close the connection
        /// </summary>
        /// <remarks></remarks>
        public void CloseChannel()
        {
            var c = new RemoteTcpipConnection[ConnectionList.Values.Count];
            ConnectionList.Values.CopyTo(c, 0);

            foreach (var rtc in c)
                rtc.Close();

            //** Terminate the reading thread
            if (_listenThread != null)
            {
                _listenThread.Abort();
            }

            if (_masterSocket != null)
            {
                _masterSocket.Stop();
            }

            _masterSocket = null;
            Logger.Write("Master listening socket closed", TraceEventType.Information, "TCP Socket");
        }

        #region " IDisposable Support "

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        internal void OnData(object sender, string message, Guid connectionId)
        {
            var arg = new DataEventArgs(Queue, message, connectionId);
            if (Data != null)
            {
                Data(this, arg);
            }
        }

        internal void OnDisconnect(RemoteTcpipConnection remoteconnection)
        {
            //' remove from connection list
            if (ConnectionList.ContainsKey(remoteconnection.ConnectionId))
            {
                ConnectionList.Remove(remoteconnection.ConnectionId);
            }

            var arg = new DisconnectedEventArgs();
            arg.Remoteconnection = remoteconnection;
            arg.Queue = Queue;
            if (Disconnected != null)
            {
                Disconnected(remoteconnection, arg);
            }
        }

        internal void OnConnect(RemoteTcpipConnection remoteconnection)
        {
            var arg = new ConnectedEventArgs(remoteconnection, Queue);
            if (Connected != null)
            {
                Connected(remoteconnection, arg);
            }
        }

        public void StartListening(Type codecType, int IpPort)
        {
            CodecType = codecType;

            //** Create a socket to listen on
            _masterSocket = new TcpListener(IPAddress.Any, IpPort);
            _masterSocket.Start();

            //** asynchronous listen for connections
            _listenThread = new Thread(ListenWorker);
            _listenThread.Name = "TCPIP Listener - " + codecType.Name + " port " + IpPort;
            _listenThread.IsBackground = true;
            _listenThread.Start();

            Logger.Write(_listenThread.Name + " has started", GetType().Name);
        }

        public RemoteTcpipConnection GetRemoteConnection(Guid connectionId)
        {
            if (ConnectionList.ContainsKey(connectionId))
            {
                return ConnectionList[connectionId];
            }
            return null;
        }

        /// <summary>
        ///     Starts listening for incoming connections on the supplied port number.
        /// </summary>
        /// <remarks></remarks>
        private void ListenWorker()
        {
            do
            {
                try
                {
                    //** wait for an incoming socket connection
                    //** THIS BLOCKS
                    var socket = _masterSocket.AcceptSocket();

                    Logger.Write("Incoming connection accepted from " + socket.RemoteEndPoint, GetType().Name);

                    var connectionId = Guid.NewGuid();


                    //** and start off background process to handle the connection
                    var conn = new RemoteTcpipConnection(this, socket, CodecType, connectionId);

                    //** create a collection of RemoteTcpipConnection so we can 
                    //** look them up

                    ConnectionList.Add(connectionId, conn);

                    conn.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Caught exception in Listen worker @ :" + DateTime.Now);
                    Console.WriteLine(e.Message);
                }
                // Threading.Thread.Sleep(100)
            } while (true);
        }

        /// <summary>
        ///     IDisposable
        /// </summary>
        /// <param name="disposing"></param>
        /// <remarks></remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CloseChannel();
                }
                CloseChannel();
            }
            disposedValue = true;
        }

        #region TCPchannel Members

        public DisconnectAction ActionOnDisconnect { get; set; }

        public bool IsConnected
        {
            get
            {
                // always return true
                return true;
            }
        }

        #endregion
    }

    /// base class for a client for listener socket connection
    public interface CodecSocket
    {
        void Send(string data);
        void Send(byte[] data);
    }

    /// <summary>
    ///     represent a single remote connection
    /// </summary>
    /// <remarks></remarks>
    public class RemoteTcpipConnection : CodecSocket, IDisposable
    {
        private const int MAXBUF = 32768;
        private readonly TcpipListener _parent;
        private string _remoteAddress;
        private Socket _socket;
        private Thread _socketListenerWorker;

        // To detect redundant calls
        private bool disposedValue;
        private ICodec withEventsField__codec;

        public RemoteTcpipConnection(TcpipListener parent, Socket socket, Type codecType, Guid connectionId)
        {
            //create a codec to interpret the stream/
            _codec = (ICodec) Activator.CreateInstance(codecType);
            ConnectionId = connectionId;
            _parent = parent;
            Socket = socket;
        }

        public Guid ConnectionId { get; }

        private ICodec _codec
        {
            get { return withEventsField__codec; }
            set
            {
                if (withEventsField__codec != null)
                {
                    withEventsField__codec.DataReceived -= CodecDataReceived;
                    withEventsField__codec.DataToSend -= oCodec_DataToSend;
                }
                withEventsField__codec = value;
                if (withEventsField__codec != null)
                {
                    withEventsField__codec.DataReceived += CodecDataReceived;
                    withEventsField__codec.DataToSend += oCodec_DataToSend;
                }
            }
        }

        public bool IsConnected
        {
            get { return _socket.Connected; }
        }

        public ICodec Codec
        {
            get { return _codec; }
            set { _codec = value; }
        }

        public Socket Socket
        {
            get { return _socket; }
            set
            {
                _socket = value;
                if (_socket.RemoteEndPoint != null)
                {
                    _remoteAddress = _socket.RemoteEndPoint.ToString();
                }
            }
        }

        //**------------------------------------------------------------------------
        //** When in client mode tis sends data but you must call Open first
        public void Send(string data)
        {
            //** send data using the Codec
            if (_codec == null)
            {
                throw new TcpipConnectionException("No codec");
            }

            _codec.Send(this, data);
        }

        public void Send(byte[] data)
        {
            //** send data using the Codec
            if (_codec == null)
            {
                throw new TcpipConnectionException("No Codec");
            }

            _codec.Send(this, data);
        }

        #region " IDisposable Support "

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void Start()
        {
            //** Start listening for incoming data on a new thread
            _socketListenerWorker = new Thread(SocketListener);
            _socketListenerWorker.IsBackground = true;
            _socketListenerWorker.Start();
            _parent.OnConnect(this);
        }

        public override string ToString()
        {
            return _remoteAddress;
        }

        /// <summary>
        ///     this gets called by the Codec when it has reeived enough data for
        ///     a single packet. We stop reading data at this point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void CodecDataReceived(object sender, DataReceivedEventArgs e)
        {
            _parent.OnData(sender, e.Message, ConnectionId);
        }

        /// <summary>
        ///     Gets calls when the Codec whats to send data - it would be compressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void oCodec_DataToSend(object sender, DataToSendEventArgs e)
        {
            try
            {
                var connection = (RemoteTcpipConnection) sender;
                if (connection.Socket.Connected)
                {
                    connection.Socket.Send(e.GetMessage());
                }
            }
            catch (SocketException ex)
            {
                Logger.Write("socket read error " + ex.Message + " on " + Socket.RemoteEndPoint, 
                    TraceEventType.Error, "TCP Socket");

                Close();
            }
        }

        /// <summary>
        ///     worker that listens for incoming data. Gets passed the socket to listen on
        ///     when the connection is established.
        /// </summary>
        /// <remarks></remarks>
        private void SocketListener()
        {
            try
            {
                do
                {
                    //** Start reading from the socket
                    var buffer = new byte[MAXBUF + 1];
                    if (Socket.Connected == false)
                    {
                        return;
                    }
                    var bytesRead = Socket.Receive(buffer);
                    if (bytesRead == 0)
                    {
                        return;
                    }
                    _codec.Receive(this, buffer, bytesRead);
                } while (true);
            }
            catch (SocketException ex)
            {
                Logger.Write("socket read error " + ex.Message + " on " + Socket.RemoteEndPoint, 
                    TraceEventType.Error, "TCP Socket");
            }
            finally
            {
                if (!disposedValue)
                {
                    _parent.OnDisconnect(this);
                    Dispose();
                }
            }
        }

        public void Close()
        {
            _socketListenerWorker.Abort();
            _socketListenerWorker.Join(1000);
            Logger.Write("socket closed " + Socket.RemoteEndPoint, TraceEventType.Information,
                "TCP Socket");
            _parent.OnDisconnect(this);
            Dispose();
        }

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_socket.Connected)
                    {
                        _socket.Close();
                    }
                }
            }
            disposedValue = true;
        }
    }
}