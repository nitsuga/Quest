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
using Autofac;
using Quest.Lib.Utils;
using Quest.Common.ServiceBus;

namespace Quest.Lib.Research.Job
{
    /// <summary>
    ///     This module reads resource and incident data from
    /// </summary>
    public class MapMatcherUtil
    {
        #region Private Fields
        private ILifetimeScope _scope;
        #endregion


        public MapMatcherUtil(
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            RoutingData data
            )
        {
            _scope = scope;
        }


        /// <summary>
        /// Analyse a multiple tracks in the database using a specified map matcher and parameters
        /// and save the results in the database.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="request"></param>
        /// <param name="jobParameters"></param>
        /// <returns></returns>
        public MapMatcherMatchAllResponse MapMatcherMatchAll(MapMatcherMatchAllRequest request)
        {
            if (request == null)
                return new MapMatcherMatchAllResponse { Success = false, Message = "Invalid payload" };

            RoadMatcherAllCommandActionWorker(_scope, request);

            return new MapMatcherMatchAllResponse { Success = true, Message = "Started" };

        }

        public void RoadMatcherBatchWorker(
            int taskid,
            int startid,
            int endid,
            int runid,
            IMapMatcher matcher,
            IRouteEngine engine,
            dynamic parms
            )
        {
            Logger.Write("Getting Incident Routes..");
            var routeids = GetIncidentRoutes(true);
            var toProcess = routeids.Where(x => x >= startid && x <= endid).ToList();
            Logger.Write($"Found {toProcess.Count} Incident Routes to process");
            RoadMatcherBatchWorker(taskid, toProcess, runid, matcher, engine, parms);
        }

