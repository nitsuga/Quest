using System.Collections.Generic;
using System.Linq;
using Quest.Common.Messages;

namespace Quest.Lib.Security
{
    public static class SecurityExtensions
    {
        public static bool HasClaim(this List<AuthorisationClaim> claims, string claim, string claimValue)
        {
            var v = claims.Any(x => x.ClaimType == claim && x.ClaimValue == claimValue);
            return v;
        }
    }

}
