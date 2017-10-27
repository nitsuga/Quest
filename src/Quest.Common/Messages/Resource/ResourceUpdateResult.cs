namespace Quest.Common.Messages.Resource
{
    public class ResourceUpdateResult : MessageBase
    {
        public QuestResource OldResource;
        public QuestResource NewResource;
    }
}