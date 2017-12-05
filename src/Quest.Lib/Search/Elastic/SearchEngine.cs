using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using Quest.Lib.Utils;
using Quest.Common.Messages.Gazetteer;
using Nest;
using Quest.Lib.Coords;

namespace Quest.Lib.Search.Elastic
{
    public class SearchEngine : ISearchEngine
    {
        #region private variables

        private ElasticClient _client = null;

        
        private ElasticSettings _settings;

        private readonly string[] _aggs = new[] { "type", "thoroughfare", "locality", "org", "ward" };

        private enum Category
        {
            History,
            Junction,
            Markerpost,
            Signpost,
            Telephone,
            Mwayjunc,
            Phonebox,
            Underground,
            Coordinate,
            Station,
            Mobile,
            Nearby,
            Towards,
            Area
        }

        private class Sequence
        {
            public Regex Expression;
            public Category Id;
        }

        // test using this.. http://regexstorm.net/tester 
        // not .net but v good:  https://regex101.com/#pcre
        private readonly List<Sequence> _sequences = new List<Sequence>
        {
            // *MOB*,99,20160620024833,Data Available,99,530822,181068,1500,1000,45
            new Sequence{Id = Category.Mobile,Expression = new Regex(@"^\*MOB\*([^,]*),(?<conf>[0-9]{1,})([^,]*),(?<date>[0-9]{1,})([^,]*),([^,]*),([^,]*),(?<east>[0-9]{1,})([^,]*),(?<north>[0-9]{1,})([^,]*),(?<major>[0-9]{1,})([^,]*),(?<minor>[0-9]{1,})([^,]*),(?<angle>[0-9]{1,})", RegexOptions.IgnoreCase | RegexOptions.Singleline)},
            new Sequence{Id = Category.Coordinate,Expression =new Regex(@"^(((?'filter'[^\@]*)\@\s*){0,1}((?'A'[0-9]*\.{0,1}\d*)(?'sep'\s*,\s*){1}(?'B'-?\d*\.{0,1}\d*)){1}\s*(?'range'\d*M){0,1})$", RegexOptions.IgnoreCase | RegexOptions.Singleline)},
            new Sequence{Id = Category.Nearby,Expression = new Regex(@"(((?'left'[^\@]*)@(?'range'\d{1,4})?(?'right'[^\@]*))(\@(?'common'[^\@]*))?)", RegexOptions.IgnoreCase | RegexOptions.Singleline)},
            new Sequence{Id = Category.Towards,Expression = new Regex(@"(?'left'[^\@]*) towards (?'right'[^\@]*)@{0,1}(?'common'[^\@]*)", RegexOptions.IgnoreCase | RegexOptions.Singleline)},
            new Sequence{Id = Category.Towards,Expression = new Regex(@"(?'left'[^\@]*) -> (?'right'[^\@]*)@{0,1}(?'common'[^\@]*)", RegexOptions.IgnoreCase | RegexOptions.Singleline)},
            new Sequence{Id = Category.Nearby,Expression = new Regex(@"(?'left'[^\@]*) nr (?'right'[^\@]*)@{0,1}(?'common'[^\@]*)", RegexOptions.IgnoreCase | RegexOptions.Singleline)},
            new Sequence {Id = Category.Area, Expression = new Regex(@"area(?'text'.*)", RegexOptions.IgnoreCase | RegexOptions.Singleline)},
            new Sequence {Id = Category.History, Expression = new Regex(@"!hx(?'text'.*)", RegexOptions.IgnoreCase | RegexOptions.Singleline)},
            new Sequence {Id = Category.Junction, Expression = new Regex(@"(.*)\/(.*)", RegexOptions.IgnoreCase | RegexOptions.Singleline)}
        };

        readonly Regex _flatPattern = new Regex(@"(.*FLAT\s[0-9]*),(\s[0-9].*)");

        private class Prefix
        {
            internal string[] Names;

            internal string TypeFilter;
        }

        private readonly Prefix[] _types =
        {
            new Prefix
            {
                Names = new[] {"address", "add", "property"},
                TypeFilter = "Address"
            },
            new Prefix
            {
                Names = new[] {"Junction", "jct"},
                TypeFilter = "Junction"
            },
            new Prefix
            {
                Names = new[] {"POI"},
                TypeFilter = "POI"
            },
            new Prefix
            {
                Names = new[] {"RoadLink", "Road"},
                TypeFilter = "RoadLink"
            },
            new Prefix
            {
                Names = new[] {"local"},
                TypeFilter = "LocalName"
            },
            new Prefix
            {
                Names = new[] {"osm"},
                TypeFilter = "OSM"
            },
            new Prefix
            {
                Names = new[] {"bus stop"},
                TypeFilter = "BUS"
            }
        };

