using Quest.Lib.Simulation.DataModelSim;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Lib.Simulation.Incidents
{
    public class IncidentStoreShapefile: IIncidentStore
    {
        public List<SimulationIncidents> GetIncidents(long fromIncidentId, int take, DateTime from, DateTime to)
        {
            using (var SimData = new QuestSimContext())
            {
                return SimData.SimulationIncidents.Where(x => x.IncidentId > fromIncidentId)
                .Where(x => x.CallStart >= from && x.CallStart <= to)
                .Take(take)
                .ToList();
            }
        }
    }
}
