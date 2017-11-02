#pragma warning disable 0169,649
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quest.Common.Messages;
using Quest.Lib.Search.Elastic;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using Quest.Lib.Trace;
using Quest.Lib.Processor;
using Quest.Common.ServiceBus;
using Quest.Common.Messages.Telephony;
using Quest.Common.Messages.Gazetteer;

namespace Quest.Lib.EISEC
{
    public class EisecServer : ServiceBusProcessor, IEisecServer
    {
        private SearchEngine _searchEngine;
        private TimedEventQueue _eventQueue;

        /* Diagnostic info */
        private EisecConfig _config;
        private volatile Dictionary<int, QueryRequest> _edbReqs = new Dictionary<int, QueryRequest>();

        public string EisecConfigFile { get; set; }

        private int _request;
        public List<EisecChannel> Channels = new List<EisecChannel>();

        public EisecServer(
            SearchEngine searchEngine,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _searchEngine = searchEngine;
            _eventQueue = eventQueue;
        }

        /// <summary>
        /// version with address matching
        /// </summary>
        /// <param name="eventQueue"></param>
        /// <param name="serviceBusClient"></param>
        /// <param name="msgHandler"></param>
        public EisecServer(
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _eventQueue = eventQueue;
        }

        protected override void OnPrepare()
        {
            // ensure EisecConfigFile is set
            if (EisecConfigFile == null || EisecConfigFile.Length == 0)
                throw new ApplicationException("EisecConfigFile is not set");

            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler<CallLookupRequest>(CallDetailsHandler);
        }

        protected override void OnStart()
        {
            if (!_eventQueue.IsRunning)
                _eventQueue.Start();

            Initialise();
        }

        protected override void OnStop()
        {
        }


        public void Initialise()
        {

            _config = Config.LoadConfig<EisecConfig>(EisecConfigFile);

            if (_config == null)
                _config = new EisecConfig();

            if (_config.IpList == null)
            {
                _config.IpList = new[] { new IpDetails { Addr = "127.0.0.1", Port = 5000, LocalPollTimeoutSeconds = 15, RemotePollTimeoutSeconds = 15, SendPollSeconds = 10, User = new User() { Password = "password", Username = "username" } } };

                Config.SaveConfig(_config, EisecConfigFile);
            }

            var index = 0;
            foreach (var details in _config.IpList)
            {
                var channel = new EisecChannel(details, this, index, (x)=>SetMessage(x));

                channel.StartAutoLogon();

                Channels.Add(channel);
                index++;
            }
        }


        /// <summary>
        ///     removes aprocessed item from the queue
        /// </summary>
        /// <returns></returns>
        private void RemoveQueuedItem(int reqNo)
        {
            lock (_edbReqs)
            {
                if (_edbReqs.ContainsKey(reqNo))
                {
                    // request complete now remove the request from the queue
                    _edbReqs.Remove(reqNo);
                }
            }
            // take this opportunity to send any queued items
            SendQueuedItems();
        }

        private void DoRequery(TaskEntry te)
        {
            var reqNo = (int)te.DataTag;

            lock (_edbReqs)
            {
                if (_edbReqs.ContainsKey(reqNo))
                {
                    var qr = _edbReqs[reqNo];
                    qr.RequeryCount ++;
                    qr.State= RequestState.Queued;
                    SendQueuedItems();
                }
            }
        }

        /// <summary>
        /// gets called by the channel when an address is found by EISEC
        /// here is our opportunity to broadcast the results
        /// </summary>
        /// <param name="reqNo"></param>
        /// <param name="addr"></param>
        public void SetAddress(int reqNo, CallLookupResponse addr)
        {
            lock (_edbReqs)
            {
                if (_edbReqs.ContainsKey(reqNo))
                {
                    var qr = _edbReqs[reqNo];

                    // request complete now remove the request from the queue
                    qr.Address = addr;
                    qr.State = RequestState.Complete;

                    addr.TelephoneNumber = qr.Details.CLI;
                    addr.CallId = qr.Details.CallId;

                    if (_searchEngine != null)
                    {
                        // find the address
                        if (!addr.IsMobile)
                        {
                            int count;
                            var addressParts = new List<string>()
                        {
                            addr.Name
                        };
                            addressParts.AddRange(addr.Address);

                            do
                            {
                                addr.SearchableAddress = string.Join(" ", addressParts);
                                var result = _searchEngine.SemanticSearch(new SearchRequest()
                                {
                                    displayGroup = SearchResultDisplayGroup.none,
                                    searchMode = SearchMode.RELAXED,
                                    searchText = addr.SearchableAddress,
                                    take = 1
                                });
                                count = result.Documents.Count;
                                addressParts.RemoveAt(0);
                            } while (count == 0 && addressParts.Count > 0);

                        }
                    }

                    // if requery is set then put the request into the timed events queue
                    // else get rid of the request
                    if (addr.Requery > 0 && qr.RequeryCount < 3)
                    {
                        // push events onto the queue
                        var taskEntry = new TaskEntry(_eventQueue, new TaskKey("EISEC " + reqNo, "MARK"),
                            DoRequery, reqNo, addr.Requery);
                    }
                    else
                    {
                        RemoveQueuedItem(reqNo);
                    }

                    ServiceBusClient.Broadcast(addr);
                }
            }

            // take this opportunity to send any queued items
            SendQueuedItems();
        }

