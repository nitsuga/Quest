using System;

namespace Quest.Common.Messages
{
    [Serializable]
    public class IndexResponse : Response
    {
        public string Index { get; set; }
        public string Indexers { get; set; }
        public string IndexMode { get; set; }
        public string FromIndex { get; set; }
        public int Shards { get; set; }
        public int Replicas { get; set; }
        public bool UseMaster { get; set; }
    }


}
