using Autofac;
using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
using Quest.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace Quest.Lib.Telephony.AspectCTIPS
{
    public class AspectServer : ServiceBusProcessor
    {
        private List<CTIPSChannel> _ctiChannels;
        private MQChannel _mq;
        private Timer _primarytimer = new Timer();

        private SystemConfig _config;
        private readonly ILifetimeScope _scope;

        public string channelConfig { get; set; }

        public AspectServer(
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
            _mq = new MQChannel(serviceBusClient);
        }



        private Response MakeCallHandler(NewMessageArgs t)
        {
            var request = t.Payload as MakeCall;
            if (request != null)
            {
                _ctiChannels.ForEach(x => x.Dial(request));
            }
            return null;
        }

        protected override void OnStop()
        {
            Stop();
        }

        protected override void OnStart()
        {
            MsgHandler.AddHandler<MakeCall>(MakeCallHandler);
            try
            {
                Logger.Write(string.Format("Controller starting", ToString()), TraceEventType.Information, "AspectServer");

                _config = SystemConfig.Load(channelConfig);

                // start off the CTI links
                _ctiChannels = new List<CTIPSChannel>();
                foreach (var cfg in _config.CTIChannelConfigs)
                {
                    try
                    {
                        if (cfg.Enabled)
                        {
                            CTIPSChannel channel = new CTIPSChannel();
                            channel.Initialise(cfg, _mq);
                            channel.Start();
                            _ctiChannels.Add(channel);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(string.Format("Error: {0}", ex.ToString()), TraceEventType.Information, "AspectServer");

                    }
                }

                Logger.Write(string.Format("Controller \"who is primary\" timer started", ToString()), TraceEventType.Information, "AspectServer");
                _primarytimer.Interval = 5000;
                _primarytimer.Elapsed += _primarytimer_Elapsed;
                _primarytimer.Start();

            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Error: {0}", ex.ToString()), TraceEventType.Information, "AspectServer");
            }
        }

        /// <summary>
        /// check state of each channel and set primary channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _primarytimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var currentPrimaries = _ctiChannels.FindAll(x => x.IsPrimary == true).ToList();
                var candidatePrimary = _ctiChannels.FirstOrDefault(x => x.IsRunning == true);

                //Logger.Write(string.Format("Controller \"who is primary\" timer fired: primaries {0}, candidate primary {1}", currentPrimaries.Count, candidatePrimary), TraceEventType.Information, "AspectServer");

                // no primary available
                if (candidatePrimary == null)
                {
                    // deselect an possible primary
                    _ctiChannels.ForEach(x => x.IsPrimary = false);
                    Logger.Write(string.Format("No channels online !! "), TraceEventType.Information, "AspectServer");
                    return;
                }

                // selectedPrimary!=null

                if (currentPrimaries.Count == 0)
                {
                    Logger.Write(string.Format("Channel {0} is now PRIMARY", candidatePrimary), TraceEventType.Information, "AspectServer");
                    candidatePrimary.IsPrimary = true;
                    return;
                }

                // more than 1 primary ? how did that happen?

                if (ReferenceEquals(candidatePrimary, currentPrimaries[0]))
                {
                    //Logger.Write(string.Format("Channel {0} is (and was) PRIMARY", currentPrimaries[0]), TraceEventType.Information, "AspectServer");
                }
                else
                {
                    Logger.Write(string.Format("Channel {0} is now PRIMARY", currentPrimaries[0]), TraceEventType.Information, "AspectServer");
                }

                _ctiChannels.ForEach(x => x.IsPrimary = false);
                candidatePrimary.IsPrimary = true;

                currentPrimaries = _ctiChannels.FindAll(x => x.IsPrimary == true).ToList();
                candidatePrimary = _ctiChannels.FirstOrDefault(x => x.IsRunning == true);

                //Logger.Write(string.Format("Controller \"who is primary\" timer complete: primaries {0}, candidate primary {1}", currentPrimaries.Count, candidatePrimary), TraceEventType.Information, "AspectServer");
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Error: {0}", ex.ToString()), TraceEventType.Information, "AspectServer");
            }
        }

        public void Stop()
        {
            try
            {
                // start off the CTI links
                foreach (var chan in _ctiChannels)
                {
                    try
                    {
                        chan.Stop();
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(string.Format("Error: {0}", ex.ToString()), TraceEventType.Information, "AspectServer");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Error: {0}", ex.ToString()), TraceEventType.Information, "AspectServer");
            }
        }
    }
}