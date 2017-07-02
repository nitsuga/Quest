using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Common.Messages
{
    public class SecurityGetAppClaimsRequest : Request
    {
        public string Username { get; set; }
    }

    public class SecurityGetAppClaimsResponse : Response
    {
        public List<AuthorisationClaim> Claims { get; set; }
    }

    public class SecurityGetNetworkRequest : Request
    {
    }
    public class SecurityGetNetworkResponse : Response
    {
        public SecurityNetwork Network { get; set; }
    }

    public class SecurityNetwork
    {
        public List<SecurityItem> Items;
        public List<SecurityItemLink> Links;
    }


    public class SecurityRequest : Request
    {
    }
    public class SecurityResponse : Response
    {
    }

    [DataContract]
    public class AuthorisationClaim
    {
        [DataMember]
        public string ClaimType;

        [DataMember]
        public string ClaimValue;

        public override string ToString()
        {
            return $"{ClaimType}:{ClaimValue}";
        }
    }

    /// <summary>
    /// Used to store SecuredItems
    /// </summary>
    [DataContract]
    public class SecurityItem
    {
        [DataMember]
        public int SecuredItemID { get; set; }

        [DataMember]
        public string SecuredItemName { get; set; }

        [DataMember]
        public string SecuredValue { get; set; }

        [DataMember]
        public int? Priority { get; set; }

        [DataMember]
        public string Description { get; set; }
    }


    /// <summary>
    /// Used to Store SecuredItemLinks
    /// </summary>
    [DataContract]
    public class SecurityItemLink
    {
        [DataMember]
        public int SecuredItemLinkId { get; set; }

        [DataMember]
        public int? SecuredItemIDParent { get; set; }

        [DataMember]
        public int? SecuredItemIDChild { get; set; }
    }


}
