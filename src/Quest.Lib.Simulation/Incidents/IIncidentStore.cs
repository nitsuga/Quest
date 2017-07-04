using Quest.Lib.Simulation.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Lib.Simulation.Incidents
{
    public interface IIncidentStore
    {
        List<SimulationIncident> GetIncidents(long fromIncidentId, int take, DateTime from, DateTime to);
    }
}
