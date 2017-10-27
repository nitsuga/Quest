using System;
using System.Collections.Generic;
using System.Text;
using Quest.Common.Messages;
using Quest.Lib.Utils;
using Quest.Common.Messages.Telephony;

// All request types are transferred as strings. Each request and response is handled by a class that
// can make a request packet or can interpret a request packet.

namespace Quest.Lib.EISEC
{
    public class Status
    {
        public bool Running;
        public String Message;
        public DateTime Started;
        public DateTime Stopped;
        public String LastError;

        void OnSetPassword( String password )
        {
        }
    }
    
    internal class constants
    {
        internal const int MaxReqNo = 100;
        internal const int MaxCurrentReqs = 16;

        /* PDU type identifiers - REQUESTS */
        internal const string EisecLogonRequest = "LR";
        internal const string EisecPasswordChangeRequest = "PC";
        internal const string EisecQueryRequest = "NQ";
        internal const string EisecLogoffRequest = "LO";
        internal const string EisecPoll = "PO";
        internal const string EisecSettimeout = "TS";
        internal const string EisecTimeoutaccept = "TA";
        internal const string EisecTimeoutreject = "TJ";

        /* PDU type identifiers - RESPONSES */
        internal const string EisecLogonAccept = "LA";
        internal const string EisecLogonReject = "LJ";
        internal const string EisecLogonGrace = "LG";
        internal const string EisecPasswordChangeAccept = "PA";
        internal const string EisecPasswordChangeReject = "PJ";
        internal const string EisecAddressQueryAccept = "QP";
        internal const string EisecAddressQueryReject = "QN";

        /* Data field type identifiers */
        internal const string EisecFieldTeleNo = "TN";
        internal const string EisecFieldName = "NA";
        internal const string EisecFieldAddress1 = "AA";
        internal const string EisecFieldAddress2 = "AB";
        internal const string EisecFieldAddress3 = "AC";
        internal const string EisecFieldAddress4 = "AD";
        internal const string EisecFieldAddress5 = "AE";
        internal const string EisecFieldAddress6 = "AF";

        /* Standard PDU characters */
        internal const char EisecPktStart = (char)0x02;
        internal const char EisecPktEnd = (char)0x03;
    }

    /// <summary>
    /// initial logon request
    /// </summary>
    internal class EisecLogonRequest : EisecPacket
    {
        private string _username;
        private string _password;

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        internal override string Serialize()
        {
            string body = constants.EisecLogonRequest + _username.PadRight(8) + _password.PadRight(24);
            return MakeFrame(body);
        }

        internal override void Deserialize(string packet)
        {
            _username = packet.Substring(6, 8).Trim();
            _password = packet.Substring(14, 24).Trim();
        }
    } ;

    /// <summary>
    /// request to change our password on EISEC
    /// </summary>
    internal class EisecPasswordChgReq : EisecPacket
    {
        internal override string Serialize() 
        {
            string body = constants.EisecPasswordChangeRequest + _oldPassword.PadRight(24) + _newPassword.PadRight(24);
            return MakeFrame(body);
        }

        internal override void Deserialize(string packet)
        {
            _oldPassword = packet.Substring(6, 24).Trim();
            _newPassword = packet.Substring(22, 24).Trim();
        }

        private string _oldPassword;
        private string _newPassword;

        internal string OldPassword
        {
            get { return _oldPassword; }
            set { _oldPassword = value; }
        }

        internal string NewPassword
        {
            get { return _newPassword; }
            set { _newPassword = value; }
        }
    } ;

    /// <summary>
    /// request a lookup
    /// </summary>
    internal class EisecAddressQueryReq : EisecPacket
    {
        internal override string Serialize()
        {
            string body = constants.EisecQueryRequest + $"{_request:00}" + "TN" + _number.PadRight(20);
            return MakeFrame(body);
        }

        internal override void Deserialize(string packet)
        {
            if (Int32.TryParse(packet.Substring(6, 2), out _request))
            {
                _number = packet.Substring(10, 20).Trim();
            }
        }

