#pragma warning disable 0169,649
using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Common.Messages;
using Quest.Lib.Visuals;
using Autofac;
using Quest.Lib.Research.DataModelResearch;

namespace Quest.Lib.Research
{
    public class RoadMapMatcherVisualProvider : IVisualProvider
    {

        public List<Visual> GetVisualsCatalogue(ILifetimeScope scope, GetVisualsCatalogueRequest request)
        {
            using (var db = new QuestDataContext())
            {
                IQueryable<IncidentRoutes> query = db.IncidentRoutes
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
                        Id = x.IncidentRouteId.ToString(),
                        VisualType ="Route,Fixes"
                    },
                    Timeline = new List<TimelineData> { new TimelineData (x.IncidentRouteId, x.StartTime, x.EndTime, $"{x.IncidentId}:{x.Callsign}", "") },
                }).ToList();
            }

        }

        public GeoJSON.Net.Feature.FeatureCollection GetVisualsData(ILifetimeScope scope, GetVisualsDataRequest request)
        {
            return null;
        }

        public QueryVisualResponse QueryVisual(ILifetimeScope scope, QueryVisualRequest request)
        {
            QueryVisualResponse result = new QueryVisualResponse();
            result.Visuals = new List<Visual>();
            try
            {
                var response = ResearchMapMatcherManager.QueryVisual(scope, request);

                result.Success = response.Success;
                result.Message = response.Message;

                if (response.Result != null)
                {
                    result.Visuals.Add(response.Result.Fixes);
                    result.Visuals.Add(response.Result.Route);
                    result.Visuals.Add(response.Result.Particles);
                }
            }
                // ReSharper disable once UnusedVariable
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                // ignored
            }

            return result;
        }

    }
}
