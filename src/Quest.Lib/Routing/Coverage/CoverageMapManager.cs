using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using Quest.Lib.DataModel;
using Quest.Lib.Trace;
using Quest.Lib.Data;
using Quest.Lib.DependencyInjection;
using Quest.Common.Messages.Routing;

namespace Quest.Lib.Routing.Coverage
{
    [Injection]
    public class CoverageMapManager
    {
        private CoverageMap _standardCoverage;
        private IGeometry _standardGeometry;
        private IDatabaseFactory _dbFactory;

        public CoverageMapManager(  
            RoutingData data,
            IDatabaseFactory dbFactory
            )
        {
            _dbFactory = dbFactory;
        }

        public IGeometry GetStandardGeometry()
        {
            if (_standardGeometry == null)
                _standardGeometry = GetOperationalAreaInternal();
            return _standardGeometry;
        }

        public CoverageMap GetStandardMap(int tilesize)
        {
            if (_standardCoverage == null)
            {
                _standardGeometry = GetStandardGeometry();
                if (_standardGeometry != null)
                    _standardCoverage = MapFromGeometry("standard", _standardGeometry, tilesize);
            }
            return _standardCoverage;
        }

        public IGeometry GetOperationalAreaInternal()
        {
            Logger.Write("Calculating operational area", TraceEventType.Information, "CoverageMapUtil");
            for (var i = 1; i < 5; i++)
            {
                try
                {
                    return _dbFactory.Execute<QuestContext, IGeometry>((db) =>
                    {
                        // add in programmable ones.
                        var area = db.GetOperationalArea(2000);
                        // convert
                        var reader = new WKTReader();
                        var geoms = reader.Read(area.ToString());
                        Logger.Write($"Operational area is {geoms.Area} sq m", TraceEventType.Information, "CoverageMapUtil");
                        return geoms;
                    });
                }
                catch (Exception ex)
                {
                    Logger.Write($"Failed to calculate operational area: {ex.ToString()}", TraceEventType.Error, "CoverageMapUtil");
                }
            }
            return null;
        }

        public CoverageMap GetOperationalArea(int tilesize)
        {
            try
            {
                var map = GetStandardMap(tilesize);
                if (map != null)
                    return map.CalcCoverage(GetStandardGeometry());
            }
            catch (Exception ex)
            {
                Logger.Write($"Failed to calculate operational area: {ex}", TraceEventType.Error, "CoverageMapUtil");
            }
            return null;
        }

        public CoverageMap MapFromGeometry(string name, IGeometry geom, int tilesize)
        {
            var map = new CoverageMap();

            var e = new Envelope(
                Math.Floor(geom.EnvelopeInternal.MinX / 1000) * 1000,
                Math.Ceiling(geom.EnvelopeInternal.MaxX / 1000) * 1000,
                Math.Floor(geom.EnvelopeInternal.MinY / 1000) * 1000,
                Math.Ceiling(geom.EnvelopeInternal.MaxY / 1000) * 1000
                );

            map.SetExtent(name, e, tilesize);
            return map;
        }
    }
}