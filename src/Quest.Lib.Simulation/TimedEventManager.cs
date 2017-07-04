using System;
using System.Diagnostics;
using Quest.Common.Messages;
using Quest.Lib.Utils;
using Quest.Lib.ServiceBus;
using Quest.Lib.Processor;
using System.Threading.Tasks;
using Quest.Common.ServiceBus;

namespace Quest.Lib.Simulation
{

    public class TimedEventManager : ServiceBusProcessor
    {
        private SimContext _context;

        public TimedEventManager(
            SimContext context,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _context = context;
            _eventQueue = eventQueue;
        }

        
        private TimedEventQueue _eventQueue;

        protected override void OnPrepare()
        {
            _eventQueue.Now = _context.StartDate;
            _eventQueue.Speed = _context.Speed;
        }

        protected override void OnStart()
        {
            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddActionHandler<TimedEventRequest>(TimedEventRequestHandler);

            LogMessage($"Parameters StartTime={_context.StartDate} Speed={_context.Speed}", TraceEventType.Warning);

            _eventQueue.Start();
            _eventQueue.TimeChanged += _eventQueue_TimeChanged1;
        }

        private void _eventQueue_TimeChanged1(object sender, TimeChangedEvent e)
        {
           // ServiceBusClient.Broadcast(new TimedEventTimeChange { Time = e.Value });
        }

        private void TimedEventRequestHandler(MessageBase msg)
        {
            var request = (TimedEventRequest)msg;
            if (request != null)
            {
                var taskEntry = new TaskEntry(_eventQueue, new TaskKey(request.Key, ""), Fire, request.Message, request.FireTime);
                //var t = Task.Factory.StartNew(() =>
                //{
                //});
            }
        }

        private void Fire(TaskEntry te)
        {
            ServiceBusClient.Broadcast((MessageBase)(te.DataTag));
        }

   }
}