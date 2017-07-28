using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.ResolveAnything;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quest.Lib.Processor;
using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Quest.Cmd
{
    class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IContainer ApplicationContainer { get; private set; }

        private static Dictionary<string, string> switchMappings = new Dictionary<string, string>
            {
                {"-config", "config" },
                {"-c", "config" },
                {"-session", "session" },
                {"-components", "components" },
                {"-exec", "exec" },
                {"-args", "args" },
            };

        // to run the simulator
        // -components=simulation.json -exec=RosterSimulator

        // to run the full stack
        // -exec=SearchManager;SecurityManager;RoutingManager;MapMatcherManager;VisualsManager;IndexerManager

        // for research, use these
        // -exec=MapMatcherAll -args=Workers=8,InProcess=false,MapMatcher='HmmViterbiMapMatcher',MaxRoutes=15,RoadGeometryRange=50,RoadEndpointEnvelope=50,DirectionTolerance=120,RoutingEngine='DijkstraRoutingEngine',RoutingData='Standard',MinSeconds=10,Skip=3,Take=9999,Emission='GpsEmission',EmissionP1=1,EmissionP2=0,Transition='Exponential',TransitionP1=0.0168,TransitionP2=0,SumProbability=false,NormaliseTransition=false,NormaliseEmission=false,GenerateGraphVis=false,MinDistance=25,MaxSpeed=80,MaxCandidates=100
        // -exec=MapMatcherWorker -args=Workers=8,InProcess=false,MapMatcher='HmmViterbiMapMatcher',MaxRoutes=15,RoadGeometryRange=50,RoadEndpointEnvelope=50,DirectionTolerance=120,RoutingEngine='DijkstraRoutingEngine',RoutingData='Standard',MinSeconds=10,Skip=3,Take=9999,Emission='GpsEmission',EmissionP1=1,EmissionP2=0,Transition='Exponential',TransitionP1=0.0168,TransitionP2=0,SumProbability=false,NormaliseTransition=false,NormaliseEmission=false,GenerateGraphVis=false,MinDistance=25,MaxSpeed=80,MaxCandidates=100 /taskid=0 /runid=69 /startrouteid=1254133 /endrouteid=1254136


        static void Main(string[] args)
        {
            try
            {
                var maj = Assembly.GetExecutingAssembly().GetName().Version.Major;
                var min = Assembly.GetExecutingAssembly().GetName().Version.Minor;

                Logger.Write($"-----------------------------------------", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"--               Q U E S T               ", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"--               V {maj}.{min}           ", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"--  Copyright (C) 2017  Marcus Poulton   ", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"-- This program is free software: you can redistribute it and / or modify", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"-- it under the terms of the GNU General Public License as published by", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"-- the Free Software Foundation, either version 3 of the License, or", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"-- (at your option) any later version.", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"-- This program is distributed in the hope that it will be useful,", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"-- but WITHOUT ANY WARRANTY; without even the implied warranty of", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"-- MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"-- GNU General Public License for more details.", TraceEventType.Information, "Quest.Cmd");
                Logger.Write($"-----------------------------------------", TraceEventType.Information, "Quest.Cmd");

                //var inc = new IncSimulator();
                //var inc = new MapMatcherProcessor();

                // load settings file 
                var configFile = Parameters.GetParameter(args, "-config", null);
                if (configFile!=null)
                    Logger.Write($"config file: {configFile}", TraceEventType.Information, "Quest.Cmd");
                else
                    Logger.Write($"config file: not set", TraceEventType.Information, "Quest.Cmd");

                var sessionOverride = Parameters.GetParameter(args, "-session", "0");                

                // load components file unless overriden on the command line
                var components = Parameters.GetParameter(args, "-components", "components.json");
                if (components != null)
                    Logger.Write($"components file: {components}", TraceEventType.Information, "Quest.Cmd");
                else
                    Logger.Write($"components file: not set", TraceEventType.Information, "Quest.Cmd");


                // load module names to execute 
                var modules = Parameters.GetParameter(args, "-exec", null);
                if (!string.IsNullOrEmpty(configFile) && !string.IsNullOrEmpty(modules))
                {
                    Logger.Write("usage: Quest.Cmd [-config:CONFIGFILENAME] [-components:COMPFILE] [-exec:MODULES]", TraceEventType.Error, "Quest.Cmd");
                    Logger.Write("Specify either a -config or -exec but not both together", TraceEventType.Error, "Quest.Cmd");
                    return;
                }

                var config = GetConfiguration(configFile, components, args, switchMappings);
                var global = config.GetSection("global");

                var serviceCollection = new ServiceCollection();

                // load components into repository
                ServiceProvider = ConfigureServices(serviceCollection, config);

                if (args.Length == 0 && string.IsNullOrEmpty(configFile) && string.IsNullOrEmpty(modules))
                {
                    Logger.Write("usage: Quest.Cmd [-config:CONFIGFILENAME] [-components:COMPFILE] [-exec:MODULES]", TraceEventType.Error, "Quest.Cmd");
                    Logger.Write("Specifying CONFIGFILE", TraceEventType.Error, "Quest.Cmd");

                    var all = ApplicationContainer.Resolve<IEnumerable<IProcessor>>();

                    return;
                }

                // get the processor runner
                var runner = ApplicationContainer.Resolve<ProcessRunner>();
                if (runner == null)
                {
                    Logger.Write($"Failed to load process runner. The components.json must contain a defition for 'Quest.Cmd.ProcessRunner, Quest.Cmd' marked as single instance", TraceEventType.Error, "Quest.Cmd");
                    return;
                }

                // get list of modules to run from the config file which can be in the config or set 
                // explicitly in the ProcessRunnerConfig properties via autofac property injection
                var modulesToRun = GetProcessorsList(config, modules);
                if (modulesToRun.Count == 0 && runner.settings.modules.Count==0)
                {
                    Logger.Write($"No modules found to execute. Make sure you specify either -exec or a config file (by setting parameters on ProcessRunner) that contains modules to run", TraceEventType.Error, "Quest.Cmd");
                    return;
                }

                var session = GetSession(config, sessionOverride);
                runner.settings.session = session;
                runner.settings.modules.AddRange(modulesToRun);                

                // prepare processors ready for action
                var Ok = runner.PrepareProcessors(ServiceProvider, ApplicationContainer, config);
                if (Ok)
                {
                    // start them off
                    runner.Start(ServiceProvider, ApplicationContainer);
                    while (true)
                        Thread.Sleep(100);
                }
            }

            catch (Exception ex)
            {
                Logger.Write(ex.ToString(), TraceEventType.Error, "Quest.Cmd");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Load the configuration
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        internal static IConfiguration GetConfiguration(string configFile, string components, string[] args, Dictionary<string, string> switchMappings)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddCommandLine(args, switchMappings)
                .AddJsonFile("appsettings.json", true );
            //.AddCommandLine(args, GetSwitchMappings(switchMappings));

            if (!string.IsNullOrEmpty(configFile))
                configBuilder.AddJsonFile(configFile);

            if (!string.IsNullOrEmpty(components))
                configBuilder.AddJsonFile(components);

            IConfiguration config = configBuilder.Build();

            return config;
        }

        /// <summary>
        /// load components into repository
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        internal static IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration config)
        {

            // Register the ConfigurationModule with Autofac.
            var module = new ConfigurationModule(config);

            var builder = new ContainerBuilder();
            builder.RegisterModule(module);

            //services.AddProcessRunnerService();

            var dataAccess = Assembly.GetExecutingAssembly();

            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            builder.Populate(services);
            ApplicationContainer = builder.Build();

            var provider = new AutofacServiceProvider(ApplicationContainer);
            return provider;

        }

        /// <summary>
        /// get the session id
        /// </summary>
        /// <param name="config"></param>
        /// <param name="sessionOverride"></param>
        /// <returns></returns>
        internal static string GetSession(IConfiguration config, string sessionOverride)
        {
            var global = config.GetSection("global");
            var session = global["session"];
            if (session == null || session.Length == 0)
                session = sessionOverride;
            return session;
        }

        /// <summary>
        /// build a list of modules to execute based on configuration or extra list supplied
        /// </summary>
        /// <param name="config"></param>
        /// <param name="exec">; separated list of modules to load</param>
        /// <returns></returns>
        internal static List<string> GetProcessorsList(IConfiguration config, string exec)
        {
            List<string> modules = new List<string>();

            var p = config.GetSection("Processors");

            if (!string.IsNullOrEmpty(exec))
                modules.AddRange(exec.Split(';'));

            foreach (var item in p.GetChildren())
            {
                var proc = item["processor"];
                var enabled = item["enabled"];
                if (enabled == null || enabled.ToUpper() == "TRUE")
                        modules.Add(proc);
            }

            return modules;
        }
    }
}
