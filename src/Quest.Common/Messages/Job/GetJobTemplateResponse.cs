using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Job
{
    [Serializable]
    public class GetJobTemplateResponse : Response
    {
        public List<JobTemplate> Items;
    }
}