        #endregion

        #region public method

        public SearchEngine(ElasticSettings settings)
        {
            _settings = settings;
        }

        public void Initialise()
        {
            if (_client != null)
                return;

            _client = ElasticClientFactory.CreateClient(_settings);
            IndexBuilder.CreateHistoryIndex(_settings, _client, false);
            GetIndexGroups();
        }

        public void Analyse(string text, string analyser, string index)
        {
            Initialise();
            var result = _client.Analyze(x => x.Analyzer(analyser).Index(index).Text(text));
            foreach( var t in result.Tokens)
                Debug.Print($"{t.Token}    {t.Type}" );
        }


        public SearchResponse SemanticSearch(Common.Messages.Gazetteer.SearchRequest baserequest)
        {
            Initialise();

            bool tophit =false;
            baserequest.searchTime = DateTime.Now;

            SearchResponse results;

            var watch = new Stopwatch();
            watch.Start();

            //added for unit testing
            if (baserequest.filters == null)
                baserequest.filters = new List<TermFilter>();

            baserequest.searchText = baserequest.searchText.Trim().ToUpper();
            baserequest.searchText = StripIgnored(baserequest.searchText);
            
            // use top hit
            if (baserequest.searchText.Contains("!"))
            {
                tophit = true;
            }

            // use phonetic search if ~ found
            if (baserequest.searchText.Contains("~~"))
            {
                baserequest.searchText = baserequest.searchText.Replace("~", "");
                baserequest.searchMode = SearchMode.FUZZY;
            }

            if (baserequest.searchText.Contains("~"))
            {
                baserequest.searchText = baserequest.searchText.Replace("~", "");
                baserequest.searchMode = SearchMode.RELAXED;
            }

            // extract type search - the search is preceeded by the type
            foreach (var t in _types)
            {
                foreach (var p in t.Names)
                {
                    if (baserequest.searchText.StartsWith(p + " ", StringComparison.CurrentCultureIgnoreCase))
                    {
                        baserequest.searchText = baserequest.searchText.Substring(p.Length + 1);
                        baserequest.filters.Add(new TermFilter {field = "type", include = true, value = t.TypeFilter});
                    }
                }
            }

            //remove comma between flat number and house number to stop it being treated as coordinate
            baserequest.searchText = _flatPattern.Replace(baserequest.searchText, "$1$2");

            foreach (var s in _sequences)
            {
                var mc = s.Expression.Matches(baserequest.searchText);
                if (mc.Count > 0)
                {
                    var rangeValue = 150;
                    switch (s.Id)
                    {
                        case Category.History:
                            var text = mc[0].Groups["text"].Value;
                            results = History();
                            results.millisecs = watch.ElapsedMilliseconds;
                            return results;

                        case Category.Mobile:
                            var easting = mc[0].Groups["east"].Value;
                            var northing = mc[0].Groups["north"].Value;
                            var major = mc[0].Groups["major"].Value;
                            var minor = mc[0].Groups["minor"].Value;
                            var angle = mc[0].Groups["angle"].Value;
                            results = Eisec(easting, northing, major, minor, angle);
                            goto finish;

                        case Category.Coordinate:
                            var atext = mc[0].Groups["A"].Value;
                            var btext = mc[0].Groups["B"].Value;
                            var filter = mc[0].Groups["filter"].Value;
                            var range = mc[0].Groups["range"].Value;

                            if (atext == "" || btext == "")
                                continue;

                            if (range == "") range = "50m";
                            double a;
                            double b;

                            double.TryParse(atext, out a);
                            double.TryParse(btext, out b);
                            var point = a > 90
                                ? ConvertToLatLon(a, b)
                                : new PointGeoShape {Coordinates = new GeoCoordinate(a, b)};

                            results = CoordinateSearch(baserequest, point, filter, range);
                            goto finish;

                        case Category.Junction:
                            // enforce a junction-only search
                            baserequest.filters.Add(new TermFilter {field = "type", include = true, value = "Junction"});
                            results = BasicSearch(baserequest);
                            goto finish;

                        case Category.Nearby:
                            var left = mc[0].Groups["left"].Value;
                            var right = mc[0].Groups["right"].Value;
                            var common = mc[0].Groups["common"].Value;
                            var rangestr = mc[0].Groups["range"].Value;

                            int.TryParse(rangestr, out rangeValue);

                            if (left == "" || right == "")
                                break;

                            results = Nearby(left, right, common, baserequest.skip, baserequest.take, rangeValue, baserequest.indexGroup);
                            goto finish;

                        case Category.Towards:
                            left = mc[0].Groups["left"].Value;
                            right = mc[0].Groups["right"].Value;
                            common = mc[0].Groups["common"].Value;
                            rangestr = mc[0].Groups["range"].Value;

                            int.TryParse(rangestr, out rangeValue);

                            if (left == "" || right == "")
                                break;

                            results = Towards(left, right, common, baserequest.skip, baserequest.take, rangeValue);
                            goto finish;

                        case Category.Area:
                            var area = mc[0].Groups["text"].Value;
                            results = Area(area, baserequest.skip, baserequest.take);
                            goto finish;
                    }
                }
            }

            // regular search
            baserequest.searchText = baserequest.searchText.Replace(",", "");
            results = BasicSearch(baserequest);

            finish:

            if (tophit && results.Documents.Count>1)
            {
                results.Documents.RemoveRange(1, results.Documents.Count - 1);
            }

            // RemovePoorScore(results);
            DemoteBusStops(results, baserequest);

            // perform grouping on the results
            if (baserequest.displayGroup != SearchResultDisplayGroup.none)
                UpdateAggregations(results, baserequest);

            watch.Stop();

            results.millisecs = watch.ElapsedMilliseconds;

            AddAudit(baserequest, results);

            return results;
        }

