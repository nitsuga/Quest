using Quest.Common.Messages;
using Quest.Lib.ServiceBus;
using System;
using System.Collections.Generic;

namespace Quest.WebCore.Services
{
    public class SecurityService
    {
        MessageCache _msgClientCache;

        public SecurityService(MessageCache msgClientCache)
        {
            _msgClientCache = msgClientCache;
        }

        /// <summary>
        /// Get claims for the current user
        /// </summary>
        /// <returns></returns>
        public List<AuthorisationClaim> GetAppClaims(string username)
        {
            if (username != null)
            {
                SecurityGetAppClaimsRequest request = new SecurityGetAppClaimsRequest { Username = username };
                
                var result = _msgClientCache.SendAndWait<SecurityGetAppClaimsResponse>(request, new TimeSpan(0, 0, 10));
                if (result == null)
                    return null;
                return result.Claims;
            }
            return null;
        }

        public bool HasClaim(List<AuthorisationClaim> claims, string claim, string value)
        {
            return Lib.Security.SecurityExtensions.HasClaim(claims, claim, value);
        }

        public SecurityNetwork GetNetwork()
        {
            SecurityGetNetworkRequest request = new SecurityGetNetworkRequest ();
            var result = _msgClientCache.SendAndWait<SecurityGetNetworkResponse>(request, new TimeSpan(0, 0, 10));
            return result.Network;
        }
    }
}