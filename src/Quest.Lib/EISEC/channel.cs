#pragma warning disable 0169,649
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Quest.Common.Messages;
using Quest.Lib.Net;

namespace Quest.Lib.EISEC
{
    public class EisecChannel
    {
        public enum State
        {
            StateNotInitialised,
            StateNotConnected,
            StateConnecting,
            StateConnected,
            StateLoggingIn,
            StateLoggedIn,
            StateLoggingOut,
            StateDisconnecting
        }

        //private const int _maxPktLen = 332;
            /* In practice the max is 332 ( for a QP pkt ) but protocol spec allows 10006 */

        private const int DefaultTimeout = 10000;
        private IpDetails _config;
        private TcpClient _eisecFd;
        private readonly Status _eisecStatus;
        private readonly int _index;
        private DateTime _lastResponse = DateTime.Now;
        private DateTime _lastSend = DateTime.Now;

        private readonly string[] _ljErrcode =
        {
            "",
            "Invalid name/password",
            "User already logged on",
            "Temporary system fault",
            "User barred (after grace logins)",
            "User locked out"
        };

        private EisecResponse _logonEisecResponse;

        private AutoResetEvent _logonEvent;
        private Thread _logonWorker;
        private EisecResponse _logoutEisecResponse;
        private AutoResetEvent _logoutEvent;

        private readonly IEisecServer _parent;
        private Action<string> _logger;
        private EisecResponse _passwordEisecResponse;
        private AutoResetEvent _passwordEvent;
        /* Where we store our password for the current user id*/
        private string _pendingPasswordChange = "";

        private readonly string[] _pjErrcode =
        {
            "", "Invalid password",
            "Password in history",
            "User not logged in",
            "Temporary system fault"
        };

        private int _pollSendTick;

        private PollState _pollstate = PollState.Off;

        private State _status = State.StateNotConnected; /* Our state with EDB */

        private readonly StxStreamCodec _streamProcessor = new StxStreamCodec();

        /// <summary>
        ///     called by the Open routine to open up a connection to EISEC on an available TCP/IP channel.
        /// </summary>
        /// <param name="ipAddr"></param>
        private EisecResponse ConnectToEisec(IpDetails ipAddr)
        {
            var eisecResponse = new EisecResponse();
            Status = State.StateConnecting;
            _pollstate = PollState.Off;

            _logger($"Start connecting to {ipAddr.Addr}:{ipAddr.Port}");

            /* Create the socket to use to connect to EDB */
            _eisecFd = new TcpClient();

            try
            {
                _eisecFd.Connect(ipAddr.Addr, ipAddr.Port);
            }
            catch (SocketException ex)
            {
                eisecResponse.Code = ReturnCode.CommunicationFailure;
                eisecResponse.SubCode = ex.ErrorCode;
                eisecResponse.Message = ex.Message;
                Status = State.StateNotConnected;
                _eisecFd = null;

                _logger($"Connect to {ipAddr.Addr}:{ipAddr.Port} completed with EISECResponse {ex.ErrorCode} {ex.Message}");

                return eisecResponse;
            }

            Status = State.StateLoggingIn;

            Thread.Sleep(1000);

            // now start the reader..
            var readWorker = new Thread(TcpReaderWorker) {IsBackground = true};
            readWorker.Start();

            Thread.Sleep(1000);

            eisecResponse = SendLogonRequest();

            if (eisecResponse.Code != ReturnCode.Success)
                SetDisconnected("[10001] Logon failure");
            else
                Status = State.StateLoggedIn;

            return eisecResponse;
        }

        public EisecResponse StartAutoLogon()
        {
            _logger($"Starting Autologon");

            StopAutoLogon();

            Task.Factory.StartNew(LogonWorker);

            _eisecStatus.Message = "Starting";

            return new EisecResponse(ReturnCode.Success);
        }

