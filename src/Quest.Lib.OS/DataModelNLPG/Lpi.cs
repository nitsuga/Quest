using System;
using System.Collections.Generic;

namespace Quest.Lib.OS.DataModelNLPG
{
    public partial class Lpi
    {
        public short RecordIdentifier { get; set; }
        public string ChangeType { get; set; }
        public int ProOrder { get; set; }
        public long Uprn { get; set; }
        public string LpiKey { get; set; }
        public string Language { get; set; }
        public short LogicalStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime EntryDate { get; set; }
        public short? SaoStartNumber { get; set; }
        public string SaoStartSuffix { get; set; }
        public short? SaoEndNumber { get; set; }
        public string SaoEndSuffix { get; set; }
        public string SaoText { get; set; }
        public short? PaoStartNumber { get; set; }
        public string PaoStartSuffix { get; set; }
        public short? PaoEndNumber { get; set; }
        public string PaoEndSuffix { get; set; }
        public string PaoText { get; set; }
        public int Usrn { get; set; }
        public string UsrnMatchIndicator { get; set; }
        public string AreaName { get; set; }
        public string Level { get; set; }
        public string OfficialFlag { get; set; }

        public Blpu UprnNavigation { get; set; }
        public Street UsrnNavigation { get; set; }
    }
}
