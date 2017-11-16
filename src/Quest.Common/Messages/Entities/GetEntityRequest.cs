using System;

namespace Quest.Common.Messages.Entities
{
    /// <summary>
    /// get (latest) named entity
    /// </summary>
    [Serializable]
    public class GetEntityRequest : Request
    {
        public string Name { get; set; }
    }


}