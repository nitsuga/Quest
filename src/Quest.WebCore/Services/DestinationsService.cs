using System;
using System.Collections.Generic;
using GeoJSON.Net.Geometry;
using Quest.Lib.ServiceBus;
using Quest.Mobile.Models;
using System.Threading.Tasks;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Destination;

namespace Quest.WebCore.Services
{
    /// <summary>
    /// 
    /// </summary>

    public class DestinationService
    {

        AsyncMessageCache _messageCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageCache"></param>
        public DestinationService(AsyncMessageCache messageCache)
        {
            _messageCache = messageCache;
        }

        public DestinationService()
        {
        }
    }
}
