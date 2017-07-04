using Microsoft.Extensions.Configuration;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.Lib.Utils;

/// <summary>
/// Lightweight processor
/// </summary>
namespace Quest.Lib.Processor
{
    /// <summary>
    ///     base class for job processors
    /// </summary>
    public class ServiceBusProcessor : SimpleProcessor
    {
        /// <summary>
        /// The name of the service bus queue
        /// </summary>
        public string Queue { get; set; }

        public ServiceBusProcessor(TimedEventQueue eventQueue, IServiceBusClient serviceBusClient, MessageHandler msgHandler): base(eventQueue)
        {
            ServiceBusClient = serviceBusClient;
            MsgHandler = msgHandler;
        }

        /// <summary>
        /// Service bus message handler
        /// </summary>
        protected MessageHandler MsgHandler;

        /// <summary>
        /// service bus connection
        /// </summary>
        protected IServiceBusClient ServiceBusClient;


        /// <summary>
        /// called by the framework to initialise this unit
        /// </summary>
        /// <param name="id"></param>
        /// <param name="localParams"></param>
        /// <param name="globalParams"></param>
        /// <returns></returns>
        public override void Prepare(ProcessingUnitId id, IConfiguration config)
        {
            // get subclass name and start up mesage queue
            Queue = $"{id.Name}_{id.Session}_{id.Instance}";

            LogMessage($"Attaching {Id.Name} to queue {Queue}");

            ServiceBusClient.Initialise(Queue);
            ServiceBusClient.NewMessage += (s, e) => MsgHandler.ProcessMessage(ServiceBusClient, e);

            base.Prepare(id, config);

            MsgHandler.SetReply(Queue);
            MsgHandler.AddHandler<StopProcessingRequest>(StopProcessorHandler);
            MsgHandler.AddHandler<StartProcessingRequest>(StartProcessorHandler);
        }

        private Response StopProcessorHandler(NewMessageArgs t)
        {
            var request = t.Payload as StopProcessingRequest;
            StopRunning.Set();
            OnStop();
            return new StopProcessingResponse();
        }

        private Response StartProcessorHandler(NewMessageArgs t)
        {
            var request = t.Payload as StartProcessingRequest;

            if (request.Id.Session != Id.Session)
                return null;

            if (request.Id.Instance != Id.Instance)
                return null;

            if (request.Id.Name != Id.Name)
                return null;

            if (Status != ProcessorStatusCode.Ready)
                return null;

            Start();

            return new StartProcessingResponse();
        }

        protected override void SetStatus(ProcessorStatusCode status)
        {
            base.SetStatus(status);
            ServiceBusClient.Broadcast(new ProcessorStatus { Id = Id, Status = status });
        }

        protected override void SetPrepareStatus(int percentComplete, string message)
        {
            base.SetPrepareStatus(percentComplete, message);
            ServiceBusClient.Broadcast(new ProcessorPrepareStatus { Id = Id, Message = message, PercentComplete = percentComplete });
        }

        /// <summary>
        /// fires a message at a specified time
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fireTime"></param>
        /// <param name="message"></param>
        protected void SetTimedMessage(string key, DateTime fireTime, MessageBase message)
        {
            var msg = new TimedEventRequest() { Key = key, FireTime = fireTime, Message = message };
            var queue = $"TimedEventManager_{Id.Session}_{Id.Instance}";
            ServiceBusClient.Send(msg, queue);
        }

        public void SetMessage(string message, TraceEventType severity = TraceEventType.Information)
        {
            LogMessage(message, severity);
            ServiceBusClient.Broadcast(new ProcessorMessage { Id = Id, Message = message, Severity = severity });
        }
    }
}



