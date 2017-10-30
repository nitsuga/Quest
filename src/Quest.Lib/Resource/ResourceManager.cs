#define USE_ELASTIC
using Quest.Lib.Search.Elastic;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Quest.Lib.Processor;
using Quest.Lib.Trace;
using Quest.Common.ServiceBus;
using Quest.Lib.Notifier;
using Quest.Lib.Incident;
using Quest.Common.Messages.CAD;
using Quest.Common.Messages.Resource;

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

        public ResourceManager(
            IIncidentStore incStore,
            ResourceHandler resourceHandler,
            ElasticSettings elastic,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _incStore = incStore;
            _elastic = elastic;
            _resourceHandler = resourceHandler;
        }

        protected override void OnPrepare()
        {
            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler<DeleteResource>(DeleteResourceHandler);
            MsgHandler.AddHandler<ResourceLogon>(ResourceLogonHandler);
            MsgHandler.AddHandler<ResourceUpdateRequest>(ResourceUpdateHandler);
            MsgHandler.AddHandler<BeginDump>(BeginDumpHandler);
        }

        protected override void OnStart()
        {
            Initialise();
            Logger.Write("ResourceManager initialised", "Device");
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
            var resourceUpdate = t.Payload as ResourceUpdateRequest;

            if (resourceUpdate != null)
                _resourceHandler.ResourceUpdate(resourceUpdate, ServiceBusClient, _config);

            return null;
        }

        private Response DeleteResourceHandler(NewMessageArgs t)
        {
            var item = t.Payload as DeleteResource;
            _resourceHandler.DeleteResource(item, ServiceBusClient);
            return null;
        }

        private Response ResourceLogonHandler(NewMessageArgs t)
        {
            var item = t.Payload as ResourceLogon;
            _resourceHandler.ResourceLogon(item);
            return null;
        }

        private Response BeginDumpHandler(NewMessageArgs t)
        {
            var item = t.Payload as BeginDump;
            _resourceHandler.BeginDump(item);
            return null;
        }

    }
}