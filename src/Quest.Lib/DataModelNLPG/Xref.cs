using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModelNLPG
{
    public partial class Xref
    {
        public short RecordIdentifier { get; set; }
        public string ChangeType { get; set; }
        public int ProOrder { get; set; }
        public long Uprn { get; set; }
        public string XrefKey { get; set; }
        public string CrossReference { get; set; }
        public int? Version { get; set; }
        public string Source { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime EntryDate { get; set; }

        public Blpu UprnNavigation { get; set; }
    }
}
