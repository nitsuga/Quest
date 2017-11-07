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
using Quest.Lib.Resource;
using Quest.Common.Messages.Device;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.Geo
{
    public class GeoManager : ServiceBusProcessor
    {
        GeoHandler _handler;

        public GeoManager(

            GeoHandler handler,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _handler = handler;
        }

        protected override void OnPrepare()
        {
            MsgHandler.AddHandler<MapItemsRequest>(GetMapItemsRequestHandler);
        }

        protected override void OnStart()
        {
            Initialise();
            Logger.Write("GeoManager initialised", "GeoManager");
        }
       

        /// <summary>
        /// 
        /// </summary>
        private void Initialise()
        {
        }

        private Response GetMapItemsRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as MapItemsRequest;
            if (request != null)
            {
                return _handler.GetMapItems(request);
            }
            return null;
        }

    }
}