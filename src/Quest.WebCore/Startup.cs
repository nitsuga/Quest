using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Configuration;
using Quest.WebCore.Services;
using Quest.Common.ServiceBus;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Models;
using Quest.Lib.DependencyInjection;
using System.Collections.Generic;
using Quest.WebCore.SignalR;
using Quest.Lib.Trace;

namespace Quest.WebCore
{
    public class Startup
    {
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            Logger.Write($"Hosting environment:  WebRootPath={env.WebRootPath}");
            Logger.Write($"                   :  App={env.ApplicationName}");
            Logger.Write($"                   :  ContentRootPath={env.ContentRootPath}");
            Logger.Write($"                   :  Env={env.EnvironmentName}");

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables()
                .AddJsonFile("components.json", optional: false)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Register the ConfigurationModule with Autofac.
            var builder = new ContainerBuilder();

            var module = new ConfigurationModule(Configuration);
            builder.RegisterModule(module);

            services.AddMvc();

            services.AddSignalR();

            // Add Application Services
            services.AddScoped<IViewRenderService, ViewRenderService>();

            // Add Plugin Services
            services.AddScoped<IPluginService, PluginService>();

            // Add any Autofac modules or registrations.
            // register other libraries for autoinjection
            List<string> libraries = new List<string>();
            var p = Configuration.GetSection("libraries");
            foreach (var item in p.GetChildren())
                libraries.Add(item.Value);
            builder.RegisterModule(new AutofacModule(libraries));

            // Add any Autofac modules or registrations.
            builder.RegisterModule(new Modules.PluginModule());

            // Add application services.
            //services.AddSingleton<IServiceBusClient, ActiveMqClientAsync>();
            //services.AddSingleton<AsyncMessageCache>();
            services.AddSingleton<ResourceService>();
            services.AddSingleton<IncidentService>();
            services.AddSingleton<DestinationService>();
            services.AddSingleton<SearchService>();
            services.AddSingleton<RouteService>();
            services.AddSingleton<TelephonyService>();
            services.AddSingleton<VisualisationService>();
            services.AddSingleton<SecurityService>();
            services.AddSingleton<ServiceBusHub>();

            services.AddProcessRunnerService();

            // Populate the services.
            builder.Populate(services);

            // Build the container.
            this.ApplicationContainer = builder.Build();
            var provider = new AutofacServiceProvider(this.ApplicationContainer);

            var runner = provider.GetRequiredService<IProcessRunner>();
            if (runner != null)
            {
                // start them off
                runner.Start(provider, ApplicationContainer, Configuration);
            }
            return provider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //app.UseSession( new SessionOptions());
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseSignalR((routes)=> {
                routes.MapHub<CentralHub>("hub");
            });

            //app.Use(async (http, next) =>
            //{
            //    if (http.WebSockets.IsWebSocketRequest)
            //    {
            //        Handle WebSocket Requests here.         
            //    }
            //    else
            //    {
            //        await next();
            //    }
            //});

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            appLifetime.ApplicationStopped.Register(() => this.ApplicationContainer.Dispose());


        }
    }
}
