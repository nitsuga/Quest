using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Device
{
    /// <summary>
    ///     returns a list of status codes. see <seealso cref="GetStatusCodesRequest" /> for more details
    /// </summary>
    [Serializable]
    
    public class GetStatusCodesResponse : Response
    {
        
        public List<StatusCode> Items { get; set; }
    }

    
}