#if OAUTH
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Quest.Mobile.Startup))]
namespace Quest.Mobile
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseCors(CorsOptions.AllowAll);
            ConfigureAuth(app);
        }
    }
}
#endif