        public EisecResponse SendPoll()
        {
            try
            {
                _logger($"Sending Poll - last poll sent {(DateTime.Now - _lastSend).TotalSeconds} seconds ago");

                _lastSend = DateTime.Now;

                var reqPkt = new EisecPoll();
                SendPacketToEisec(reqPkt.Serialize());
            }
            catch (Exception ex)
            {
                // Standard application exeption handling
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                    throw;
            }

            return new EisecResponse(ReturnCode.Exception);
        }

        /// <summary>
        ///     send the timeout request to EISEC. If successful, we should start recieving "Poll" messages
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public EisecResponse SendPollTimeout(int timeout)
        {
            try
            {
                _logger($"Sending Poll timeout to {timeout} seconds");

                var reqPkt = new EisecSettimeoutRequest {Timeout = timeout};

                SendPacketToEisec(reqPkt.Serialize());
            }
            catch (Exception ex)
            {
                // Standard application exeption handling
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                    throw;
            }

            return new EisecResponse(ReturnCode.Exception);
        }

        public EisecResponse StopAutoLogon()
        {
            _eisecStatus.Stopped = DateTime.Now;
            _eisecStatus.Message = "Stopped";
            _pollstate = PollState.Off;

            if (_logonWorker != null)
                if (_logonWorker.IsAlive)
                {
                    _logger($"Stopping Autologon");
                    _logonWorker.Abort();
                }

            return new EisecResponse(ReturnCode.Success);
        }

