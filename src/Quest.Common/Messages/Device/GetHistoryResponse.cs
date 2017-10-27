using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Device
{
    /// <summary>
    ///     a response to a GetHistory request contains historic items
    /// </summary>
    [Serializable]
    
    public class GetHistoryResponse : Response
    {
        public List<DeviceHistoryItem> Items { get; set; }
    }

    
}