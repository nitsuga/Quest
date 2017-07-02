using System;
namespace Quest.Lib.Telephony.AspectCTIPS
{
    public interface ICADChannel
    {
        void NewInboundCall(int callid, string cli, string extension, string Group);
        void NewOutboundCall(int callid, string DDI, string Group);
        void Connected(int callid, string extension);
        void EndCall(int callid);
        void SendLogon(string extension);
        void SendLogoff(string extension);
    }

    public class SetDataRequest : EventArgs
    {
        public int callid;
        public string station;
        public int udf;
        public string data;
    }

}
