using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Quest.Lib.ServiceBus.Messages;
using Quest.Lib.Processor;
using Quest.Lib.Utils;

namespace Quest.Lib.Trackers
{
    [Export]
    public class TimedEventManager : Processor
    {

        [Import]
        private TimedEventQueue _eventQueue;

        protected override void OnPrepare()
        {
                try
                {
                    Initialise();

                    // create a list of actions associated with each object type arriving from the queue
                    MsgHandler.AddActionHandler<TimedEventRequest>(TimedEventRequestHandler);
                    
                    SetMessage(data, "TimedEvent  initialised");
                    Wait();
                }
                catch (Exception ex)
                {
                    SetMessage(data, $"Failed: {ex.Message}", TraceEventType.Error);
                }
        }
       

        /// <summary>
        /// </summary>
        private void Initialise()
        {
            _eventQueue.Now = DateTime.Now;

            // how do we get the speed here??
            _eventQueue.Speed = 1;
            _eventQueue.Start();
            _eventQueue.TimeChanged += _eventQueue_TimeChanged1;
        }

        private void _eventQueue_TimeChanged1(object sender, TimeChangedEvent e)
        {
            ServiceBusClient.Broadcast(new TimedEventTimeChange { Time = e.Value });
        }

        private void TimedEventRequestHandler(MessageBase msg)
        {
            var request = (TimedEventRequest)msg;
            if (request != null)
            {
                var taskEntry = new TaskEntry(_eventQueue, new TaskKey(request.Key, ""), Fire, request.Message, request.FireTime);
            }
        }

        private void Fire(TaskEntry te)
        {
            ServiceBusClient.Broadcast((MessageBase)(te.DataTag));
        }

   }
}