using System.ComponentModel.DataAnnotations;

namespace Quest.WebCore.Services
{

    public class MultisearchViewModel
    {
        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Locations")]
        public string Locations { get; set; }
    }
}                                                                                                                                     