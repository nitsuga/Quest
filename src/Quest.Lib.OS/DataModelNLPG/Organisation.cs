﻿using System;

namespace Quest.Lib.OS.DataModelNLPG
{
    public partial class Organisation
    {
        public short RecordIdentifier { get; set; }
        public string ChangeType { get; set; }
        public int ProOrder { get; set; }
        public long Uprn { get; set; }
        public string OrgKey { get; set; }
        public string Organisation1 { get; set; }
        public string LegalName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime EntryDate { get; set; }

        public Blpu UprnNavigation { get; set; }
    }
}
