using System.Collections.Generic;

namespace Quest.Common.Messages.Security
{
    public class SecurityGetAppClaimsResponse : Response
    {
        public List<AuthorisationClaim> Claims { get; set; }
    }


}
