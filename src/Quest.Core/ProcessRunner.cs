using Autofac;
using Microsoft.Extensions.Configuration;
using Quest.Common.Messages.System;
using Quest.Lib.Processor;
using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Core
{
    public class ProcessRunnerConfig
    {
        public ProcessRunnerConfig()
        {
            modules = new List<string>();
        }

        public List<string> modules { get; set; }
    }

    public class ProcessRunner : IProcessRunner
    {
        private Dictionary<string, IProcessor> AllProcessors = new Dictionary<string, IProcessor>();

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

            foreach (var proc in settings.modules)
            {
                if (applicationContainer.IsRegisteredWithName<IProcessor>(proc))
                {
                    Logger.Write($"Creating {proc}", GetType().Name);
                    var t = System.IO.Directory.GetCurrentDirectory();
                    IProcessor procInstance = applicationContainer.ResolveNamed<IProcessor>(proc);
                    AllProcessors.Add(proc, procInstance);
                }
                else
                    Logger.Write($"Failed to find processor: {proc}", GetType().Name, System.Diagnostics.TraceEventType.Error);
            }

            foreach (var proc in AllProcessors)
            {
                Logger.Write($"Preparing {proc.Key}", GetType().Name);
                proc.Value.Prepare(new ProcessingUnitId { Name = proc.Key }, config);
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
            var queue = $"ProcessRunner";
            Logger.Write($"Attaching to queue {queue}", GetType().Name);

            foreach (var proc in AllProcessors)
            {
                Logger.Write($"Starting {proc}", GetType().Name);
                proc.Value.Start();
            }
        }
    }
}
