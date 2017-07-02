using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Quest.Lib.Net
{
    /// <summary>
    ///     Utilities for sending a command via tcp/ip to a remote service
    /// </summary>
    /// <remarks></remarks>
    public class TcpipConnection : DataChannel, IDisposable
    {
        //** Half and hour in milliseconds
        private const int Maxbuf = 4196;
        //** Wait only a short while.
        private const int DefaultReceiveTimeout = 0;
        private const int DefaultSendtimeout = 5000;
        private ReadCallbackDelegate _callback;

        //** Defines a Codec to use to interpret the stream
        private ICodec _codec;

        private Guid _queue;
        private Thread _readerThread;
        private string _remoteIpAddress;
        private int _remoteIpPort;
        private MyTcpClientDerivedClass _tcpClientA;

        public bool Break { get; set; }

        public DisconnectAction ActionOnDisconnect { get; set; } = DisconnectAction.ThrowError;

        //**------------------------------------------------------------------------
        //** When in client mode this sends data but you must call Open first
        public void Send(byte[] data)
        {
            try
            {
                //** send data using the Codec
                if (_codec == null)
                {
                    throw new TcpipConnectionException("No Codec");
                }


                if (!IsConnected)
                {
                    LinkBroken(null);
                    return;
                }

                _codec.Send(_tcpClientA, data);
            }
            catch (Exception ex)
            {
                LinkBroken(ex);
            }
        }

        public void Send(string packet)
        {
            try
            {
                //** send data using the Codec
                if (_codec == null)
                {
                    throw new TcpipConnectionException("No Codec");
                }


                if (!IsConnected)
                {
                    LinkBroken(null);
                    return;
                }

                _codec.Send(_tcpClientA, packet);
            }
            catch (Exception ex)
            {
                LinkBroken(ex);
            }
        }


        /// <summary>
        ///     clients doesn't make sense for client type TCP connections
        /// </summary>
        /// <param name="data"></param>
        /// <param name="clients"></param>
        public void Send(string data, Guid[] clients)
        {
            Send(data);
        }

        /// <summary>
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (_tcpClientA != null)
                {
                    return _tcpClientA.IsConnected;
                }
                return false;
            }
        }

        /// <summary>
        ///     close the connection and dispose of any unmanaged objects.
        /// </summary>
        public void CloseChannel()
        {
            try
            {
                //** Terminate the reading thread
                _readerThread?.Abort();

                if (_tcpClientA != null && _tcpClientA.IsConnected)
                {
                    _tcpClientA.Close();
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                _tcpClientA = null;
            }
        }

        protected void OnData(object sender, string message, Guid connectionId)
        {
            var arg = new DataEventArgs(_queue, message, connectionId);

            Data?.Invoke(sender, arg);
        }

        //** Use this to reconnect
        protected void Reconnect()
        {
            if (!IsConnected)
            {
                Connect(_codec.GetType(), _queue, _remoteIpAddress, _remoteIpPort);
            }
        }

        public override string ToString()
        {
            return $"{_remoteIpAddress}:{_remoteIpPort}";
        }

        /// <summary>
        ///     Creates a TCPClient using hostname and port.
        /// </summary>
        /// <param name="codecType"></param>
        /// <param name="queue">the qorkflow to send data back via</param>
        /// <param name="remoteIpAddress"></param>
        /// <param name="remoteIpPort"></param>
        /// <remarks></remarks>
        /// <returns>true if connected</returns>
        public bool Connect(Type codecType, Guid queue, string remoteIpAddress, int remoteIpPort)
        {
            try
            {
                _queue = queue;

                _codec = (ICodec) Activator.CreateInstance(codecType);

                _codec.DataReceived += CodecDataReceived;
                _codec.DataToSend += CodecDataToSend;


                if (_tcpClientA != null)
                {
                    CloseChannel();
                }

                _remoteIpAddress = remoteIpAddress;
                _remoteIpPort = remoteIpPort;

                _tcpClientA?.Close();

                _tcpClientA = new MyTcpClientDerivedClass(this)
                {
                    SendTimeout = DefaultSendtimeout,
                    ReceiveTimeout = DefaultReceiveTimeout,
                    LingerState = {Enabled = false}
                };
                //** five seconds to send the command
                //** five millsecs seconds to receive data

                _tcpClientA.Connect(remoteIpAddress, remoteIpPort);

                if (_tcpClientA.IsConnected)
                {
                    Connected?.Invoke(this, new ConnectedEventArgs(null, _queue));
                }

                ReadStream(null);
                return true;
            }
            catch
            {
                CloseChannel();
                return false;
            }
        }

        //** This is used by ReadResponse if you want to supply your own callback function
        //** for notification when a message arrives
        internal delegate void ReadCallbackDelegate(string message);

        #region "Private functions"

        private void ReadStream(ReadCallbackDelegate oCallback)
        {
            _callback = oCallback;
            _readerThread = new Thread(ReadStreamThread)
            {
                Name = "Tcp/ip Reader",
                IsBackground = true
            };
            _readerThread.Start();
        }

        /// <summary>
        ///     used when reading from the TCPClient - NOT the socket
        /// </summary>
        /// <remarks></remarks>
        private void ReadStreamThread()
        {
            var numberOfBytesRead = 0;

            try
            {
                if (!IsConnected)
                {
                    throw new TcpipConnectionException("Not Connected");
                }

                // Get a client stream for reading and writing.
                var stream = _tcpClientA.GetStream();

                var readBuffer = new byte[Maxbuf];

                //** Keep reading from the stream ad infinitum
                do
                {
                    retry:
                    try
                    {
                        //** This is to force the loop to break when attempting to shut down the gateway
                        //** Set by the Break property
                        if (!Break)
                        {
                            numberOfBytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch (IOException ex1)
                    {
                        //** Detect a timeout, which we ignore.
                        var exception = ex1.InnerException as SocketException;
                        if (exception != null)
                        {
                            var errorCode = exception.ErrorCode;

                            switch (errorCode)
                            {
                                case 10053:
                                    return;

                                case 10054:
                                    LinkBroken(null);
                                    return;

                                case 10060:
                                    goto retry;
                            }
                        }

                        if (ex1.InnerException is ThreadAbortException)
                        {
                            return;
                        }
                    }
                    catch
                    {
                        //** could be a timeout.
                        return;
                    }

                    //'** check for a disconnection
                    if (numberOfBytesRead == 0)
                    {
                        LinkBroken(null);
                        return;
                    }

                    //** Start receiving again
                    _codec.Receive(_tcpClientA, readBuffer, numberOfBytesRead);
                } while (true);
            }
            catch (ThreadAbortException)
            {
                //** Threadaborts are ignored
            }
        }

        private void LinkBroken(Exception ex)
        {
            switch (ActionOnDisconnect)
            {
                case DisconnectAction.RaiseDisconnectEvent:
                    try
                    {
                        CloseChannel();
                    }
                    catch
                    {
                        // ignored
                    }
                    //** If there is a callback function, then call it.
                    if (_callback == null)
                    {
                        var e = new DisconnectedEventArgs {Queue = _queue};
                        Disconnected?.Invoke(this, e);
                    }
                    break;
                case DisconnectAction.RetryConnection:
                    Reconnect();
                    break;
                case DisconnectAction.ThrowError:
                    throw new TcpipConnectionException("No Link", ex);
            }
        }

        //**------------------------------------------------------------------------
        //** this gets called by the Codec when it has reeived enough data for
        //** a single packet. We stop reading data at this point.
        private void CodecDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (_callback != null)
            {
                _callback(e.Message);
            }
            else
            {
                OnData(sender, e.Message, Guid.Empty);
            }
        }

        //**------------------------------------------------------------------------
        //** Gets calls when the Codec whats to send data - it would be compressed
        private void CodecDataToSend(object sender, DataToSendEventArgs e)
        {
            lock (this)
            {
                // Get a client stream for reading and writing.
                var stream = _tcpClientA.GetStream();

                if (!IsConnected)
                {
                    var disconnectedArgs = new DisconnectedEventArgs {Queue = _queue};
                    Disconnected?.Invoke(this, disconnectedArgs);
                    return;
                }

                try
                {
                    var msg = e.GetMessage();
                    stream.Write(msg, 0, msg.GetLength(0));
                }
                catch (IOException)
                {
                    var disconnectedArgs = new DisconnectedEventArgs {Queue = _queue};
                    Disconnected?.Invoke(this, disconnectedArgs);
                }
                catch (SocketException)
                {
                }
            }
        }

        #endregion

        #region " IDisposable Support "

        /// <summary>
        ///     Use to detect redundant calls
        /// </summary>
        /// <remarks></remarks>
        private bool _disposedValue;

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CloseChannel();
                }
            }
            _disposedValue = true;
        }


        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region TCPchannel Members

        public event EventHandler<DataEventArgs> Data;

        public event EventHandler<DisconnectedEventArgs> Disconnected;

        public event EventHandler<ConnectedEventArgs> Connected;

        #endregion
    }
}