        public SearchResponse InfoSearch(InfoSearchRequest request)
        {
            Initialise();

            //TODO: Search OverlayDocuments as well

            // perform the search
            Common.Messages.Gazetteer.SearchRequest baserequest = new Common.Messages.Gazetteer.SearchRequest()
            {
                skip = 0,
                take=999,
                searchText = "",
                infopoint = new GeoCoordinate(request.lat, request.lng)
            };

            var searchresults = BasicSearch(baserequest);


            // zip with the score and order by the score and then the description

            return searchresults;
        }

        public IndexGroupResponse GetIndexGroups()
        {
            IndexGroupResponse result = new IndexGroupResponse()
            {
                Groups = new List<IndexGroup>()
            };

            Initialise();

            PolygonManager mgr = new PolygonManager();

            mgr.BuildFromShapefile(_settings.IndexGroups);
            var all = mgr.PolygonIndex.QueryAll();

            List<IndexGroup> groups = new List<IndexGroup>();
            all.ToList().ForEach(x=>
            {
                var g = new IndexGroup();

                g.IndexGroupId = 0;
                if (x.data.Length > 4)
                {
                    g.Name = x.data[1];
                    g.Indices = x.data[2];
                    g.isEnabled = x.data[3] == "1";
                    g.isDefault = x.data[4] == "1";
                    DateTime.TryParse(x.data[5] ?? "1/1/2000", out g.ValidFrom);
                    DateTime.TryParse(x.data[6] ?? "1/1/2099", out g.ValidTo);
                    g.useGeometry = x.data[7] == "1";
                    var poly = GeomUtils.MakePolygon(x.geom.Envelope);
                    g.Polygon = poly;
                    g.PolygonWkt = x.geom.AsText();
                }

                if (g.isEnabled && g.ValidFrom <= DateTime.Today && g.ValidTo >= DateTime.Today)
                    groups.Add(g);
            });

            result.Groups = groups;

            return result;
        }

        #endregion

        #region private methods

        public ISearchResponse<LocationDocument> SearchTest(string searchText)
        {
            var queryContainer = Query<LocationDocument>
                .MatchPhrase(p => p.Field(t => t.indextext.Suffix("strict"))
                    .Query(searchText)
                    .Slop(15)
                    //.MinimumShouldMatch("75%")
                    .Operator(Operator.And)
                    .Analyzer("search_strict_analyser"));

            var descriptor = new SearchDescriptor<LocationDocument>();

            // only get the results asked for..
            var d = descriptor.Explain().Query(q => q.Bool(y => y.Must(queryContainer)));
                
            var searchresults = _client.Search<LocationDocument>(d);

            return searchresults;

        }

