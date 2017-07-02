using GeoAPI.Geometries;
using ServiceBus.Objects;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using RTree;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Quest.Lib.DataModel;
using Quest.Common.Messages;
using Quest.Lib.Trace;

namespace Quest.Lib.Routing
{
    internal class CoverageStats
    {
        private int SampleEveryMinutes = 15;
        private List<CoverStat> _mapItems = new List<CoverStat>();
        private RTree.RTree<RoutingLocation> _mapIndex = new RTree.RTree<RoutingLocation>(100, 1);        
        
        /// <summary>
        /// Calculate a coverage mask for a given map overlay item
        /// </summary>
        /// <returns>Complete coverage map for the entire area</returns>
        public CoverageMap CalcGeometryCoverage(Rectangle limits)
        {
            CoverageMap complete;

            String[] TrackOverlays = Utils.SettingsHelper.GetVariable("Coverage.TrackOverlays", "").Split(',');

            Logger.Write("Calculating geometries for monitored map overlays");

            // initialise a complete coverage map - i.e. every cell in london is covered
            complete = new CoverageMap();
            complete.SetExtent("Complete", limits, VehicleCoverageTracker.TILESIZE);

            using (QuestEntities db = new QuestEntities())
            {
                // get list of overlays to work out 

                foreach (String overlay in TrackOverlays)
                {

                    int? overlayID = db.MapOverlays.Where(x => x.OverlayName == overlay).Select(x => x.MapOverlayId).FirstOrDefault();
                    if (overlayID == null)
                    {
                        Logger.Write("Can't find layer " + overlay + " to track coverage, make sure items variable Coverage.TrackOverlays are defined in MapOverlays table");
                    }
                    else
                    {
                        var items = db.MapOverlayItems.Where(x => x.MapOverlay.MapOverlayId == overlayID);
                        foreach (var item in items)
                        {
                            CoverageMap map = new CoverageMap();
                            map.SetExtent(item.Description, limits, VehicleCoverageTracker.TILESIZE);

                            // calculate the map fom the coverage
                            map.CalcGeometryCoverage(item.geom);
                            complete.CalcGeometryCoverage(item.geom);

                            item.CoverageMap = map.Data;

                            //NetTopologySuite.IO.WKTReader reader = new NetTopologySuite.IO.WKTReader();
                            //IGeometry geometry = reader.Read(item.geom.ToString());

                            CoverStat stat = new CoverStat() { map = map, MapOverlayItemID = item.MapOverlayItemId, Cells = map.Hits() };
                            _mapItems.Add(stat);

                            Logger.Write(String.Format("Calculated coverage map for layer {0} .. {1} cells",  item.Description, stat.Cells ));
                        }
                        db.SaveChanges();
                    }

                }
            }

            Logger.Write("Finished calculating overlay coverage maps.");

            return complete;
        }

        /// <summary>
        /// update current and historic records and save in database
        /// </summary>
        public void CalculateCoverageStats()
        {
            using (QuestEntities db = new QuestEntities())
            {
                // get oldest non-temporary result
                var runcount = db.MapOverlayStatsRuns
                                    .Where(x => x.IsLatest == false)
                                    .Count();


                DateTime fromTime;

                // never run before? use the oldest data available as a start point
                if (runcount == 0)
                {
                    var firstdata = db.CoverageMapStores
                                    .Where(x => x.Data != null)
                                    .Min(x => x.tstamp);

                    fromTime = firstdata;
                }
                else
                { 
                    fromTime = db.MapOverlayStatsRuns
                                .Where(x => x.IsLatest == false)
                                .Max(x => x.TimeStamp);
                }

                // get here if no data and never run.. cant continue
                if (fromTime == null)
                {
                    Logger.Write(String.Format("Can't calculate coverages for areas as there is no data"));
                    return;
                }

                // while last run older that what we're supposed to have, keep looping and calculating coverage
                while (DateTime.Now.Subtract((DateTime)fromTime).TotalMinutes > SampleEveryMinutes)
                {
                    DateTime toTime = fromTime.AddMinutes(1);
                    toTime = toTime.Subtract(new TimeSpan(0, 0, toTime.Second));
                    toTime = toTime.Subtract(new TimeSpan(0, 0, 0, 0, toTime.Millisecond));

                    // add minutes until you get to on the hour/quarter to/past, half past
                    while ( Math.IEEERemainder( toTime.Minute, 15.0)!=0)                        
                        toTime = toTime.AddMinutes(1);


                    // save to database - step 1 - create the run
                    MapOverlayStatsRun thisRun = new MapOverlayStatsRun() { IsLatest = false, TimeStamp = toTime };
                    db.MapOverlayStatsRuns.Add(thisRun);
                    db.SaveChanges();

                    CalculateCoverageStats(db, fromTime, toTime, thisRun);
                    db.SaveChanges();

                    fromTime = toTime;
                }

                // now update the temporary records - first remove old records

                var trans = db.Database.BeginTransaction();
                var records = db.MapOverlayStatsRuns.Where(x => x.IsLatest);
                db.MapOverlayStatsRuns.RemoveRange(records);

                // save to database - step 1 - create the run
                MapOverlayStatsRun tempRun = new MapOverlayStatsRun() { IsLatest = true, TimeStamp = DateTime.Now };
                db.MapOverlayStatsRuns.Add(tempRun);
                    db.SaveChanges();

                CalculateCoverageStats(db, fromTime, tempRun.TimeStamp, tempRun);

                trans.Commit();
                
            }
        }

