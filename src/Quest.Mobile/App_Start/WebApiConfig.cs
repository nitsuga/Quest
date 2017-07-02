using System.Web.Http;
#if OAUTH
using Microsoft.Owin.Security.OAuth;
#endif

namespace Quest.Mobile
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
#if OAUTH
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
#endif
            // Uncomment this line to debug web api issues
            // config.EnableSystemDiagnosticsTracing();

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
