using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class Determinants
    {
        public int DeterminantId { get; set; }
        public string Determinant { get; set; }
        public string Ambulances { get; set; }
        public string Paramedics { get; set; }
        public string AllResponders { get; set; }
        public string Ecps { get; set; }
        public string Comresp { get; set; }
    }
}
