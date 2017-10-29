using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Job
{
    [Serializable]
    public class GetJobLogResponse : Response
    {
        public JobView Job;
        public List<JobLog> Items;
    }
}
