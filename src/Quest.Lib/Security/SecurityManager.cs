using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Quest.Lib.DataModel;
using Quest.Lib.Processor;
using Autofac;
using Quest.Lib.ServiceBus;
using Quest.Common.ServiceBus;
using Quest.Lib.Trace;
using Quest.Lib.Utils;
using Quest.Lib.Data;
using Quest.Common.Messages.Security;

namespace Quest.Lib.Security
{
    /// <summary>
    /// Security Claims Manager. This processor can tell you what you're allowed to do.
    /// Security is held in the databas eunder SecureItems and SecureItemLinks
    /// </summary>
    public class SecurityManager : ServiceBusProcessor
    {
        private readonly ILifetimeScope _scope;
        IDatabaseFactory _dbFactory;

        public SecurityManager(
            ILifetimeScope scope,
            IDatabaseFactory dbFactory,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _dbFactory = dbFactory;
            _scope = scope;
        }

        protected override void OnPrepare()
        {
            MsgHandler.AddHandler<SecurityGetAppClaimsRequest>(GetAppClaimsRequestHandler);
            MsgHandler.AddHandler<SecurityGetNetworkRequest>(GetGetNetworkRequestHandler);
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        private SecurityGetAppClaimsResponse GetAppClaimsRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as SecurityGetAppClaimsRequest;
            if (request != null)
            {
                SecurityGetAppClaimsResponse result = new SecurityGetAppClaimsResponse
                {
                    Claims = GetAppClaims(request.Username)
                };
                Logger.Write($"User '{request.Username}' has {result.Claims.Count} claims");
                return result;
            }
            else
                return null;
        }

        private SecurityGetNetworkResponse GetGetNetworkRequestHandler(NewMessageArgs t)
        {
            var request = t.Payload as SecurityGetNetworkRequest;
            var response = new SecurityGetNetworkResponse();
            if (request != null)
            {
                response.Network = new SecurityNetwork()
                {
                    Items = GetAllSecuredItems(),
                     Links = GetAllSecuredItemLinks()
                };
            }
            return response;
        }


        /// <summary>
        /// Get all the claims belonging to the Principle.
        /// </summary>
        /// <param name="name">The Principle to which the claims belong</param>
        /// <returns></returns>
        private List<AuthorisationClaim> GetAppClaims(string name)
        {
            return _dbFactory.Execute<QuestContext, List<AuthorisationClaim>>((db) =>
            {
                return GetAppClaims(name, db);
            });
        }

        private List<AuthorisationClaim> GetAppClaims(string name, QuestContext db)
        {
            var results = db.GetClaims("user", name);
            if (results != null)
            {
                var list = results.Select(
                            x => new AuthorisationClaim() { ClaimType = x.SecuredItemName, ClaimValue = x.SecuredValue })
                            .ToList();
                return list;
            }
            else
                return new List<AuthorisationClaim>();
        }

        /// <summary>
        /// Get a list of claims that belong to this Principle, filtered by claims that are children of the
        /// supplied claim. This allows the caller to get a subset of claims in the claim heirarchy
        /// </summary>
        /// <param name="identity">The Principle to which the claims belong</param>
        /// <param name="claimType">the parent claim type</param>
        /// <param name="claimValue">the parent claim value</param>
        /// <param name="checkExists"></param>
        /// <returns>a list of claims</returns>
        private List<AuthorisationClaim> GetAppClaims(ClaimsPrincipal identity, string claimType,
            string claimValue, bool checkExists = true)
        {
            if (checkExists)
                CheckExists(claimType, claimValue);
            return GetAppClaims(identity.Identity.Name, claimType, claimValue, false);
        }


        /// <summary>
        /// Get a list of claims that belong to this user, filtered by claims that are children of the
        /// supplied claim. This allows the caller to get a subset of claims in the claim heirarchy
        /// </summary>
        /// <param name="username">The username to which the claims belong</param>
        /// <param name="claimType">the parent claim type</param>
        /// <param name="claimValue">the parent claim value</param>
        /// <param name="checkExists"></param>
        /// <returns>a list of claims</returns>
        private List<AuthorisationClaim> GetAppClaims(String username, string claimType, string claimValue,
            bool checkExists = true)
        {
            if (checkExists)
                CheckExists(claimType, claimValue);

            return GetSubClaims("user", username, claimType, claimValue, false);
        }

