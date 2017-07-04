using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessageBroker.Objects;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System.Diagnostics;
using Quest.Lib.Utils;
using Quest.Lib;

namespace Quest.XC
{
    /// <summary>
    /// provides a connection between the database and the message queue. This class processes
    /// messages arriving on the message queue and saves them to the database. It also monitors
    /// the database for XC outbound messages and puts them on the message queue
    /// </summary>
    public class MessageProcessor
    {
        private int _timeout;
        private int _retries;
        private String _channel="Msg";
        private System.Timers.Timer _timer = new System.Timers.Timer(5000);
        private MessageHelper msgSource;

        public void Initialise(String QueueName)
        {
            Logger.Write(string.Format("Initialising"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "MessageProcessor");

            _timeout = SettingsHelper.GetVariable( _channel + ".SQLTimeout", 3);
            _retries = SettingsHelper.GetVariable( _channel + ".SQLRetries", 5);

            msgSource = new MessageHelper();
            msgSource.Initialise(QueueName);

            msgSource.NewMessage += new EventHandler<MessageBroker.NewMessageArgs>(MessageHelper_NewMessage);
            _timer.Start();
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            Logger.Write(string.Format("Initialised Timeout={0} Retries={1}", _timeout, _retries), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "MessageProcessor");

            using (QuestDataClassesDataContext db = new QuestDataClassesDataContext())
            {
                Logger.Write(string.Format("connection: {0}", db.Database.Connection.ConnectionString), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "MessageProcessor");                
            }
        }

        public void Stop()
        {
            try
            {
                Logger.Write(string.Format("Stopping"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "MessageProcessor");
                msgSource.Stop();
                Logger.Write(string.Format("Stopped"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "MessageProcessor");
            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, LoggingPolicy.Policy.TracePolicy.ToString()))
                    throw;
            }
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ProcessOutboundXC();
        }

        void MessageHelper_NewMessage(object sender, MessageBroker.NewMessageArgs e)
        {
            CloseIncident instance1 = e.Payload as CloseIncident;
            DeleteResource instance2 = e.Payload as DeleteResource;
            BeginDump instance3 = e.Payload as BeginDump;
            IncidentUpdate instance4 = e.Payload as IncidentUpdate;
            ResourceUpdate instance5 = e.Payload as ResourceUpdate;
            ResourceLogon instance6 = e.Payload as ResourceLogon;
            
            int i;

            for (i = 0; i < _retries; i++)
            {
                try
                {
                    if (i != 0)
                        Logger.Write(string.Format("Processing: #{0} {1}", i, e.Payload.ToString()), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _channel);
                    if (instance1 != null) DatabaseWriter.Save(instance1, _timeout, _channel);
                    if (instance2 != null) DatabaseWriter.Save(instance2, _timeout, _channel);
                    if (instance3 != null) DatabaseWriter.Save(instance3, _timeout, _channel);
                    if (instance4 != null) DatabaseWriter.Save(instance4, _timeout, _channel);
                    if (instance5 != null) DatabaseWriter.Save(instance5, _timeout, _channel);
                    if (instance6 != null) DatabaseWriter.Save(instance6, _timeout, _channel);
                    break;
                }
                catch (Exception e1)
                {
                    Logger.Write(string.Format("#{0} {1} {2}", i, e.Payload.ToString(), e1.Message), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _channel);
                }
            }

            if (i != 0)
                Logger.Write(string.Format("Exit @ try #{0}/{1} {2}", i, _retries, e.Payload.ToString()), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _channel);
            else
                Logger.Write(string.Format("Processed: {0}", e.Payload.ToString()), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "Quest Channel " + _channel);
        }

        /// <summary>
        /// read from database and emit to message queue
        /// </summary>
        private void ProcessOutboundXC()
        {
            // record this using _standbySession
            List<int> completed = new List<int>();

            try
            {
                List<MessageBroker.Objects.XCOutbound> list = DatabaseWriter.GetOutboundList(_timeout);

                foreach (var x in list)
                    msgSource.BroadcastMessage(x);

            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, LoggingPolicy.Policy.TracePolicy.ToString()))
                    throw;
            }
        }

    }
}
