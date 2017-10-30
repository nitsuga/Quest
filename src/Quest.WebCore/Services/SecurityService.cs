using Quest.Common.Messages.Security;
using Quest.Lib.ServiceBus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quest.WebCore.Services
{
    public class SecurityService
    {
        AsyncMessageCache _msgClientCache;

        public SecurityService(AsyncMessageCache msgClientCache)
        {
            _msgClientCache = msgClientCache;
        }

        /// <summary>
        /// Get claims for the current user
        /// </summary>
        /// <returns></returns>
        public async Task<List<AuthorisationClaim>> GetAppClaims(string username)
        {
            if (username != null)
            {
                SecurityGetAppClaimsRequest request = new SecurityGetAppClaimsRequest { Username = username };
                
                var result = await _msgClientCache.SendAndWaitAsync<SecurityGetAppClaimsResponse>(request, new TimeSpan(0, 0, 10));
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

        public async Task<SecurityNetwork> GetNetwork()
        {
            SecurityGetNetworkRequest request = new SecurityGetNetworkRequest ();
            var result = await _msgClientCache.SendAndWaitAsync<SecurityGetNetworkResponse>(request, new TimeSpan(0, 0, 10));
            return result.Network;
        }
    }
}