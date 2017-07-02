using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Quest.Common.Messages;

namespace Quest.Mobile.Models
{

    public class HomeViewModel
    {
        [Required]
        [Display(Name = "Templates")]
        public List<IndexGroup> IndexGroups { get; set; }

        public List<AuthorisationClaim> Claims { get; set; }
        public string User { get; set; }

        public bool HasClaim(List<AuthorisationClaim> claims, string claim, string value)
        {
            return Lib.Security.SecurityExtensions.HasClaim(claims, claim, value);
        }

    }
}