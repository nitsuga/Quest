using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Quest.Mobile.Models
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
        public Common.Messages.JobTemplate Template { get; set; }

        [Required]
        [Display(Name = "Url")]
        public String Url { get; set; }
    }
}