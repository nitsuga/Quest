using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModelNLPG
{
    public partial class Street
    {
        public Street()
        {
            Lpi = new HashSet<Lpi>();
        }

        public short RecordIdentifier { get; set; }
        public string ChangeType { get; set; }
        public int ProOrder { get; set; }
        public int Usrn { get; set; }
        public short RecordType { get; set; }
        public short SwaOrgRefNaming { get; set; }
        public short? State { get; set; }
        public DateTime? StateDate { get; set; }
        public short? StreetSurface { get; set; }
        public short? StreetClassification { get; set; }
        public short Version { get; set; }
        public DateTime StreetStartDate { get; set; }
        public DateTime? StreetEndDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime RecordEntryDate { get; set; }
        public double StreetStartX { get; set; }
        public double StreetStartY { get; set; }
        public double StreetStartLat { get; set; }
        public double StreetStartLong { get; set; }
        public double StreetEndX { get; set; }
        public double StreetEndY { get; set; }
        public double StreetEndLat { get; set; }
        public double StreetEndLong { get; set; }
        public double StreetTolerance { get; set; }

        public ICollection<Lpi> Lpi { get; set; }
    }
}
