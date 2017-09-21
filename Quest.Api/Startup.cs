using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Autofac.Configuration;
using Autofac;
using Quest.Lib.Trace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Autofac.Extensions.DependencyInjection;
using Quest.Api.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;

namespace Quest.Api
{
    public class Startup
    {
        private const string SecretKey = "needtogetthisfromenvironment";
        private readonly SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

        public IContainer ApplicationContainer { get; private set; }

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var configBuilder = new ConfigurationBuilder()
                //.SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("components.json", optional: false)
                .AddEnvironmentVariables();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Register the ConfigurationModule with Autofac.
            var builder = new ContainerBuilder();

            var module = new ConfigurationModule(Configuration);
            builder.RegisterModule(module);

            services.AddOptions();

            services.AddMvc(config =>
            {
#if !NOSEC
                // Make authentication compulsory across the board (i.e. shut
                // down EVERYTHING unless explicitly opened up).
                var policy = new AuthorizationPolicyBuilder()
                         .RequireAuthenticatedUser()
                         .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
#endif
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.Formatting = Formatting.Indented;
                options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
                options.SerializerSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;

                options.SerializerSettings.Error = (x, y) => {
                    Logger.Write("JSON error");
                };
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                //options.SerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            });

            // Use policy auth.
            services.AddAuthorization(options =>
            {
#if NOSEC
                options.AddPolicy("DataAdminReader", policy => policy.RequireAssertion(x => true));
                options.AddPolicy("DataAdminWriter", policy => policy.RequireAssertion(x => true));
#else
                options.AddPolicy("DataAdminReader", policy => policy.RequireClaim("specialrole", "DungbeetleDataAccess"));
                options.AddPolicy("DataAdminWriter", policy => policy.RequireClaim("specialrole", "DungbeetleDataAccess"));
#endif
            });

#if NOSEC
#else
            // Get options from app settings
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });
#endif

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
            });



            // Add any Autofac modules or registrations.
            builder.RegisterModule(new AutofacModule());

            // Populate the services.
            builder.Populate(services);

            // Build the container.
            ApplicationContainer = builder.Build();

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
            //loggerFactory.AddFile(Configuration.GetSection("Logging"));

            //var logger = loggerFactory.CreateLogger("Dungbeetle.Api");
            //var dbFactory = ApplicationContainer.Resolve<DatabaseFactory>();
            //Logger.Configure(logger, dbFactory);

            // integrate error handling into the pipeline
            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            app.UseMvc();
        }
    }
}
