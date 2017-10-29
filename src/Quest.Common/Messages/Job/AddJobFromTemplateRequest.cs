using System;

namespace Quest.Common.Messages.Job
{
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
}
