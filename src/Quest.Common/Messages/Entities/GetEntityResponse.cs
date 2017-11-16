using System;

namespace Quest.Common.Messages.Entities
{
    [Serializable]
    public class GetEntityResponse : Response
    {
        public EntityData Item { get; set; }
    }


}