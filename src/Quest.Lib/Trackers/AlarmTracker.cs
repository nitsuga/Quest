using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quest.Lib.Utils;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Threading;
using ServiceBus.Objects;
using System.Data;
using System.Diagnostics;
using Quest.Lib.Processor;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Quest.Lib.Trackers
{
    [Export("AlarmTracker", typeof(ProcessorModule))]
    public class AlarmTracker : ProcessorModule
    {
        private bool _enabled = true;
        private MessageHelper _msgSource;
        private string _QueueName;

        private System.Timers.Timer _timer = new System.Timers.Timer(10000);

        public AlarmTracker()
            : base("Alarm Tracker", Worker)
        {
        }

        /// <summary>
        /// This routing gets called when requested to start.
        /// Perform initialisation here 
        /// </summary>
        /// <param name="module"></param>
        static void Worker(ProcessorModule module)
        {
            AlarmTracker me = module as AlarmTracker;

            me.Initialise();

            for (; ; )
            {
                // StopRunning gets set when the system wants to shut us down
                if (module.StopRunning.WaitOne(1000))
                {
                    if (me._msgSource != null)
                        me._msgSource.Stop(); 
                    return;
                }
            }
        }

        public void Initialise()
        {
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            _timer.Start();
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            /// re-read variables in case something changed.
            _QueueName = SettingsHelper.GetVariable( "AlarmTracker.Queue", "Quest.AlarmTracker");
            _enabled = SettingsHelper.GetVariable("AlarmTracker.Enabled", false);
            
            // we're enabled..
            if (_enabled && _msgSource == null)
            {
                _msgSource = new MessageHelper();
                _msgSource.Initialise(_QueueName); // MDT messages
                _msgSource.NewMessage += new EventHandler<ServiceBus.NewMessageArgs>(msgSource_NewMessage);
                return;
            }

            // we're disabled..
            if (_enabled==false && _msgSource != null)
            {
                _msgSource.Stop();
                _msgSource.Dispose();
                _msgSource = null;
                return;
            }
        }

        void msgSource_NewMessage(object sender, ServiceBus.NewMessageArgs e)
        {
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    Logger.Write(string.Format("Got " + e.Payload.ToString()), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "AlarmTracker");

                    ResourceAlarm alarm1 = e.Payload as ResourceAlarm;
                    IncidentAlarm alarm2 = e.Payload as IncidentAlarm;

                    if (alarm1 != null)
                        ProcessMessage(alarm1);

                    if (alarm2 != null)
                        ProcessMessage(alarm2);

                    break;

                }
                catch (Exception e1)
                {
                    if (e.Payload != null)
                        Logger.Write(string.Format("Try #{0} {1} {2}", retry + 1, e.Payload.ToString(), e1.Message), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "AlarmTracker");
                    else
                        Logger.Write(string.Format("Try #{0} {1}", retry + 1, e1.ToString()), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "AlarmTracker");
                }
            }
        }

        void ProcessMessage(ResourceAlarm message)
        {
            //Stick it in the DB
            using (QuestEntities db = new QuestEntities())
            {
                db.NotifyResource(message.Callsign, message.Message, message.IsWarning, message.Source, message.Destination);
            }
        }

        void ProcessMessage(IncidentAlarm message)
        {
            //Stick it in the DB
            using ( var db = new QuestEntities())
            {
                    db.NotifyEvent(message.Incident, message.Message, message.IsWarning, message.Source, message.Destination);
            }

        }

    }
}