        Int32 _request;
        string _number;

        public int Request
        {
            get { return _request; }
            set { _request = value; }
        }

        public string Number
        {
            get { return _number; }
            set { _number = value; }
        }
    } ;

    /// <summary>
    /// log off from EISEC
    /// </summary>
    internal class EisecLogoff : EisecPacket
    {
        internal override string Serialize()
        {
            return MakeFrame(constants.EisecLogoffRequest);
        }

        internal override void Deserialize(string packet)
        {
        }

    } ;

    internal class EisecSettimeoutAccept : EisecPacket
    {
        internal override string Serialize()
        {
            return MakeFrame(constants.EisecTimeoutaccept);
        }

        internal override void Deserialize(string packet)
        {
        }

    } ;

    internal class EisecSettimeoutReject : EisecPacket
    {
        internal override string Serialize()
        {
            return MakeFrame(constants.EisecTimeoutreject);
        }

        internal override void Deserialize(string packet)
        {
        }

    } ;

    internal class EisecSettimeoutRequest : EisecPacket
    {
        public int Timeout { get; set; }

        internal override string Serialize()
        {
            string body = constants.EisecSettimeout + Timeout.ToString("#####").PadRight(5);
            return MakeFrame(body);
        }

        internal override void Deserialize(string packet)
        {
            Timeout = int.Parse(packet.Substring(6, 5).Trim());
        }
    } ;

    /// <summary>
    /// log off from EISEC
    /// </summary>
    internal class EisecPoll : EisecPacket
    {
        internal override string Serialize()
        {
            return MakeFrame(constants.EisecPoll);
        }

        internal override void Deserialize(string packet)
        {
        }

    } ;

    /// <summary>
    /// timeout reject from EISEC
    /// </summary>
    internal class EisecTimeoutReject : EisecPacket
    {
        internal override string Serialize()
        {
            string body = constants.EisecTimeoutreject + $"{_rejectCode}".Substring(0, 1);
            return MakeFrame(body);
        }
        internal override void Deserialize(string packet)
        {
            _rejectCode = Int32.Parse(packet.Substring(6, 1));
        }

        int _rejectCode;

        public int RejectCode
        {
            get { return _rejectCode; }
            set { _rejectCode = value; }
        }
    } ;

    /// <summary>
    ///  the timeout was accepted
    /// </summary>
    internal class EisecTimeoutAccept : EisecPacket
    {
        internal override string Serialize()
        {
            return MakeFrame(constants.EisecTimeoutaccept);
        }
        internal override void Deserialize(string packet)
        {
        }
    } ;

    /// <summary>
    /// The logon was rejected
    /// </summary>
    internal class EisecLogonReject : EisecPacket
    {
        internal override string Serialize()
        {
            string body = constants.EisecLogonReject + $"{_rejectCode}".Substring(0,1);
            return MakeFrame(body);
        }
        internal override void Deserialize(string packet)
        {
            _rejectCode = Int32.Parse(packet.Substring(6,1));
        }

        int _rejectCode;

        public int RejectCode
        {
            get { return _rejectCode; }
            set { _rejectCode = value; }
        }
    } ;

    /// <summary>
    ///  the logon was accepted
    /// </summary>
    internal class EisecLogonAccept : EisecPacket
    {
        internal override string Serialize()
        {
            return MakeFrame(constants.EisecLogonAccept);
        }
        internal override void Deserialize(string packet)
        {
        }
    } ;

    /// <summary>
    /// the logon was accepted but within the grace period
    /// </summary>
    internal class EisecGraceLogon : EisecPacket
    {
        internal override string Serialize()
        {
            string body = constants.EisecLogonGrace + $"{_graceLogons}".Substring(0,1);
            return MakeFrame(body);
        }
        internal override void Deserialize(string packet)
        {
            Int32.TryParse(packet.Substring(6, 1), out _graceLogons);
        }

