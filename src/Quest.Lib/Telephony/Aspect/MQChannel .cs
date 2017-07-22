using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.Lib.Trace;
using System.Collections.Generic;
using System.Diagnostics;

namespace Quest.Lib.Telephony.AspectCTIPS
{
    public class MQChannel : ICADChannel 
    {
        private int _lastCallId;
        private IServiceBusClient _serviceBusClient;

        public MQChannel(IServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        /// <summary>
        /// maintain a list of inbound calls
        /// </summary>
        private HashSet<int> _tracker = new HashSet<int>();

        public void Initialise()
        {
            Logger.Write(string.Format("Channel {0} initialising", this.ToString()), TraceEventType.Information, "CollabChannel");
        }

        public void SendLogoff(string extension)
        {
            _serviceBusClient.Broadcast(new CallLogoff
            {
                Extension = extension
            });
        }

        public void SendLogon(string extension)
        {
            _serviceBusClient.Broadcast(new CallLogon
            {
                Extension = extension
            });
        }

        public void Connected(int callid, string extension)
        {
            // only send if inbound call
            if (_tracker.Contains(callid))
                _serviceBusClient.Broadcast(new CallEvent
                {
                    Extension = extension,
                    CallId = callid, 
                    EventType= CallEvent.CallEventType.Connected
                });
        }
        
        public void EndCall(int callid)
        {
            // only send if inbound call
            if (_tracker.Contains(callid))
                {
                _serviceBusClient.Broadcast(new CallEnd
                {
                    CallId = callid,
                });
                _tracker.Remove(callid);
                }

        }

        public void NewOutboundCall(int callid, string DDI, string Group)
        {
            if (_tracker.Contains(callid))
                _tracker.Remove(callid);
        }
        
        /// <summary>
        /// new call arrived, might not have extension
        /// </summary>
        /// <param name="callid"></param>
        /// <param name="CLI"></param>
        /// <param name="extension"></param>
        /// <param name="Group"></param>

        public void NewInboundCall(int callid, string CLI, string extension, string Group)
        {
            if (_lastCallId != callid)
            {
                if (_tracker.Contains(callid))
                    _tracker.Remove(callid);

                _tracker.Add(callid);

                _serviceBusClient.Broadcast(new CallEvent
                {
                    Extension = extension,
                    CLI = CLI,
                    CallId = callid,
                    EventType = CallEvent.CallEventType.Alerting
                });

                _lastCallId = callid;
            }
        }

    }

}
