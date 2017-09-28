using System;

namespace Quest.Lib.OS.DataModelOS
{
    public partial class Paf
    {
        public long Id { get; set; }
        public short RecordIdentifier { get; set; }
        public string ChangeType { get; set; }
        public int ProOrder { get; set; }
        public long Uprn { get; set; }
        public long? ParentAddressableUprn { get; set; }
        public int RmUprn { get; set; }
        public string OrganisationName { get; set; }
        public string DepartmentName { get; set; }
        public string SubBuildingName { get; set; }
        public string BuildingName { get; set; }
        public short? BuildingNumber { get; set; }
        public string DependentThoroughfare { get; set; }
        public string Thoroughfare { get; set; }
        public string DoubleDependentLocality { get; set; }
        public string DependentLocality { get; set; }
        public string PostTown { get; set; }
        public string Postcode { get; set; }
        public string PostcodeType { get; set; }
        public string WelshDependentThoroughfare { get; set; }
        public string WelshThoroughfare { get; set; }
        public string WelshDoubleDependentLocality { get; set; }
        public string WelshDependentLocality { get; set; }
        public string WelshPostTown { get; set; }
        public string PoBoxNumber { get; set; }
        public DateTime ProcessDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime EntryDate { get; set; }
        public double XCoordinate { get; set; }
        public double YCoordinate { get; set; }
        public string Fulladdress { get; set; }
        public int Usrn { get; set; }
    }
}