        public void CalculateCoverageStats(QuestEntities db, DateTime fromTime, DateTime toTime, MapOverlayStatsRun thisRun)
        {
            double AmberHolesLimit=0;
            double RedHolesLimit=0;
            double FlashLimit = 0;
            String AmberHolesColour="Orange";
            String RedHolesColour="Red";

            // compute the coverage between those times
            List<ItemCoverage> AMBresult= CalculateCoverageStats(fromTime, toTime, "AMB Coverage");
            List<ItemCoverage> FRUresult= CalculateCoverageStats(fromTime, toTime, "FRU Coverage");
            List<ItemCoverage> INCresult= CalculateCoverageStats(fromTime, toTime, "Expected Incidents");
            List<ItemCoverage> HOLresult= CalculateCoverageStats(fromTime, toTime, "Resource Holes");

            // no data
            if (HOLresult == null && AMBresult == null && FRUresult == null && INCresult == null)
                return;

            if (thisRun.IsLatest)
            {
                AmberHolesLimit = Utils.SettingsHelper.GetVariable("Coverage.Holes.AmberLimit", 0.4);
                RedHolesLimit = Utils.SettingsHelper.GetVariable("Coverage.Holes.RedLimit", 0.7);
                AmberHolesColour = Utils.SettingsHelper.GetVariable("Coverage.Holes.AmberColour", "Orange");
                RedHolesColour = Utils.SettingsHelper.GetVariable("Coverage.Holes.RedColour", "Red");
                FlashLimit = Utils.SettingsHelper.GetVariable("Coverage.Holes.FlashLimit", 0.9);
            }

            String[] TrackOverlays = Utils.SettingsHelper.GetVariable("Coverage.TrackOverlays", "").Split(',');
//            using (QuestEntities db = new QuestEntities())
//            {
                foreach (String overlay in TrackOverlays)
                {
                    int? overlayID = db.MapOverlays.Where(x => x.OverlayName == overlay).Select(x => x.MapOverlayId).FirstOrDefault();
                    if (overlayID != null)
                    {
                        List<MapOverlayItem> items = db.MapOverlayItems.Where(x => x.MapOverlay.MapOverlayId == overlayID).ToList();
                        foreach (var overlayItem in items)
                        {
                            MapOverlayItemStat stat = new MapOverlayItemStat() 
                            { 
                                MapOverlayStatsRun = thisRun, 
                                MapOverlayItem = overlayItem,
                                AvailableAEU =0,
                                AvailableFRU =0 
                            };

                            // pick out each statistic
                            if (AMBresult != null)
                                stat.AMBCoverage = AMBresult.Where(x => x.MapOverlayItemID == overlayItem.MapOverlayItemId).Select(x => x.Coverage).DefaultIfEmpty(0).FirstOrDefault();

                            if (FRUresult != null)
                                stat.FRUCoverage = FRUresult.Where(x => x.MapOverlayItemID == overlayItem.MapOverlayItemId).Select(x => x.Coverage).DefaultIfEmpty(0).FirstOrDefault();

                            if (INCresult != null)
                                stat.INCCoverage = INCresult.Where(x => x.MapOverlayItemID == overlayItem.MapOverlayItemId).Select(x => x.Coverage).DefaultIfEmpty(0).FirstOrDefault();

                            if (HOLresult != null)
                                stat.Holes = HOLresult.Where(x => x.MapOverlayItemID == overlayItem.MapOverlayItemId).Select(x => x.Coverage).DefaultIfEmpty(0).FirstOrDefault();

                            db.MapOverlayItemStats.Add(stat);

                            float alimit = overlayItem.AmberLimit == null ? (float)AmberHolesLimit : overlayItem.AmberLimit.Value;
                            float rlimit = overlayItem.RedLimit == null ? (float)RedHolesLimit : overlayItem.RedLimit.Value;
                            float flimit = overlayItem.FlashLimit == null ? (float)FlashLimit : overlayItem.FlashLimit.Value;
                            
                            // update RAG status
                            if (thisRun.IsLatest)
                            {
                                overlayItem.FillColour = "Transparent";
                                if (stat.Holes > alimit)
                                    overlayItem.FillColour = AmberHolesColour;

                                if (stat.Holes > rlimit)
                                    overlayItem.FillColour = RedHolesColour;

                                overlayItem.Flash = (stat.Holes > flimit);
                                
                            }
                        }
                    }
                }
                db.SaveChanges();
            //            }
        }

