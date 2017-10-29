using System.Collections.Generic;
using System.Linq;
using Quest.Common.Messages;
using Quest.Lib.Visuals;
using FeatureCollection = GeoJSON.Net.Feature.FeatureCollection;
using Autofac;
using Quest.Lib.Research.DataModelResearch;
using Quest.Lib.Data;
using Quest.Common.Messages.Visual;

namespace Quest.Lib.Research
{
    public class ResourceVisuals : IVisualProvider
    {
        private IDatabaseFactory _dbFactory;

        public ResourceVisuals(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public List<Visual> GetVisualsCatalogue(ILifetimeScope scope, GetVisualsCatalogueRequest request)
        {
            return _dbFactory.Execute<QuestDataContext, List<Visual>>((db) =>
            {
                var query = db.Avls
                    .Where(x => request.DateFrom <= x.AvlsDateTime.Value)
                    .Where(x => request.DateTo >= x.AvlsDateTime.Value);

                if (request.Resource.Any())
                    query = query.Where(x => x.Callsign == request.Resource);

                if (request.Incident.Any())
                    query = query.Where(x => x.IncidentId == long.Parse(request.Incident));

                return query
                    .OrderBy(x => x.AvlsDateTime.Value)
                    .ToList()
                    .GroupBy(x => $"{x.IncidentId}:{x.Callsign}")
                    .Select(x => new Visual
                    {
                        Id = new VisualId()
                        {
                            Source = "Resource",
                            Name = $"{x.Key}",
                            Id = $"{x.Key}",
                            VisualType = "Fixes"
                        },
                        Timeline = x.OrderBy(z => z.AvlsDateTime)
                                .Select(y => new TimelineData(y.RawAvlsId, y.AvlsDateTime, null, $"{y.Status}", $"{y.Status}"))
                                .ToList(),

                    }).ToList();
            });

        }

        public FeatureCollection GetVisualsData(ILifetimeScope scope, GetVisualsDataRequest request)
        {
            return null;
        }

        public QueryVisualResponse QueryVisual(ILifetimeScope scope, QueryVisualRequest request)
        {
            QueryVisualResponse result = new QueryVisualResponse();
            result.Visuals = new List<Visual>();
            return result;
        }
    }
}
