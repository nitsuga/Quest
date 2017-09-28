namespace Quest.Lib.DataModel
{
    public partial class SecuredItemLinks
    {
        public int SecuredItemLinkId { get; set; }
        public int? SecuredItemIdparent { get; set; }
        public int? SecuredItemIdchild { get; set; }

        public SecuredItems SecuredItemIdchildNavigation { get; set; }
        public SecuredItems SecuredItemIdparentNavigation { get; set; }
    }
}
