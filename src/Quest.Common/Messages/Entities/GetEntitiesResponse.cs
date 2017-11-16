using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Entities
{
    [Serializable]
    public class GetEntitiesResponse : Response
    {
        public List<EntityData> Items { get; set; }
    }
}