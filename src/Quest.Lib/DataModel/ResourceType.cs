using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class ResourceType
    {
        public ResourceType()
        {
            Resource = new HashSet<Resource>();
        }

        public int ResourceTypeId { get; set; }
        public string ResourceType1 { get; set; }
        public string ResourceTypeGroup { get; set; }

        public ICollection<Resource> Resource { get; set; }
    }
}
