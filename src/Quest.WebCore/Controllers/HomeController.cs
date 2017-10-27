using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quest.WebCore.Services;
using Quest.WebCore.Models;
using Quest.Common.Messages;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using Quest.Lib.Utils;
using Quest.Lib.ServiceBus;
using Quest.Common.Messages.Gazetteer;
using Quest.Common.Messages.Routing;

namespace Quest.WebCore.Controllers
{
    public class HomeController : Controller
    {
        private AsyncMessageCache _messageCache;
        private ResourceService _resourceService;
        private IncidentService _incidentService;
        private DestinationService _destinationService;
        private SearchService _searchService;
        private RouteService _routeService;
        private TelephonyService _telephonyService;
        private SecurityService _securityService;
        private readonly IPluginService _pluginService;

        public HomeController(AsyncMessageCache messageCache,
                IPluginService pluginFactory,
                ResourceService resourceService,
                IncidentService incidentService,
                DestinationService destinationService,
                SearchService searchService,
                RouteService routeService,
                TelephonyService telephonyService,
                VisualisationService visualisationService,
                SecurityService securityService
            )
        {
            _messageCache = messageCache;
            _resourceService = resourceService;
            _incidentService = incidentService;
            _destinationService = destinationService;
            _searchService = searchService;
            _routeService = routeService;
            _telephonyService = telephonyService;
            _securityService = securityService;
            _pluginService = pluginFactory;
        }

        async Task<HomeViewModel> DefaultHomeViewModel()
        {
            var user = HttpContext.User.Identity.Name;

            if (user == null)
                user = "Guest";

            var claims = await _securityService.GetAppClaims(user);
            //var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var groupResult = await _searchService.GetIndexGroups();

            HomeViewModel model = new HomeViewModel
            {
                User = user,
                Claims = claims,
                IndexGroups = groupResult?.Groups,
            };
            return model;
        }


        // GET: Home
        public ActionResult Index()
        {
            //var summary = CookieProxy.GetSelectedPluginLayout(Request, Response);
            //var model = _pluginService.GetLayoutModel(layout);

            var model = new HudModel
            {
                Scripts = _pluginService.GetScripts(),
                Styles = _pluginService.GetStyles(),
                Layout = _pluginService.DefaultLayout()
            };

            return View(model);
        }
        
        //  old stuff below here

        public async Task<IActionResult> Map()
        {
            HomeViewModel model = await DefaultHomeViewModel();
            return View(model);
        }

        
        public ActionResult TestLocations()
        {
            TestLocationsModel model = new TestLocationsModel();
            return View(model);
        }

        
        public async Task<ActionResult> TestRoutes()
        {
            HomeViewModel model = await DefaultHomeViewModel();
            return View(model);
        }

       
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public ActionResult SubmitCLI(string cli, string extension)
        {
            _telephonyService.SubmitCli(cli, extension);
            var js = JsonConvert.SerializeObject(true);
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };

            return result;
        }

        
        [HttpGet]
        [ResponseCache(NoStore =true, Location =ResponseCacheLocation.None)]
        public ActionResult StartIndexing(string Index, string Indexers, string IndexMode, int Shards, int Replicas, bool UseMaster)
        {
            IndexRequest request = new IndexRequest
            {
                Index = Index,
                Indexers = Indexers,
                IndexMode = IndexMode,
                Shards = Shards,
                Replicas = Replicas,
                UseMaster = UseMaster
            };

            var results = _searchService.Index(request);
            var js = JsonConvert.SerializeObject(results);
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };

            return result;
        }

        
        [HttpGet]
        [ResponseCache(NoStore =true, Location =ResponseCacheLocation.None)]
        public async Task<ActionResult> Route(string from, string to, string roadSpeedCalculator, string vehicle, int hour)
        {
            try
            {
                var route = await _routeService.Route(from, to, roadSpeedCalculator, vehicle, hour, User.Identity.Name);

                if (route == null || route.Items.Count == 0)
                    return new ContentResult
                    {
                        Content = "no route",
                        ContentType = "application/json"
                    };

                var firstRoute = route.Items.First();
                var points = firstRoute.PathPoints.Select(p => LatLongConverter.OSRefToWGS84(p.X, p.Y)).Select(c => new p { x = c.Longitude, y = c.Latitude }).ToList();

                var js = JsonConvert.SerializeObject(points);
                return new ContentResult
                {
                    Content = js,
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                return new ContentResult
                {
                    Content = ex.Message,
                    ContentType = "application/json"
                };
            }
        }

        
        [HttpGet]
        [ResponseCache(NoStore =true, Location =ResponseCacheLocation.None)]
        public ActionResult RouteCompare(int id, string roadSpeedCalculator)
        {
            RoutingResponse route = _routeService.RouteCompare(id, roadSpeedCalculator, User.Identity.Name);

            if (route == null || route.Items.Count == 0)
                return new ContentResult
                {
                    Content = "no route",
                    ContentType = "application/json"
                };

            var firstRoute = route.Items.First();
            var points = firstRoute.PathPoints.Select(p => LatLongConverter.OSRefToWGS84(p.X, p.Y)).Select(c => new p { x = c.Longitude, y = c.Latitude }).ToList();

            var js = JsonConvert.SerializeObject(points);
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };

            return result;
        }

        
        [HttpGet]
        [ResponseCache(NoStore =true, Location =ResponseCacheLocation.None)]
        public async Task<ActionResult> SemanticSearch(string searchText, int searchMode, bool includeAggregates, int skip, int take, double w, double s, double e, double n, bool boundsfilter, string filterterms, string displayGroup, string indexGroup)
        {
            try
            {
                BBFilter bb = null;
                if (boundsfilter)
                    bb = new BBFilter() { br_lat = s, br_lon = e, tl_lat = n, tl_lon = w };

                var filters = new List<TermFilter>();

                if (!string.IsNullOrEmpty(filterterms))
                {
                    filters = (List<TermFilter>)JsonConvert.DeserializeObject(filterterms, typeof(List<TermFilter>));
                }

                SearchResultDisplayGroup displayGroupEnum = SearchResultDisplayGroup.description;
                if (displayGroup != null)
                    Enum.TryParse<SearchResultDisplayGroup>(displayGroup, out displayGroupEnum);

                var request = new SearchRequest()
                {
                    includeAggregates = includeAggregates,
                    searchMode = (SearchMode)searchMode,
                    take = take,
                    skip = skip,
                    searchText = searchText,
                    box = bb,
                    filters = filters,
                    displayGroup = displayGroupEnum,
                    username = User.Identity.Name,
                    indexGroup = indexGroup
                };

                var searchResult = await _searchService.SemanticSearch(request);

                var data = GetDisplaySearchResult(searchResult);

                if (data != null)
                {
                    var js = JsonConvert.SerializeObject(data);

                    var msg = string.Format("Search: '{0}' mode={1} bound {6} {2}/{3} {4}/{5} filters='{7}' found {8} in {9}ms", searchText, searchMode, w, n, e, s, boundsfilter ? "on" : "off", filterterms, searchResult.Count, searchResult.millisecs);
                    var result = new ContentResult
                    {
                        Content = js,
                        ContentType = "application/json"
                    };

                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                var js = JsonConvert.SerializeObject(new { error = ex.Message });
                var result = new ContentResult
                {
                    Content = js,
                    ContentType = "application/json"
                };
                return result;
            }
        }

        DisplaySearchResult GetDisplaySearchResult(SearchResponse searchResult)
        {
            if (searchResult == null)
                return null;

            // create abridged version of the results for speed
            var data = new DisplaySearchResult
            {
                Aggregates = searchResult.Aggregates,
                Count = searchResult.Count,
                Documents = searchResult.Documents.Select(x => new DisplayDocument()
                {
                    d = x.l.Description,
                    grp = x.l.GroupingIdentity,
                    ID = x.l.ID,
                    l = x.l.Location,
                    ml = x.l.MultiLine,
                    pg = x.l.Poly,
                    s = (float)x.s,
                    src = x.l.Source,
                    t = x.l.Type,
                    url = x.l.Url,
                    i = x.l.Image,
                    v = x.l.Video,
                    st = x.l.Status,
                    c = x.l.Content
                }).ToList(),
                Grouping = searchResult.Grouping,
                ms = searchResult.millisecs,
                Removed = searchResult.Removed,
                Bounds = searchResult.Bounds
            };

            return data;
        }

        [HttpGet]
        public async Task<ActionResult> InformationSearch(double lng, double lat)
        {
            var request = new InfoSearchRequest() { lat = lat, lng = lng };
            Debug.WriteLine($"Searching at lat: {lat}, long: {lng}");
            var searchResult = await _searchService.InfoSearch(request);
            var jsonp = GetDisplaySearchResult(searchResult);
            var js = JsonConvert.SerializeObject(jsonp);
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };
            return result;
        }

        [HttpGet]
        public ActionResult CancelDevice(string callsign, string eventId)
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            var ser = new JsonSerializer();
            try
            {
                var result = _resourceService.CancelDevice(callsign, eventId);
                ser.Serialize(writer, result);
                var ok = new ContentResult
                {
                    Content = builder.ToString(),
                    ContentType = "application/json"
                };
                return ok;
            }
            catch
            {
                ser.Serialize(writer, "request failed");
                var error = new ContentResult
                {
                    Content = builder.ToString(),
                    ContentType = "application/json"
                };
                return error;
            }
        }

        [HttpGet]
        [ResponseCache(NoStore =true, Location =ResponseCacheLocation.None)]
        public ActionResult GetDestinations(bool hosp = false, bool standby = false, bool station = false, bool road = false, bool ae = false)
        {
            try
            {
                var features = _destinationService.GetDestinations(hosp, standby, station);
                var js = JsonConvert.SerializeObject(features);
                var result = new ContentResult
                {
                    Content = js,
                    ContentType = "application/json"
                };

                return result;
            }
            catch (Exception ex)
            {
                var errorMsg = "ERROR: Cannot complete Resource" + ex.Message;
                var error = new ContentResult
                {
                    Content = errorMsg,
                    ContentType = "application/json"
                };

                return error;
            }
        }

        
        [HttpGet]
        public ActionResult GetResources(bool avail = false, bool busy = false)
        {
            try
            {
                var r1 = _resourceService.GetResources(avail, busy);
                var js = JsonConvert.SerializeObject(r1);
                var result = new ContentResult
                {
                    Content = js,
                    ContentType = "application/json"
                };

                return result;
            }
            catch (Exception ex)
            {
                var js = JsonConvert.SerializeObject(new { error = ex.Message });
                var result = new ContentResult
                {
                    Content = js,
                    ContentType = "application/json"
                };
                return result;

            }
        }

        
        [HttpGet]
        public ActionResult GetIncidents(bool includeCatA = false, bool includeCatB = false)
        {
            try
            {
                var incs = _incidentService.GetIncidents(includeCatA, includeCatB);
                var js = JsonConvert.SerializeObject(incs);
                var result = new ContentResult
                {
                    Content = js,
                    ContentType = "application/json"
                };
                return result;
            }
            catch (Exception ex)
            {
                var errorMsg = "ERROR: Cannot complete Incidents" + ex.Message;
                var error = new ContentResult
                {
                    Content = errorMsg,
                    ContentType = "application/json"
                };

                return error;

                //throw new Exception("Couldn't get list of resources", ex);
            }
        }