        /// <summary>
        ///     Get a count of the number of ACTIVE requests
        /// </summary>
        /// <returns></returns>
        private int GetRequestCount()
        {
            var count = 0;
            lock (_edbReqs)
            {
                foreach (var req in _edbReqs.Values)
                    if (req.State == RequestState.Sent)
                        count++;
            }
            return count;
        }

        /// <summary>
        ///     generate a new request ID (0-99)
        /// </summary>
        /// <returns></returns>
        private int GetNextRequestId()
        {
            _request = ++_request%100;
            return _request;
        }

        #region "Public methods"


        private Response CallDetailsHandler(NewMessageArgs t)
        {
            var request = t.Payload as CallLookupRequest;
            if (request != null)
            {
                SubmitQuery(request);
            }
            return null;
        }


        public void UpdatePassword(int index, string password)
        {
            var config = Config.LoadConfig<EisecConfig>(EisecConfigFile);
            config.IpList[index].User.Password = password;
            config.IpList[index].User.Datechanged = DateTime.Now;
            Logger.Write($"Saving config (password on profile {index} changed)", TraceEventType.Information, "EISEC");
            Config.SaveConfig(config, EisecConfigFile);
            Logger.Write($"Save complete for profile {index}", TraceEventType.Information, "EISEC");
        }

        /// <summary>
        ///     queue a telephone lookup query for processing.
        /// </summary>
        /// <returns></returns>
        public EisecResponse SubmitQuery(CallLookupRequest details)
        {
            try
            {
                var request = new QueryRequest
                {
                    TimeSent = DateTime.MinValue,
                    Details = details,
                    RequestId = GetNextRequestId(),
                    State = RequestState.Queued
                };

                /* Now have a free slot */

                lock (_edbReqs)
                {
                    _edbReqs.Add(request.RequestId, request);
                }

                // attempt to send the queued requests
                return SendQueryToEisec(request);
            }
            catch 
            {
            }

            return new EisecResponse(ReturnCode.Exception);
        }

        #endregion

        #region "Send EISEC requests"

        /// <summary>
        ///     send any queued items upto a maximum number of requests
        /// </summary>
        private void SendQueuedItems()
        {
            RemoveOld();

            lock (_edbReqs)
            {
                var l = _edbReqs.Values.Where(request => request.State == RequestState.Queued);
                foreach (var queryRequest in l)
                    SendQueryToEisec(queryRequest);
            }
        }

        private void RemoveOld()
        {
            lock (_edbReqs)
            {
                // longest sent time before cleaning out
                double MaxQueueSentTime = 120;

                // longest completed time
                double MaxCompleteTime = 360;

                var noreply = _edbReqs.Where(request => request.Value.State == RequestState.Sent && (DateTime.UtcNow - request.Value.TimeSent).TotalSeconds > MaxQueueSentTime).ToList();

                foreach (var request in noreply)
                    _edbReqs.Remove(request.Key);

                var completed = _edbReqs.Where(request => request.Value.State == RequestState.Complete && (DateTime.UtcNow - request.Value.TimeSent).TotalSeconds > MaxCompleteTime).ToList();

                foreach (var request in completed)
                    _edbReqs.Remove(request.Key);

            }

        }


        private EisecResponse SendQueryToEisec(QueryRequest edbReq)
        {
            SetMessage($"Sending to EISEC {edbReq.Details.CLI}");

            var channel = Channels.FirstOrDefault(x => x.Status == EisecChannel.State.StateLoggedIn);

            /* If we are not connected to the EDB, ignore this */
            if (channel == null)
            {
                var message = "Request failed, No open channels to EISEC";
                Logger.Write(message, TraceEventType.Information, "EISEC");

                return new EisecResponse(ReturnCode.NotLoggedIn, message, 0, null);
            }

            var reqPkt = new EisecAddressQueryReq
            {
                Request = edbReq.RequestId,
                Number = edbReq.Details.CLI
            };

            edbReq.TimeSent = DateTime.UtcNow;
            edbReq.State = RequestState.Sent;

            var sent = GetRequestCount();
            var message3 = $"{_edbReqs.Count - sent} Queued, {sent} Active";
            Logger.Write(message3, TraceEventType.Information, "EISEC");
            channel.SendPacketToEisec(reqPkt.Serialize());

            return new EisecResponse(ReturnCode.Success, "Request Queued", edbReq.RequestId, null);
        }


        #endregion
    }
}