using Autofac;
using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.Lib.Net;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
using Quest.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Quest.Lib.Telephony.StormLink
{
    public class StormTelephonyServer : ServiceBusProcessor
    {

        public int port { get; set; }


        /// <summary>
        /// format the CLI into a normal telephone number without 0wxyz or wxyz
        /// </summary>
        /// <remarks></remarks>
        public bool normaliseCLI { get; set; } = true;

        /// <summary>
        /// extract the left most n characters from the extension
        /// </summary>
        /// <remarks></remarks>

        public int rightMostExtension { get; set; } = 4;

        private readonly ILifetimeScope _scope;
        private Dictionary<int, CallTracker> _tracker = new Dictionary<int, CallTracker>();
        private const int MAXRECORDS = 723;
        private int _dialrequestId = 1;

        /// <summary>
        /// the server socket we're listening on
        /// </summary>
        /// <remarks></remarks>
        private TcpipListener listener = new TcpipListener();

        /// <summary>
        /// a list of connected clients.
        /// </summary>
        /// <remarks></remarks>
        private List<RemoteTcpipConnection> connections = new List<RemoteTcpipConnection>();

        IServiceBusClient _serviceBusClient;
        public StormTelephonyServer(
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
            _serviceBusClient = serviceBusClient;

        }

        protected override void OnStart()
        {
            // handle events from telephony and EISEC
            MsgHandler.AddHandler<CallLookupResponse>(CallLookupHandler);
            MsgHandler.AddHandler<CallEvent>(CallEventHandler);
            MsgHandler.AddHandler<CallEnd>(CallEndHandler);
            MsgHandler.AddHandler<CallLogon>(CallLogonHandler);
            MsgHandler.AddHandler<CallLogoff>(CallLogoffHandler);

            Register();
        }

        private void Register()
        {
            //' A connection is handled by creating a subscription. the subscription acts like a handle to the connection. each
            //' subscription is refered to by the workflowInstanceId unique guid and all actions on the connection require the guid to be passed

            //' create a new subscription
            listener.StartListening(typeof(STORMCodec), port);

            //' wire up event handlers from the TCP/IP layer
            listener.Connected += Connected;
            listener.Data += Data;
            listener.Disconnected += DisConnected;

            Logger.Write("CAD Service Session created", TraceEventType.Information, "CAD");
        }

        /// <summary>
        /// data arrives from remote connection
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>

        private void Data(object Sender, DataEventArgs e)
        {
            try
            {
                Logger.Write(string.Format("STORM DATA  {0}", e.Data.ToString()), TraceEventType.Information, "STORM");

                XElement doc = XElement.Parse(e.Data);

                switch (doc.Elements().First().Name.LocalName)
                {
                    case "HeartBeatRequest":
                        SendHeartBeat(e.ConnectionId);

                        break;
                    case "DialRequest":
                        MakeCall request = new MakeCall();

                        request.Caller = doc.Elements().First().Elements("WorkstationId").FirstOrDefault().Value;
                        request.Callee = doc.Elements().First().Elements("TelephoneNumber").FirstOrDefault().Value;
                        request.RequestId = _dialrequestId;


                        foreach (RemoteTcpipConnection c in connections)
                        {
                            if (c.ConnectionId == e.ConnectionId)
                            {
                                if (request != null)
                                {
                                    _serviceBusClient.Broadcast(request);
                                }
                                _dialrequestId += 1;

                            }
                        }



                        break;
                }



            }
            catch 
            {
            }
        }

        private void SendHeartBeat(Guid key)
        {
                string s = Properties.Resources.StormHeartbeat + "\r\n";
                foreach (RemoteTcpipConnection rc in connections.ToArray())
                {
                    Logger.Write(string.Format("Heartbeat sent to {0}", rc.ToString()), TraceEventType.Information, "STORM");
                    rc.Send(s);
                }
        }

        /// <summary>
        /// a remote computer connected to us. Save the subscription in the connectionlist
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void Connected(object Sender, ConnectedEventArgs e)
        {
            connections.Add(e.RemoteTcpipConnection);
            Logger.Write("CAD Service remote connection accepted from " + e.RemoteTcpipConnection.ToString(), TraceEventType.Information, "CAD");
        }

        private void DisConnected(object Sender, DisconnectedEventArgs e)
        {
            connections.Remove(e.Remoteconnection);
            Logger.Write("CAD Service remote connection disconnected from " + e.Remoteconnection.ToString(), TraceEventType.Information, "CAD");
        }

        /// <summary>
        /// Send a new call to the CAD
        /// </summary>
        /// <param name="key"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private void CallDetails(Guid key, CallLookupResponse details)
        {
            CallTracker t = default(CallTracker);

            if (!_tracker.ContainsKey(details.CallId))
            {
                return;
            }

            t = _tracker[details.CallId];


            string s = Properties.Resources.StormDetails + "\r\n";

            s = s.Replace("{CallId}", Norm(details.CallId.ToString()));
            s = s.Replace("{CLI}", GetTelephoneNumber(t.CLI));
            s = s.Replace("{Name}", Norm(details.Name));
            s = s.Replace("{Address1}", Norm(details.Address[0]));
            s = s.Replace("{Address2}", Norm(details.Address[1]));
            s = s.Replace("{Address3}", Norm(details.Address[2]));
            s = s.Replace("{Address4}", Norm(details.Address[3]));
            s = s.Replace("{Address5}", Norm(details.Address[4]));
            s = s.Replace("{Address6}", Norm(details.Address[5]));

            Logger.Write(string.Format("CAD CallId={0} details {1}", details.CallId, s), TraceEventType.Information, "STORM");


            if (t.CallAnswered == true)
            {
                foreach (RemoteTcpipConnection rc in connections.ToArray())
                {
                    Logger.Write(string.Format("CAD CallId={0} details sent to {1}", details.CallId, rc.ToString()), TraceEventType.Information, "STORM");
                    rc.Send(s);
                }
            }
            else
            {
                Logger.Write(string.Format("CAD CallId={0} details saved until answered", details.CallId), TraceEventType.Information, "STORM");
                t.Details = s;
                // store until it has been answered
            }

        }

        private void CallEnd(System.Guid key, CallEnd details)
        {
            if (_tracker.ContainsKey(details.CallId))
            {
                _tracker.Remove(details.CallId);
            }
        }

        private void CallRinging(CallEvent details)
        {
            CallTracker t = default(CallTracker);

            if (!_tracker.ContainsKey(details.CallId))
            {
                return;
            }

            t = _tracker[details.CallId];


            string s = Properties.Resources.StormConnected + "\r\n";

            s = s.Replace("{CallId}", Norm(details.CallId.ToString()));
            s = s.Replace("{Extension}", GetExtension(details.Extension));
            s = s.Replace("{CLI}", GetTelephoneNumber(t.CLI));

            Logger.Write(string.Format("CAD CallId={0} details {1}", details.CallId, s), TraceEventType.Information, "STORM");

            foreach (RemoteTcpipConnection rc in connections.ToArray())
            {
                Logger.Write(string.Format("CAD CallId={0} end call event sent to {1}", details.CallId, rc.ToString()), TraceEventType.Information, "STORM");
                rc.Send(s);
            }

            // 
            t.CallAnswered = true;

            if (t.Details != null && t.Details.Length > 0)
            {
                // send the details as well
                foreach (RemoteTcpipConnection rc in connections.ToArray())
                {
                    Logger.Write(string.Format("CAD CallId={0} sending saved details to {1}", details.CallId, rc.ToString()), TraceEventType.Information, "STORM");
                    rc.Send(t.Details);
                }
            }


        }

        private string Norm(string txt)
        {
            if (txt == null)
                return "";
            txt = txt.Replace("&", "&amp;");
            txt = txt.Replace("\"", "&quot;");
            txt = txt.Replace("<", "&lt;");
            txt = txt.Replace(">", "&gt;");
            txt = txt.Replace("~", "&tilde;");
            return txt;
        }

        private void CallNew(CallEvent details)
        {
            if (_tracker.ContainsKey(details.CallId))
            {
                _tracker.Remove(details.CallId);
            }
            CallTracker t = new CallTracker
            {
                CallId = details.CallId,
                CLI = details.CLI
            };

            _tracker.Add(details.CallId, t);


        }

        private string GetTelephoneNumber(string cli)
        {

            if (normaliseCLI)
            {
                string wxyz = "";
                string number = "";

                GetTelephoneParts(cli, ref wxyz, ref number);

                return number;
            }
            else
            {
                return cli;
            }
        }

        private string GetExtension(string cli)
        {
            return cli.Substring(cli.Length - rightMostExtension, rightMostExtension);
        }

        private void GetTelephoneParts(string cli, ref string wxyz, ref string number)
        {
            Logger.Write(string.Format("Normalising Cli {0}", cli), TraceEventType.Information, "STORM");
            cli = cli + "";
            if (cli.StartsWith("09"))
            {
                wxyz = cli.Substring(1, 4);
                number = "0" + cli.Substring(5);
                Logger.Write(string.Format("Normalised Cli {0} number={1}", cli, number), TraceEventType.Information, "STORM");
                return;
            }

            if (cli.StartsWith("9"))
            {
                wxyz = cli.Substring(0, 4);
                number = "0" + cli.Substring(4);
                Logger.Write(string.Format("Normalised Cli {0} number={1}", cli, number), TraceEventType.Information, "STORM");
                return;
            }

            number = cli;
            wxyz = "";

            if (number.ToLower() == "anonymous")
            {
                number = "ANON";
                Logger.Write(string.Format("Normalised Cli {0} number={1}", cli, number), TraceEventType.Information, "STORM");
                return;
            }

            if (!number.StartsWith("0") & number.Length > 8)
            {
                number = "0" + cli.Substring(4);
                return;
            }

        }
        
        private Response CallLookupHandler(NewMessageArgs t)
        {
            var request = t.Payload as CallLookupResponse;
            if (request != null)
            {
            }
            return null;
        }

        private Response CallEventHandler(NewMessageArgs t)
        {
            var request = t.Payload as CallEvent;
            if (request != null)
            {
                switch (request.EventType)
                {
                    case CallEvent.CallEventType.Alerting:
                        CallNew(request);
                        break;

                    case CallEvent.CallEventType.Connected:
                        CallRinging(request);
                        break;
                }
            }
            return null;
        }

        private Response CallEndHandler(NewMessageArgs t)
        {
            var request = t.Payload as CallEnd;
            if (request != null)
            {
            }
            return null;
        }

        private Response CallLogonHandler(NewMessageArgs t)
        {
            var request = t.Payload as CallLogon;
            if (request != null)
            {
            }
            return null;
        }

        private Response CallLogoffHandler(NewMessageArgs t)
        {
            var request = t.Payload as CallLogoff;
            if (request != null)
            {
            }
            return null;
        }

    }
}