using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quest.Common.Messages.Gazetteer;
using Quest.Common.Messages.Gazetteer.Gazetteer;
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
        private GazetteerPlugin _plugin;

        public GazetteerController(
                GazetteerPlugin plugin,
                AsyncMessageCache messageCache,
                IPluginService pluginFactory
            )
        {
            _messageCache = messageCache;
            _plugin = plugin;
        }

        [HttpGet]
        public GazSettings GetSettings()
        {
            return _plugin.GetSettings();
        }

        [HttpGet]
        public async Task<SearchResponse> InfoSearch(InfoSearchRequest request)
        {
            var result = await _messageCache.SendAndWaitAsync<SearchResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        [HttpGet]
        public async Task<IndexGroupResponse> GetIndexGroups()
        {
            IndexGroupRequest request = new IndexGroupRequest();
            var result = await _messageCache.SendAndWaitAsync<IndexGroupResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult> StartIndexing(string Index, string Indexers, string IndexMode, int Shards, int Replicas, bool UseMaster)
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

            var results = await _messageCache.SendAndWaitAsync<IndexResponse>(request, new TimeSpan(0, 0, 10));
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
        public async Task<ActionResult> SemanticSearch(SemanticSearchRequest request)
        {
            try
            {
                BBFilter bb = null;
                if (request.BoundsFilter)
                    bb = new BBFilter() {
                        br_lat = request.S,
                        br_lon = request.E,
                        tl_lat = request.N,
                        tl_lon = request.W
                    };

                var filters = new List<TermFilter>();

                if (!string.IsNullOrEmpty(request.FilterTerms))
                {
                    filters = (List<TermFilter>)JsonConvert.DeserializeObject(request.FilterTerms, typeof(List<TermFilter>));
                }

                SearchResultDisplayGroup displayGroupEnum = SearchResultDisplayGroup.description;
                if (request.DisplayGroup != null)
                    Enum.TryParse<SearchResultDisplayGroup>(request.DisplayGroup, out displayGroupEnum);

                var gazrequest = new SearchRequest()
                {
                    includeAggregates = request.IncludeAggregates,
                    searchMode = (SearchMode)request.SearchMode,
                    take = request.Take,
                    skip = request.Skip,
                    searchText = request.SearchText,
                    box = bb,
                    filters = filters,
                    displayGroup = displayGroupEnum,
                    username = User.Identity.Name,
                    indexGroup = request.IndexGroup
                };

                var searchResult = await _messageCache.SendAndWaitAsync<SearchResponse>(gazrequest, new TimeSpan(0, 0, 10));

                var data = GetDisplaySearchResult(searchResult);

                if (data != null)
                {
                    var js = JsonConvert.SerializeObject(data);

                    var msg = string.Format("Search: '{0}' mode={1} bound {6} {2}/{3} {4}/{5} filters='{7}' found {8} in {9}ms", request.SearchText, request.SearchMode, request.W, request.N, request.E, request.S, request.BoundsFilter ? "on" : "off", request.FilterTerms, searchResult.Count, searchResult.millisecs);
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
   }
}