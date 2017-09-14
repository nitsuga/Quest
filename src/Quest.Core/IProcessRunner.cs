using Autofac;
using Microsoft.Extensions.Configuration;
using System;

namespace Quest.Core
{
    public interface IProcessRunner
    {
        ProcessRunnerConfig settings { get; set; }
        bool PrepareProcessors(IServiceProvider container, IContainer applicationContainer, IConfiguration config);
        void Start(IServiceProvider container, IContainer applicationContainer);
    }
}
