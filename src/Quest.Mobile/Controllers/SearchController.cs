using Newtonsoft.Json;
using Quest.Common.Messages;
using Quest.Mobile.Models;
using Quest.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;

namespace Quest.Mobile.Controllers
{
    public class SearchController : ApiController
    {
        private SearchService _searchService;

        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        [ActionName("IndexGroups")]
        [HttpGet]
        public IndexGroupResponse IndexGroups()
        {
            return _searchService.GetIndexGroups();
        }

        [ActionName("InformationSearch")]
        [HttpGet]
        public DisplaySearchResult InformationSearch(double lng, double lat)
        {
            var request = new InfoSearchRequest() { lat = lat, lng = lng };
            Debug.WriteLine($"Searching at lat: {lat}, long: {lng}");
            var searchResult = _searchService.InfoSearch(request);
            return GetDisplaySearchResult(searchResult);
        }

        //[Authorize(Roles = "administrator,user")]
        [ActionName("Find")]
        [HttpGet]
        public DisplaySearchResult Find(string searchText = "", int searchMode=0, bool includeAggregates=false, int skip=0, int take=20, double w=0, double s=0, double e=0, double n=0, bool boundsfilter=false, string filterterms=null, string displayGroup=null, string indexGroup=null)
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

            var request = new Common.Messages.SearchRequest()
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

            var searchResult = _searchService.SemanticSearch(request);

            return GetDisplaySearchResult(searchResult);
        }
        
        private DisplaySearchResult GetDisplaySearchResult(SearchResponse searchResult)
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

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}