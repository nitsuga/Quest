using System;
using System.Collections.Generic;
using System.Text;

namespace Quest.Common.Messages
{
    public class IncidentDensity
    {
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> CellX { get; set; }
        public Nullable<int> CellY { get; set; }
    }

}
