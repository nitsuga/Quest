using System;
using System.Linq;
using Quest.Lib.DataModel;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using GeoAPI.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using GeoAPI.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using NetTopologySuite.IO;
using Quest.Common.Messages;
using Quest.Lib.Trace;
using Quest.Lib.Data;

namespace Quest.Lib.Search.Indexers
{
    internal class OverlayIndexer : ElasticIndexer
    {
        private IDatabaseFactory _dbFactory;

        public OverlayIndexer(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public override void StartIndexing(BuildIndexSettings config)
        {
            if (config == null)
                return;

            var logFreq=10000;
            try
            {
                logFreq = config.Logfrequency;
                config.Logfrequency = 500;
                Build(config);
            }
            finally
            {
                config.Logfrequency = logFreq;
            }

        }

        private void Build(BuildIndexSettings config)
        {
            CoordinateSystemFactory csFact = new CoordinateSystemFactory();
            CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory();
            ICoordinateSystem from = csFact.CreateFromWkt(@"PROJCS[""OSGB 1936 / British National Grid"",GEOGCS[""OSGB 1936"",DATUM[""D_OSGB_1936"",SPHEROID[""Airy_1830"",6377563.396,299.3249646]],PRIMEM[""Greenwich"",0],UNIT[""Degree"",0.017453292519943295]],PROJECTION[""Transverse_Mercator""],PARAMETER[""latitude_of_origin"",49],PARAMETER[""central_meridian"",-2],PARAMETER[""scale_factor"",0.9996012717],PARAMETER[""false_easting"",400000],PARAMETER[""false_northing"",-100000],UNIT[""Meter"",1]]");
            IGeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;
            ICoordinateTransformation trans = ctFact.CreateFromCoordinateSystems(from, wgs84);
            WKTReader _reader = new WKTReader();

            _dbFactory.Execute<QuestContext>((db) =>
            {
                var descriptor = GetBulkRequest(config);

                foreach (var overlay in db.MapOverlay)
                {
                    try
                    {
                        config.RecordsTotal = db.MapOverlayItem.Count(x => x.MapOverlayId == overlay.MapOverlayId);
                        var data = db.MapOverlayItem
                            .Where(x => x.MapOverlayId == overlay.MapOverlayId);
                        foreach (var item in data)
                        {
                            config.RecordsCurrent++;

                            // commit any messages and report progress
                            CommitCheck(this, config, descriptor);

                            //var geom = _reader.Read(item.Wkt);
                            var geom = _reader.Read("");
                            var centre = geom.Centroid;

                            if (centre == null)
                            {
                                Logger.Write($"{GetType().Name}: Failed {overlay.OverlayName} {item.Description} has no centroid, maybe invalid geometry.", GetType().Name);
                                config.Skipped++;
                                continue;
                            }

                            var point = GeomUtils.ConvertToLatLonLoc(centre.Y, centre.X);

                            var address = new LocationDocument
                            {
                                Type = overlay.OverlayName,
                                Source = IndexBuilder.AddressDocumentType.Overlay,
                                ID = IndexBuilder.AddressDocumentType.Overlay + item.MapOverlayItemId,
                                Description = item.Description,
                                indextext = item.Description,
                                Location = point,
                                Poly = GeomUtils.GetPolygon(geom, trans),
                                Created = DateTime.UtcNow,
                                Status = "Active"
                            };


                            // add to the list of stuff to index
                            address.indextext = address.indextext.Replace("&", " and ");

                            // add item to the list of documents to index
                            AddIndexItem(address, descriptor);
                        }
                        // commit anything else
                        CommitBultRequest(config, descriptor);

                    }
                    catch (Exception ex)
                    {
                        Logger.Write($"{GetType().Name}: Failed {overlay.OverlayName} {ex}", GetType().Name);
                    }

                }
            });
        }
    }
}