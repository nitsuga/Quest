namespace Quest.Lib.DataModel
{
    public partial class CoverageMapDefinition
    {
        public int CoverageMapDefinitionId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string VehicleCodes { get; set; }
        public int? MinuteLimit { get; set; }
        public bool IsEnabled { get; set; }
        public string RoutingResource { get; set; }
    }
}
