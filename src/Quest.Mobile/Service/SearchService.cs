#pragma warning disable 0169,649
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Quest.Mobile.Models;
using Quest.Common.Messages;

namespace Quest.Mobile.Service
{
    public class SearchService
    {
        private int _jobid;
        private readonly Dictionary<int, LocationSearchJob> _jobs;

        public SearchService()
        {
            _jobs = new Dictionary<int, LocationSearchJob>();
        }

        public SearchResponse SimpleSearch(string location, string userName)
        {
            // get coords of start and end
            var fromRequest = new SearchRequest()
            {
                includeAggregates = false,
                searchMode = SearchMode.RELAXED,
                take = 1,
                skip = 0,
                searchText = location,
                box = null,
                filters = null,
                displayGroup = SearchResultDisplayGroup.none,
                username = userName,
                indexGroup = null,
            };

            var f = SemanticSearch(fromRequest);
            return f;
        }

        public SearchResponse SemanticSearch(SearchRequest request)
        {
            var result = MvcApplication.MsgClientCache.SendAndWait<SearchResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public IndexResponse Index(IndexRequest request)
        {
            var result = MvcApplication.MsgClientCache.SendAndWait<IndexResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public SearchResponse InfoSearch(InfoSearchRequest request)
        {
            var result = MvcApplication.MsgClientCache.SendAndWait<SearchResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public IndexGroupResponse GetIndexGroups()
        {
            IndexGroupRequest request = new IndexGroupRequest();
            var result = MvcApplication.MsgClientCache.SendAndWait<IndexGroupResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        public LocationSearchJob GetJob(int jobid)
        {
            if (_jobs.ContainsKey(jobid))
                return _jobs[jobid];
            else
                return null;
        }

        public void DeleteJob(int jobid)
        {
            if (_jobs.ContainsKey(jobid))
                _jobs.Remove(jobid);
        }

        public void CancelJob(int jobid)
        {
            if (_jobs.ContainsKey(jobid))
                _jobs[jobid].cancelflag = true;
        }

#if false
        /// <summary>
        /// process a batch of searches
        /// </summary>
        /// <param name="searchText">a CR separated list of search requests</param>
        /// <returns>a job id</returns>
        public LocationSearchJob BatchSearch(string searchText)
        {
            // create new job record and add to dictionary
            var newjob = new LocationSearchJob()
            {
                jobid = ++_jobid,
                cancelflag = false,
                request = searchText.Split('\n')
                .Select(t => new SingleSearch() { searchText = t, bestmatch = null, complete=false, status="pending" })
                .ToList()
            };

            _jobs.Add(newjob.jobid, newjob);

            new TaskFactory().StartNew(() => 
            {
                foreach (var item in newjob.request)
                {
                    try
                    {
                        Debug.Print(item.searchText);

                        if (newjob.cancelflag)
                            break;

                        item.searchText = item.searchText.Trim();

                        if (item.searchText.StartsWith(@"//") || item.searchText.Length==0 )
                        {
                            item.searchText = item.searchText.Replace(@"//", "");
                            item.status = "";
                            item.header = true;
                            item.complete = true;
                        }
                        else
                        {
                            var watch = new Stopwatch();
                            watch.Start();
                            var search = new GazSearchRequest { searchText = item.searchText, skip = 0, take = 1, searchMode = SearchMode.EXACT, includeAggregates = false };
                            var result = _searchEngine.SemanticSearch(search);
                            watch.Stop();

                            if (result != null && result.Documents.Count > 0)
                                item.bestmatch = result.Documents[0].l;

                            item.status = watch.ElapsedMilliseconds + "ms";
                            item.complete = true;
                            item.count = result.Count;
                            item.score = result.Documents.Count>0?result.Documents[0].s:0;
                        }
                        Debug.Print(item.status);
                    }
                    catch (Exception ex)
                    {
                        item.status = ex.Message;
                        item.complete = true;
                    }
                }
                newjob.complete = true;
                Debug.Print("job complete");
            });

            return newjob;
        }
#endif
    }
}