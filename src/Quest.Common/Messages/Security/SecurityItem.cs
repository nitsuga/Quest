using System.Runtime.Serialization;

namespace Quest.Common.Messages.Security
{
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


}
