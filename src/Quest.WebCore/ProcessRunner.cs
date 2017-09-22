﻿using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
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
        void Start(IServiceProvider container, IContainer ApplicationContainer);
    }

    public class ProcessRunner: IProcessRunner
    {
        IServiceCollection _serviceCollection;

        public ProcessRunner(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public void Start(IServiceProvider container, IContainer applicationContainer)
        {
            var queue = $"WebCore";
            Logger.Write($"Web: Attaching to queue {queue}", GetType().Name);

            var serviceBusClient = container.GetService<Common.ServiceBus.IServiceBusClient>();
            serviceBusClient.Initialise(queue);

            var msgCache = container.GetService<MessageCache>();
            msgCache.Initialise(queue);

        }
    }
}