        int _graceLogons;

        public int GraceLogons
        {
            get { return _graceLogons; }
            set { _graceLogons = value; }
        }
    } ;

    /// <summary>
    /// Password change was accepted
    /// </summary>
    internal class EisecPasswordChgAccept : EisecPacket
    {
        internal override string Serialize()
        {
            string body = constants.EisecPasswordChangeAccept;
            return MakeFrame(body);
        }
        internal override void Deserialize(string packet)
        {
        }
    } ;

    /// <summary>
    /// password change was rejected
    /// </summary>
    internal class EisecPasswordChgReject : EisecPacket
    {
        internal override string Serialize()
        {
            string body = constants.EisecPasswordChangeReject + $"{_rejectCode}".Substring(0,1);
            return MakeFrame(body);
        }
        internal override void Deserialize(string packet)
        {
            _rejectCode = Int32.Parse(packet.Substring(6, 1));
        }

        int _rejectCode;

        public int RejectCode
        {
            get { return _rejectCode; }
            set { _rejectCode = value; }
        }
    } ;

    /// <summary>
    /// a positive response to an address lookup
    /// </summary>
    internal class EisecAddressQueryResp : EisecPacket
    {
        internal override string Serialize()
        {
            StringBuilder sb=new StringBuilder();
            sb.Append(constants.EisecAddressQueryAccept);
            sb.Append($"{_request:00}");
            sb.Append(MakeFixedField("TN", _details.TelephoneNumber, 20));
            sb.Append(MakeVariableField("NA", _details.Name));
            sb.Append(MakeVariableField("AA", _details.Address[0]));
            sb.Append(MakeVariableField("AB", _details.Address[1]));
            sb.Append(MakeVariableField("AC", _details.Address[2]));
            sb.Append(MakeVariableField("AD", _details.Address[3]));
            sb.Append(MakeVariableField("AE", _details.Address[4]));
            sb.Append(MakeVariableField("AF", _details.Address[5]));
            return MakeFrame(sb.ToString());
        }

        internal override void Deserialize(string packet)
        {
            CallLookupResponse addr = new CallLookupResponse();
            int pktPtr = 6;

            Details = addr;

            

            Request = Int32.Parse(packet.Substring(pktPtr, 2));

            pktPtr += 2;

            /* If the telephone number is returned, copy it */
            if (packet.Substring(pktPtr,2)==constants.EisecFieldTeleNo)
            {
                addr.TelephoneNumber = packet.Substring(pktPtr+2, 20);
                pktPtr += 22;
            }

            List<string> lines = new List<string>(); 

            while (pktPtr+1 < packet.Length)
            {
                string code = packet.Substring(pktPtr, 2);
                int fieldLen;
                if (Int32.TryParse(packet.Substring(pktPtr + 2, 3), out fieldLen))
                {
                    pktPtr += 5;

                    switch (code)
                    {
                        case constants.EisecFieldName:
                            addr.Name = packet.Substring(pktPtr, fieldLen);
                            break;
                        case constants.EisecFieldAddress1:
                        case constants.EisecFieldAddress2:
                        case constants.EisecFieldAddress3:
                        case constants.EisecFieldAddress4:
                        case constants.EisecFieldAddress5:
                        case constants.EisecFieldAddress6:
                            lines.Add(packet.Substring(pktPtr, fieldLen));
                            break;
                    }
                }
                    // no match.. increment to next field.
                else
                    fieldLen = 1;

                pktPtr += fieldLen;
            }

            addr.Address = lines.ToArray();

            // split out mobile fields
            if (addr.Name.StartsWith("*MOB*") || addr.Name.StartsWith("*TMS*"))
            {
                bool searching = addr.Name.Contains("Searching");
                addr.Status = searching ? "Searching" : "Location Found";
                addr.StatusCode = searching ? CallerDetailsStatusCode.Searching : CallerDetailsStatusCode.LocationFound;
                addr.IsMobile = true;
                addr.Requery = GetNumeric(addr.Name, 54, 3);
                addr.Eastings = GetNumeric(addr.Address[0], 1, 11);
                addr.Northings = GetNumeric(addr.Address[0], 12, 11);
                addr.SemiMajor = GetNumeric(addr.Address[0], 23, 7);
                addr.SemiMinor = GetNumeric(addr.Address[0], 30, 6);
                addr.Confidence = GetNumeric(addr.Address[1], 1, 4);
                addr.Angle = GetNumeric(addr.Address[1], 5, 7);
                addr.Altitude = GetNumeric(addr.Address[1], 20, 6);
                addr.Direction = GetNumeric(addr.Address[1], 26, 4);
                addr.Speed = GetNumeric(addr.Address[1], 30, 3);

                if (addr.Eastings > 0 && addr.Northings > 0 && addr.SemiMajor > 0 && addr.SemiMinor > 0)
                    addr.Shape = GeomUtils.MakeEllipseBng(addr.Angle, addr.Eastings, addr.Northings, addr.SemiMajor, addr.SemiMinor);
            }
        }

