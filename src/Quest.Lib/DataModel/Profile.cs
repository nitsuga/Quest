using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class Profile
    {
        public Profile()
        {
            ProfileParameter = new HashSet<ProfileParameter>();
        }

        public int ProfileId { get; set; }
        public string ProfileName { get; set; }

        public ICollection<ProfileParameter> ProfileParameter { get; set; }
    }
}
