using System;

namespace Quest.Common.Messages.Job
{
    [Serializable]
    public class GetJobsRequest: Request
    {
        public int Skip;
        public int Take;
    }
}
