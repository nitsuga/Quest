using Quest.Lib.Simulation.Model;
using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.Incidents
{
    public interface IIncidentStore
    {
        List<SimulationIncident> GetIncidents(long fromIncidentId, int take, DateTime from, DateTime to);
    }
}
