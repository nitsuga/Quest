using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using GeoAPI.Geometries;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Trace;
using Quest.Lib.Utils;

namespace Quest.Lib.Routing
{
    /// <summary>
    ///     class is responsible for interatively managing a coverage map for a specific definition
    /// </summary>
    public class VehicleCoverageTracker<T> 
    {
        /// <summary>
        ///     a list of coverage maps and positions by callsign
        /// </summary>
        private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();

        public CoverageMap CombinedMap;
        public CoverageMapDefinition Definition;
        public T Manager;

        public string Name;

        public CoverageMap GetCoverage(IRouteEngine routingEngine)
        {
            UpdateCoverage(routingEngine);
            return CombinedMap;
        }

        /// <summary>
        ///     return a combined coverage map calculated using supplied overrides
        /// </summary>
        /// <param name="overrides"></param>
        /// <param name="routingEngine"></param>
        /// <returns></returns>
        public TrialCoverageResponse GetTrialCoverage(List<VehicleOverride> overrides, IRouteEngine routingEngine)
        {
            var result = new TrialCoverageResponse();
            result.LowIsBad = true;

            // we work on a copy ONLY
            if (CombinedMap != null)
            {
                var copymap = CombinedMap.Clone();
                result.Before = CombinedMap.Coverage()*100;

                foreach (var o in overrides)
                {
                    if (_cache.ContainsKey(o.Callsign))
                    {
                        var entry = _cache[o.Callsign];
                        var updatedRoutingPoint = new Coordinate {  X = o.Easting, Y = o.Northing};

                        if (Definition.MinuteLimit != null)
                        {
                            var request = new RouteRequestCoverage
                            {
                                StartPoints = new[] {updatedRoutingPoint},
                                VehicleType = Definition.RoutingResource,
                                HourOfWeek = DateTime.Now.HourOfWeek(),
                                DistanceMax = 10000,
                                DurationMax = 60*(int) Definition.MinuteLimit,
                                SearchType = RouteSearchType.Quickest,
                                RoadSpeedCalculator = "ConstantSpeedCalculator"
                            };

                            // calculate the coverage map
                            var newMap = routingEngine.CalculateCoverage(request);

                            CoverageMapUtil.Move(entry.Map, newMap, copymap);
                        }
                        result.After = copymap.Coverage()*100;
                        result.UpdateDelta();

                        result.Map = copymap;
                        return result;
                    }
                }
            }
            return null; // nothing changed
        }

        private void UpdateCoverage(IRouteEngine routingEngine)
        {
            if (Definition.VehicleCodes != null)
            {
                var vehicleCodes = Definition.VehicleCodes.Split(',');

                // get a list of vehicles and there positions from the database that match the criteria
                var resources = GetAvailableResources(vehicleCodes);

                // mark each cache entry as invalid.. will be used later to remove unwanted coverage maps.
                _cache.Values.ToList().ForEach(x => x.Valid = false);

                Logger.Write(
                    $"Calculating Coverage for {Definition.VehicleCodes} = {string.Join(",", resources.Select(x => x.Callsign))} ", TraceEventType.Error, "Vehicle Coverage Tracker");

                // update cache location entry and create new ones as necessary
                UpdateCacheLocations(resources);

                // now all Current routing points have been updated and the cache entry made Valid .
                // now extract Valid records where the routing point is different from the previous routing point and recalculate the coverage map
                // we do this with one request

                // remove all cache entries that are not valid.. i.e. dont exist in the resource list from the database
                RemoveInvalidEntries();

                // calculate changed entries
                UpdateMapsforChangedLocations(routingEngine);

                Logger.Write(
                    $"Calculating Coverage for {Definition.VehicleCodes} Coverage = {CombinedMap.Percent*100,2} ", TraceEventType.Error,
                    "Vehicle Coverage Tracker");

                //_cache.Values.AsParallel().ForAll(x => x.PrevLocation = x.CurLocation);
            }
        }

