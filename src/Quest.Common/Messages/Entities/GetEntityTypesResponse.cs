using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Entities
{
    /// <summary>
    ///     a list of entities
    /// </summary>
    [Serializable]
    public class GetEntityTypesResponse : Response
    {
        public List<string> Items { get; set; }
    }
}