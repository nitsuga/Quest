using System.Runtime.Serialization;

namespace Quest.Common.Messages.Security
{


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
