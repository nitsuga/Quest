using System.ComponentModel.DataAnnotations;

namespace Quest.Mobile.Models
{

    public class MultisearchViewModel
    {
        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Locations")]
        public string Locations { get; set; }
    }
}