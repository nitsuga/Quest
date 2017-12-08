using Autofac;
using Quest.Common.ServiceBus;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using System.IO;

namespace Quest.LAS.Processor
{
    /// <summary>
    /// Read and write files in ExpressQ directories
    /// Emits CadInboundMessage
    /// Listens for 
    /// </summary>
    public class ExpressQGateway : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        #endregion

        public string UserPath { get; set; }

        public ExpressQGateway(
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
        }

        protected override void OnPrepare()
        {
        }

        protected override void OnStart()
        {
            Run();
        }

        public void Run()
        {
            FileSystemWatcher watcher = new FileSystemWatcher()
            {
                Path = UserPath,
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            };

            watcher.Changed += Watcher_Changed;
            watcher.Created += Watcher_Created;
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            // convert to CadOutboundMessage
            throw new System.NotImplementedException();
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            
        }
    }
}
