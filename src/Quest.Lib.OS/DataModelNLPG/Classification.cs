using System;
using System.Collections.Generic;

namespace Quest.Lib.OS.DataModelNLPG
{
    public partial class Classification
    {
        public short RecordIdentifier { get; set; }
        public string ChangeType { get; set; }
        public int ProOrder { get; set; }
        public long Uprn { get; set; }
        public string ClassKey { get; set; }
        public string ClassificationCode { get; set; }
        public string ClassScheme { get; set; }
        public float SchemeVersion { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime EntryDate { get; set; }

        public Blpu UprnNavigation { get; set; }
    }
}
