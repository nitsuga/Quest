using System;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Quest.Lib.DataModel;
using Quest.Common.Messages;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Common.ServiceBus;
using Quest.Lib.Utils;

namespace Quest.Lib.Trackers
{
    public class CallTracker : ServiceBusProcessor
    {
        public CallTracker(
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
        }

        protected override void OnPrepare()
        {
            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler<CallLookupRequest>(CallEventHandler);
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        static public void recordcallEnd(CallEnd msg)
        {
            using (QuestEntities db = new QuestEntities())
            {

                var call = (from c in db.Calls where c.SwitchId == msg.CallId select c).FirstOrDefault();
                if (call != null)
                {
                    call.Updated = DateTime.Now;
                    call.TimeClosed = DateTime.Now;
                    call.IsClosed = true;
                    db.SaveChanges();
                }
            }
        }

        static public Response CallEventHandler(NewMessageArgs t)
        {
            CallEvent msg = t.Payload as CallEvent;
            using (QuestEntities db = new QuestEntities())
            {
                var call = (from c in db.Calls where c.SwitchId == msg.CallId select c).FirstOrDefault();
                if (call == null)
                {
                    call = new Call();
                    db.Calls.Add(call);
                }

                call.Extension = msg.Extension.ToString();
                if (msg.EventType == CallEvent.CallEventType.Alerting)
                    call.TimeConnected = DateTime.Now;

                if (msg.EventType == CallEvent.CallEventType.Connected)
                    call.TimeAnswered = DateTime.Now;

                call.Updated = DateTime.Now;
                db.SaveChanges();                
            }
            return null;
        }

        static public void recordcallerdetails(CallLookupResponse msg)
        {
            using (QuestEntities db = new QuestEntities())
            {
                var call = (from c in db.Calls where c.SwitchId == msg.CallId select c).FirstOrDefault();
                if (call != null)
                {
                    call.Address1 = msg.Address[0] ?? "";
                    call.Address2 = msg.Address[1] ?? "";
                    call.Address3 = msg.Address[2] ?? "";
                    call.Address4 = msg.Address[3] ?? "";
                    call.Address5 = msg.Address[4] ?? "";
                    call.Address6 = msg.Address[5] ?? "";
                    call.Easting = msg.Eastings;
                    call.Northing = msg.Northings;
                    call.SemiMajor = msg.SemiMajor;
                    call.SemiMinor = msg.SemiMinor;
                    call.Speed = msg.Speed;
                    call.Name = msg.Name ?? "";
                    call.Confidence = msg.Confidence;
                    call.Angle = msg.Angle;
                    call.Altitude = msg.Altitude;
                    call.Direction = msg.Direction;
                    call.Updated = DateTime.Now;
                    call.Status = msg.Status;
                    db.SaveChanges();
                }
            }
        }
    }
}
