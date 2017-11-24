namespace Quest.Common.Messages.Resource
{
    /// <summary>
    /// notification that resource assignment status has changed
    /// </summary>
    public class ResourceAssignmentChanged: MessageBase
    {
        public ResourceAssignments Items;
    }
}
