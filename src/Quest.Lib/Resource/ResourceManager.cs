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
using Quest.Lib.Incident;
using Quest.Lib.Resource;
using Quest.Lib.Device;

namespace Quest.Lib.Resource
{
    public class ResourceManager : ServiceBusProcessor
    {
#if USE_ELASTIC
        private ElasticSettings _elastic;
        private BuildIndexSettings _config;
        private ResourceHandler _resourceHandler;
        private IIncidentStore _incStore;
#endif
        private NotificationSettings _notificationSettings = new NotificationSettings();

        public ResourceManager(
            IIncidentStore incStore,
            ResourceHandler resourceHandler,
            ElasticSettings elastic,
            NotificationSettings notificationSettings,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _incStore = incStore;
            _elastic = elastic;
            _resourceHandler = resourceHandler;
            _notificationSettings = notificationSettings;
        }

        protected override void OnPrepare()
        {
            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler("DeleteResource", DeleteResourceHandler);
            MsgHandler.AddHandler("ResourceLogon", ResourceLogonHandler);
            MsgHandler.AddHandler("ResourceUpdate", ResourceUpdateHandler);
        }

        protected override void OnStart()
        {
            Initialise();
            Logger.Write("DeviceTracker initialised","Device");
        }
       

        /// <summary>
        ///     "Quest.ResourceTracker"
        /// </summary>
        private void Initialise()
        {
            var syns = IndexBuilder.LoadSynsFromFile(_elastic.SynonymsFile);
            _config = new BuildIndexSettings(_elastic, _elastic.DefaultIndex, null);
            _config.RestrictToMaster = false;
        }

        /// <summary>
        ///     handle resource updates from GT.
        ///     Detects resource changing to DSP (Dispatched) and sends incident details to callsign
        ///     Detects resource changing to status and sends status update to callsign
        /// </summary>
        /// <param name="t"></param>
        private Response ResourceUpdateHandler(NewMessageArgs t)
        {
            var resourceUpdate = t.Payload as ResourceUpdate;

            if (resourceUpdate != null)
                _resourceHandler.ResourceUpdate(resourceUpdate, _notificationSettings, ServiceBusClient, _config,_incStore);
            return null;
        }

        private Response DeleteResourceHandler(NewMessageArgs t)
        {
            var item = t.Payload as DeleteResource;
            _resourceHandler.DeleteResource(item, _notificationSettings, ServiceBusClient);
            return null;
        }

        private Response ResourceLogonHandler(NewMessageArgs t)
        {
            var item = t.Payload as ResourceLogon;
            _resourceHandler.ResourceLogon(item, _notificationSettings);
            return null;
        }


    }
}