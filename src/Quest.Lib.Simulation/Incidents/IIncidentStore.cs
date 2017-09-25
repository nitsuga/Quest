using Quest.Lib.Simulation.DataModelSim;
using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.Incidents
{
    public interface IIncidentStore
    {
        List<SimulationIncidents> GetIncidents(long fromIncidentId, int take, DateTime from, DateTime to);
    }
}
