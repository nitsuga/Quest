using Autofac;
using Quest.Common.ServiceBus;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;

namespace Quest.Lib.Telephony.Avaya
{
    /// <summary>
    /// 
    /// TSAP manager connects to the telephony CTI server and broadcasts
    /// telephony events. The manager also listens for MakeCall requests
    /// to place an outbound call.
    /// 
    /// http://hamstr.net/f%23/2016/12/16/Accessing-Linux-C-libraries-with-F-and-.Net-Core/
    /// 
    /// </summary>

    class TSAPIManager : ServiceBusProcessor
    {
        private readonly ILifetimeScope _scope;

        public string channelConfig { get; set; }

        public TSAPIManager(
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
        }
    }
}
