using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class ProfileParameterType
    {
        public ProfileParameterType()
        {
            ProfileParameter = new HashSet<ProfileParameter>();
        }

        public int ProfileParameterTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<ProfileParameter> ProfileParameter { get; set; }
    }
}
