using System;

namespace Quest.Common.Messages.Job
{
    [Serializable]
    public class DeleteJobRequest : Request
    {
        public int Jobid;
    }
}
