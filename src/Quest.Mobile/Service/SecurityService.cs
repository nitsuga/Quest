using Microsoft.VisualBasic.FileIO;
using Quest.Common.Messages;
using System;
using System.ComponentModel.Composition;
using System.IO;
using GeoAPI.Geometries;
using Quest.Lib.Utils;
using System.Collections.Generic;
using Quest.Lib.ServiceBus;

namespace Quest.Mobile.Service
{
    public class SecurityService
    {
        MessageCache _msgClientCache;
        List<AuthorisationClaim> _defaultClaims = new List<AuthorisationClaim>()
                {
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.routing"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.showResourceMenu"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.showEventsMenu"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.showLayersMenu"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.dispatch"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.analysisMenu"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.timeline"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.adminMenu"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.jobMenu"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.test"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.showLayersMenu"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.eisec"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.search"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.boundsLock"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.searchModes"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.coordSearch"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.indexGroups"},
                    new AuthorisationClaim{ ClaimType="permission", ClaimValue="ui.addressGroups"},
                };

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
            if (String.IsNullOrEmpty(username))
                return _defaultClaims;
            else
            {
                SecurityGetAppClaimsRequest request = new SecurityGetAppClaimsRequest { Username = username };
                var result = _msgClientCache.SendAndWait<SecurityGetAppClaimsResponse>(request, new TimeSpan(0, 0, 10));
                if (result == null)
                    return _defaultClaims;
                return result.Claims;
            }
        }

        public bool HasClaim(List<AuthorisationClaim> claims, string claim, string value)
        {
            return Lib.Security.SecurityExtensions.HasClaim(claims, claim, value);
        }

        public SecurityNetwork GetNetwork()
        {
            SecurityGetNetworkRequest request = new SecurityGetNetworkRequest ();
            var result = MvcApplication.MsgClientCache.SendAndWait<SecurityGetNetworkResponse>(request, new TimeSpan(0, 0, 10));
            return result.Network;
        }
    }
}