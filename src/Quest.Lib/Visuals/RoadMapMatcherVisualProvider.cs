using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Quest.Lib.DataModel;
using Quest.Lib.MapMatching;
using Quest.Lib.ServiceBus.Messages;
using Quest.Common.Messages;

namespace Quest.Lib.Visuals
{
    [Export(typeof(IVisualProvider))]
    public class RoadMapMatcherVisualProvider : IVisualProvider
    {
        [Import]
        private CompositionContainer _container;

        public List<Visual> GetVisualsCatalogue(GetVisualsCatalogueRequest request)
        {
            using (QuestEntities db = new QuestEntities())
            {
                IQueryable<IncidentRoute> query = db.IncidentRoutes
                    .Where(x => request.DateFrom <= x.StartTime)
                    .Where(x => request.DateTo >= x.EndTime);

                if (request.Resource.Any())
                    query = query.Where(x => x.Callsign == request.Resource);

                if (request.Incident.Any())
                    query = query.Where(x => x.IncidentId == long.Parse(request.Incident));

                return query.OrderBy(x=>x.StartTime).ToList().Select(x => new Visual
                {
                    Id = new VisualId()
                    { 
                        Source = "MapMatcher",
                        Name = $"{x.IncidentId}:{x.Callsign}",
                        Id = x.IncidentRouteID.ToString(),
                        VisualType ="Route,Fixes"
                    },
                    Timeline = new List<TimelineData> { new TimelineData (x.IncidentRouteID, x.StartTime, x.EndTime, $"{x.IncidentId}:{x.Callsign}", "") },
                }).ToList();
            }

        }

        public GeoJSON.Net.Feature.FeatureCollection GetVisualsData(GetVisualsDataRequest request)
        {
            return null;
        }

        public List<Visual> QueryVisual(QueryVisualRequest request)
        {
            List<Visual> result = new List<Visual>();
            try
            {
                var response = MapMatcherManager.QueryVisual(_container, request);

                result.Add(response.Result.Fixes);
                result.Add(response.Result.Route);
                result.Add(response.Result.Particles);
            }
            catch (Exception ex)
            {
                // ignored
            }

            return result;
        }

    }
}
