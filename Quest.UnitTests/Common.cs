using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.ResolveAnything;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quest.Lib.DataModel;
using Quest.Lib.DependencyInjection;
using Quest.Lib.OS.DataModelNLPG;
using Quest.Lib.OS.DataModelOS;
using Quest.Lib.Research.DataModelResearch;
using Quest.Lib.Simulation.DataModelSim;
using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quest.UnitTests
{
    public static class Common
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IContainer ApplicationContainer { get; private set; }

        public static void Init()
        {
            var config = GetConfiguration();

            var serviceCollection = new ServiceCollection();

            // load components into repository
            ServiceProvider = ConfigureServices(serviceCollection, config);
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
            services.AddDbContext<QuestSimContext>(options => options.UseSqlServer(config.GetConnectionString("QuestSim")));
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


    }
}
