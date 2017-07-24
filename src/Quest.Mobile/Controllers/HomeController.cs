#pragma warning disable 0169,649
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.IO;
#if NET45
using System.Data.Spatial;
#endif
using Quest.Mobile.Attributes;
using Quest.Common.Messages;
using System.Diagnostics;
using System.Web.Script.Serialization;
#if OAUTH
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
#endif
using Quest.Lib.Utils;
using Quest.Mobile.Models;
using Quest.Mobile.Service;
using Quest.Lib.Trace;
using Quest.Lib.ServiceBus;
using System.Web;

namespace Quest.Mobile.Controllers
{
#if OAUTH
    [Authorize]
#endif
    public class HomeController : Controller
    {
        private MessageCache _messageCache;
        private ResourceService _resourceService;
        private IncidentService _incidentService;
        private DestinationService _destinationService;
        private SearchService _searchService;
        private RouteService _routeService;
        private TelephonyService _telephonyService;
        private SecurityService _securityService;

        public HomeController(MessageCache messageCache,
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
        }


        HomeViewModel DefaultHomeViewModel()
        {
            var user = User.Identity.Name;
            var fullname = user;
#if OAUTH
            var context = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var userrec = userManager.FindByName(User.Identity.Name);
            fullname = userrec.Fullname;
#endif
            var claims = _securityService.GetAppClaims(user);

            var groupResult = _searchService.GetIndexGroups();

            HomeViewModel model = new HomeViewModel
            {
                User = fullname,
                Claims =claims,
                IndexGroups = groupResult?.Groups,
            };
            return model;
        }

        [HttpPost]
        // use IEnumerable<HttpPostedFileBase> files if you want to post multiple files
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/UploadFile/"), fileName);
                file.SaveAs(path);
            }
            return RedirectToAction("Index");
        }

#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
        public ActionResult Index()
        {
            // not initialised properly
            if (_searchService == null)
                return null;

            if (User?.Identity != null)
                Logger.Write($"Access: {Request.RawUrl} by {User.Identity.Name}", GetType().Name);


            HomeViewModel model = DefaultHomeViewModel();

            return View(model);

        }

#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
        public ActionResult TestLocations()
        {
            if (User?.Identity != null)
                Logger.Write($"Access: {Request.RawUrl} by {User.Identity.Name}", GetType().Name);

            TestLocationsModel model =new TestLocationsModel();

            return View(model);
        }

        [Authorize(Roles = "administrator,user")]
        public ActionResult TestRoutes()
        {
            if (User?.Identity != null)
                Logger.Write($"Access: {Request.RawUrl} by {User.Identity.Name}", GetType().Name);

            HomeViewModel model = DefaultHomeViewModel();

            return View(model);

        }


#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
        [HttpGet]
        [NoCache]
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

#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
        [HttpGet]
        [NoCache]
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

#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
        [HttpGet]
        [NoCache]
        public ActionResult Route(string from, string to, string roadSpeedCalculator, string vehicle, int hour)
        {
            try
            {
                RoutingResponse route = _routeService.Route(from, to, roadSpeedCalculator, vehicle, hour, User.Identity.Name);

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

#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
        [HttpGet]
        [NoCache]
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

#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
        [HttpGet]
        [NoCache]
        public ActionResult BatchRoutes(string routes, string roadSpeedCalculator)
        {
            var job = _routeService.BatchRoutes(routes, roadSpeedCalculator);

            var jsonp = job;

            var js = JsonConvert.SerializeObject(jsonp);
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };

            return result;
        }

#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
        [HttpGet]
        [NoCache]
        public ActionResult GetRoutesJob(int jobid)
        {
            var job = _routeService.GetJob(jobid);
            var jsonp = job;

            var js = JsonConvert.SerializeObject(jsonp);
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };

            return result;
        }

//#if OAUTH
//        [Authorize(Roles = "administrator,user")]
//#endif
//        [HttpGet]
//        [NoCache]
//        public ActionResult SemanticSearch(string searchText,int searchMode,bool includeAggregates,int skip,int take,double w, double s,double e,double n,bool boundsfilter,string filterterms,string displayGroup,string indexGroup)
//        {
//            try
//            {
//                BBFilter bb = null;
//                if (boundsfilter)
//                    bb = new BBFilter() { br_lat = s, br_lon = e, tl_lat = n, tl_lon = w };

//                var filters = new List<TermFilter>();

//                if (!string.IsNullOrEmpty(filterterms))
//                {
//                    filters = (List<TermFilter>)JsonConvert.DeserializeObject(filterterms, typeof(List<TermFilter>));
//                }

//                SearchResultDisplayGroup displayGroupEnum = SearchResultDisplayGroup.description;
//                if (displayGroup != null)
//                    Enum.TryParse<SearchResultDisplayGroup>(displayGroup, out displayGroupEnum);

//                var request = new Common.Messages.SearchRequest()
//                {
//                    includeAggregates = includeAggregates,
//                    searchMode = (SearchMode)searchMode,
//                    take = take,
//                    skip = skip,
//                    searchText = searchText,
//                    box = bb,
//                    filters = filters,
//                    displayGroup = displayGroupEnum,
//                    username = User.Identity.Name,
//                    indexGroup = indexGroup
//                };

//                var searchResult = _searchService.SemanticSearch(request);

//                var data = GetDisplaySearchResult(searchResult);

//                if (data != null)
//                {
//                    var js = JsonConvert.SerializeObject(data);

//                    var msg = string.Format("Search: '{0}' mode={1} bound {6} {2}/{3} {4}/{5} filters='{7}' found {8} in {9}ms", searchText, searchMode, w, n, e, s, boundsfilter ? "on" : "off", filterterms, searchResult.Count, searchResult.millisecs);
//                    Logger.Write(msg, TraceEventType.Information, "Search");
//                    var result = new ContentResult
//                    {
//                        Content = js,
//                        ContentType = "application/json"
//                    };

//                    return result;
//                }
//                return null;
//            }
//            catch (Exception ex)
//            {
//                var js = JsonConvert.SerializeObject(new { error = ex.Message });
//                var result = new ContentResult
//                {
//                    Content = js,
//                    ContentType = "application/json"
//                };
//                return result;
//            }
//        }


#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
        [HttpGet]
        public ActionResult AssignDevice(string callsign, string eventId, Boolean nearby)
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            var ser = new JsonSerializer();
            try
            {
                var request = new AssignDeviceRequest() { Callsign = callsign, EventId = eventId, Nearby = nearby };
                MvcApplication.MsgClientCache.BroadcastMessage(request);

                ser.Serialize(writer, "request was sent");
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

#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
        [HttpGet]
        public ActionResult CancelDevice(string callsign, string eventId)
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            var ser = new JsonSerializer();
            try
            {
                var request = new CancelDeviceRequest() { Callsign = callsign, EventId = eventId };
                MvcApplication.MsgClientCache.BroadcastMessage(request);

                ser.Serialize(writer, "request was sent");
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
        [NoCache]
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

#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
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
            catch(Exception ex)
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

#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
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

#if OAUTH
        [Authorize(Roles = "administrator,user")]
#endif
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
                if (map==null)
                    map = new GetCoverageResponse() { Message = "no vehicle coverage available", Success = false };

                var serializer = new JavaScriptSerializer()
                {
                    MaxJsonLength = Int32.MaxValue
                };
                var result = new ContentResult
                {
                    Content = serializer.Serialize(map),
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


#if false
        [Authorize(Roles = "administrator,user")]
        [HttpGet]
        [NoCache]
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
        [Authorize(Roles = "administrator,user")]
        [HttpGet]
        [NoCache]
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

#endif

    }


}
