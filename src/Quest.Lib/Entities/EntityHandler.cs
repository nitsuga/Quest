using System;
using System.Linq;
using Nest;
using Quest.Common.Messages;
using Quest.Lib.Search.Elastic;
using Quest.Common.ServiceBus;
using Quest.Lib.Notifier;
using Quest.Lib.Incident;
using Quest.Lib.Data;
using Quest.Lib.Trace;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using Quest.Lib.Device;
using Quest.Common.Messages.CAD;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Gazetteer;
using Quest.Common.Messages.Device;
using Quest.Common.Messages.Incident;
using Quest.Common.Messages.Resource;
using Quest.Common.Messages.Notification;
using Quest.Common.Messages.Entities;
using Quest.Lib.DependencyInjection;

namespace Quest.Lib.Entities
{
    [Injection]
    public class EntityHandler
    {
        private IDatabaseFactory _dbFactory;
        private const string Version = "1.0.0";

        public EntityHandler(IDatabaseFactory dbFactory )
        {
            _dbFactory = dbFactory;
        }


        public GetEntityTypesResponse GetEntityTypes(GetEntityTypesRequest request)
        {
            // check the timestamp
            var layers = new[]
            {
                "StatusCodes", "Hospitals", "Skills", "StandbyPoints", "Stations", "Fuel"
            };

            return new GetEntityTypesResponse
            {
                RequestId = request.RequestId,
                Items = layers.ToList(),
                Success = true,
                Message = "successful"
            };
        }

        internal Response GetEntities(GetEntitiesRequest request)
        {
            var status = new Dictionary<string, List<string>>();
            status.Add("Available", new List<string> { "AOR", "AIQ" });
            status.Add("Enroute", new List<string> { "ENR" });
            status.Add("Busy", new List<string> { "TRN", "TAR" });
            status.Add("Offroad", new List<string> { "OOS" });

            var skill = new Dictionary<string, string>();
            skill.Add("Paramedic", "PAR");
            skill.Add("Doctor", "DOC");
            skill.Add("Technician", "EMT");

            return new GetEntitiesResponse()
            {
                Items = new List<EntityData> {
                new EntityData { Entity="StatusCodes", Data= status },
                new EntityData{ Entity="Hospitals", Data=null, Revision=1 },
                new EntityData{ Entity="Skills", Data=skill},
                new EntityData{ Entity="StandbyPoints", Data=null, Revision=1 },
                new EntityData{ Entity="Stations", Data=null, Revision=1 },
                new EntityData{ Entity="Fuel", Data=null, Revision=1 }
                }
            };
        }

        internal Response GetEntity(GetEntityRequest request)
        {
            return new GetEntityResponse()
            {
                Item  = new EntityData{ Entity="StatusCodes", Data="{ 'Available': ['AOR,'AIQ'],'Enroute': ['ENR'],'Busy': ['TAR,'TRN'] }", Revision=1 }
            };
        }
    }

}