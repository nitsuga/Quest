using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class Ampdscodes
    {
        public short AmpdsKey { get; set; }
        public string Ampdscode { get; set; }
        public string Chiefcomplaint { get; set; }
        public byte? Chiefcomplaintcode { get; set; }
        public string Description { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Enddate { get; set; }
        public byte? Version { get; set; }
        public string Category { get; set; }
        public string DefaultLascategory { get; set; }
        public string DefaultLascategoryFull { get; set; }
        public string DefaultDohCategory { get; set; }
        public string DefaultDohSubcategory { get; set; }
        public string DefaultDohSubcategoryFull { get; set; }
    }
}
