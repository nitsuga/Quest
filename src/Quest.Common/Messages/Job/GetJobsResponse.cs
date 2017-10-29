using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Job
{
    [Serializable]
    public class GetJobsResponse : Response
    {
        public List<JobView> Items;
        public int Total;
    }
}
