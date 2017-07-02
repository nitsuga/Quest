using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Quest.Mobile.Models;
using GeoJSON;
using Newtonsoft;
using Newtonsoft.Json;
using Quest.Mobile.Code;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using Quest.Lib;
using Quest.Lib.DataModel;

namespace Quest.Mobile.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
         [Authorize(Roles = "administrator,user")]
        public ActionResult Index()
        {
            ViewBag.Message = "Quest Mobile - Dashboard";

            return View();
        }


        [Authorize(Roles = "administrator,user")]
        [HttpGet]
        public ActionResult GetCCGData()
        {
            try
            {
                using(QuestEntities db=new QuestEntities())
                {
                    var results = (from c in db.GetCCGLatestCoverageStats() select new {c.desc, c.amb,c.fru,c.hol}).ToList();
                    var serializer = new JavaScriptSerializer();

                    // For simplicity just use Int32's max value.
                    // You could always read the value from the config section mentioned above.
                    serializer.MaxJsonLength = Int32.MaxValue;

                    var result = new ContentResult
                    {
                        Content = serializer.Serialize(results),
                        ContentType = "application/json"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
                //throw new Exception("Error getting data", ex);
            }
        }


         [Authorize(Roles = "administrator,user")]
        public ActionResult GetHeldCalls(string startdate, int numhours, int daysback)
        {
            StringBuilder Builder = new StringBuilder();
            StringWriter Writer = new StringWriter(Builder);
            Newtonsoft.Json.JsonSerializer ser = new JsonSerializer();

            try
            {
                using (QuestEntities _db = new QuestEntities())
                {
                    var fromTime = DateTime.Parse(startdate);
                    var toTime = fromTime.AddHours(numhours);

                    var results = (from x in _db.HeldCallsWithHistory(fromTime, toTime, daysback) where x.t > fromTime && x.t < toTime orderby x.t select x).ToList();

                    ser.Serialize(Writer, results);

                    var result = new ContentResult
                    {
                        Content = Builder.ToString(),
                        ContentType = "application/json"
                    };
                    return result;
                }
            }
            catch
            {
            }
            return null;
        }


         [Authorize(Roles = "administrator,user")]
        [HttpGet]
        public ActionResult GetHeldCallsData()
        {
            StringBuilder Builder = new StringBuilder();
            StringWriter Writer = new StringWriter(Builder);
            Newtonsoft.Json.JsonSerializer ser = new JsonSerializer();

            var results = GetHeldCallsEx(10, 10, 10);

            ser.Serialize(Writer, results);

            var serializer = new JavaScriptSerializer();

            // For simplicity just use Int32's max value.
            // You could always read the value from the config section mentioned above.
            serializer.MaxJsonLength = Int32.MaxValue;


            var result = new ContentResult
            {
                Content = Builder.ToString(),
                ContentType = "application/json"
            };
            return result;
        }


         [Authorize(Roles = "administrator,user")]
        public HeldCallsTotal GetHeldCallsEx(int daysback, int hoursBack, int hoursForward)
        {

            HeldCallsTotal retval = new HeldCallsTotal();
            retval.AreaSummary = new List<HeldCallsArea>();
            retval.PrioritySummary = new List<HeldCallData>();
            retval.Qty = 0;

            Dictionary<String, int> priorityCounts = new Dictionary<string, int>();
            
            try
            {
                using (QuestEntities db = new QuestEntities())
                {
                   
                        //DateTime s1to = new DateTime(2013, 09, 11, 12, 0, 0);
                        DateTime s1to = DateTime.Now;

                        DateTime s1from = s1to.Subtract(new TimeSpan(hoursBack, 0, 0));
                        DateTime s1topred = DateTime.Now.Add(new TimeSpan(hoursForward, 0, 0)); ;

                        DateTime s2from = s1from.Subtract(new TimeSpan(daysback * 24, 0, 0));
                        DateTime s2to = s2from.Add(new TimeSpan(hoursBack + hoursForward, 0, 0));

                        // get the history
                        var r1 = db.HeldCallsSummaries.Where(h => h.TStamp >= s1from && h.TStamp <= s1to).OrderBy(h => h.TStamp).ToList();
                        retval.S1 = r1.Select(h => new HeldCallsHistoryRecord() { Qty = (int)h.Qty, TStamp = (DateTime)h.TStamp }).ToList();

                        var r2 = db.HeldCallsSummaries.Where(h => h.TStamp >= s2from && h.TStamp <= s2to).OrderBy(h => h.TStamp).ToList();
                        retval.S2 = r2.Select(h => new HeldCallsHistoryRecord() { Qty = (int)h.Qty, TStamp = (DateTime)h.TStamp + new TimeSpan(daysback * 24, 0, 0), Hour = ((DateTime)h.TStamp + new TimeSpan(daysback * 24, 0, 0)).Hour }).OrderBy(i => i.Hour).ToList();
                        

                        // for each area, get the area data

                        var areas = from result in db.GetHeldCalls()
                                    group result by result.Area into g
                                    orderby g.Key
                                    select g;

                        int totalsum = 0;
                        foreach (var v in areas)
                        {
                            int sum = 0;
                            HeldCallsArea area = new HeldCallsArea() { Area = v.Key};
                            retval.AreaSummary.Add(area);

                            //area.History = (from h in db.HeldCallHistoryByAreas where h.Area == v.Key && h.tstamp > s1from orderby h.tstamp select new HeldCallsHistoryRecord() { Qty = (int)h.Qty, TStamp = (DateTime)h.tstamp }).ToList();

                            var calls = from r in db.GetHeldCalls() where r.Area == v.Key orderby r.Ordinal select r;

                            foreach (var call in calls)
                            {
                                var d = new HeldCallData();

                                d.Oldest = call.Oldest ?? 0;
                                d.Priority = call.Priority;
                                d.Qty = call.Qty ?? 0;
                                d.Histogram = new List<HistogramData>();
                                d.ChartColour = call.ChartColour;

                                sum += d.Qty;

                                // get the histogram
                                var groups = from h in db.GetHeldCallsViewGrouped() where h.Area == call.Area && h.Priority == call.Priority select h;
                                foreach (var g in groups)
                                    d.Histogram.Add(new HistogramData() { Qty = g.Qty ?? 0, Time = g.FromMins.ToString() });

                                // history

                                //area.Data.Add(d);
                            }
                            area.Qty = sum;
                            totalsum += sum;
                        }

                        //retval.PrioritySummary = db.HeldCallBreaches
                        //    .OrderBy(call => call.Ordinal)
                        //    .Select(call => new HeldCallData()
                        //    {
                        //        Priority = call.Priority,
                        //        Qty = (int)call.Qty,
                        //        QtyRBFailed = call.CallbackFail,
                        //        QtyRBOk = (int)call.CallbackOk,
                        //        Oldest = (int)call.Oldest,
                        //        ChartColour = call.ChartColour.Trim()
                        //    })
                        //        .ToList();
#if true
                        // update priority counts
                        var priorities = from result in db.GetHeldCalls()
                                         group result by result.Priority into g
                                         orderby g.Key
                                         select new { Priority = g.Key, Qty = g.Sum(x => x.Qty), Oldest = g.Max(x => x.Oldest), ChartColour = g.Min(x => x.ChartColour), Ordinal = g.Min(x => x.Ordinal) };

                        foreach (var call in priorities.OrderBy(x => x.Ordinal))
                        {
                            retval.PrioritySummary.Add(
                                new HeldCallData() 
                                { 
                                    Priority = call.Priority, 
                                    Qty = (int)call.Qty, 
                                    QtyRBFailed = 0,
                                    QtyRBOk = (int)call.Qty,
                                    Oldest = (int)call.Oldest, 
                                    ChartColour = call.ChartColour.Trim() 
                                }
                                );
                        }
#endif
                        retval.Qty = totalsum;
                }
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                
            }
            return retval;
        }
    }
}
