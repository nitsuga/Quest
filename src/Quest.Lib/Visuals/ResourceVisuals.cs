using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GeoJSON.Net.Feature;
using Quest.Lib.DataModel;
using Quest.Lib.ServiceBus.Messages;

namespace Quest.Lib.Visuals
{
    [Export(typeof(IVisualProvider))]
    public class ResourceVisuals : IVisualProvider
    {
        public List<Visual> GetVisualsCatalogue(GetVisualsCatalogueRequest request)
        {
            using (QuestEntities db = new QuestEntities())
            {
                IQueryable<FinalRawSpeedData> query = db.FinalRawSpeedDatas
                    .Where(x => request.DateFrom <= x.TimeStamp)
                    .Where(x => request.DateTo >= x.TimeStamp);

                if (request.Resource.Any())
                    query = query.Where(x => x.Callsign == request.Resource);

                if (request.Incident.Any())
                    query = query.Where(x => x.IncidentId == long.Parse(request.Incident));

                return query
                    .OrderBy(x => x.TimeStamp)
                    .ToList()
                    .GroupBy(x=> $"{x.IncidentId}:{x.Callsign}")
                    .Select(x => new Visual
                {
                    Id = new VisualId()
                    { 
                        Source = "Resource",
                        Name = $"{x.Key}",
                        Id = $"{x.Key}",
                        VisualType = "Fixes"
                    },
                    Timeline = x.OrderBy(z=>z.TimeStamp)
                                .Select( y=> new TimelineData(y.RawSpeedDataID, y.TimeStamp, null, $"{y.Status}", $"{y.Status}") )
                                .ToList(),

                }).ToList();
            }

        }

        public FeatureCollection GetVisualsData(GetVisualsDataRequest request)
        {
            return null;
        }

        public List<Visual> QueryVisual(QueryVisualRequest request)
        {
            return new List<Visual>();
        }
    }
}
