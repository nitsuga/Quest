using Quest.Common.Simulation;
using Quest.Lib.Simulation.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Lib.Simulation.Incidents
{
    public class IncidentStoreDb: IIncidentStore
    {
        public List<SimulationIncident> GetIncidents(long fromIncidentId, int take, DateTime from, DateTime to)
        {
            using (var SimData = new QuestSimEntities())
            {
                return SimData.SimulationIncidents.Where(x => x.IncidentId > fromIncidentId)
                .Where(x => x.CallStart >= from && x.CallStart <= to)
                .Take(take)
                .ToList();
            }
        }
    }
}
