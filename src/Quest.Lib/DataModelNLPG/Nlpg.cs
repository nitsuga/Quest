using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModelNLPG
{
    public partial class Nlpg
    {
        public long Id { get; set; }
        public long? Uprn { get; set; }
        public string ClassificationCode { get; set; }
        public string SaoText { get; set; }
        public short? SaoStartNumber { get; set; }
        public string SaoStartSuffix { get; set; }
        public short? SaoEndNumber { get; set; }
        public string SaoEndSuffix { get; set; }
        public string PaoText { get; set; }
        public short? PaoStartNumber { get; set; }
        public string PaoStartSuffix { get; set; }
        public short? PaoEndNumber { get; set; }
        public string PaoEndSuffix { get; set; }
        public int? Usrn { get; set; }
        public short? LogicalStatus { get; set; }
        public string StreetDescription { get; set; }
        public string TownName { get; set; }
        public string LocalityName { get; set; }
        public string PostcodeLocator { get; set; }
        public double? NlpgXCoordinate { get; set; }
        public double? NlpgYCoordinate { get; set; }
        public string GeoSingleAddressLabel { get; set; }
    }
}
