using System.Collections.Generic;

namespace Quest.Common.Messages.Resource
{
    /// <summary>
    /// notification that resource assignment status has changed
    /// </summary>
    public class DestinationStatusChanged : MessageBase
    {
        public DestinationStatus Item;
    }
}
