using System;

namespace Quest.Common.Messages.Job
{
    [Serializable]
    public class JobView
    {
        public int JobInfoId { get; set; }
        public string Taskname { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
        public string Parameters { get; set; }
        public DateTime? Scheduled { get; set; }
        public int? JobStatusId { get; set; }
        public string Message { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Stopped { get; set; }
        public bool? Success { get; set; }
        public DateTime? Created { get; set; }
        public string NotifyAddresses { get; set; }
        public int? NotifyLevel { get; set; }
        public string NotifyReplyTo { get; set; }
        public string JobStatus { get; set; }
    }
}
