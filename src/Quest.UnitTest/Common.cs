using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Quest.Lib.Trace;
using Quest.Lib.DependencyInjection;
using System.Diagnostics;
using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;

namespace Quest.UnitTest
{
    public class Common 
    {
        static bool _initialised = false;

        public static IServiceProvider ServiceProvider { get; private set; }
        public static IContainer ApplicationContainer { get; private set; }


        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            if (!_initialised)
            {

                //webAPI = new Dungbeetle.ClientAPI.Web.Client();

                Logger.Write("Test logger started", TraceEventType.Verbose);


                IServiceCollection serviceCollection = new ServiceCollection();

                var config = GetConfiguration(null, "components.json");

                ServiceProvider = ConfigureServices(serviceCollection, config);

                var modulesToRun = GetProcessorsList(config, "");

                //dbFactory = ApplicationContainer.Resolve<DatabaseFactory>();

                _initialised = true;

            }
        }

        /// <summary>
        /// build a list of modules to execute based on configuration or extra list supplied
        /// </summary>
        /// <param name="config"></param>
        /// <param name="batchOverride"></param>
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

        /// <summary>
        /// Load the configuration
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        internal static IConfiguration GetConfiguration(string configFile, string components)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (!string.IsNullOrEmpty(configFile))
                configBuilder.AddJsonFile(configFile);

            if (!string.IsNullOrEmpty(components))
                configBuilder.AddJsonFile(components);

            IConfiguration config = configBuilder.Build();

            return config;
        }


        public static IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            // Register the ConfigurationModule with Autofac.
            var module = new ConfigurationModule(config);

            var builder = new ContainerBuilder();
            builder.RegisterModule(module);

            // Add any Autofac modules or registrations.
            builder.RegisterModule(new AutofacModule(new string[] { "Quest.Lib" }));

            //services.AddProcessRunnerService(config);

            builder.Populate(services);
            ApplicationContainer = builder.Build();

            var provider = new AutofacServiceProvider(ApplicationContainer);
            return provider;

        }

    }
}