using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quest.Common.Messages.Gazetteer;
using Quest.Common.Messages.Gazetteer.Gazetteer;
using Quest.Common.Messages.Resource;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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