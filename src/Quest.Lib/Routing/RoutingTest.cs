#pragma warning disable 0169,649
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Lib.Utils;
using Quest.Lib.Trace;
using Autofac;
using Quest.Common.ServiceBus;

namespace Quest.Lib.Routing
{
    /// <summary>
    ///     This class generates coverage maps.
    ///     1. It continually generates coverage maps for vehicle coverage
    ///     2. It continually produces incident prediction coverages
    ///     3. Calculates vehicle ETA's to incidents (broadcast as ETAResult)
    ///     4. Calculates vehicle ETA's to standby points (i.e. those on th standby point tracker list) (broadcast as
    ///     ETAResult)
    ///     All coverage maps are
    ///     a) broadcast as CoverageMap object
    ///     b) saved in the database in CoverageMapStore
    ///     c) saved as ArcGrid into directory specified in the Coverage.ExportDirectory variable
    /// </summary>
    public class RoutingTest : SimpleProcessor
    {
        #region Private Fields

        #endregion

        public RoutingTest(
            //IServiceBusClient serviceBusClient,
            TimedEventQueue eventQueue,
            MessageHandler msgHandler) : base(eventQueue)
        {
        }

        protected override void OnPrepare()
        {
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

   }
}