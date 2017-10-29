using System.Runtime.Serialization;

namespace Quest.Common.Messages.Security
{
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


}
