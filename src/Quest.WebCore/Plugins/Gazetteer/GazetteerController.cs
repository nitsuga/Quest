using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quest.Common.Messages.Gazetteer;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Resource;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quest.WebCore.Plugins.Gazetteer
{
    public class GazetteerController : Controller
    {
        private AsyncMessageCache _messageCache;
        private SearchService _searchService;
        private readonly IPluginService _pluginService;
        GazetteerPlugin _plugin;

        public GazetteerController(
                GazetteerPlugin plugin,
                AsyncMessageCache messageCache,
                IPluginService pluginFactory,
                SearchService searchService
            )
        {
            _plugin = plugin;
            _searchService = searchService;
        }

        [HttpGet]
        public GazSettings GetSettings()
        {
            return _plugin.GetSettings();
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
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
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
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

        /// <summary>
        /// Get selected map items to display
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
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

        [HttpPost]
        public ActionResult AssignResource(ResourceAssign request)
        {
            return null;
        }
    }
}