        private void UpdateMapsforChangedLocations(IRouteEngine routingEngine)
        {
#if BINARYMAP
            var unchangedEntries = from x in _cache.Values where x.CurLocation.CompareTo(x.PrevLocation) == 0 select x;

            // calculate coverage for remaining points where they have changed
            var changedEntries = from x in _cache.Values where x.CurLocation.CompareTo(x.PrevLocation) != 0 select x;

#else
            // calculate coverage for all 
            var changedEntries = from x in _cache.Values select x;

            // in min-travel mode we have to rebuild the combined map from scratch
            if (CombinedMap != null)
                CombinedMap.ClearData();
#endif

            foreach (var ce in changedEntries)
            {
                var request = new RouteRequestCoverage
                {
                    StartPoints = new[] {ce.CurLocation},
                    VehicleType = Definition.RoutingResource,
                    HourOfWeek = DateTime.Now.HourOfWeek(),
                    DistanceMax = 16000,
                    DurationMax = 60*(int) Definition.MinuteLimit,
                    SearchType = RouteSearchType.Quickest,
                    RoadSpeedCalculator = "ConstantSpeedCalculator"
                };

                // calculate the coverage map

                try
                {
                    var newMap = routingEngine.CalculateCoverage(request);

                    newMap.Percent = newMap.Coverage();

                    if (CombinedMap == null)
                    {
                        CombinedMap = CoverageMapUtil.CreateEmptyCopy(newMap);
                        CombinedMap.Name = Name;
                    }
#if BINARYMAP
    // take off old one
                if (ce.Map != null)
                    CombinedMap.Subtract(ce.Map);

                ce.Map = newMap.Value;

                CombinedMap.Add(ce.Map);
#else

                    // update the cache entry 
                    ce.Map = newMap;
                    CombinedMap.MergeMin(ce.Map);

#endif
                }
                catch 
                {
                }
            }

            CombinedMap.Percent = Math.Round(CombinedMap.Coverage()*100, 1)/100;
        }

        private void RemoveInvalidEntries()
        {
            var old = from x in _cache.Values where x.Valid == false select x;

            // substract off the map from the combined picture
            foreach (var ce in old)
                if (ce.Map != null)
                    CombinedMap.Subtract(ce.Map);

            foreach (var callsign in old.Select(x => x.Callsign).ToList())
                _cache.Remove(callsign);
        }

        private void UpdateCacheLocations(List<DataModel.Resource> resources)
        {
            // cycle through resources updating the cache if the resource is
            foreach (var res in resources)
            {
                try
                {
                    CacheEntry entry;
                    Coordinate updatedRoutingPoint = null;

                    if (res.Callsign == null)
                        continue;

                    if (!_cache.ContainsKey(res.Callsign.Callsign1))
                    {
                        // create a new cache entry as this one doesn't exist
                        updatedRoutingPoint = new Coordinate
                        {
                            X = res.Longitude ?? 0,
                            Y = res.Latitude ?? 0
                        };

                        entry = new CacheEntry {Callsign = res.Callsign.Callsign1, CurLocation = updatedRoutingPoint, Map = null};
                        _cache.Add(res.Callsign.Callsign1, entry);
                    }
                    else
                        entry = _cache[res.Callsign.Callsign1];

                    // entry is now either a new or existing cache entry

                    if (updatedRoutingPoint == null) // i.e. not overriden or new, take new position from the record
                        updatedRoutingPoint = new Coordinate
                        {
                            X = res.Longitude ?? 0,
                            Y = res.Latitude ?? 0
                        };

                    entry.CurLocation = updatedRoutingPoint;
                    entry.Valid = true;
                }
                catch (Exception ex)
                {
                    Logger.Write(ex.ToString(), TraceEventType.Error,
                        "Vehicle Coverage Tracker");
                }
            }
        }

        private List<DataModel.Resource> GetAvailableResources(string[] vehicleCodes)
        {
            List<DataModel.Resource> resources = null;

            // calculate available 
            using (var db = new QuestContext())
            {
                using (
                    var scope = new TransactionScope(TransactionScopeOption.Required,
                        new TransactionOptions {IsolationLevel = IsolationLevel.ReadUncommitted}))
                {
                    var results = from result in db.Resource
                        where
                            result.ResourceStatus.Available == true
//                            && vehicleCodes.Contains(result.ResourceType)
                            && result.Latitude != null
                        select result;
                    resources = results.ToList();
                }
            }
            return resources;
        }

        private class CacheEntry
        {
            public string Callsign;
            public Coordinate CurLocation;
            public CoverageMap Map;
            public bool Valid;
        }
    }
}