        /// <summary>
        /// Get a list of claims that belong to this claim, filtered by claims that are children of the
        /// supplied claim. This allows the caller to get a subset of claims in the claim heirarchy
        /// </summary>
        /// <param name="parentClaimType">the parent claim type</param>
        /// <param name="parentClaimValue">the parent claim value</param>
        /// <param name="claimType">the claim type</param>
        /// <param name="claimValue">the claim value</param>
        /// <param name="checkExists"></param>
        /// <returns>a list of claims</returns>
        private List<AuthorisationClaim> GetSubClaims(string parentClaimType, string parentClaimValue,
            string claimType, string claimValue, bool checkExists = true)
        {
            if (checkExists)
                CheckExists(claimType, claimValue);

            return _dbFactory.Execute<QuestContext, List<AuthorisationClaim>>((db) =>
            {
                var claimList1 = db.GetClaims(parentClaimType, parentClaimValue).ToList();
                var claimList2 = db.GetClaims(claimType, claimValue).ToList();

                var query = from claim1 in claimList1
                            from claim2 in claimList2
                            where
                                            (claim1.SecuredItemName == claim2.SecuredItemName && claim1.SecuredValue == claim2.SecuredValue)
                            select
                                            new AuthorisationClaim() { ClaimType = claim2.SecuredItemName, ClaimValue = claim2.SecuredValue };

                return query.ToList();
            });
        }


        /// <summary>
        /// This makes sure that the specified CliamType and ClaimValue exist in the secured list in the database
        /// and that the Claim is linked to a specialRole=WebService
        /// </summary>
        /// <param name="claimType"></param>
        /// <param name="claimValue"></param>
        private void CheckExists(string claimType, string claimValue)
        {
            _dbFactory.Execute<QuestContext>((db) =>
            {
                if (
                    !db.SecuredItems.Any(x => x.SecuredItemName == claimType && x.SecuredValue == claimValue))
                {
                    var ws =
                        db.SecuredItems
                            .FirstOrDefault(x => x.SecuredItemName == "specialrole" && x.SecuredValue == "WebService");
                    if (ws == null)
                    {
                        ws = new SecuredItems { SecuredItemName = "specialrole", SecuredValue = "WebService" };
                        db.SecuredItems.Add(ws);
                        db.SaveChanges();
                    }

                    var item = new SecuredItems { SecuredItemName = claimType, SecuredValue = claimValue };
                    db.SecuredItems.Add(item);
                    db.SaveChanges();

                    var link = new SecuredItemLinks
                    {
                        SecuredItemIdparent = ws.SecuredItemId,
                        SecuredItemIdchild = item.SecuredItemId
                    };
                    db.SecuredItemLinks.Add(link);
                    db.SaveChanges();
                }
            });
        }


        /// <summary>
        /// Retuns a full list of all SecuredItems
        /// </summary>
        /// <returns></returns>
        private List<SecurityItem> GetAllSecuredItems()
        {
            List<SecurityItem> returnItem;

            return _dbFactory.Execute<QuestContext, List<SecurityItem>>((db) =>
            {
                returnItem =
                    db.SecuredItems.Select(
                        x =>
                            new SecurityItem()
                            {
                                SecuredItemID = x.SecuredItemId,
                                SecuredItemName = x.SecuredItemName,
                                SecuredValue = x.SecuredValue,
                                Priority = x.Priority
                            }).ToList();
                return returnItem;
            });
        }

#if false
        public SecurityNetwork GetNetwork()
        {
            SecurityNetwork returnItem=new SecurityNetwork();

                        return _dbFactory.Execute<QuestContext, QuestResource>((db) =>
            {
                db.Configuration.ProxyCreationEnabled = false;
                returnItem.Items = db.SecuredItems.ToList();
                returnItem.Links = db.SecuredItemLinks.ToList();
            }
            return returnItem;
        }
#endif

        /// <summary>
        /// Returns a list of all SecuredItemLinks
        /// </summary>
        /// <returns></returns>
        public List<SecurityItemLink> GetAllSecuredItemLinks()
        {
            List<SecurityItemLink> returnItem;
            return _dbFactory.Execute<QuestContext, List<SecurityItemLink>>((db) =>
            {
                returnItem =
                    db.SecuredItemLinks.Select(
                        x =>
                            new SecurityItemLink()
                            {
                                SecuredItemLinkId = x.SecuredItemLinkId,
                                SecuredItemIDChild = x.SecuredItemIdchild,
                                SecuredItemIDParent = x.SecuredItemIdparent
                            }).ToList();
                return returnItem;
            });
        }
    }

}
