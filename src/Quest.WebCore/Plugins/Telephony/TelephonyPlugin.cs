using Autofac;
using Microsoft.AspNetCore.Hosting;
using Quest.Common.Messages.Telephony;
using Quest.Lib.DependencyInjection;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Plugins.Lib;

namespace Quest.WebCore.Plugins.Telephony
{
    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("TelephonyPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    [Injection(typeof(TelephonyPlugin), Lifetime.PerDependency)]
    public class TelephonyPlugin : StandardPlugin
    {
        private int _callid = 0;

        public TelephonyPlugin(ILifetimeScope scope, IHostingEnvironment env)
            : base("TelephonyPlugin", "TEL", "hud.plugins.tel.init(panelId, pluginId)", "/plugins/Telephony/Lib", scope, env)
        {
        }

        public TelephonySettings GetSettings()
        {
            return new TelephonySettings
            {
                 Latitude=51.5,
                 Longitude=-0.2,
                 Zoom=12,
                 MapServer = "192.168.0.3:8090"
            };
        }

        public void sendTestCli(string cli, string extension, AsyncMessageCache messageCache)
        {
            _callid++;

            var m1 = new CallLookupRequest() { CallId = _callid, DDI = "", CLI = cli };

            messageCache.BroadcastMessage(m1);

            var m2 = new CallEvent { CallId = _callid, Extension = extension, EventType = CallEvent.CallEventType.Alerting };
            messageCache.BroadcastMessage(m2);

            var m3 = new CallEvent { CallId = _callid, Extension = extension, EventType = CallEvent.CallEventType.Connected };
            messageCache.BroadcastMessage(m3);
        }
    }
}