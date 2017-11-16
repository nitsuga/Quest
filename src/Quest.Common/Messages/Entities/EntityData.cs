using System;

namespace Quest.Common.Messages.Entities
{
    [Serializable]
    public class EntityData
    {
        public int Revision { get; set; }
        public string Entity { get; set; }
        public dynamic Data { get; set; }
    }


}