#pragma warning disable 0169
#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Quest.Lib.Coords;
using Quest.Lib.Research.Model;
using Quest.Lib.Routing;
using Quest.Lib.Trace;
using Quest.Lib.Utils;
using Quest.Lib.Processor;
using Autofac;
using Quest.Lib.ServiceBus;
using Quest.Common.ServiceBus;

namespace Quest.Lib.Research.Job
{

    public class FindNearestRoads : ServiceBusProcessor
    {
        private readonly AutoResetEvent _are = new AutoResetEvent(false);
        private bool _quiting = false;
        private readonly List<string> _writequeue = new List<string>();

        #region Private Fields
        private bool _stopping;
        private ILifetimeScope _scope;
        private RoutingData _data;
        private DijkstraRoutingEngine _selectedRouteEngine;
        #endregion


        public FindNearestRoads(
            ILifetimeScope scope,
            RoutingData data,
            DijkstraRoutingEngine selectedRouteEngine,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _selectedRouteEngine = selectedRouteEngine;
            _scope = scope;
            _data = data;
        }

        protected override void OnPrepare()
        {
        }
        protected override void OnStart()
        {
            //Process();
        }

#if true
        /// <summary>
        ///     find the nearest road and road type for each avls message
        /// </summary>
        public void Process()
        {
            // load in nodes and links
            Debug.Print($"Loading road network {DateTime.Now}");
            var data = new RoutingData();
            while (data.IsInitialised == false)
                Thread.Sleep(1000);

            var msg = "Adding nearest road links for AVLS data";
            Logger.Write(msg, GetType().Name);

            var chunks = new List<Task>();

            const int maxtasks = 10000;

            // start the writer
            Task.Run(() => Writer());

            // get all the records
            using (var context = new QuestResearchEntities())
            {
                context.Database.CommandTimeout = 36000;
                context.Configuration.ProxyCreationEnabled = false;

                var total = context.Avls.AsNoTracking().Count(x => x.Process == true);
                var max = context.Avls.AsNoTracking().Where(x => x.Process == true).Max(x => x.RawAvlsId);
                var min = context.Avls.AsNoTracking().Where(x => x.Process == true).Min(x => x.RawAvlsId);
                var batchsize = (max - min)/maxtasks;
                var batch = 0;
                for (int i = min; i < max; i += batchsize)
                {
                    var p = new Parms
                    {
                        Batch = batch++,
                        From = i,
                        BatchSize = batchsize,
                        Routingdata = data
                    };

                    chunks.Add(Task.Run(() => FindNearestRoadsWorker(p)));

                    // only do 4 tasks a a time
                    if (chunks.Count > 16)
                    {
                        Task.WaitAll(chunks.ToArray());
                        chunks.Clear();
                    }
                }
                msg = $"Unscanned records #{total} there will be {batch} batches";
                Logger.Write(msg, GetType().Name);
            }


            // wait for them all to finish
            if (chunks.Count > 0)
                Task.WaitAll(chunks.ToArray());
            _quiting = true;

            msg = "Updated  Avls complete";
            Logger.Write(msg, GetType().Name);
        }

        private void Writer()
        {
            using (var context2 = new QuestResearchEntities())
            {
                while (!_quiting)
                {
                    _are.WaitOne(1000);
                    if (_writequeue.Count > 0)
                    {
                        var msg = $"Queue size {_writequeue.Count}";
                        Logger.Write(msg, GetType().Name);

                        var s = new StringBuilder();

                        // only lock the queue briefly to extract the commands
                        lock (_writequeue)
                        {
                            foreach (string entry in _writequeue)
                                s.Append(entry);
                            _writequeue.Clear();
                        }
                        try
                        {
                            context2.Database.ExecuteSqlCommand(s.ToString());
                        }
                        catch (Exception)
                        {
                            
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     This is a worker method for processing a chunk for EnrouteSpeedData records. It locates the nearest roadlink and
        ///     update the records and marked it as scanned
        /// </summary>
        /// <param name="p"></param>
        private void FindNearestRoadsWorker(Parms p)
        {
            var msg = $"Batch #{p.Batch} Started";
            Logger.Write(msg, GetType().Name);
            var s = new StringBuilder();

            using (var context2 = new QuestResearchEntities())
            {
                context2.Database.CommandTimeout = 36000;
                context2.Configuration.ValidateOnSaveEnabled = false;
                context2.Configuration.ProxyCreationEnabled = false;
                int itemsAppended = 0;
                var items =
                    context2.Avls.AsNoTracking()
                        .Where(x => x.RawAvlsId >= p.From && x.RawAvlsId < (p.From + p.BatchSize) && x.Process);
                foreach (var r in items) // for each GPS fix..
                {
                    try
                    {
                        var ll = new LatLng((double) (r.LocationY ?? 0), (double) (r.LocationX ?? 0));
                        var en = ll.WGS84ToOSRef();

                        // create a point for the fix
                        var coord = new Coordinate((float) en.Easting, (float) en.Northing);

                        var pointEn = new Point(coord) {SRID = 27700};

                        var e = pointEn.Buffer(50).EnvelopeInternal;

                        // get a block of nearest roads
                        var nearest = p.Routingdata.ConnectionIndex.Query(e);

                        if (nearest.Count >= 1)
                        {
                            // iterate through and get the actual nearest
                            var bestlist = nearest.OrderBy(x => x.Geometry.Distance(pointEn)).ToList();
                            var best = bestlist.First();
                            var distance = best.Geometry.Distance(pointEn);
                            itemsAppended++;
                            s.Append(
                                $"insert AvlsRoad ([AvlsId],[RoadTypeId],[DistanceToRoad],[RoadLinkEdgeId]) values( {r.RawAvlsId},{best.RoadTypeId},{distance},{best.RoadLinkEdgeId});");

                            if (itemsAppended > 250)
                            {
                                itemsAppended = 0;
                                lock (_writequeue)
                                {
                                    _writequeue.Add(s.ToString());
                                    s.Clear();
                                    _are.Set();
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                if (s.ToString().Length > 0)
                {
                    lock (_writequeue)
                    {
                        _writequeue.Add(s.ToString());
                    }
                }

                msg = $"Batch #{p.Batch} completed";
                Logger.Write(msg, GetType().Name);
            }
        }
#endif

    }

    internal class Parms
    {
        internal int Batch;
        internal RoutingData Routingdata;
        public int BatchSize { get; internal set; }
        public int From { get; set; }
    }

    internal class RawParms
    {
        internal List<int> Data;
    }
}