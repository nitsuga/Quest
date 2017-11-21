#define USE_ELASTIC
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using Quest.Common.Messages;
using Quest.Lib.Processor;
using Quest.Lib.Trace;
using Quest.Common.ServiceBus;
using Quest.Common.Messages.Entities;
using Quest.Lib.DependencyInjection;

namespace Quest.Lib.Entities
{
    [Injection("EntityManager", typeof(IProcessor), Lifetime.Singleton) ]
    public class EntityManager : ServiceBusProcessor
    {
        private EntityHandler _handler;

        public EntityManager(
            EntityHandler handler,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _handler = handler;
        }

        protected override void OnPrepare()
        {
            // create a list of actions associated with each object type arriving from the queue
            MsgHandler.AddHandler<GetEntityTypesRequest>(GetEntityTypesRequestHandler);
            MsgHandler.AddHandler<GetEntitiesRequest>(GetEntitiesRequestHandler);
            MsgHandler.AddHandler<GetEntityTypesRequest>(GetEntityTypesRequestHandler);
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
        }


        private Response GetEntityTypesRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as GetEntityTypesRequest;
            if (request != null)
            {
                return _handler.GetEntityTypes(request);
            }
            return null;
        }

        private Response GetEntitiesRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as GetEntitiesRequest;
            if (request != null)
            {
                return _handler.GetEntities(request);
            }
            return null;
        }

        private Response GetEntityRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as GetEntityRequest;
            if (request != null)
            {
                return _handler.GetEntity(request);
            }
            return null;
        }


    }
}