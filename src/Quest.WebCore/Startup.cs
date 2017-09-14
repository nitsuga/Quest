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

namespace Quest.WebCore
{
    public class Startup
    {
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
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

            // Add any Autofac modules or registrations.
            builder.RegisterModule(new AutofacModule());

            // Add application services.
            services.AddSingleton<IServiceBusClient, ActiveMqClient>();
            services.AddSingleton<MessageCache>();
            services.AddSingleton<ResourceService>();
            services.AddSingleton<IncidentService>();
            services.AddSingleton<DestinationService>();
            services.AddSingleton<SearchService>();
            services.AddSingleton<RouteService>();
            services.AddSingleton<TelephonyService>();
            services.AddSingleton<VisualisationService>();
            services.AddSingleton<SecurityService>();

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
                runner.Start(provider, ApplicationContainer);
            }
            return provider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

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

            app.Use(async (http, next) =>
            {
                if (http.WebSockets.IsWebSocketRequest)
                {
                    //Handle WebSocket Requests here.         
                }
                else
                {
                    await next();
                }
            });

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