        /// <summary>
        /// calculate the coverage stats between two times and return a list if mapitem results.
        /// </summary>
        /// <param name="fromTime"></param>
        /// <param name="toTime"></param>
        /// <returns></returns>
        public List<ItemCoverage> CalculateCoverageStats(DateTime fromTime, DateTime toTime, String CoverageName)
        {
            List<ItemCoverage> results = new List<ItemCoverage>();

            Logger.Write(String.Format("Calculating {2} coverage stats for between {0} and {1}", fromTime, toTime, CoverageName));


            // keep count of how many coverages we process.
            int coverCount = 0;

            // make sure others dont tamper with our map items while we're processing
            lock (_mapItems)
            {
                // zero counts
                _mapItems.ForEach(x => { 
                    x.Coverage = 0; 
                    x.TotalHits = 0;
                });

                using (QuestEntities db = new QuestEntities())
                {

                    // get each coverage between dates specified
                    var coverageRecords = db.CoverageMapStores.Where(x => x.tstamp > fromTime && x.tstamp <= toTime && x.Data != null && x.Name == CoverageName);

                    // process each coverage layer
                    foreach (var cr in coverageRecords)
                    {
                        coverCount++;

                        // reconstruct a coverage map from the database data
                        CoverageMap map = new CoverageMap() { Blocksize = cr.Blocksize, Columns = cr.Columns, Data = cr.Data, Name = cr.Name, OffsetX = cr.OffsetX, OffsetY = cr.OffsetY, Rows = cr.Rows, Percent = cr.Percent ?? 0 };

                        //foreach (var item in _mapItems)
                        //{
                        //    double coverage = map.Coverage(item.map);
                        //    item.Coverage += coverage;
                        //}
                        _mapItems.AsParallel().ForAll( x=>
                        {
                            double coverage = map.Coverage(x.map);
                            x.Coverage += coverage;
                        });


                    } // for each coverage map found

                    // update averages
                    _mapItems.ForEach(x => x.Coverage /= coverCount);
                }
            }

            if (coverCount == 0)
                return null;

            // create empty set of results
            foreach (var item in _mapItems)
            {
                if (double.IsNaN( item.Coverage))
                    item.Coverage = 0;

                results.Add(new ItemCoverage() { Coverage = item.Coverage, MapOverlayItemID = item.MapOverlayItemID });
            }


            Logger.Write(String.Format("Calculated coverage stats for between {0} and {1} using {2} historic coverages", fromTime, toTime, coverCount));

            return results;
        }

    }

    /// <summary>
    /// class to keep track of current map overlay item.
    /// </summary>
    internal class CoverStat
    {
        public int MapOverlayItemID;
        public double Coverage;
        public int TotalHits;
        public int Cells;
        public CoverageMap map;
    }

    internal class ItemCoverage
    {
        public int MapOverlayItemID;
        public double Coverage;
    }

}
