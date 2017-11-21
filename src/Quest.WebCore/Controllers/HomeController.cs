using Microsoft.AspNetCore.Mvc;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Models;
using Quest.WebCore.Services;

namespace Quest.WebCore.Controllers
{
    public class HomeController : Controller
    {
        private AsyncMessageCache _messageCache;
        private SecurityService _securityService;
        private readonly IPluginService _pluginService;

        public HomeController(AsyncMessageCache messageCache,
                IPluginService pluginFactory,
                SecurityService securityService
            )
        {
            _messageCache = messageCache;
            _securityService = securityService;
            _pluginService = pluginFactory;
        }

        // GET: Home
        public ActionResult Index()
        {
            var model = new HudModel
            {
                Scripts = _pluginService.GetScripts(),
                Styles = _pluginService.GetStyles(),
                Layout = _pluginService.DefaultLayout()
            };

            return View(model);
        }

    }

}
