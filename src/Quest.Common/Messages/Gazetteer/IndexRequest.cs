using System;

namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class IndexRequest : Request
    {
        public string Index { get; set; }
        public string Indexers { get; set; }
        public string IndexMode { get; set; }
        public int Shards { get; set; }
        public int Replicas { get; set; }
        public bool UseMaster { get; set; }
    }


}
