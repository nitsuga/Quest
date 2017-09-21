using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class Callsign
    {
        public Callsign()
        {
            Resource = new HashSet<Resource>();
        }

        public int CallsignId { get; set; }
        public string Callsign1 { get; set; }

        public ICollection<Resource> Resource { get; set; }
    }
}
