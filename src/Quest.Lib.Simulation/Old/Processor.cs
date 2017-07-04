using Quest.Lib.ServiceBus;
using Quest.Lib.ServiceBus.Messages;
using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quest.Lib.Simulation
{
    /// <summary>
    ///     base class for job processors
    /// </summary>
    public class Processor
    {
        private const string Name = "Processor";
        private const string Trace = "Trace";

        [Import]
        protected MessageHandler MsgHandler;

        [Import]
        protected IServiceBusClient ServiceBusClient;

        public event EventHandler<JobStatusArgs> StatusChanged;

        public AutoResetEvent StopRunning = new AutoResetEvent(false);

        public bool Start(string Configuration, string QueueName)
        {
            Logger.Write("Starting");

            // get subclass name and start up mesage queue
            ServiceBusClient.Initialise(QueueName);
            ServiceBusClient.NewMessage += (s, e) => MsgHandler.ProcessMessage(ServiceBusClient, e);

            MsgHandler.AddHandler<StopProcessorResponse>(StopProcessorHandler);

            // call the jobs' start method
            Start(data);

            return true;
        }

        private Response StopProcessorHandler(NewMessageArgs t)
        {
            var request = t.Payload as CancelJobRequest;
            if (request?.Jobid == Instance.Info.JobInfoId)
            {
                DoStop();
            }
            return null;
        }

        protected void SetStatus(JobStatusCodes status, bool updateDatabase = true)
        {
            if (Instance == null)
                return;

            Instance.Info.JobStatusId = (int)status;

            if (StatusChanged != null)
                StatusChanged(this, new JobStatusArgs { Code = JobStatusCodes.Complete });

            if (Instance.Info.JobInfoId > 0 && updateDatabase)
                UpdateDatabaseStatus();
        }

        public static void SetMessage(JobInfo info, string message, string name, TraceEventType severity = TraceEventType.Information)
        {
            var msg = String.Format("{1}: Job {0}: {2}", info.JobInfoId, severity, message);
            Logger.Write(msg, LoggingPolicy.Category.Trace, severity, name);
            Debug.Print(msg);

            // not a database job
            if (info.JobInfoId < 1)
                return;

            Write(message, severity.ToString(), 2, info.JobInfoId, TraceEventType.Information);
        }

        public class JobStatusArgs : EventArgs
        {
            public JobStatusCodes Code { get; set; }
        }
    }
}