        private void LogonWorker()
        {
            _logger($"Logon worker started");
            try
            {
                // keep attempting to log in.
                do
                {
                    try
                    {
                        _pollSendTick++;

                        switch (Status)
                        {
                            case State.StateNotConnected:

                                _logger($"Autologon detected user was not logged on");

                                Open();
                                break;

                            case State.StateLoggingIn:
                                var logonage = (int) (DateTime.Now - _lastResponse).TotalSeconds;
                                if (logonage >= _config.LocalPollTimeoutSeconds)
                                {
                                    _logger($"No response during logon phase. disconnecting");

                                    // Logon took too long. disconnect
                                    SetDisconnected("[10002] Logon took too long");
                                }
                                break;

                            case State.StateLoggedIn:
                                if (_config.SendPollSeconds == 0 || _config.LocalPollTimeoutSeconds == 0 ||
                                    _config.RemotePollTimeoutSeconds == 0)
                                {
                                    _logger(
                                        $"Polling system disabled, configure SendPollSeconds, PollTimeoutSeconds and RecvPollSeconds to > 0 values to enable");
                                }
                                else
                                {
                                    switch (_pollstate)
                                    {
                                        // send a poll set timeout to start the poll system off
                                        case PollState.Off:
                                            SendPollTimeout(_config.RemotePollTimeoutSeconds);
                                            _pollstate = PollState.Senttimeout;
                                            break;

                                        // keep waiting.. we wait for timeout accept or reject to continue
                                        case PollState.Senttimeout:
                                            break;

                                        // time to send another poll?
                                        case PollState.Enabled:
                                            if (_pollSendTick%_config.SendPollSeconds == 0)
                                                SendPoll();

                                            // check we have a response from the far side within limits
                                            var age = (int) (DateTime.Now - _lastResponse).TotalSeconds;
                                            if (age >= _config.LocalPollTimeoutSeconds)
                                            {
                                                SetDisconnected($"Poll not recieved for {age}s. Disconnecting");
                                            }

                                            break;

                                        case PollState.Disabled:
                                            break;
                                    }
                                }
                                break;
                        }

                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        _logger($"Poller: {ex.Message}");
                    }
                } while (true);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void SetDisconnected(string reason)
        {
            _logger($"SetDisconnected, reason: {reason}");
            Status = State.StateNotConnected;
            _eisecFd?.Close();
            _eisecFd = null;
        }

        private void streamProcessor_DataToSend(object sender, DataToSendEventArgs e)
        {
            var client = (TcpClient) sender;
            if (client != null)
            {
                var stream = client.GetStream();
                var bytes = e.GetMessage();
                _logger($"Sending: {Encoding.ASCII.GetString(bytes)}");
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private void streamProcessor_DataReceived(object sender, DataReceivedEventArgs e)
        {
            // Process the data sent by the client, a response packet is generated.
            ProcessEisecResponse(e.Message);
        }

        /// <summary>
        ///     read a block of bytes from eisec
        /// </summary>
        /// <returns></returns>
        private void TcpReaderWorker()
        {
            try
            {
                var buf = new byte[4096];

                if (_eisecFd == null)
                {
                    SetDisconnected("reader quit as eisec connection is null");
                    return;
                }

                if (!_eisecFd.Connected)
                {
                    SetDisconnected("reader quit not connected");
                    return;
                }

                var stream = _eisecFd.GetStream();

                _eisecFd.ReceiveTimeout = 0; // DEFAULT_TIMEOUT;

                var len = -1;

                while (len != 0)
                {

                    try
                    {
                        len = 0;
                        len = stream.Read(buf, 0, buf.Length);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    /* Now check to see if the connection is closed */
                    if (len == 0)
                    {
                        SetDisconnected("reader: connection closed");
                        return;
                    }

                    _streamProcessor.Receive(this, buf, len);
                }
            }
            catch (Exception ex)
            {
                SetDisconnected(ex.ToString());

                //state status = this.Status;
                // probably a timeout
                _logger("reader failed: " + ex.Message);

            }
        }


        /// <summary>
        ///     wait for and process an EISEC response. return an address if one is supplied.
        /// </summary>
        /// <returns>0 if successful, an error code if not</returns>
        private void ProcessEisecResponse(string response)
        {
            if (response != null)
            {
                _lastResponse = DateTime.Now;

                switch (EisecPacket.GetPduType(response))
                {
                    case constants.EisecPoll:
                        ThreadPool.QueueUserWorkItem(ProcessPollResponse, response);
                        break;

                    case constants.EisecTimeoutaccept:
                        ThreadPool.QueueUserWorkItem(ProcessTimeoutAccept, response);
                        break;

                    case constants.EisecTimeoutreject:
                        ThreadPool.QueueUserWorkItem(ProcessTimeoutReject, response);
                        break;

                    case constants.EisecAddressQueryAccept:
                        ThreadPool.QueueUserWorkItem(ProcessQueryAcceptResponse, response);
                        break;

                    case constants.EisecAddressQueryReject:
                        ThreadPool.QueueUserWorkItem(ProcessQueryRejectResponse, response);
                        break;

                    case constants.EisecLogonAccept:
                        ThreadPool.QueueUserWorkItem(ProcessLogonSuccessResponse, response);
                        break;

                    case constants.EisecLogonReject:
                        ThreadPool.QueueUserWorkItem(ProcessLogonRejectResponse, response);
                        break;

                    case constants.EisecLogonGrace:
                        ThreadPool.QueueUserWorkItem(ProcessLogonGraceResponse, response);
                        break;

                    case constants.EisecLogoffRequest:
                        ThreadPool.QueueUserWorkItem(ProcessLogoffResponse, response);
                        break;

                    case constants.EisecPasswordChangeAccept:
                        ThreadPool.QueueUserWorkItem(ProcessPwdAcceptResponse, response);
                        break;

                    case constants.EisecPasswordChangeReject:
                        ThreadPool.QueueUserWorkItem(ProcessPwdRejectResponse, response);
                        break;
                }
            }
        }

        /// <summary>
        ///     generate a new password
        /// </summary>
        /// <returns></returns>
        private string GeneratePassword()
        {
            var pwd = "";
            var r = new Random((int) DateTime.Now.Ticks);
            for (var i = 0; i < 12; i++)
                pwd += r.Next(15).ToString("X");
            return pwd;
        }

        private enum PollState
        {
            Disabled,
            Off,
            Senttimeout,
            Enabled
        }

        #region "Public methods"

        public EisecChannel(IpDetails config, IEisecServer parent, int index, Action<string> logger)
        {
            _parent = parent;
            _config = config;
            _index = index;
            _logger = logger;

            _eisecStatus = new Status
            {
                LastError = "None",
                Running = true,
                Started = DateTime.Now,
                Message = "Initialising"
            };

            _streamProcessor.DataReceived += streamProcessor_DataReceived;
            _streamProcessor.DataToSend += streamProcessor_DataToSend;

            // Publish this instance to WMI
            // System.Management.Instrumentation.Instrumentation.Publish(eisecStatus);

            //StartAutoLogon();
        }


        /// <summary>
        ///     open a connection to EISEC using the user details 0 or 1
        /// </summary>
        /// <returns></returns>
        public EisecResponse Open()
        {
            try
            {
                _eisecStatus.Message = "Connecting to EISEC ";

                _logger(_eisecStatus.Message);

                SetDisconnected("Ensure disconnected before reconnecting");

                ConnectToEisec(_config);

                return new EisecResponse(ReturnCode.Success);
            }

            catch (Exception ex)
            {
                // Standard application exeption handling
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                    throw;
            }

            return new EisecResponse(ReturnCode.Exception);
        }

        public State Status
        {
            get
            {
                if (_eisecFd == null)
                    _status = State.StateNotConnected;

                if (_eisecFd != null && _eisecFd.Connected == false)
                    _status = State.StateNotConnected;

                return _status;
            }
            private set
            {
                _logger($"State change {_status}->{value}");
                _status = value;
            }
        }
        
        /// <summary>
        ///     change the password of the current user, must be online for this to have an effect
        /// </summary>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public EisecResponse ChangePasswordOnline(string newPassword)
        {
            try
            {
                _logger($"Change password to {newPassword}");

                if (newPassword.Length == 0)
                    newPassword = GeneratePassword();

                return SendPasswordChangeRequest(newPassword);
            }
            catch (Exception ex)
            {
                // Standard application exeption handling
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                    throw;
            }

            return new EisecResponse(ReturnCode.Exception);
        }

        public EisecResponse Close()
        {
            try
            {
                _logger("Close connection");

                /* If we are not connected to the EDB, ignore this */
                if (Status != State.StateLoggedIn)
                    return new EisecResponse(ReturnCode.NotLoggedIn, "Not currently connected or logged in to EISEC", 0,
                        null);

                Status = State.StateLoggingOut;

                /* Now send logoff */
                return SendLogoutRequest();
            }
            catch (Exception ex)
            {
                // Standard application exeption handling
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                    throw;
            }
            return new EisecResponse(ReturnCode.Exception);
        }

        #endregion

        #region "Response handlers"

        /// <summary>
        ///     we have recieved a poll from the far side. record the time. The poll worker will check
        ///     the time delay.
        /// </summary>
        /// <param name="stateInfo"></param>
        private void ProcessPollResponse(object stateInfo)
        {
            _logger($"Received Poll - previous poll was sent {(DateTime.Now - _lastSend).TotalSeconds}s ago");
        }

        private void ProcessTimeoutReject(object stateInfo)
        {
            _logger("EISEC rejected setting up a timeout poll - polling disabled");
            _pollstate = PollState.Disabled;
        }

        private void ProcessTimeoutAccept(object stateInfo)
        {
            _logger("EISEC accepted setting up a timeout poll - polling enabled");

            _pollstate = PollState.Enabled;
        }

        private void ProcessLogonSuccessResponse(object stateInfo)
        {
            try
            {
                _logger("EISEC Logon Success");

                // signal completion of this command
                _logonEisecResponse = new EisecResponse(ReturnCode.Success, "logged on successfully", 0, null);
                _logonEvent.Set();
            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                {
                    throw;
                }
            }
        }

        private void ProcessLogonGraceResponse(object stateInfo)
        {
            try
            {
                _logger("EISEC Grace logon Success");

                var response = (string) stateInfo;
                var reject = new EisecGraceLogon();

                try
                {
                    reject.Deserialize(response);
                }
                catch (Exception ex1)
                {
                    if (ExceptionPolicy.HandleException(ex1, "TRACE"))
                    {
                        throw;
                    }
                }

                _logger($"Grace logon - {reject.GraceLogons} left");

                var newPw = GeneratePassword();

                // signal completion of this command
                _logonEisecResponse = ChangePasswordOnline(newPw);
                _logonEvent.Set();
            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///     process a logon failure.
        /// </summary>
        /// <returns></returns>
        private void ProcessLogonRejectResponse(object stateInfo)
        {
            try
            {
                var response = (string) stateInfo;
                var reject = new EisecLogonReject();
                var message = "";
                var rejectCode = 0;

                try
                {
                    reject.Deserialize(response);
                    rejectCode = reject.RejectCode;

                    // it failed
                    if (rejectCode < 1 || rejectCode > _ljErrcode.GetUpperBound(0))
                        message = $"Logon rejected ({rejectCode}) - no description";
                    else
                        message = $"Logon rejected ({rejectCode}) - {_ljErrcode[rejectCode]}";

                    _logger(message);

                    // prevent trying again
                    StopAutoLogon();

                    SetDisconnected($"Logon rejected");
                }
                catch (Exception ex1)
                {
                    if (ExceptionPolicy.HandleException(ex1, "TRACE"))
                    {
                        throw;
                    }
                }

                // signal completion of this command
                _logonEisecResponse = new EisecResponse(ReturnCode.LogonRejected, message, rejectCode, null);
                _logonEvent.Set();
            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///     process a password accept message and save the password in the password file
        /// </summary>
        /// <returns></returns>
        private void ProcessPwdAcceptResponse(object stateInfo)
        {
            try
            {
                try
                {
                    _logger("Password changed successfully");
                    _config.User.Password = _pendingPasswordChange;
                    _parent.UpdatePassword(_index, _pendingPasswordChange);
                }
                catch (Exception ex1)
                {
                    if (ExceptionPolicy.HandleException(ex1, "TRACE"))
                    {
                        throw;
                    }
                }

                // signal completion of this command
                _passwordEisecResponse = new EisecResponse(ReturnCode.Success, "Password changed successfully", 0, null);
                _passwordEvent.Set();
            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///     process a password reject message
        /// </summary>
        /// <returns></returns>
        private void ProcessPwdRejectResponse(object stateInfo)
        {
            try
            {
                var message = "";
                var rejectCode = 0;

                try
                {
                    var response = (string) stateInfo;
                    var reject = new EisecPasswordChgReject();
                    reject.Deserialize(response);
                    rejectCode = reject.RejectCode;

                    // it failed
                    if (rejectCode < 1 || rejectCode >= _ljErrcode.GetUpperBound(0))
                        message = $"Password change rejected ({rejectCode}) - no description";
                    else
                        message = $"Password change reject  ({rejectCode}) - {_pjErrcode[rejectCode]}";

                    // prevent trying again
                    StopAutoLogon();

                    SetDisconnected(message);
                }
                catch (Exception ex1)
                {
                    if (ExceptionPolicy.HandleException(ex1, "TRACE"))
                    {
                        throw;
                    }
                }

                // signal completion of this command
                _passwordEisecResponse = new EisecResponse(ReturnCode.PasswordRejected, message, rejectCode, null);
                _passwordEvent.Set();
            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///     process a successful query
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stateInfo"></param>
        /// <returns></returns>
        private void ProcessQueryAcceptResponse(object stateInfo)
        {
            try
            {
                var response = (string) stateInfo;
                var addrResp = new EisecAddressQueryResp();
                int reqNo;

                try
                {
                    addrResp.Deserialize(response);
                    reqNo = addrResp.Request;

                    _logger($"Query response: {addrResp.Details}");

                    // remember the address found and store it with the request
                    _parent.SetAddress(reqNo, addrResp.Details);
                }
                catch (Exception ex1)
                {
                    if (ExceptionPolicy.HandleException(ex1, "TRACE"))
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///     process an unsuccessful query
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stateInfo"></param>
        /// <returns></returns>
        private void ProcessQueryRejectResponse(object stateInfo)
        {
            try
            {
                var reject = new EisecAddressQueryRej();
                var reqNo = reject.Request;
                var addr = new CallLookupResponse
                {
                    Status="Rejected",
                    RejectCode = reject.ErrorCode
                };

                _logger($"Query rejected ({addr.RejectCode})");

                _parent.SetAddress(reqNo, addr);
            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///     process a logoff response by disconnecting from EISEC and tidying up.
        /// </summary>
        private void ProcessLogoffResponse(object stateInfo)
        {
            try
            {
                SetDisconnected("logging off");

                Status = State.StateNotConnected;

                if (Status != State.StateLoggingOut)
                    _logoutEisecResponse = new EisecResponse(ReturnCode.LogoffFailed,
                        "Logoff response received but not in logout state.", 0, null);
                else
                    _logoutEisecResponse = new EisecResponse(ReturnCode.Success, "logged off");

                _logger(_logoutEisecResponse.Message);

                // signal completion of this command
                _logoutEvent.Set();
            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, "TRACE"))
                {
                    throw;
                }
            }
        }

        #endregion

        #region "Send EISEC requests"

        private EisecResponse SendLogonRequest()
        {
            try
            {
                _logonEisecResponse = new EisecResponse(ReturnCode.Timeout);
                var reqPkt = new EisecLogonRequest();

                _logger("Sent log on " + _config.User.Username);

                // make the request packet
                reqPkt.Username = _config.User.Username;
                reqPkt.Password = _config.User.Password;

                SendPacketToEisec(reqPkt.Serialize());

                // wait for EISECResponse
                _logonEvent = new AutoResetEvent(false);
                var signalled = _logonEvent.WaitOne(DefaultTimeout, false);

                if (!signalled)
                    _logonEisecResponse = new EisecResponse(ReturnCode.Exception);
            }
            catch
            {
                // ignored
            }
            return _logonEisecResponse;
        }

        private EisecResponse SendLogoutRequest()
        {
            var reqPkt = new EisecLogoff();
            _logger("Sent log off");

            SendPacketToEisec(reqPkt.Serialize());

            // wait for EISECResponse
            _logoutEvent = new AutoResetEvent(false);
            _logoutEisecResponse = new EisecResponse(ReturnCode.Timeout);
            _logoutEvent.WaitOne(DefaultTimeout, false);
            return _logoutEisecResponse;
        }

        private EisecResponse SendPasswordChangeRequest(string newPasswd)
        {
            var reqPkt = new EisecPasswordChgReq();
            _pendingPasswordChange = newPasswd;
            reqPkt.OldPassword = _config.User.Password;
            reqPkt.NewPassword = _pendingPasswordChange;

            SendPacketToEisec(reqPkt.Serialize());

            // wait for EISECResponse
            _passwordEvent = new AutoResetEvent(false);
            _passwordEisecResponse = new EisecResponse(ReturnCode.Timeout);
            _passwordEvent.WaitOne(DefaultTimeout, false);
            return _passwordEisecResponse;
        }

        private void SendQueryToEisec(QueryRequest edbReq)
        {
            var reqPkt = new EisecAddressQueryReq();
            reqPkt.Request = edbReq.RequestId;
            reqPkt.Number = edbReq.Details.CLI;

            edbReq.TimeSent = DateTime.Now;
            edbReq.State = RequestState.Sent;

            SendPacketToEisec(reqPkt.Serialize());
        }

        /// <summary>
        ///     send a packet to EISEC via TCP/IP
        /// </summary>
        public void SendPacketToEisec(string response)
        {
            // convert response packet to bytes that we can send.
            var buffer = Encoding.ASCII.GetBytes(response);

            // Send back a response.
            _streamProcessor.Send(_eisecFd, buffer);
        }

        #endregion
    }
}