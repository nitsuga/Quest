using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Quest.Lib;
using ServiceBus.Objects;
using System.Diagnostics;
using System.Transactions;
using Quest.Lib.DataModel;

namespace Quest.Lib.Routing
{
    public class StandbyCoverage
    {
        public enum Tier
        {
            Unknown,
            Desirable,
            Essential,
        };

        public enum Status : int
        {
            UnCovered,
            Coverable,
            Covered
        };
        //                                        ?  D  E                
        private byte[,] coverageValueMap = {    { 0, 1, 4 },    // uncovered
                                                { 0, 2, 5 },    // coverable
                                                { 0, 0, 0 } };  // covered

        class StandbyCacheEntry
        {

            public int destinationId;
            public CoverageMap currentMinCoverage;
            public CoverageMap currentMaxCoverage;
            public int CoverageTier;
            public Status status;
            public bool calculated;
        }

        private Dictionary<int, StandbyCacheEntry> _sbpCache = new Dictionary<int, StandbyCacheEntry>();
        private DateTime _lastCalculatedCoverage = DateTime.MinValue;
        CoverageMap _totalCoverage = null;

        /*
        1. at startup.. prebuild the coverage map for each sbp for each hour for ambulances. Build 8 minute coverage and a 1 minute coverage.
        2. a report that shows how many available vehicles are in 8 and 1 minute ranges for each SBP.
        3. by tier.. 
         *  status is UNCOVERED if no vehicles in 1 minute range and no vehicles in 8 minute range
         *  status is COVERABLE if no vehicles in 1 minute range and vehicles are 8 minute range
         *  status is COVERED if vehicles are in 1 minute range
         *  
         * save the status for each SBP, record for each a list of the ResourceReportId's for the callsigns in 8 and 1 minute range
         * 
         * produce 1-Minute and 8-minute coverage maps showing colour coding
         *      8 minute  - Red if sbp UNCOVERED
         *      1 minute  - Red if sbp UNCOVERED
         *      8 minute  - LightRed if sbp COVERABLE
         *      1 minute  - Red if sbp COVERABLE
         *      8 minute  - transparent if sbp COVERED
         *      1 minute  - transparent if sbp COVERED
        */


        /// <summary>
        /// Calculate the coverage status of each standby point - updated each hour.
        /// check each standby point individually looking for vehicles that fall within its footprint
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="QuestConnectionString"></param>
        /// <param name="vehicleType"></param>
        /// <param name="distanceMax"></param>
        /// <param name="timeSecsMax"></param>
        /// <param name="tileSize"></param>
        /// <returns></returns>
        public CoverageMap CalculateTotalSBPCoverage(String name, RoutingManager manager, int vehicleType, int minFootprint, int maxFootprint, int tileSize, RequestQueue workQueue)
        {
            if (_totalCoverage == null || Math.Abs(DateTime.Now.Subtract(_lastCalculatedCoverage).TotalMinutes) > 60)
            {
                if (_totalCoverage != null)
                    CoverageMapUtil.Clear(_totalCoverage);

                foreach (StandbyCacheEntry ce in _sbpCache.Values)
                    ce.calculated = false;

                // get a list of standby points to update
                List<DestinationView> destinations = GetStandbyPoints();

                foreach (DestinationView d in destinations)
                {
                    // calculate min footprint
                    RoutingPoint[] startpoints = { new RoutingPoint { X = (int)d.e, Y = (int)d.n, Tag = d } };
                    RouteRequestCoverage request = new RouteRequestCoverage()
                    {
                        StartPoints = startpoints,
                        VehicleType = vehicleType,
                        Hour = DateTime.Now.Hour,
                        DistanceMax = int.MaxValue,
                        DurationMax = minFootprint,
                        SearchType = SearchType.Quickest,
                        TileSize = tileSize,
                        Name = name
                    };

                    // calculate the coverage map
                    CoverageMapResult minMap = (CoverageMapResult)workQueue.PerformQuery(request);

                    request.DurationMax = maxFootprint;

                    // calculate the coverage map
                    CoverageMapResult maxMap = (CoverageMapResult)workQueue.PerformQuery(request);

                    maxMap.Value.Percent = CoverageMapUtil.Coverage(maxMap.Value) * 100;

                    // update our cache value
                    if (!_sbpCache.ContainsKey(d.DestinationId))
                        _sbpCache.Add(d.DestinationId, new StandbyCacheEntry());

                    StandbyCacheEntry ce = _sbpCache[d.DestinationId];
                    ce.calculated = true;
                    ce.currentMaxCoverage = maxMap.Value;
                    ce.currentMinCoverage = minMap.Value;
                    ce.destinationId = d.DestinationId;
                    ce.CoverageTier = (int)d.CoverageTier;

                    // update te total coverage
                    if (_totalCoverage != null)
                        _totalCoverage.Add(maxMap.Value);
                    else
                        _totalCoverage = maxMap.Value.Clone();
                }

                // finally, remove any unused cache entries
                foreach (int idList in _sbpCache.Values.Where(x => x.calculated == false).Select(x => x.destinationId))
                    _sbpCache.Remove(idList);

                // record the time we did it
                _lastCalculatedCoverage = DateTime.Now;
            }
            _totalCoverage.Percent = CoverageMapUtil.Coverage(_totalCoverage) * 100;

            return _totalCoverage;
        }