        private SearchResponse BasicSearch(Common.Messages.Gazetteer.SearchRequest request)
        {
            // flag if spatial limits have been applied
            bool spatialLimited = false;

            if (_client == null)
                return null;

            // set the min score here for now, move into the search request.
            // the minscore gets overriden if we doing a coordinate search 
            double minscore = 0;

            Nest.QueryContainer queryContainer = null;

            switch (request.searchMode)
            {
                // regular text search
                case SearchMode.EXACT:

                    if (request.searchText == "*")
                        request.searchText = "";

                    queryContainer = Query<LocationDocument>
                        .MatchPhrase(p => p.Field(t => t.indextext.Suffix("strict"))
                            .Query(request.searchText)
                            .Slop(15)
                            //.MinimumShouldMatch("75%")
                            .Operator(Operator.And)
                            .Analyzer("search_strict_analyser")
                        );
                    break;

                case SearchMode.RELAXED:

                    queryContainer = Query<LocationDocument>
                        .Match(p => p.Field(t => t.indextext.Suffix("address"))
                            .Slop(15)
                            .MinimumShouldMatch("75%")
                            .Fuzziness(Fuzziness.Auto)
                            .Analyzer("search_strict_analyser")
                            .Query(request.searchText)
                        );
                    break;

                case SearchMode.FUZZY:

                    queryContainer = Query<LocationDocument>
                        .MatchPhrase(p => p.Field(t => t.indextext.Suffix("address"))
                            .Slop(15)
                            .MinimumShouldMatch("75%")
                            //.Fuzziness(Fuzziness.Auto)
                            .Analyzer("search_adress_analyser")
                            .Query(request.searchText)
                        );
                    break;
            }


            var filterlist = new QueryContainer();

            if (request.filters != null && request.filters.Count > 0)
            {
                foreach (var filter in request.filters)
                    if (filter.include)
                        filterlist |= Query<LocationDocument>.Term(filter.field, filter.value);
                    else
                        filterlist &= !Query<LocationDocument>.Term(filter.field, filter.value);
                queryContainer &= filterlist;
            }


            var result = new SearchResponse();
            var descriptor = new SearchDescriptor<LocationDocument>();

            // add distance filter if passed
            if (request.distance != null)
            {
                // relax minscore as if the user put no search terms in then it finds everything at a score of 1 (i.e.under the threshold of 2.5)
                minscore = 1;

                descriptor
                    .PostFilter(p => p
                        .GeoDistance(f => f
                            .Field(field => field.Location)
                            .Distance(request.distance.distance.ToLower())
                            .Location(request.distance.lat, request.distance.lng)
                        ));

                spatialLimited = true;
            }

            if (request.box != null) // add BB filter if passed
            {
                descriptor.PostFilter(p => p
                    .GeoBoundingBox(c => c
                        .Field(field => field.Location)
                        .BoundingBox(request.box.tl_lat, request.box.tl_lon, request.box.br_lat, request.box.br_lon)));

                spatialLimited = true;
            }

            if (request.polygon != null) // add BB filter if passed
            {
                descriptor.PostFilter(p => p
                    .GeoShapePolygon(c => c
                        .Field(field => field.Point)
                        .Coordinates(request.polygon.Coordinates)));

                spatialLimited = true;
            }

            if (request.infopoint != null) // add infopoint filter if passed
            {
                descriptor.PostFilter(p => p
                    .GeoShapePoint(c => c.Relation(GeoShapeRelation.Intersects)
                        .Field(field => field.InfoGeofence)
                        .Coordinates( request.infopoint)));

                spatialLimited = true;
            }

            // only get the results asked for..
            descriptor.From(request.skip)
                .MinScore(minscore)
                .Size(request.take)
                //.Explain()
                .Query(q => q.Bool(y => y.Must(queryContainer)));
                

            // add any required aggregates
            if (request.includeAggregates)
            {
                var ag = new AggregationContainerDescriptor<LocationDocument>();
                foreach (var agg in _aggs)
                    ag = ag.Terms(agg, st => st.Field(agg).Size(30));
                descriptor.Aggregations(x => ag);
            }

            // set the indices to use and limit to polygon area if no other
            //spatial limits have been set
            var indices = GetIndexGroups();
            var record = indices.Groups.FirstOrDefault(x => x.Name == request.indexGroup) ??
                         indices.Groups.FirstOrDefault(x => x.isDefault);
            if (record != null)
            {
                // use the indicies specified
                descriptor.Index(record.Indices);

                // use bounding box specified
                if (!spatialLimited && record.useGeometry)
                    descriptor.PostFilter(p => p
                        .GeoShapePolygon(c => c
                        .Field(field => field.Point)
                        .Coordinates(record.Polygon.Coordinates)));
            }
            else
                // use default index
                descriptor.Index("locations");


            // perform the search
            var searchresults = DoSearch(descriptor);

            result.Count = searchresults.Total;

            // extract any required aggregate results
            if (request.includeAggregates)
            {
                result.Aggregates = new List<Aggregate>();
                foreach (var agg in _aggs)
                {
                    var myAgg = searchresults.Aggs.Terms(agg);
                    if (myAgg != null)
                    {
                        result.Aggregates.Add(
                            new Aggregate
                            {
                                Name = agg,
                                Items = myAgg.Buckets.Select(y => new AggregateItem
                                {
                                    Name = y.Key,
                                    Value = y.DocCount
                                }).ToList()
                            });
                    }
                }
            }

            // zip with the score and order by the score and then the description
            result.Documents = searchresults
                .Documents
                .Zip(searchresults.Hits, (a, b) => new { a, b })
                .OrderByDescending(x => x.b.Score)
                .ThenBy(x => x.a.Description.Replace(",", " "))
                .Select(x => new SearchHit { l = x.a, s = x.b.Score??0 })
                .ToList();

            Deduplicate(result);

            return result;
        }

        
        private Nest.ISearchResponse<LocationDocument> DoSearch(Nest.SearchDescriptor<LocationDocument> descriptor)
        {
            var searchresults = _client.Search<LocationDocument>(descriptor);
            return searchresults;
        }

