using Quest.Lib.Search.Elastic;

namespace Quest.Lib.Search.Indexers
{
    internal class OsmIndexer : ElasticIndexer
    {
#if false
        private Dictionary<int, string> _tags;
        private int[] _keeptags;
#endif

        public override void StartIndexing(BuildIndexSettings config)
        {
#if false
            Build(config);
#endif
        }

#if false
        private void Build(BuildIndexSettings config)
        {
            var database = "";

            DeleteDataSet<LocationDocument>(config.DefaultIndex, config.Client, IndexBuilder.AddressDocumentType.Osm);
            using (var context = new OsmEntities())
            {
                _tags = context.tTagTypes.ToDictionary(x => x.Typ, y => y.Name);
                var nameTag = _tags.Where(x => x.Value == "name").FirstOrDefault().Key;
                string[] keeplist ={"name", "description", "shop", "amenity", "addr:housenumber", "addr:street", "addr:postcode", "network" };
                _keeptags = keeplist.Select(x => _tags.FirstOrDefault(y => y.Value == x).Key).ToArray();

                // get nodeids to process
                var nodesForTask = context.tNodeTags.AsNoTracking().Where(y => y.Typ == nameTag)
                    .Select(x=>x.NodeId)
                    .Distinct()
                    .ToList();

                var totalNodes = nodesForTask.Count;
                config.RecordsTotal = totalNodes;

                // how man records for each?
                var numWorkers = 4;
                var chunkSize = totalNodes/(numWorkers+1);

                Logger.Write($"{this.GetType().Name}: Nodes to index:{totalNodes} workers: {numWorkers} chunks: {chunkSize}", GetType().Name);

                var workers = new List<Task>();

                int i = 0;
                while (nodesForTask.Count > 0)
                {
                    i++;
                    var list = nodesForTask.Take(chunkSize).ToList();
                    nodesForTask.RemoveRange(0, list.Count);
                    var task = Task.Factory.StartNew(() => OsmIndexNodeDocuments(i, list, config, database));
                    workers.Add(task);
                }

                Task.WaitAll(workers.ToArray());
            }
        }

        private void OsmIndexNodeDocuments(int workerNumber, List<long> ids, BuildIndexSettings config, string database)
        {
            Logger.Write($"{this.GetType().Name}: worker:{workerNumber} started", GetType().Name);

            var i = 0;
            var total = ids.Count;
            long id = 0;
            var descriptor = GetBulkRequest(config);

            try
            {
                do
                {
                    var sublist = ids.Take(500).ToList();
                    ids = ids.Skip(sublist.Count).ToList();

                    using (var context = new OsmEntities())
                    {
                        context.Configuration.ProxyCreationEnabled = false;

                        var nodesForTask = context.tNodes
                            .AsNoTracking()
                            .Where(x => sublist.Contains(x.Id))
                            .Select(x => new { x.Longitude, x.Latitude, x.Id })
                            .OrderBy(n => n.Id)
                            .ToList();

                        var nodesTags = context.tNodeTags
                            .Where(x => sublist.Contains(x.NodeId)).ToList();

                        foreach (var node in nodesForTask)
                        {
                            config.RecordsCurrent++;
                            i++;
                            id = node.Id;

                            // check whether point is in master area if required
                            if (!IsPointInRange(config, node.Longitude, node.Latitude))
                            {
                                config.Skipped++;
                                continue;
                            }

                            var address = OsmGetDocument(node.Id,node.Latitude,node.Longitude, nodesTags, config);

                            // add item to the list of documents to index
                            AddIndexItem(address, descriptor);

                            // commit any messages and report progress
                            CommitCheck(this, config, descriptor);
                        }
                    }

                } while (ids.Count > 0);

                CommitBultRequest(config, descriptor);

                Logger.Write($"{this.GetType().Name}: worker:{workerNumber} Complete: {i} records ", GetType().Name);

            }
            catch (Exception ex)
            {
                Logger.Write($"{this.GetType().Name}: worker:{workerNumber} failed after {i} records on node id {id} with error: {ex} ", GetType().Name);
            }

        }

        private string BuildIndexDescription(long Id, double Latitude, double Longitude, List<tNodeTag> nodesTags, string[] terms)
        {
            List<string> additional = new List<string>();

            var tags = nodesTags.Where(x => x.NodeId == Id && _keeptags.Contains(x.Typ));

            foreach (var tag in tags)
                additional.Add(tag.Info.Replace("_", " ").ToUpper().Trim());

            if (terms != null)
                additional.AddRange(terms);

            return string.Join(", ", additional);
        }

        private LocationDocument OsmGetDocument(long Id, double Latitude, double Longitude, List<tNodeTag> nodeTags, BuildIndexSettings config)
        {
            var point = new GeoLocation(Latitude, Longitude);

            var terms = GetLocalAreas(point, config.LocalAreaNames);
            var description = BuildIndexDescription(Id,Latitude,Longitude, nodeTags, terms);

            if (description.Length == 0)
                return null;

            return new LocationDocument
            {
                Created = DateTime.Now,
                ID = IndexBuilder.AddressDocumentType.Osm + " " + Id,
                Type = IndexBuilder.AddressDocumentType.Osm,
                Source = IndexBuilder.AddressDocumentType.Osm,
                Location = point,
                Point = PointfromGeoLocation(point),
                indextext = description.ToUpper().Replace(",","").Decompound(config.DecompoundList),
                Description = description.ToUpper()
            };
        }
#endif

    }
}