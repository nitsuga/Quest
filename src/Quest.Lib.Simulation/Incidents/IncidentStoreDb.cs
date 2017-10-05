using Quest.Lib.Data;
using Quest.Lib.Simulation.DataModelSim;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Lib.Simulation.Incidents
{
    public class IncidentStoreDb: IIncidentStore
    {
        private IDatabaseFactory _dbFactory;

        public IncidentStoreDb(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public List<SimulationIncidents> GetIncidents(long fromIncidentId, int take, DateTime from, DateTime to)
        {
            return _dbFactory.Execute<QuestSimContext, List<SimulationIncidents>>((db) =>
            {
                return db.SimulationIncidents.Where(x => x.IncidentId > fromIncidentId)
                .Where(x => x.CallStart >= from && x.CallStart <= to)
                .Take(take)
                .ToList();
            });
        }
    }
}