        /// <summary>
        /// execute a road matching algorithm
        /// </summary>
        /// <param name="container"></param>
        /// <param name="request"></param>
        /// <param name="jobParameters"></param>
        private static void RoadMatcherAllCommandActionWorker(ILifetimeScope scope, MapMatcherMatchAllRequest request)
        {
            int runid;

            try
            {
                var matcher = scope.ResolveNamed<IMapMatcher>(request.MapMatcher);
                Logger.Write($"Starting new run");
                using (var context = new QuestResearchEntities())
                {
                    var myparams = request.Parameters.Replace("'", "''");
                    var cmd = $" SET NOCOUNT ON; INSERT [dbo].[IncidentRouteRun]([Parameters], [Timestamp]) VALUES('{myparams}', '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}');SELECT scope_identity()\n";
                    context.Database.ExecuteSqlCommand(cmd);
                    runid = context.IncidentRouteRuns.Max(x => x.IncidentRouteRunId);
                    Logger.Write($"Run id {runid}");
                }

                Logger.Write($"Getting incident routes");
                // get unscanned records
                var routeids = GetIncidentRoutes(true);

                Logger.Write($"Found {routeids.Count} that need processing");

                // split into batches

                var batchsize = routeids.Count / (request.Workers);

                Logger.Write($"Create {request.Workers} each with {batchsize} incident routes");

                if (batchsize == 0)
                    batchsize = 8;

                var workers = new List<Task>();
                var arglist = new List<string>();
                var taskid = 0;
                while (routeids.Count > 0)
                {
                    var batch = routeids.Take(batchsize).ToList();
                    var engine = scope.ResolveNamed<IRouteEngine>(request.RoutingEngine);

                    if (batch.Count > 0)
                    {
                        routeids.RemoveRange(0, batch.Count);
                        if (request.InProcess)
                        {
                            Logger.Write($"Creating in-process worker {batch}");
                            dynamic parms = ExpandoUtils.MakeExpandoFromString(request.Parameters);
                            var t =
                                new Task(
                                    () =>
                                        RoadMatcherBatchWorker(taskid++, batch, runid, matcher, engine, parms),
                                    TaskCreationOptions.LongRunning);

                            workers.Add(t);
                        }
                        else
                        {
                            Logger.Write($"Creating external process worker {batch}");

#if DOCKER
                            var quotedjobParameters = request.Parameters.Replace("\"", "\\\"").Replace("'", @"\'"); 
#else
                            var quotedjobParameters = request.Parameters.Replace("\"", "\\\"");
#endif

                            var additional = $"/taskid={taskid} /runid={runid} /startrouteid={batch.First()} /endrouteid={ batch.Last()}";
                            var cmdParms = $"-exec=MapMatcherWorker -args={quotedjobParameters} {additional}";

                            if (!string.IsNullOrEmpty(request.Components))
                                cmdParms += $" -components={request.Components}";

                            arglist.Add(cmdParms);
                        }
                    }
                }

                if (request.InProcess)
                {
                    // start the workers
                    workers.ForEach(x => x.Start());

                    // wait for work to finish
                    Task.WaitAll(workers.ToArray());
                }
                else
                {
                    foreach (var arg in arglist)
                    {
                        Logger.Write($"Starting: {arg}");
                        Process.Start("Quest.Cmd.exe", arg);
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }



        public static void RoadMatcherBatchWorker(
            int taskid,
            IReadOnlyCollection<int> routeids,
            int runid,
            IMapMatcher matcher,
            IRouteEngine engine,
            dynamic parms
            )
        {
            try
            {
                string msg =
                    $"Task {taskid} Analysing {routeids.Count} tracks, started @ {DateTime.Now.ToLongTimeString()} from {routeids.Min()} - {routeids.Max()}";
                Logger.Write(msg, TraceEventType.Verbose, "Map Matcher");

                while (!engine.IsReady)
                {
                    Logger.Write($"Task {taskid} waiting for routing engine to be ready", 
                        TraceEventType.Verbose, "Map Matcher");
                    Thread.Sleep(2000);
                }


                foreach (var incidentRouteId in routeids)
                {
                    try
                    {
                        string msg2 = $"Task {taskid} Analysing track {incidentRouteId}";
                        Logger.Write(msg2, TraceEventType.Verbose, "Map Matcher");

                        var result = ProcessRoute(incidentRouteId, matcher, engine, parms);
                        if (result!=null)
                            SaveMatchResult(result, runid, incidentRouteId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(ex.ToString(), "MapMatcher");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex.ToString(),"MapMatcher");
            }
            Logger.Write($"Task {taskid} complete", TraceEventType.Verbose, "Map Matcher");

        }


        public static RouteMatcherResponse ProcessRoute(int routeId, IMapMatcher matcher, IRouteEngine engine, dynamic parms)
        {
            var trackuri = $"db.inc:{routeId}";
            var track = Tracks.GetTrack(trackuri, (int)parms.Skip);

            bool isGood = track.CleanTrack((int)parms.MinSeconds, (int)parms.MinDistance, (int)parms.MaxSpeed, (int)parms.Take);

            if (!isGood)
            {
                MarkRouteBad(routeId);
                return null;
            }

            var analyseRequest = new RouteMatcherRequest
            {
                Name = trackuri.ToString(),
                RoadSpeedCalculator = "ConstantSpeedCalculator",
                RoutingData = engine.Data,
                Fixes = track.Fixes,
                RoutingEngine = engine,
                Parameters = parms
            };

            return matcher.AnalyseTrack(analyseRequest);
        }

        /// <summary>
        /// save results to the database
        /// </summary>
        /// <param name="result"></param>
        /// <param name="runid"></param>
        private static void SaveMatchResult(RouteMatcherResponse result, int runid, int incidentRouteId)
        {
            var builder = new StringBuilder();
            try
            {
                using (QuestResearchEntities context = new QuestResearchEntities())
                {
                    if (result.Results != null && result.Results.Count > 0)
                    {
                        foreach (var s in result.Results)
                        {
                            foreach (var r in s.Edges)
                            {
                                var cmd = $"INSERT [dbo].[RoadSpeedItem]([IncidentRouteRunId], [IncidentRouteId], [DateTime], [Speed], [RoadLinkEdgeId]) VALUES({runid}, {incidentRouteId}, '{s.StartTime.ToString("yyyy-MM-dd  HH:mm:ss")}', {s.SpeedMs * Constant.ms2mph}, {r.Edge.RoadLinkEdgeId});\n";
                                builder.Append(cmd);
                            }
                        }
                        context.Database.ExecuteSqlCommand(builder.ToString());
                    }
                    var cmd1 = $"update IncidentRoutes set scanned = 1, IsBadGPS=0 where IncidentRouteId = {incidentRouteId};\n";
                    context.Database.ExecuteSqlCommand(cmd1);
                }

            }
            catch(Exception ex)
            {
                Logger.Write(ex.ToString(), "MapMatcher");
                Logger.Write(builder.ToString(), "MapMatcher");

                using (QuestResearchEntities context = new QuestResearchEntities())
                {
                    var cmd1 = $"update IncidentRoutes set scanned = 0, IsBadGPS=0 where IncidentRouteId = {incidentRouteId};\n";
                    context.Database.ExecuteSqlCommand(cmd1);
                }
            }
        }

        private static void MarkRouteBad(int incidentRouteId)
        {
            using (QuestResearchEntities context = new QuestResearchEntities())
            {
                var cmd1 = $"update IncidentRoutes set scanned = 1, IsBadGPS=1 where IncidentRouteId = {incidentRouteId};\n";
                context.Database.ExecuteSqlCommand(cmd1);
            }
        }


        /// <summary>
        ///     get all Cat A incident routes (gives incident and callsign)
        /// </summary>
        /// <returns></returns>
        private static List<int> GetIncidentRoutes(bool notScanned)
        {
            List<int> incidents;
            using (var context = new QuestResearchEntities())
            {
                context.Database.CommandTimeout = 36000;
                context.Configuration.ProxyCreationEnabled = false;

                if (notScanned)
                    incidents = context.IncidentRoutes
                        .Where(x => x.Scanned == false)
                        // remove this to process everything
                        .Where(x => x.IncidentId >= 20161001000000 && x.IncidentId < 20161101000000)
                        .Select(x => x.IncidentRouteID)
                        .OrderBy(x => x)
                        .ToList();
                else
                    incidents = context.IncidentRoutes
                        .Select(x => x.IncidentRouteID)
                        .OrderBy(x => x)
                        .ToList();
            }
            return incidents;
        }
    } // End of Class
} //End of Namespace