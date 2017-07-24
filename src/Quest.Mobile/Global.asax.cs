using Quest.Mobile.Properties;
using System;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Microsoft.Extensions.Configuration;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using System.Collections.Generic;
using Quest.Mobile.Controllers;
using System.Data.Entity;
using Quest.Mobile.Code;

namespace Quest.Mobile
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static MessageCache MsgClientCache;
        public static bool RunningOk;
        public static bool Initialised;
        public static string FailureMessage = "";
        IContainer container;

        public MvcApplication()
        {
            BeginRequest += MvcApplication_BeginRequest;
        }

        void MvcApplication_BeginRequest(object sender, EventArgs e)
        {
            while (!Initialised)
                Thread.Sleep(500);
        }

        void Initialise()
        {
            try
            {
                var args = Settings.Default.Args.Split(' ');

                var parts = Settings.Default.Parts.OfType<string>().ToArray();

                StartMessageQueue();

            }
            catch (Exception ex)
            {
                RunningOk = false;
                var sb = new StringBuilder();

                for (var e = ex; e != null; e = e.InnerException)
                {
                    sb.Append(e.Message);
                    sb.Append("\n");
                }

                FailureMessage = sb.ToString();
            }
            Initialised = true;
        }

        private void StartMessageQueue()
        {
            var queueName = Settings.Default.Queue;

#if DEBUG_PROFILE
            Environment.SetEnvironmentVariable("ActiveMQ", "activemq:tcp://127.0.0.1:61616");
#endif

            MsgClientCache = container.Resolve<MessageCache>();

            MsgClientCache.Initialise(queueName);
        }

        protected void Application_Start()
        {
            Logger.Write("web site starting", TraceEventType.Information, "Web");

            //SqlServerTypes.Utilities.LoadNativeAssemblies(Server.MapPath("~/bin"));

            IServiceProvider provider = Container_Start();

            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalConfiguration.Configuration.EnsureInitialized();

            if (Settings.Default.DownFlag)
            {
                Logger.Write("Web is set to DOWN", TraceEventType.Information, "Web");
                RunningOk = false;
                FailureMessage = Settings.Default.DownMessage;
                return;
            }

            RunningOk = true;

            if (!Initialised)
                Initialise();
        }

        IServiceProvider Container_Start()
        {
            var configBuilder = new ConfigurationBuilder()
                //.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("components.json")
                .AddEnvironmentVariables();

            IConfiguration config = configBuilder.Build();

            // Register the ConfigurationModule with Autofac.
            var module = new ConfigurationModule(config);

            var builder = new ContainerBuilder();

            builder.RegisterModule(module);

            // Register your MVC controllers. (MvcApplication is the name of
            // the class in Global.asax.)
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            //builder.RegisterApiControllers(Assembly.GetExecutingAssembly()); //Register WebApi Controllers
            builder.RegisterType<NotificationsController>().InstancePerRequest();
            builder.RegisterType<DeviceController>().InstancePerRequest();
            builder.RegisterType<SearchController>().InstancePerRequest();
            builder.RegisterType<ResourcesController>().InstancePerRequest();            

            // OPTIONAL: Register model binders that require DI.
            //builder.RegisterModelBinders(typeof(MvcApplication).Assembly);
            //builder.RegisterModelBinderProvider();

            // OPTIONAL: Register web abstractions like HttpContextBase.
            //builder.RegisterModule<AutofacWebTypesModule>();

            // OPTIONAL: Enable property injection in view pages.
            //builder.RegisterSource(new ViewRegistrationSource());

            // OPTIONAL: Enable property injection into action filters.
            //builder.RegisterFilterProvider();

            //builder.RegisterAssemblyTypes(
            //    Assembly.GetExecutingAssembly())
            //    .Where(t => !t.IsAbstract && typeof(ApiController).IsAssignableFrom(t))
            //    .InstancePerMatchingLifetimeScope("AutofacWebRequest");

            // OPTIONAL: Enable action method parameter injection (RARE).
            //builder.InjectActionInvoker();

            // Set the dependency resolver to be Autofac.
            container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver((IContainer)container); //Set the WebApi DependencyResolver

            var provider = new AutofacServiceProvider(container);

            return provider;

        }

        public Type TryFindConfigurationType(Type contextType, IEnumerable<Type> typesToSearch = null)
        {
            var typeFromAttribute = contextType.GetCustomAttributes(inherit: true)
                                               .OfType<DbConfigurationTypeAttribute>()
                                               .Select(a => a.ConfigurationType)
                                               .FirstOrDefault();

            if (typeFromAttribute != null)
            {
                if (!typeof(DbConfiguration).IsAssignableFrom(typeFromAttribute))
                {
                    Console.Error.Write("Bad 1");
                }
                return typeFromAttribute;
            }

            var configurations = (typesToSearch ?? contextType.Assembly.GetAccessibleTypes())
                .Where(
                    t => typeof(DbConfiguration).IsAssignableFrom(t)
                         && t != typeof(DbConfiguration)
                         && !t.IsAbstract
                         && !t.IsGenericType)
                .ToList();

            if (configurations.Count > 1)
            {
                Console.Error.Write("Bad 2");
            }

            return configurations.FirstOrDefault();
        }
    }
}



