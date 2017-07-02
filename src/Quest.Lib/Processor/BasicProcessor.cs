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
    public class SimpleProcessor : IProcessor
    {

        /// <summary>
        /// current status
        /// </summary>
        public ProcessorStatusCode Status { get; set; }

        /// <summary>
        /// ID information for this container
        /// </summary>
        public ProcessingUnitId Id { get; set; }

        /// <summary>
        /// configuration parameters set in the json config source
        /// </summary>
        protected IConfiguration Configuration;

        protected TimedEventQueue EventQueue { get; set; }

        public SimpleProcessor(TimedEventQueue _eventQueue)
        {
            EventQueue = _eventQueue;
        }

        /// <summary>
        /// Autoreset event that gets signalled when commanded to stop
        /// </summary>
        public AutoResetEvent StopRunning = new AutoResetEvent(false);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if success</returns>
        protected virtual void OnPrepare()
        { }

        protected virtual void OnStart()
        { }

        protected virtual void OnStop()
        { }

        /// <summary>
        /// called by the framework to initialise this unit
        /// </summary>
        /// <param name="id"></param>
        /// <param name="localParams"></param>
        /// <param name="globalParams"></param>
        /// <returns></returns>
        public virtual void Prepare(ProcessingUnitId id, IConfiguration config)
        {
            Id = id;
            Configuration = config;

            LogMessage($"Preparing");

            SetStatus(ProcessorStatusCode.Preparing);

            // call the jobs' Prepare method
            var t = Task.Factory.StartNew(() =>
            {
                try
                {
                    OnPrepare();
                    SetPrepareStatus(100, $"Prepared");
                    SetStatus(ProcessorStatusCode.Ready);
                }
                catch (Exception ex)
                {
                    SetPrepareStatus(100, $"Failed: {ex.Message}");
                    SetStatus(ProcessorStatusCode.Failed);
                }
            }
                );

        }

        public virtual void Start()
        {
            new TaskFactory().StartNew(() =>
            {
                LogMessage($"Starting", TraceEventType.Start);
                StopRunning.Reset();
                OnStart();
            });
        }

        protected virtual void SetStatus(ProcessorStatusCode status)
        {
            LogMessage($"Status is {Status}");
            Status = status;
        }

        protected virtual void SetPrepareStatus(int percentComplete, string message)
        {
            LogMessage($"Preparation progress {percentComplete}% {message}");
        }

        protected void SetTimedEvent(string key, DateTime fireTime, Action action)
        {
            new TaskEntry(EventQueue, new TaskKey(key), HandleTimedEvent, action, fireTime);
        }

        protected void SetTimedEvent<T>(string key, DateTime fireTime, Action action)
        {
            new TaskEntry(EventQueue, new TaskKey(key), HandleTimedEvent, action, fireTime);
        }

        void HandleTimedEvent(TaskEntry te)
        {
            Action action = (Action)te.DataTag;
            action.Invoke();
        }

        public void LogMessage(string message, TraceEventType severity = TraceEventType.Information)
        {
            string name = $"{Id.Name}:{Id.Session}/{ Id.Instance}";
            Logger.Write(message, severity, name); 
        }
    }
}



