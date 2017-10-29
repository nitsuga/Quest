using System;

namespace Quest.Common.Messages.Job
{
    [Serializable]
    public class CancelJobRequest : Request
    {
        public int Jobid;
    }
}
