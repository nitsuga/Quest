using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.ResolveAnything;
using Quest.Lib.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Quest.Lib.DataModel;
using Quest.Lib.OS.DataModelOS;
using Quest.Lib.Research.DataModelResearch;
using Quest.Lib.OS.DataModelNLPG;
using Microsoft.Extensions.Logging;
using Quest.Lib.Data;

namespace Quest.Core
{
    class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IContainer ApplicationContainer { get; private set; }

        // Set the Modules environment variable to specify which components to run
        //
        // to run the full web stack set Modules environment variable to:
        // SecurityManager;DeviceManager;NotificationManager;SearchManager;RoutingManager;MapMatcherManager;VisualsManager;IndexerManager

        // for research, use these
        // MapMatcherAll -args=Workers=8,InProcess=false,MapMatcher='HmmViterbiMapMatcher',MaxRoutes=15,RoadGeometryRange=50,RoadEndpointEnvelope=50,DirectionTolerance=120,RoutingEngine='DijkstraRoutingEngine',RoutingData='Standard',MinSeconds=10,Skip=3,Take=9999,Emission='GpsEmission',EmissionP1=1,EmissionP2=0,Transition='Exponential',TransitionP1=0.0168,TransitionP2=0,SumProbability=false,NormaliseTransition=false,NormaliseEmission=false,GenerateGraphVis=false,MinDistance=25,MaxSpeed=80,MaxCandidates=100 -components=components.json
        // MapMatcherWorker -args=Workers=8,InProcess=false,MapMatcher='HmmViterbiMapMatcher',MaxRoutes=15,RoadGeometryRange=50,RoadEndpointEnvelope=50,DirectionTolerance=120,RoutingEngine='DijkstraRoutingEngine',RoutingData='Standard',MinSeconds=10,Skip=3,Take=9999,Emission='GpsEmission',EmissionP1=1,EmissionP2=0,Transition='Exponential',TransitionP1=0.0168,TransitionP2=0,SumProbability=false,NormaliseTransition=false,NormaliseEmission=false,GenerateGraphVis=false,MinDistance=25,MaxSpeed=80,MaxCandidates=100 /taskid=0 /runid=69 /startrouteid=1254133 /endrouteid=1254136
        // AnalyseEdgeCosts
        // to run the simulator
        // simulation.json -exec=RosterSimulator

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


                // load settings file 

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var enc1252 = Encoding.GetEncoding(1252);

                var config = GetConfiguration();
                var global = config.GetSection("global");

                var serviceCollection = new ServiceCollection();

                // load components into repository
                ServiceProvider = ConfigureServices(serviceCollection, config);

                foreach( var component in ApplicationContainer.ComponentRegistry.Registrations)
                {
                    foreach(var service in component.Services)
                        Logger.Write($"Loaded {service.Description}");
                }

                var loggerFactory = ServiceProvider.GetService<ILoggerFactory>();
                loggerFactory
                    .AddConsole()
                    .AddDebug();

                // get the processor runner
                var runner = ApplicationContainer.Resolve<ProcessRunner>();
                if (runner == null)
                {
                    Logger.Write($"Failed to load process runner. The components.json must contain a defition for 'Quest.Cmd.ProcessRunner, Quest.Cmd' marked as single instance", TraceEventType.Error, "Quest.Cmd");
                    return;
                }

                // get list of modules to run from the config file which can be in the config or set 
                // explicitly in the ProcessRunnerConfig properties via autofac property injection
                var modulesToRun = GetProcessorsList(config);
                if (modulesToRun.Count == 0 && runner.settings.modules.Count == 0)
                {
                    Logger.Write($"No modules found to execute. Make sure you specify either -exec or a config file (by setting parameters on ProcessRunner) that contains modules to run", TraceEventType.Error, "Quest.Cmd");
                    return;
                }

                runner.settings.modules.AddRange(modulesToRun);

                WaitFordatabase();

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

        static void WaitFordatabase()
        {
            var dbFactory = ApplicationContainer.Resolve<IDatabaseFactory>();

            var success = false;
            for (int i = 0; i < 60; i++)
            {
                try
                {
                    success = dbFactory.ExecuteNoTracking<QuestContext, bool>((db) =>
                    {
                        db.Database.OpenConnection();
                        Logger.Write($"Successfully opened database...", TraceEventType.Information, "Quest Core");
                        return true;
                    });

                    return;
                }
                catch
                {
                    Logger.Write($"Failed to access database...", TraceEventType.Error, "Quest Core");
                }

                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// Load the configuration
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        internal static IConfiguration GetConfiguration()
        {
            // override config if env variable is set
            var cfgFile = Environment.GetEnvironmentVariable("ComponentsConfig");
            if (string.IsNullOrEmpty(cfgFile))
                cfgFile = "components.json";

            // override app config if env variable is set
            var appFile = Environment.GetEnvironmentVariable("ApplicationConfig");
            if (string.IsNullOrEmpty(appFile))
                appFile = "appsettings.json";

            Logger.Write($"Using ApplicationConfig={appFile}");
            Logger.Write($"Using ComponentsConfig={cfgFile}");

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile(appFile, false)
                .AddJsonFile(cfgFile, false);

            IConfiguration config = configBuilder.Build();

            return config;
        }

        /// <summary>
        /// load components into dependency injection container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        internal static IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddMemoryCache();

            services.AddDbContext<QuestContext>(options => options.UseSqlServer(config.GetConnectionString("Quest")));
            services.AddDbContext<QuestOSContext>(options => options.UseSqlServer(config.GetConnectionString("QuestOS")));
            services.AddDbContext<QuestDataContext>(options => options.UseSqlServer(config.GetConnectionString("QuestData")));
            //services.AddDbContext<QuestSimContext>(options => options.UseSqlServer(config.GetConnectionString("QuestSim")));
            services.AddDbContext<QuestNLPGContext>(options => options.UseSqlServer(config.GetConnectionString("QuestNLPG")));

            LoggerFactory factory = new LoggerFactory();
            services.AddSingleton((ILoggerFactory)factory);
            services.AddLogging();


            var builder = new ContainerBuilder();

            // Register the ConfigurationModule with Autofac.
            builder.RegisterModule(new ConfigurationModule(config));

            // register other libraries for autoinjection
            List<string> libraries = new List<string>();
            var p = config.GetSection("libraries");
            foreach (var item in p.GetChildren())
                libraries.Add(item.Value);
            builder.RegisterModule(new AutofacModule(libraries));

            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            builder.Populate(services);

            ApplicationContainer = builder.Build();

            var provider = new AutofacServiceProvider(ApplicationContainer);
            
            return provider;

        }

        /// <summary>
        /// build a list of modules to execute based on configuration or extra list supplied
        /// </summary>
        /// <param name="config"></param>
        /// <param name="exec">; separated list of modules to load</param>
        /// <returns></returns>
        internal static List<string> GetProcessorsList(IConfiguration config)
        {
            List<string> modules = new List<string>();

            // override app config if env variable is set
            var envmodules = Environment.GetEnvironmentVariable("Modules");
            if (!string.IsNullOrEmpty(envmodules))
            {
                Logger.Write($"Using Modules={envmodules}");
                modules.AddRange(envmodules.Split(";"));
            }               

            var p = config.GetSection("Processors");

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
