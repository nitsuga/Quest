using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModelNLPG
{
    public partial class Blpu
    {
        public Blpu()
        {
            Classification = new HashSet<Classification>();
            Lpi = new HashSet<Lpi>();
            Organisation = new HashSet<Organisation>();
            Successor = new HashSet<Successor>();
            Xref = new HashSet<Xref>();
        }

        public short RecordIdentifier { get; set; }
        public string ChangeType { get; set; }
        public int ProOrder { get; set; }
        public long Uprn { get; set; }
        public short LogicalStatus { get; set; }
        public short? BlpuState { get; set; }
        public DateTime? BlpuStateDate { get; set; }
        public long? ParentUprn { get; set; }
        public double XCoordinate { get; set; }
        public double YCoordinate { get; set; }
        public int Rpc { get; set; }
        public short LocalCustodianCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime EntryDate { get; set; }
        public string PostalAddress { get; set; }
        public string PostcodeLocator { get; set; }
        public int MultiOccCount { get; set; }

        public Dpa Dpa { get; set; }
        public ICollection<Classification> Classification { get; set; }
        public ICollection<Lpi> Lpi { get; set; }
        public ICollection<Organisation> Organisation { get; set; }
        public ICollection<Successor> Successor { get; set; }
        public ICollection<Xref> Xref { get; set; }
    }
}
