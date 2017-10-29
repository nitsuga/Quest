using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
using Quest.WebCore.SignalR;
using System;

namespace Quest.WebCore
{
    public static class ProcessRunnerExtensions
    {
        public static IServiceCollection AddProcessRunnerService(this IServiceCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return collection.AddTransient<IProcessRunner, ProcessRunner>((x) =>new ProcessRunner(collection));
        }
    }

    public interface IProcessRunner
    {
        void Start(IServiceProvider container, IContainer ApplicationContainer, IConfiguration config);
    }

    public class ProcessRunner: IProcessRunner
    {
        private IServiceCollection _serviceCollection;

        public ProcessRunner(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public void Start(IServiceProvider container, IContainer applicationContainer, IConfiguration config)
        {
            var queue = $"WebCore";

            Logger.Write($"Web: Attaching to queue {queue}", GetType().Name);

            var serviceBusClient = container.GetService<Common.ServiceBus.IServiceBusClient>();
            serviceBusClient.Initialise(queue);

            var sbHub = container.GetService<ServiceBusHub>();
            sbHub.Initialise();

            //var msgCache = container.GetService<AsyncMessageCache>();
        }

    }
}
