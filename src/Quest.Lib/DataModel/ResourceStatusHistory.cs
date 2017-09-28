namespace Quest.Lib.DataModel
{
    public partial class ResourceStatusHistory
    {
        public int ResourceStatusHistoryId { get; set; }
        public int? ResourceId { get; set; }
        public int? ResourceStatusId { get; set; }
        public long? Revision { get; set; }

        public Resource Resource { get; set; }
        public ResourceStatus ResourceStatus { get; set; }
    }
}
