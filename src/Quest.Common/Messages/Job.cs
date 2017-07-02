using System;
using System.Collections.Generic;

namespace Quest.Common.Messages
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


    [Serializable]
    public class JobTemplate
    {
        public int JobTemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Parameters { get; set; }
        public string Key { get; set; }
        public string NotifyAddresses { get; set; }
        public int? NotifyLevel { get; set; }
        public string NotifyReplyTo { get; set; }
        public string Classname { get; set; }
        public string Task { get; set; }
        public string Group { get; set; }
        public int? Order { get; set; }
    }

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

    [Serializable]
    public class AddJobResponse : Response
    {
        public int Jobid;
    }

    [Serializable]
    public class AddJobFromTemplateRequest : Request
    {
        /// <summary>
        /// use the id of the template
        /// </summary>
        public int JobTemplateId;

        /// <summary>
        /// use the name of the template
        /// </summary>
        public string JobTemplateName;
    }

    [Serializable]
    public class AddJobFromTemplateResponse : Response
    {
        public int Jobid;
    }


    [Serializable]
    public class GetJobTemplateRequest : Request
    {
    }

    [Serializable]
    public class GetJobTemplateResponse : Response
    {
        public List<JobTemplate> Items;
    }

    [Serializable]
    public class CancelJobRequest : Request
    {
        public int Jobid;
    }

    [Serializable]
    public class CancelJobResponse : Response
    {

    }
    
    [Serializable]
    public class DeleteJobRequest : Request
    {
        public int Jobid;
    }

    [Serializable]
    public class DeleteJobResponse : Response
    {
        
    }

    [Serializable]
    public class GetJobsRequest: Request
    {
        public int Skip;
        public int Take;
    }

    [Serializable]
    public class GetJobsResponse : Response
    {
        public List<JobView> Items;
        public int Total;
    }

    [Serializable]
    public class GetJobLogRequest : Request
    {
        public int Jobid;
    }

    [Serializable]
    public class GetJobLogResponse : Response
    {
        public JobView Job;
        public List<JobLog> Items;
    }
}
