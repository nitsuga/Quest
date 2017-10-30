using GeoJSON.Net.Geometry;
using Quest.Lib.ServiceBus;
using Quest.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Incident;

namespace Quest.WebCore.Services
{

    public class IncidentService
    {

        AsyncMessageCache _messageCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageCache"></param>
        public IncidentService(AsyncMessageCache messageCache)
        {
            _messageCache = messageCache;
        }
        public IncidentService()
        {
        }

    }
}
