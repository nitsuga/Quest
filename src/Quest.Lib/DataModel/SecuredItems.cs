using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class SecuredItems
    {
        public SecuredItems()
        {
            SecuredItemLinksSecuredItemIdchildNavigation = new HashSet<SecuredItemLinks>();
            SecuredItemLinksSecuredItemIdparentNavigation = new HashSet<SecuredItemLinks>();
        }

        public int SecuredItemId { get; set; }
        public string SecuredItemName { get; set; }
        public string SecuredValue { get; set; }
        public string Description { get; set; }
        public int? Priority { get; set; }

        public ICollection<SecuredItemLinks> SecuredItemLinksSecuredItemIdchildNavigation { get; set; }
        public ICollection<SecuredItemLinks> SecuredItemLinksSecuredItemIdparentNavigation { get; set; }
    }
}
