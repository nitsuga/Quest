using Autofac;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Quest.Cmd
{
    public interface IProcessRunner
    {
        ProcessRunnerConfig settings { get; set; }
        bool PrepareProcessors(IServiceProvider container, IContainer applicationContainer, IConfiguration config);
        void Start(IServiceProvider container, IContainer applicationContainer);
    }
}
