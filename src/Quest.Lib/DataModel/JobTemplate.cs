namespace Quest.Lib.DataModel
{
    public partial class JobTemplate
    {
        public int JobTemplateId { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public int? Order { get; set; }
        public string Task { get; set; }
        public string Description { get; set; }
        public string Parameters { get; set; }
        public string Key { get; set; }
        public string NotifyAddresses { get; set; }
        public int? NotifyLevel { get; set; }
        public string NotifyReplyTo { get; set; }
        public string Classname { get; set; }
    }
}