        private void AddAudit(Common.Messages.Gazetteer.SearchRequest baserequest, SearchResponse results)
        {
            try
            {
                //log the search in history index
                _client.Index(baserequest, i => i.Index(ElasticSettings.Historyindex));

                var poly = GeomUtils.Polygonize(results, 10);
                var audit = new AuditDocument
                {
                    description = baserequest.searchText,
                    poly = poly.nest_polygon,
                    timestamp = DateTime.UtcNow,
                    type = "Search",
                    user = baserequest.username,
                    duration = results.millisecs
                };

                _client.Index(audit, i => i.Index(ElasticSettings.Historyindex));

            }
            catch 
            {
                throw;
            }
        }

        private SearchResponse History()
        {
            var result = new SearchResponse { Aggregates = null, Count = 1, Grouping = null, millisecs = 0, Removed = 0 };

            // perform the search
            var searchresults = _client.Search<AuditDocument>(
                q => q.Index(ElasticSettings.Historyindex)
                    .MatchAll()
                    .Sort(p => p.Descending(x => x.timestamp))
                    .Take(300)
                );

            result.Count = searchresults.Total;

            // zip with the score and order by the score and then the description
            result.Documents = searchresults
                .Documents
                .Select(x => new SearchHit
                {
                    l = new LocationDocument
                    {
                        Description = x.description,
                        Source = x.timestamp.ToShortTimeString(),
                        Type = x.user,
                        Location = new GeoLocation(-1, -1),
                        Poly = x.poly
                    },
                    s = x.timestamp.Ticks
                })
                .ToList();

            return result;
        }

        private SearchResponse CoordinateSearch(Common.Messages.Gazetteer.SearchRequest baserequest, PointGeoShape point, string filter,
            string range)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            baserequest.searchText = filter;
            baserequest.distance = new DistanceFilter
            {
                distance = range,
                lat = point.Coordinates.Latitude,
                lng = point.Coordinates.Longitude
            };
            var results = BasicSearch(baserequest);

            if (baserequest.searchText.Length == 0)
            {
                foreach (var refdoc in results.Documents)
                    refdoc.s = (int) Distance(point.Coordinates, refdoc.l.Location);
                if (results.Documents.Count > 0)
                {
                    var max = results.Documents.Max(x => x.s);

                    foreach (var refdoc in results.Documents)
                        refdoc.s = 10 - (10*refdoc.s/max);
                }
            }

            var doc = new LocationDocument
            {
                Description = $"{point.Coordinates.Latitude}, {point.Coordinates.Longitude}",
                Source = "Point",
                Type = "Point",
                Location = new GeoLocation(point.Coordinates.Latitude, point.Coordinates.Longitude)
            };
            results.Documents.Insert(0, new SearchHit { l = doc, s = 10 });

            return results;
        }