        /// <summary>
        /// Calculate vehicle compliance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="QuestConnectionString"></param>
        /// <param name="vehicleCodes"></param>
        /// <param name="minFootprint"></param>
        /// <param name="maxFootprint"></param>
        /// <param name="tileSize"></param>
        /// <param name="workQueue"></param>
        /// <returns></returns>
        public CoverageMap CalculateCompliance(String name, String[] vehicleCodes, int minFootprint, int maxFootprint, int tileSize, RequestQueue workQueue)
        {
            CoverageMap newMap = null;
            List<ResourceView> resources = GetAvailableResources(vehicleCodes);

            // check each sbp for compliance
            foreach (StandbyCacheEntry ce in _sbpCache.Values)
            {
                ce.status = Status.UnCovered;

                // check each resource location to see if it is in the two footprints
                foreach (ResourceView v in resources)
                {
                    switch (ce.status)
                    {
                        case Status.UnCovered:
                            if (CoverageMapUtil.Value(ce.currentMinCoverage, (int)v.Easting, (int)v.Northing) > 0)
                                ce.status = Status.Covered;
                            else
                                if (CoverageMapUtil.Value(ce.currentMaxCoverage, (int)v.Easting, (int)v.Northing) > 0)
                                    ce.status = Status.Coverable;
                            break;
                        case Status.Coverable:
                            if (CoverageMapUtil.Value(ce.currentMinCoverage, (int)v.Easting, (int)v.Northing) > 0)
                                ce.status = Status.Covered;
                            break;
                        case Status.Covered:
                            break;
                    }
                }


                // record the status in the destinations table
                UpdateDestinationStatus(
                    ce.destinationId, ce.status.ToString());
            }

            // splice together uncovered areas
            foreach (StandbyCacheEntry ce in _sbpCache.Values)
            {
                // now we know its status build up the compliance map by splicing together coverage maps
                if (ce.status != Status.Covered)
                {
                    if (newMap == null)
                        newMap = ce.currentMaxCoverage.CloneAsValue(coverageValueMap[(int)ce.status, ce.CoverageTier]);
                    else
                        CoverageMapUtil.MergeAsValue(ce.currentMaxCoverage, newMap, coverageValueMap[(int)ce.status, ce.CoverageTier]);
                }
            }

            // remove covered areas
            foreach (StandbyCacheEntry ce in _sbpCache.Values)
            {
                // now we know its status build up the compliance map by splicing together coverage maps
                if (ce.status == Status.Covered)
                {
                    if (newMap != null)
                        CoverageMapUtil.MergeAsValue(ce.currentMaxCoverage, newMap, coverageValueMap[(int)ce.status, ce.CoverageTier]);
                }
            }

            // calculate the coverage map
            newMap.Name = name;
            return newMap;
        }


        private static void UpdateDestinationStatus(int destinationId, string status)
        {
            using (QuestEntities db = new QuestEntities())
            {
                int changes=db.UpdateDestinationStatus(destinationId, status);
            }
        }

        private static List<DestinationView> GetStandbyPoints()
        {
            List<DestinationView> resources = null;

            // calculate available 
            using (QuestEntities db = new QuestEntities())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
                {

                    var results = from result in db.DestinationViews where result.IsStandby == true && result.CoverageTier > 0 select result;
                    resources = results.ToList();
                }
            }
            return resources;
        }

        static private List<ResourceView> GetAvailableResources(string[] VehicleCodes)
        {
            List<ResourceView> resources = null;

            // calculate available 
            using (QuestEntities db = new QuestEntities())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
                {

                    var results = from result in db.ResourceViews
                                  where
                                  result.Available == true
                                  && VehicleCodes.Contains(result.ResourceType)
                                    && result.Easting != null
                                    && result.Northing != null
                                    && result.Easting != 0
                                    && result.Northing != 0
                                  select result;
                    resources = results.ToList();
                }
            }
            return resources;
        }

    }

}