using Quest.Common.Messages.Job;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Quest.WebCore.Services
{

    public class JobsViewModel
    {
        [Required]
        [Display(Name = "Templates")]
        public List<JobTemplateModel> Templates { get; set; }
    }

    public class JobTemplateModel
    {
        [Required]
        [Display(Name = "Template")]
        public JobTemplate Template { get; set; }

        [Required]
        [Display(Name = "Url")]
        public String Url { get; set; }
    }
}