        private SearchResponse Eisec(string easting, string northing, string major, string minor, string angle)
        {
            double vEasting, vNorthing, vMajor, vMinor, vAngle;
            double.TryParse(easting, out vEasting);
            double.TryParse(northing, out vNorthing);
            double.TryParse(major, out vMajor);
            double.TryParse(minor, out vMinor);
            double.TryParse(angle, out vAngle);

            var result = new SearchResponse { Aggregates = null, Count = 1, Grouping = null, millisecs = 0, Removed = 0 };

            var factory = new GeometricShapeFactory();

            vAngle = vAngle * -1;

            factory.Centre = new Coordinate(vEasting, vNorthing);
            factory.Height = vMajor;
            factory.Width = vMinor;
            factory.Rotation = vAngle / 180 * Math.PI;

            var ellipse = factory.CeateEllipse();

            //convert to latlong
            var coords = new List<Coordinate>();
            foreach (var p in ellipse.Coordinates)
            {
                var coord = LatLongConverter.OSRefToWGS84(p.X, p.Y);
                var en = new Coordinate { X = coord.Longitude, Y = coord.Latitude };
                coords.Add(en);
            }

            var fact = new GeometryFactory();
            var poly = fact.CreatePolygon(coords.ToArray());

            var nestPoly = GeomUtils.MakeEllipseBng(vAngle,vEasting,vNorthing,vMajor,vMinor);

            var center = GeomUtils.ConvertToLatLonLoc(vEasting, vNorthing);

            var doc = new LocationDocument
            {
                Description = "Mobile",
                Source = "EISEC",
                Type = "Mobile",
                Poly = nestPoly,
                Location = center
            };

            var hit = new SearchHit { s = 10, l = doc };
            result.Documents = new List<SearchHit> { hit };
            result.Bounds = nestPoly;
            return result;
        }

        private PointGeoShape ConvertToLatLon(double easting, double northing)
        {
            var coord = LatLongConverter.OSRefToWGS84(easting, northing);
            return new PointGeoShape { Coordinates = new GeoCoordinate(coord.Latitude, coord.Longitude) };
        }
       
        private void UpdateAggregations(SearchResponse searchResult, Common.Messages.Gazetteer.SearchRequest searchRequest)
        {
            AssignDocumentGroupKeys(searchResult, searchRequest);

            var seq=0;
            searchResult.Grouping = searchResult.Documents.Select(x => new {Document = x, Index = seq++})
                .GroupBy(x => x.Document.l.GroupingIdentity)
                .Select(x => new { Group = x.Key, Score = x.Max(y => y.Document.s), Items = x.Select(a=>a.Index).ToList() })
                .OrderByDescending(x => x.Score)
                .Select(x => x.Items)
                .ToList();

            ////Now create the aggreatedResults, use dictionary for easier sorting
            //var groups = new Dictionary<string, List<int>>();
            //var groupscores = new Dictionary<string, double>();


            //// documents are already order by score descending
            //for (var index = 0; index < searchResult.Documents.Count; index++)
            //{
            //    var doc = searchResult.Documents[index];

            //    List<int> targetlist;
            //    // find or create a container for this document                
            //    groups.TryGetValue(doc.l.GroupingIdentity, out targetlist);
            //    if (targetlist != null)
            //    {
            //        targetlist.Add(index);
            //    }
            //    else
            //    {
            //        targetlist = new List<int> {index};
            //        groups.Add(doc.l.GroupingIdentity, targetlist);
            //        groupscores.Add(doc.l.GroupingIdentity, doc.s);
            //    }
            //}

            //// add groups into the results
            //searchResult.Grouping =
            //    groupscores.OrderByDescending(x => x.Value).ThenBy(x => x.Key).Select(x => groups[x.Key]).ToList();
        }

        private static void AssignDocumentGroupKeys(SearchResponse searchResult, Common.Messages.Gazetteer.SearchRequest searchRequest)
        {
            Action<LocationDocument> grouper;

            //Assign grouping method
            switch (searchRequest.displayGroup)
            {
                case SearchResultDisplayGroup.description:
                    grouper = AddressDocumentDescriptionGrouper;
                    break;
                case SearchResultDisplayGroup.thoroughfare:
                    grouper = AddressDocumentThoroughfareGrouper;
                    break;
                case SearchResultDisplayGroup.type:
                    grouper = AddressDocumentTypeGrouper;
                    break;
                default:
                    grouper = NullGrouper;
                    break;
            }

            // execute grouper on the results
            searchResult.Documents.AsParallel().ForAll(doc => grouper(doc.l));
        }

        private static void NullGrouper(LocationDocument doc)
        {
            doc.GroupingIdentity = "";
        }

        private static void AddressDocumentDescriptionGrouper(LocationDocument doc)
        {
            doc.GroupingIdentity = doc.Description.ToUpper();
        }

