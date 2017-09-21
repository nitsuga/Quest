using System;
using System.Collections.Generic;
using GeoJSON.Net.Feature;
using Quest.Common.Messages;
using Quest.Lib.ServiceBus;
using Quest.Lib.Processor;
using Autofac;
using Quest.Common.ServiceBus;
using Quest.Lib.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace Quest.Lib.Visuals
{
    public class VisualsManager : ServiceBusProcessor
    {
        private const string Name = "VisualsManager";
        private readonly ILifetimeScope _scope;
        IMemoryCache _cache;

        public VisualsManager(
            IMemoryCache cache,
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
            _cache = cache;
        }

        protected override void OnPrepare()
        {
        }
        protected override void OnStart()
        {
            MsgHandler.AddHandler<GetVisualsCatalogueRequest>(GetVisualsCatalogueHandler);
            MsgHandler.AddHandler<GetVisualsDataRequest>(GetVisualsData);
            MsgHandler.AddHandler<QueryVisualRequest>(QueryVisual);
        }

        /// <summary>
        /// Get visuals data from providers
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public Response GetVisualsData(NewMessageArgs args)
        {
            var request = args.Payload as GetVisualsDataRequest;
            var result = new GetVisualsDataResponse() { Geometry = new FeatureCollection() };

            if (request != null)
                foreach (var r in request.Ids)
                {
                    var visual = _cache.Get<Visual>(r);
                    if (visual != null)
                        result.Geometry.Features.AddRange(visual.Geometry.Features);
                }

            // var providers = _container.GetExports<IVisualProvider>();
            //            foreach (var p in providers)
            //                result.Items.Add(p.Value.GetVisualsData(args.Payload as GetVisualsDataRequest));

            return result;
        }


        /// <summary>
        /// Get visuals catalogue from providers
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public Response GetVisualsCatalogueHandler(NewMessageArgs args)
        {
            var providers = _scope.Resolve<IEnumerable<IVisualProvider>>();
            var result = new GetVisualsCatalogueResponse() { Items = new List<Visual>() };
            foreach (var p in providers)
                result.Items.AddRange(p.GetVisualsCatalogue(_scope, args.Payload as GetVisualsCatalogueRequest));

            foreach (var v in result.Items)
            {
                _cache.CreateEntry(v.Id.Id).SetSlidingExpiration(new TimeSpan(1, 0, 0)).Value = v;
            }

            return result;
        }

        public QueryVisualResponse QueryVisual(NewMessageArgs args)
        {
            var request = args.Payload as QueryVisualRequest;
            var provider = _scope.ResolveNamed<IVisualProvider>(request.Provider);

            if (provider == null)
            {
                return new QueryVisualResponse { Message = $"No such provider '{request.Provider}'", Success = false };
            }

            var result = provider.QueryVisual(_scope, request);

            if (result != null && result.Visuals != null)
                foreach (var v in result.Visuals)
                {
                    if (v != null)
                    {
                        _cache.CreateEntry(v.Id.Id).SetSlidingExpiration(new TimeSpan(1, 0, 0)).Value = v;
                    }
                }

            return result;
        }

    }

}