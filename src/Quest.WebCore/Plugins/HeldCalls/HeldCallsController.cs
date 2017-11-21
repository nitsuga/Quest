using Microsoft.AspNetCore.Mvc;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Services;

namespace Quest.WebCore.Plugins.HeldCalls
{
    public class HeldCallsController : Controller
    {
        private AsyncMessageCache _messageCache;
        private readonly IPluginService _pluginService;
        private readonly HeldCallsPlugin _plugin;

        public HeldCallsController(
                HeldCallsPlugin plugin,
                AsyncMessageCache messageCache,
                IPluginService pluginService
            )
        {
            _plugin = plugin;
            _pluginService = pluginService;
            _messageCache = messageCache;
        }
#if false
        [HttpGet]
        public ActionResult GetHeldCallsTest()
        {
            string startdate = "2014-05-17 04:00";
            int numhours = 2;
            int daysback = 7;
            return GetHeldCalls(startdate, numhours, daysback);
        }


        [HttpGet]
        public ActionResult GetHeldCalls(string startdate, int numhours, int daysback)
        {
            var serializer = new JavaScriptSerializer();
            try
            {
                using (QuestEntities _db = new QuestEntities())
                {
                    var toTime1 = DateTime.Parse(startdate);
                    var fromTime1 = toTime1.AddHours(-numhours);

                    var toTime2 = toTime1.AddDays(-daysback);
                    var fromTime2 = fromTime1.AddDays(-daysback);

                    // get current values
                    var data1 = _db.HeldCalls10MinView.Where(x => x.t >= fromTime1 && x.t <= toTime1)
                        .GroupBy(
                            x => x.t,
                            (ts, value) => new
                            {
                                t = ts,
                                a = (double)value.Sum(x => x.q)
                            }
                        )
                        .ToList();

                    // get historic values
                    var data2 = _db.HeldCalls10MinView.Where(x => x.t >= fromTime2 && x.t <= toTime2)
                        .GroupBy(
                            x => x.t,
                            (ts, value) => new
                            {
                                t = ts,
                                a = (double)value.Sum(x => x.q)
                            }
                        )
                        .ToList();



                    var result1 = data1
                        .OrderBy(x => x.t)
                        .Select(x => new double[]
                        {
                            MilliTimeStamp(x.t),
                            x.a
                        })
                        .ToList();

                    var result2 = data2
                        .OrderBy(x => x.t)
                        .Select(x => new double[]
                        {
                            MilliTimeStamp(x.t.AddDays(daysback)),
                            x.a
                        })
                        .ToList();

                    // remove elements in result1 where tstamp is not in result2
                    result1.RemoveAll(x => !result2.Exists(y => y[0] == x[0]));

                    // combine the two series.
                    var combinedresult = new List<TimeSeries> {
                        new TimeSeries()        // new data
                        {
                             key="Current",
                             values = result1.ToArray()
                        },
                        new TimeSeries()        // old data
                        {
                             key="Last week",
                             values = result2.ToArray()
                        },

                    };

                    // For simplicity just use Int32's max value.
                    // You could always read the value from the config section mentioned above.
                    serializer.MaxJsonLength = Int32.MaxValue;

                    var result = new ContentResult
                    {
                        Content = serializer.Serialize(combinedresult),
                        ContentType = "application/json"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                var result = new ContentResult
                {
                    Content = serializer.Serialize(new { errormsg = ex.Message, stack = ex.StackTrace }),
                    ContentType = "application/json"
                };
            }
            return null;
        }



        [HttpGet]
        public ActionResult GetHeldCallsSummary()
        {
            var serializer = new JavaScriptSerializer();
            try
            {
                using (QuestEntities _db = new QuestEntities())
                {

                    // get current values
                    var data = _db.HeldCallsViews.ToList();

                    // group by category
                    var cat = data.OrderBy(x => x.Ordinal).GroupBy(
                            x => x.Priority,
                            (p, value) => new
                            {
                                p = p,
                                q = value.Select(y => y.Qty).Sum(),

                            }
                           );

                    // group by area
                    var area = data.OrderBy(x => x.Area).GroupBy(
                            x => x.Area,
                            (a, value) => new
                            {
                                a = a,
                                q = value.Select(y => y.Qty).Sum()
                            }
                        );

                    var combinedresult =
                    new
                    {
                        total = cat.Sum(x => x.q),
                        cat = cat,
                        area = area,
                        data = data.Select(x => new { p = x.Priority, a = x.Area, q = x.Qty, o = x.Ordinal, t = x.Oldest })
                    };

                    // For simplicity just use Int32's max value.
                    // You could always read the value from the config section mentioned above.
                    serializer.MaxJsonLength = Int32.MaxValue;

                    var result = new ContentResult
                    {
                        Content = serializer.Serialize(combinedresult),
                        ContentType = "application/json"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                var result = new ContentResult
                {
                    Content = serializer.Serialize(new { errormsg = ex.Message, stack = ex.StackTrace }),
                    ContentType = "application/json"
                };
            }
            return null;
        }
#endif

    }

}