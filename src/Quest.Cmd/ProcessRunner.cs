using Autofac;
using Microsoft.Extensions.Configuration;
using Quest.Common.Messages;
using Quest.Lib.Processor;
using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Cmd
{
    public class ProcessRunnerConfig
    {
        public ProcessRunnerConfig()
        {
            modules = new List<string>();
        }

        public List<string> modules { get; set; }
        public string session { get; set; }
    }

    public class ProcessRunner : IProcessRunner
    {
        public ProcessRunnerConfig settings { get; set; }

        public ProcessRunner(ProcessRunnerConfig settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// prpare all the processors and return true if all is ok
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public bool PrepareProcessors(IServiceProvider container, IContainer applicationContainer, IConfiguration config)
        {
            Dictionary<string, IProcessor> AllProcessors = new Dictionary<string, IProcessor>();

            foreach (var proc in settings.modules)
            {
                Logger.Write($"Creating {proc}", GetType().Name);
                IProcessor procInstance = applicationContainer.ResolveNamed<IProcessor>(proc);
                AllProcessors.Add(proc, procInstance);
            }

            foreach (var proc in AllProcessors)
            {
                Logger.Write($"Preparing {proc.Key}", GetType().Name);
                proc.Value.Prepare(new ProcessingUnitId { Session = settings.session, Name = proc.Key }, config);
            }

            Logger.Write($"Waiting for processors to complete preparation", GetType().Name);
            while (AllProcessors.Count(x => x.Value.Status == ProcessorStatusCode.Preparing) >0)
            {
                System.Threading.Thread.Sleep(100);
            }

            int ready = AllProcessors.Count(x => x.Value.Status == ProcessorStatusCode.Ready);
            int failed = AllProcessors.Count(x => x.Value.Status == ProcessorStatusCode.Failed);
            
            Logger.Write($"Processors prepared ready:{ready} failed:{failed}", failed == 0 ? System.Diagnostics.TraceEventType.Information:System.Diagnostics.TraceEventType.Error,  GetType().Name);

            return failed == 0;

        }

        public void Start(IServiceProvider container, IContainer applicationContainer)
        {
            var queue = $"ProcessRunner_{settings.session}";
            Logger.Write($"{settings.session}:ProcessRunner: Attaching to queue {queue}", GetType().Name);
            //IServiceBusClient serviceBusClient = container.GetService<IServiceBusClient>();
            //serviceBusClient.Initialise(queue);

            foreach (var proc in settings.modules)
            {
                Logger.Write($"Starting {proc}", GetType().Name);
                IProcessor procInstance = applicationContainer.ResolveNamed<IProcessor>(proc);
                procInstance.Start();
            }
        }
    }
}
