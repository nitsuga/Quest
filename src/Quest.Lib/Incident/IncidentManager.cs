#define USE_ELASTIC
using Quest.Lib.Search.Elastic;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Quest.Lib.Processor;
using Quest.Lib.Trace;
using Quest.Lib.DependencyInjection;
using Quest.Common.ServiceBus;
using Quest.Lib.Notifier;

namespace Quest.Lib.Incident
{
    public class IncidentManager : ServiceBusProcessor
    {
#if USE_ELASTIC
        private ElasticSettings _elastic;
        private BuildIndexSettings _config;
        private IncidentHandler _incidentHandler;
#endif
        private NotificationSettings _notificationSettings = new NotificationSettings();
        IIncidentStore _incStore;

        public IncidentManager(
            IncidentHandler incidentHandler,
            IIncidentStore incStore,
            ElasticSettings elastic,
            NotificationSettings notificationSettings,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _incStore = incStore;
            _elastic = elastic;
            _incidentHandler = incidentHandler;
            _notificationSettings = notificationSettings;
        }

        protected override void OnPrepare()
        {
            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler("CloseIncident", CloseIncidentHandler);
            MsgHandler.AddHandler("IncidentUpdate", IncidentUpdateHandler);
        }

        protected override void OnStart()
        {
            Initialise();
            Logger.Write("IncidentManager initialised","Device");
        }
       

        /// <summary>
        ///     
        /// </summary>
        private void Initialise()
        {
            var syns = IndexBuilder.LoadSynsFromFile(_elastic.SynonymsFile);
            _config = new BuildIndexSettings(_elastic, _elastic.DefaultIndex, null);
            _config.RestrictToMaster = false;
        }

        private Response CloseIncidentHandler(NewMessageArgs t)
        {
            var item = t.Payload as CloseIncident;
            _incidentHandler.CloseIncident(item, _notificationSettings, ServiceBusClient, _incStore);
            return null;
        }

        private Response IncidentUpdateHandler(NewMessageArgs t)
        {
            var item = t.Payload as IncidentUpdate;
            _incidentHandler.IncidentUpdate(item, _notificationSettings, ServiceBusClient, _incStore);
            return null;
        }

    }
}