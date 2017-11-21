using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Quest.Lib.MapMatching;
using Quest.Lib.Research.Utils;
using Quest.Lib.Trace;
using Quest.Lib.Processor;
using Quest.Lib.Search.Elastic;
using Quest.Lib.ServiceBus;
using Autofac;
using Quest.Common.ServiceBus;
using Quest.Lib.Utils;
using Quest.Lib.Research.DataModelResearch;
using Quest.Lib.Data;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.Research.Job
{
    /// <summary>
    /// Calculate actual vs estimate routing times based on all the edge 
    /// cost calculators we have. The idea is to detect problems with the avls quality
    /// by finding discrepancy in the estimated time vs. the report time
    /// </summary>
    public class AnalyseAvlsQuality : ServiceBusProcessor
    {

        #region Private Fields
        private ILifetimeScope _scope;
        private ISearchEngine _searchEngine;
        #endregion

        private IDatabaseFactory _dbFactory;
        private TrackLoader _trackLoader;

        public AnalyseAvlsQuality(
            ISearchEngine searchEngine,
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            IDatabaseFactory dbFactory,
            MessageHandler msgHandler,
            TrackLoader trackLoader,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _trackLoader = trackLoader;
            _searchEngine = searchEngine;
            _scope = scope;
            _dbFactory = dbFactory;
        }

        protected override void OnPrepare()
        {
        }

        protected override void OnStart()
        {
            AtsParms settings = new AtsParms() { MinSeconds = 10 };
            Analyse(settings);
        }

        public void Analyse(AtsParms settings)
        {
            var incs = GetIncidents();

            Logger.Write($"Analysing {incs.Count} incidents", GetType().Name);

            var counter = 0;
            foreach (var inc in incs)
            {
                try
                {
                    var t = _trackLoader.GetTracks($"db.inc:{inc}");

                    AnalyseTrackSpeeds(t, settings);

                    counter++;

                    if (counter % 200 == 0)
                        Logger.Write($"Analysed {counter} incidents", GetType().Name);

                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// get a list of all the incidents
        /// </summary>
        /// <returns></returns>
        private List<long> GetIncidents()
        {
            return _dbFactory.Execute<QuestDataContext, List<long>>((db) =>
            {
                List<long> incidents;

                incidents = db.Avls
                    .Where(x => x.RawAvlsId >= 320473511 && x.IncidentId != null) // 2016-12-02
                    .Select(x => (long)x.IncidentId)
                    .Distinct()
                    .ToList();

                return incidents;
            });
        }

        private static void AnalyseTrackSpeeds(IEnumerable<Track> tracks, AtsParms settings)
        {
            foreach (var t in tracks)
                AnalyseTrackSpeeds(t, settings);
        }

        private static void AnalyseTrackSpeeds(Track track, AtsParms settings)
        {
            if (track == null) throw new ArgumentNullException(nameof(track));

            track = track.MarkSuspectFixes(settings.MinSeconds, settings.MinDistance);

            int dupcount = track.Fixes.Count(x => x.Corrupt == Fix.CurruptReason.Duplicate);
            int duptime = track.Fixes.Count(x => x.Corrupt == Fix.CurruptReason.TooCloseTime);

            if (dupcount>4 || duptime>2)
                Debug.Print($"{track.Incident} {track.Callsign} {dupcount} {duptime}");
        }

        public class AtsParms
        {
            public int MinSeconds;
            public int MinDistance;
        }

    }

}