using System;

namespace Quest.Common.Messages.Job
{
    [Serializable]
    public class JobLog
    {
        public int JobLogId { get; set; }
        public int JobInfoId { get; set; }
        public string Message { get; set; }
        public int Level { get; set; }
        public string AdditionalInfo { get; set; }
        public int? Priority { get; set; }
        public DateTime? LogTime { get; set; }
    }
}
