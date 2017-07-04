#define XINPROCESS

////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2016 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quest.Lib.Constants;
using Quest.Lib.MapMatching;
using Quest.Lib.MapMatching.RouteMatcher;
using Quest.Lib.Research.Model;
using Quest.Lib.Research.Utils;
using Quest.Lib.Routing;
using Quest.Common.Messages;
using Quest.Lib.Trace;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Autofac;
using Quest.Common.ServiceBus;
using Quest.Lib.Utils;

namespace Quest.Lib.Research.Job
{
    /// <summary>
    ///     This module reads resource and incident data from
    /// </summary>
    public class MapMatcherProcessor : ServiceBusProcessor
    {
        #region Private Fields
        private ILifetimeScope _scope;
        private MapMatcherUtil _mapMatcherUtil;
        #endregion


        public MapMatcherProcessor(
            ILifetimeScope scope,
            MapMatcherUtil mapMatcherUtil,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _scope = scope;
            _mapMatcherUtil = mapMatcherUtil;
        }

        protected override void OnPrepare()
        {
            MsgHandler.AddHandler<MapMatcherMatchAllRequest>(MMAHandler);
            MsgHandler.AddHandler<MapMatcherMatchSingleRequest>(MapMatcherMatchSingleRequestHandler);
        }

        /// <summary>
        /// MapMatch a single route
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        MapMatcherMatchSingleResponse MapMatcherMatchSingleRequestHandler(NewMessageArgs args)
        {
            MapMatcherMatchSingleRequest request = args.Payload as MapMatcherMatchSingleRequest;
            return MapMatcherManager.MapMatcherMatchSingle(_scope, request);
        }

        public Response MMAHandler(NewMessageArgs args)
        {
            var request = args.Payload as MapMatcherMatchAllRequest;
            if (request != null)
            {
                _mapMatcherUtil.MapMatcherMatchAll(request);
            }
            return null;
        }

    } // End of Class
} //End of Namespace