        int GetNumeric(string text, int offset, int len)
        {
            int i;
            if (text == null || text.Length < offset + len - 1)
                return 0;
            int.TryParse(text.Substring(offset-1, len), out i);
            return i;
        }
        string GetText(string text, int offset, int len)
        {
            string i;
            i = text.Substring(offset, len).Trim();
            return i;
        }

        private CallLookupResponse _details;
        private int _request;

        public int Request
        {
            get { return _request; }
            set { _request = value; }
        }

        public CallLookupResponse Details
        {
            get { return _details; }
            set { _details = value; }
        }

        /// <summary>
        /// compares a string at a position in a buffer
        /// </summary>
        /// <param name="buffer">the buffer to check</param>
        /// <param name="pos">the position in the buffer to check</param>
        /// <param name="s">the string to compare</param>
        /// <returns>true if the string is at the position in the buffer</returns>
        private bool StringAtBufferPos(byte[] buffer, int pos, string s)
        {
            byte[] stringbuffer = Encoding.ASCII.GetBytes(s);
            if (buffer.Length <= pos + s.Length)
                return false;
            for (int i = 0; i < s.Length; i++)
                if (buffer[pos + i] != stringbuffer[i])
                    return false;
            return true;
        }


    } ;

    /// <summary>
    /// a negative response to an address lookup
    /// </summary>
    internal class EisecAddressQueryRej : EisecPacket
    {
        internal override string Serialize()
        {
            string body = constants.EisecAddressQueryAccept + $"{_request:00}" + $"{_errorCode:00}";
            return MakeFrame(body);
        }

        internal override void Deserialize(string packet)
        {
            _request = Int32.Parse(packet.Substring(6, 2));
            _errorCode = Int32.Parse(packet.Substring(8, 2));
        }

        private int _request;

        public int Request
        {
            get { return _request; }
            set { _request = value; }
        }

        private int _errorCode;

        public int ErrorCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }

    } ;

    /// <summary>
    /// this is the base class for all requests and responses. It provides various utilities 
    /// for packing and unpacking packets.
    /// </summary>
    internal abstract class EisecPacket
    {

        /// <summary>
        /// extract the PDU
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        internal static string GetPduType(string body)
        {
            return body.Substring(4, 2);
        }

        /// <summary>
        /// surround the request with header and trailer.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        internal static string MakeFrame(string body)
        {
            return $"{body.Length:0000}" + body;
        }

        /// <summary>
        /// make a variable field (address query only)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal string MakeVariableField(string type, string value)
        {
            value = value ?? "";
            return $"{type}{value.Length:000}{value??""}";
        }

        /// <summary>
        /// make a fixed length field 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        internal string MakeFixedField(string type, string value, int len)
        {
            return type + value.PadRight(len);
        }

        internal abstract string Serialize();
        internal abstract void Deserialize(string packet);
    }
}