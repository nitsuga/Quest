using System;

namespace Quest.Common.Messages
{
    /// <summary>
    /// notificate that a database change has occurred for this resource
    /// </summary>
    [Serializable]
    public class ResourceDatabaseUpdate : MessageBase
    {
        public string Callsign;

        // optional item details (missing if the item ha been deleted)
        public ResourceItem Item;
    }
}