        /// <summary>
        ///     assign thoroughfare-based grouping for an addresssdocument
        /// </summary>
        /// <param name="doc"></param>
        private static void AddressDocumentThoroughfareGrouper(LocationDocument doc)
        {
            if (doc.Thoroughfare != null && doc.Thoroughfare.Count > 0)
            {
                doc.GroupingIdentity = doc.Thoroughfare.First().Trim();

                if (doc.Locality != null && doc.Locality.Count > 0)
                    doc.GroupingIdentity += (doc.GroupingIdentity.Length > 0 ? ", " : "") + doc.Locality[0].Trim();
                else if (doc.Areas != null && doc.Areas.Length > 0)
                    doc.GroupingIdentity += (doc.GroupingIdentity.Length > 0 ? ", " : "") + doc.Areas[0].Trim();
            }
            else
            {
                doc.GroupingIdentity = doc.Description.Trim();
            }

            doc.GroupingIdentity = doc.GroupingIdentity.ToUpper();
        }

        private static void AddressDocumentTypeGrouper(LocationDocument doc)
        {
            if (doc.Type != null )
            {
                doc.GroupingIdentity = doc.Type;
            }
            else
            {
                doc.GroupingIdentity = "Unknown";
            }

            doc.GroupingIdentity = doc.GroupingIdentity.ToUpper();
        }


        private void DemoteBusStops(SearchResponse searchResult, Common.Messages.Gazetteer.SearchRequest searchRequest)
        {
            // documents are already order by score descending
            foreach (var doc in searchResult.Documents)
            {
                if (doc.l.Type == "BUS" && !searchRequest.searchText.Contains("BUS STOP"))
                    doc.s = doc.s/2;
            }
        }

        private void Deduplicate(SearchResponse results)
        {
            // remove duplicates
            results.Removed = 0;
            var final = new List<SearchHit>();
            var uprn = new HashSet<long>();
            foreach (var r in results.Documents)
            {
                var a = r.l;
                // find objects without UPRN.. these are ok to put in the final results
                if (a.UPRN == 0)
                    final.Add(r);
                else
                {
                    if (!uprn.Contains(a.UPRN))
                    {
                        final.Add(r);
                        uprn.Add(a.UPRN);
                    }
                    else
                        results.Removed++;
                }
            }

            results.Documents = final;
        }

        private string StripIgnored(string searchText)
        {
            var ignoreStartsWith = new[] { "OS ", @"O/S ", "OP ", "NB ", "SB ", "EB ", "WB ", "BTW " };
            foreach (var p in ignoreStartsWith)
            {
                if (searchText.StartsWith(p, StringComparison.CurrentCultureIgnoreCase))
                {
                    return searchText.Substring(p.Length);
                }
            }

            var ignoreContains = new[] { @" O/S ", @"*EXT*", "1PCO", "2PCO" };
            foreach (var p in ignoreContains)
                searchText = searchText.Replace(p, "");

            return searchText;
        }

        private SearchResponse Area(string text, int skip, int take)
        {
            var range = 150;

            var baserequest = new Common.Messages.Gazetteer.SearchRequest
            {
                searchMode = 0,
                searchText = text.ToUpper(),
                skip = skip,
                take = take,
                includeAggregates = false,
                filters = new List<TermFilter>()
            };

            baserequest.take = 200;
            baserequest.searchText = text;
            var r2 = BasicSearch(baserequest);

            // create a polygon around the base set of results so we can search inside it
            var poly = GeomUtils.Polygonize(r2, range);

            // add the actual buffer
            var buffer = new LocationDocument
            {
                Description = "Area: " + text,
                Location = poly.centroid,
                Poly = poly.nest_polygon
            };
            var hit = new SearchHit {l = buffer, s = 10};
            r2.Documents.Add(hit);

            return r2;
        }

