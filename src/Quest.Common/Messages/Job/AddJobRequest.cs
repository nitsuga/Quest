using System;

namespace Quest.Common.Messages.Job
{
    [Serializable]
    public class AddJobRequest : Request
    {
        public string TaskName;
        public string Key;
        public DateTime? StartTime;
        public string Parameters;
        public bool Start;
        public string Description;
        public string NotifyAddresses;
        public int NotifyLevel;
        public string NotifyReplyTo;
        public string Classname;
    }
}
