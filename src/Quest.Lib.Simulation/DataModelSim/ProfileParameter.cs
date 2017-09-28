namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class ProfileParameter
    {
        public int ProfileParameterId { get; set; }
        public int ProfileId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int? ProfileParameterTypeId { get; set; }

        public Profile Profile { get; set; }
        public ProfileParameterType ProfileParameterType { get; set; }
    }
}