        private SearchResponse Nearby(string left, string right, string common, int skip, int take, double range, string indexGroup)
        {
            if (range <= 0)
                range = 150;

            if (left.Length > 0 && right.Length > 0)
            {
                var baserequest = new Common.Messages.Gazetteer.SearchRequest
                { 
                    indexGroup = indexGroup,
                    searchMode = SearchMode.EXACT,
                    searchText = left.ToUpper(),
                    skip = skip,
                    take = take,
                    includeAggregates = false,
                    filters = new List<TermFilter>()
                };

                baserequest.take = 200;
                baserequest.searchText = right + common;
                //var r2 = BasicSearch(baserequest);
                SearchResponse r2 = SemanticSearch(baserequest);


                // create a polygon around the base set of results so we can search inside it
                var poly = GeomUtils.Polygonize(r2, range);

                baserequest.take = take;
                baserequest.searchText = left + common;
                baserequest.polygon = poly.nest_polygon;

                // search that polygon for the results
                var r1 = BasicSearch(baserequest);

                var refdocs = r2.Documents;
                var revised = new List<SearchHit>();
#if false
                revised = r1.Documents;
#else
                foreach (var doc in r1.Documents)
                {
                    var keep = false;
                    // check the doc is near at least one ref doc
                    foreach (var refdoc in refdocs)
                    {
                        var d = (int) Distance(doc.l.Location, refdoc.l.Location);
                        if (d < range || refdoc.l.Poly!=null)
                        {
                            keep = true;
                            if (d < 0)
                                doc.l.Description = $"{doc.l.Description} near {refdoc.l.Description}";
                            else
                                doc.l.Description = $"{doc.l.Description}"; // {d}m-> {refdoc.l.Description}";
                            break;
                        }

                    }

                    if (keep)
                        revised.Add(doc);
                }
#endif

                r1.Documents = revised;
                r1.Count = revised.Count;
                r1.Bounds = poly.nest_polygon;

#if false
    // add the actual buffer
                var buffer = new AddressDocument
                {
                    Description = "Area: " + right,
                    Location = poly.centroid,
                    Poly = poly.nest_polygon
                };
                SearchHit hit = new SearchHit { l = buffer, s = 10 };
                r1.Documents.Add(hit);
#endif

                return r1;
            }

            return null;
        }

        private SearchResponse Towards(string left, string right, string common, int skip, int take, double range)
        {
            if (range <= 0)
                range = 300;

            var results = new SearchResponse();

            if (left.Length > 0 && right.Length > 0)
            {
                var baserequest = new Common.Messages.Gazetteer.SearchRequest
                {
                    searchMode = SearchMode.EXACT,
                    searchText = left.ToUpper(),
                    skip = skip,
                    take = take,
                    includeAggregates = false,
                    filters = new List<TermFilter>()
                };

                baserequest.take = 200;
                baserequest.searchText = right + common;
                baserequest.filters = new List<TermFilter>();
                baserequest.filters.Add(new TermFilter {field = "type", include = true, value = "Junction"});
                baserequest.filters.Add(new TermFilter {field = "type", include = true, value = "RoadLink"});
                baserequest.filters.Add(new TermFilter {field = "type", include = true, value = "LocalName"});

                var r2 = BasicSearch(baserequest);

                if (r2.Count == 0)
                {
                    results.Documents = new List<SearchHit>();
                    return results;
                }
                // create a polygon around the base set of results so we can search inside it
                var poly = GeomUtils.Polygonize(r2, range);

                baserequest.take = take;
                baserequest.searchText = left + common;
                baserequest.polygon = poly.nest_polygon;
                baserequest.filters = new List<TermFilter>();

                // search that polygon for the results
                var r1 = BasicSearch(baserequest);

                var refdocs = r2.Documents;
                var revised = new List<SearchHit>();

                foreach (var doc in r1.Documents)
                {
                    // check the doc is near at least one ref doc
                    foreach (var refdoc in refdocs)
                    {
                        var d = (int) Distance(doc.l.Location, refdoc.l.Location);
                        if (d < range || refdoc.l.Poly!=null)
                        {
                            if (d < 0)
                                doc.l.Description = $"{doc.l.Description} near {refdoc.l.Description}";
                            else
                                doc.l.Description = $"{doc.l.Description} towards {refdoc.l.Description}";
                            break;
                        }
                    }

                }

                r1.Documents = revised;
                r1.Count = revised.Count;

                // add the actual buffer
                var buffer = new LocationDocument
                {
                    Description = "Area: " + right,
                    Location = poly.centroid,
                    Poly = poly.nest_polygon
                };
                var hit = new SearchHit {l = buffer, s = 10};
                r1.Documents.Add(hit);

                return r1;
            }

            return null;
        }

        private double Distance(GeoLocation p1, GeoLocation p2)
        {
            if (p1 == null)
                return double.MaxValue;

            if (p2 == null)
                return double.MaxValue;

            return
                Math.Acos(Math.Sin(DegToRadians(p1.Latitude))*Math.Sin(DegToRadians(p2.Latitude)) +
                          Math.Cos(DegToRadians(p1.Latitude))*Math.Cos(DegToRadians(p2.Latitude))*
                          Math.Cos(DegToRadians(p2.Longitude - p1.Longitude)))*6371000.0;
        }

        private double DegToRadians(double v)
        {
            return 2*Math.PI*v/360.0;
        }

        #endregion
    }
}