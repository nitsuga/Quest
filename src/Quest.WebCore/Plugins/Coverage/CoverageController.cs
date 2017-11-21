using Microsoft.AspNetCore.Mvc;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Services;

namespace Quest.WebCore.Plugins.Coverage
{
    public class CoverageController : Controller
    {
        private AsyncMessageCache _messageCache;
        private CoveragePlugin _plugin;

        public CoverageController(
                CoveragePlugin plugin,
                AsyncMessageCache messageCache,
                IPluginService pluginFactory
            )
        {
            _messageCache = messageCache;
            _plugin = plugin;
        }

        [HttpGet]
        public CoverageSettings GetSettings()
        {
            return _plugin.GetSettings();
        }


   }
}