#if false
        
                [HttpGet]
        public ActionResult GetVehicleCoverage(int vehtype)
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            var ser = new JsonSerializer();
            GetCoverageResponse map;
            try
            {
                GetCoverageRequest request = new GetCoverageRequest { vehtype = vehtype };

                map = _routeService.GetVehicleCoverage(request);
                if (map == null)
                    map = new GetCoverageResponse() { Message = "no vehicle coverage available", Success = false };

                var serializer = new JsonSerializer();
                //var serializer = new JavaScriptSerializer();
                //serializer.MaxJsonLength = Int32.MaxValue;
                var result = new ContentResult
                {
                    Content = serializer.SerializeToString(map),
                    ContentType = "application/json"
                };

                return result;
            }
            catch (Exception)
            {
                var errorMsg = "ERROR: Cannot get list of Resources";
                ser.Serialize(writer, errorMsg);
                var error = new ContentResult
                {
                    Content = builder.ToString(),
                    ContentType = "application/json"
                };

                return error;
            }
        }

        [HttpGet]
        [ResponseCache(NoStore =true, Location =ResponseCacheLocation.None)]
        public ActionResult BatchSearchLocations(string locations)
        {
            var job = _searchService.BatchSearch(locations);
            var jsonp = job;
            var js = JsonConvert.SerializeObject(jsonp);
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };

            return result;
        }        
        
        [HttpGet]
        [ResponseCache(NoStore =true, Location =ResponseCacheLocation.None)]
        public ActionResult GetLocationsJob(int jobid)
        {
            var job = _searchService.GetJob(jobid);
            var jsonp = job;
            var js = JsonConvert.SerializeObject(jsonp);
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };

            return result;
        }

        GetCoverageResponse GetCoverageHandler(int vehtype)
        {
            using (var db = new QuestEntities())
            {
                var result = new GetCoverageResponse();
                var results = db.GetVehicleCoverage(Convert.ToInt16(vehtype)).FirstOrDefault();
                if (results != null)
                {
                    var offset = LatLongConverter.OSRefToWGS84(results.OffsetX, results.OffsetY);
                    var blocksize = LatLongConverter.OSRefToWGS84(results.OffsetX + results.Blocksize, results.OffsetY + results.Blocksize);
                    var heatmap = new Heatmap();

                    heatmap.lon = offset.Longitude;
                    heatmap.lat = offset.Latitude;

                    //if (results.Data != null)
                    //heatmap.map = Convert.ToBase64String(results.Data, Base64FormattingOptions.None); //.ToList().Select(x=> (byte)x).ToArray();
                    heatmap.map = results.Data;

                    heatmap.cols = results.Columns;
                    heatmap.rows = results.Rows;
                    heatmap.lonBlocksize = blocksize.Longitude - offset.Longitude;
                    heatmap.latBlocksize = blocksize.Latitude - offset.Latitude;
                    heatmap.vehtype = vehtype;

                    result.Map = heatmap;
                }

                Logger.Write("Routing Manager: GetCoverageHandler returning GetCoverageResponse", TraceEventType.Information, "Routing Manager");
                return result;

            }
        }


        
        [HttpGet]
        public ActionResult GetCCGCoverage()
        {
            Newtonsoft.Json.JsonSerializer ser = new JsonSerializer();

            try
            {
                using (QuestEntities db = new QuestEntities())
                {
                    var results = (from c in db.GetCCGLatestCoverageStats() select c);                    

                    GTFeatureCollectionP ResourcesFC = new GTFeatureCollectionP();

                    List<GTFeatureP> features = new List<GTFeatureP>();
                   
                    foreach (var res in results)
                    {
                        
                        double[][][] polygon=GetPolygon(res.geom);

                        GTFeatureP resFeature = new GTFeatureP
                        {
                            Geometry=new CCGCoords
                            {
                              Coords=polygon,
                              Type="Polygon"
                            },
                            Type="Feature",
                            Properties = new CCGFeatureProperties
                            {
                                Name=res.desc.ToString(),
                                Amb=res.amb.ToString(),
                                Fru=res.fru.ToString(),
                                Inc=res.inc.ToString(),
                                Holes=res.hol.ToString()
                            }
                        };

                        features.Add(resFeature);                     

                    }
                    ResourcesFC.Features = features.ToArray();
                    var js = JsonConvert.SerializeObject(ResourcesFC);
                  
                    var result = new ContentResult
                    {
                        Content = js,
                        ContentType = "application/json"
                    };

                    return result;

                }
            }
            catch (Exception ex)
            {
                var errorMsg = "ERROR: Cannot complete CCG Coverage" + ex.Message;
                var error = new ContentResult
                {
                    Content = errorMsg,
                    ContentType = "application/json"
                };

                return error;
            }
        }

        
        [HttpGet]
        public ActionResult GetCCGData()
        {
            try
            {
                using(QuestEntities db=new QuestEntities())
                {
                    var results = (from c in db.GetCCGLatestCoverageStats() select new {c.desc, c.amb,c.fru,c.hol}).ToList();
                    var serializer = new JavaScriptSerializer();

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


        
        [HttpGet]
        public ActionResult GetHeldCallsTest()
        {
            string startdate="2014-05-17 04:00";
            int numhours =2;
            int daysback=7;
            return GetHeldCalls( startdate, numhours, daysback);
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
                        .OrderBy( x=>x.t )
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
            catch( Exception ex)
            {
                var result = new ContentResult
                {
                    Content = serializer.Serialize(new { errormsg = ex.Message, stack = ex.StackTrace }),
                    ContentType = "application/json"
                };
            }
            return null;
        }



        
        public ActionResult HeldcallsSummary()
        {
            if (User != null)
                if (User.Identity != null)
                    Logger.Write(String.Format("Access: {0} by {1}", Request.RawUrl, User.Identity.Name));

            ViewBag.TestFlag = Settings.Default.TestFlag;
            ViewBag.TestMessage = Settings.Default.TestMessage;

            return View();
        }


        
        public ActionResult HeldcallsHistory()
        {
            if (User != null)
                if (User.Identity != null)
                    Logger.Write(String.Format("Access: {0} by {1}", Request.RawUrl, User.Identity.Name));
            ViewBag.TestFlag = Settings.Default.TestFlag;
            ViewBag.TestMessage = Settings.Default.TestMessage;

            return View();
        }

        
        public ActionResult CCGSummary()
        {
            return View();
        }
#endif

    }

    public class p
    {
        public double x;
        public double y;
    }
}
