﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Autofac.Configuration;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Quest.Lib.Trace;
using Quest.Api.Options;
using Quest.Api.Filters;
using Quest.Api.Middleware;
using System.Collections.Generic;
using Quest.Lib.DependencyInjection;

namespace Quest.Api
{
    public class Startup
    {
        private IContainer ApplicationContainer { get; set; }
        private IConfigurationRoot Configuration { get; }
        private bool IsAuthEnabled { get; set; } = true;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            List<string> componentsList = new List<string>();

            // set up default listening ports
            var appsettings = Environment.GetEnvironmentVariable("ApplicationConfig");
            if (string.IsNullOrEmpty(appsettings))
                appsettings = "appsettings.json";

            var components = Environment.GetEnvironmentVariable("ComponentsConfig");
            if (string.IsNullOrEmpty(components))
                componentsList.Add("components.json");
            else
                componentsList.AddRange(components.Split(";"));

            Logger.Write($"Using ApplicationConfig={appsettings}");
            Logger.Write($"Using ComponentsConfig={string.Join(";", componentsList)}");

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(appsettings, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            foreach (var com in componentsList)
                configBuilder.AddJsonFile(com, optional: false);

            Configuration = configBuilder.Build();

            var p = Configuration.GetSection("Auth");
            IsAuthEnabled = p.GetValue<bool>("Enabled");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Register the ConfigurationModule with Autofac.
            var builder = new ContainerBuilder();

            var module = new ConfigurationModule(Configuration);
            builder.RegisterModule(module);

            services.AddOptions();

            // Register the IConfiguration instance which JwtIssuerOptions binds against.
            services.Configure<JwtIssuerOptions>(Configuration.GetSection("JwtIssuerOptions"));
            
            if (IsAuthEnabled)
            {
                // Use policy auth.
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("DataAdminReader", policy => policy.RequireClaim("specialrole", "QuestDataAccess"));
                    options.AddPolicy("DataAdminWriter", policy => policy.RequireClaim("specialrole", "QuestDataAccess"));
                });

                // Get options from app settings
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtIssuerOptions:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtoptions =>
                {
                    jwtoptions.SaveToken = true;
                    jwtoptions.ClaimsIssuer = Configuration["JwtIssuerOptions:Issuer"];
                    jwtoptions.Audience = Configuration["JwtIssuerOptions:Audience"];
                    jwtoptions.Authority = Configuration["JwtIssuerOptions:Authority"];
                    jwtoptions.RequireHttpsMetadata = false;
                    jwtoptions.Configuration = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration
                    {
                         
                    };
                    jwtoptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = Configuration["JwtIssuerOptions:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = Configuration["JwtIssuerOptions:Audience"],

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,

                        RequireExpirationTime = true,
                        ValidateLifetime = true,

                        ClockSkew = TimeSpan.Zero
                    };
                });
            }
            //    else
            //    {
            //        options.AddPolicy("DataAdminReader", policy => policy.RequireAssertion(x => true));
            //        options.AddPolicy("DataAdminWriter", policy => policy.RequireAssertion(x => true));
            //    }
            //});

            services.AddMvc(config =>
            {
                if (IsAuthEnabled)
                {
                    // Make authentication compulsory across the board (i.e. shut
                    // down EVERYTHING unless explicitly opened up).
                    var policy = new AuthorizationPolicyBuilder()
                             .RequireAuthenticatedUser()
                             .Build();
                    config.Filters.Add(new AuthorizeFilter(policy));
                }
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.Formatting = Formatting.Indented;
                options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
                options.SerializerSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;

                options.SerializerSettings.Error = (x, y) =>
                {
                    Logger.Write("JSON error");
                };
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            });

            services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Info { Title = "Quest API", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
            var xmlPath = Path.Combine(basePath, "Quest.Api.xml");
            c.IncludeXmlComments(xmlPath);
        });

            services.ConfigureSwaggerGen(options =>
            {
                // UseFullTypeNameInSchemaIds replacement for .NET Core
                options.CustomSchemaIds(x => x.FullName);
                options.OperationFilter<AuthorizationHeaderFilter>();
            });

            // Add any Autofac modules or registrations.
            // register other libraries for autoinjection
            List<string> libraries = new List<string>();
            var p = Configuration.GetSection("libraries");
            foreach (var item in p.GetChildren())
                libraries.Add(item.Value);
            builder.RegisterModule(new AutofacModule(libraries));


            // Populate the services.
            builder.Populate(services);

            // Build the container.
            ApplicationContainer = builder.Build();

            var loggerFactory = ApplicationContainer.Resolve<ILoggerFactory>();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Enable for file logging..
            //loggerFactory.AddFile(Configuration.GetSection("Logging"));

            // integrate error handling into the pipeline
            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quest Api V1");
            });

            if (IsAuthEnabled)
            {
                app.UseAuthentication();
            }

            app.UseMvc();
        }
    }
}