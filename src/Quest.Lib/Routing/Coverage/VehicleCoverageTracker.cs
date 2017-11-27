using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeoAPI.Geometries;
using Quest.Lib.DataModel;
using Quest.Lib.Trace;
using Quest.Lib.Data;
using Quest.Common.Utils;
using Quest.Common.Messages.Routing;
using Quest.Lib.Resource;
using Quest.Common.Messages.Resource;

namespace Quest.Lib.Routing.Coverage
{
    /// <summary>
    ///     class is responsible for interatively managing a coverage map for a specific definition
    /// </summary>
    public class VehicleCoverageTracker
    {
        private RoutingData _data;
        private CoverageMapManager _manager;
        private IResourceStore _resStore;
        private IRouteEngine _routingEngine;

        /// <summary>
        ///     a list of coverage maps and positions by callsign
        /// </summary>
        private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();

        public CoverageMap CombinedMap;

        public IDatabaseFactory _dbFactory;
        private CoverageMapDefinition _definition;

        public VehicleCoverageTracker(CoverageMapDefinition definition, RoutingData data, CoverageMapManager manager, IResourceStore resStore, IRouteEngine routingEngine, IDatabaseFactory dbFactory)
        {
            _definition = definition;
            _data = data;
            _manager = manager;
            _resStore = resStore;
            _routingEngine = routingEngine;
            _dbFactory = dbFactory;
        }

        public CoverageMap GetCoverage()
        {
            UpdateCoverage();
            return CombinedMap;
        }

        /// <summary>
        ///     return a combined coverage map calculated using supplied overrides
        /// </summary>
        /// <param name="overrides"></param>
        /// <param name="routingEngine"></param>
        /// <returns></returns>
        public TrialCoverageResponse GetTrialCoverage(List<VehicleOverride> overrides)
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

                        if (_definition.MinuteLimit != null)
                        {
                            var request = new RouteRequestCoverage
                            {
                                Epsg= 4326,
                                StartPoints = new[] {updatedRoutingPoint},
                                VehicleType = _definition.RoutingResource,
                                HourOfWeek = DateTime.Now.HourOfWeek(),
                                DistanceMax = 10000,
                                DurationMax = 60*(int) _definition.MinuteLimit,
                                SearchType = RouteSearchType.Quickest,
                                RoadSpeedCalculator = "ConstantSpeedCalculator",
                                Name = _definition.Name,
                                Code= _definition.Code
                            };

                            // calculate the coverage map
                            var newMap = CalculateCoverage(request);

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

        private void UpdateCoverage()
        {
            if (_definition.VehicleCodes != null)
            {
                var vehicleCodes = _definition.VehicleCodes.Split(',');

                // get a list of vehicles and there positions from the database that match the criteria
                var resources = _resStore.GetResources(0, vehicleCodes, true, false);

                // mark each cache entry as invalid.. will be used later to remove unwanted coverage maps.
                _cache.Values.ToList().ForEach(x => x.Valid = false);

                Logger.Write(
                    $"Calculating Coverage for {_definition.VehicleCodes} = {string.Join(",", resources.Select(x => x.Callsign))} ", TraceEventType.Information, "Vehicle Coverage Tracker");

                // update cache location entry and create new ones as necessary
                UpdateCacheLocations(resources);

                // now all Current routing points have been updated and the cache entry made Valid .
                // now extract Valid records where the routing point is different from the previous routing point and recalculate the coverage map
                // we do this with one request

                // remove all cache entries that are not valid.. i.e. dont exist in the resource list from the database
                RemoveInvalidEntries();

                // calculate changed entries
                UpdateMapsforChangedLocations();

                if (CombinedMap != null)
                    Logger.Write(
                    $"Calculating Coverage for {_definition.VehicleCodes} Coverage = {CombinedMap.Percent*100,2} ", TraceEventType.Error,
                    "Vehicle Coverage Tracker");

                //_cache.Values.AsParallel().ForAll(x => x.PrevLocation = x.CurLocation);
            }
        }

        private void UpdateMapsforChangedLocations()
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
                {   Epsg= 4326,
                    StartPoints = new[] {ce.CurLocation},
                    VehicleType = _definition.RoutingResource,
                    HourOfWeek = DateTime.Now.HourOfWeek(),
                    DistanceMax = 16000,
                    DurationMax = 60*(int) _definition.MinuteLimit,
                    SearchType = RouteSearchType.Quickest,
                    RoadSpeedCalculator = "ConstantSpeedCalculator",
                    TileSize = 500
                };

                // calculate the coverage map

                try
                {
                    var newMap = CalculateCoverage(request);

                    newMap.Percent = newMap.Coverage();

                    if (CombinedMap == null)
                    {
                        CombinedMap = CoverageMapUtil.CreateEmptyCopy(newMap);
                        CombinedMap.Name = _definition.Name;
                        CombinedMap.Code = _definition.Code;
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

            if (CombinedMap != null)
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

        private void UpdateCacheLocations(List<QuestResource> resources)
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

                    if (res.Position == null)
                        continue;

                    if (res.Position.Latitude == 0 || res.Position.Longitude==0)
                        continue;

                    if (!_cache.ContainsKey(res.Callsign))
                    {
                        // create a new cache entry as this one doesn't exist
                        updatedRoutingPoint = new Coordinate
                        {
                            X = res.Position.Longitude,
                            Y = res.Position.Latitude
                        };

                        entry = new CacheEntry {Callsign = res.Callsign, CurLocation = updatedRoutingPoint, Map = null};
                        _cache.Add(res.Callsign, entry);
                    }
                    else
                        entry = _cache[res.Callsign];

                    // entry is now either a new or existing cache entry

                    if (updatedRoutingPoint == null) // i.e. not overriden or new, take new position from the record
                        updatedRoutingPoint = new Coordinate
                        {
                            X = res.Position.Longitude,
                            Y = res.Position.Latitude
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


        /// <summary>
        ///     calculate a coverage report for a given set of vehicles
        /// </summary>
        /// <returns></returns>
        public CoverageMap CalculateCoverage(RouteRequestCoverage request)
        {

            var map = _manager.GetStandardMap(request.TileSize)
                .Clone()
                .ClearData()
                .Name(request.Name)
                .Code(request.Code);

            foreach (var p in request.StartPoints)
            {
                var start = _data.GetEdgeFromPoint(p, request.Epsg);

                var routerequest = new RouteRequestMultiple
                {
                    StartLocation = start,
                    EndLocations = null,
                    DistanceMax = request.DistanceMax,
                    DurationMax = request.DurationMax,
                    InstanceMax = 0,
                    VehicleType = request.VehicleType,
                    SearchType = request.SearchType,
                    HourOfWeek = request.HourOfWeek,
                    Map = map,
                    RoadSpeedCalculator = request.RoadSpeedCalculator
                };

                _routingEngine.CalculateRouteMultiple(routerequest);
            }

            return map;
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