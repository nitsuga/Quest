using System;

namespace Quest.Common.Messages.Job
{

    [Serializable]
    public class GetJobLogRequest : Request
    {
        public int Jobid;
    }
}
