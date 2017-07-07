﻿using Quest.Common.Simulation;
using Quest.Lib.DependencyInjection;
using Quest.Lib.Simulation.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Lib.Simulation.Incidents
{
    // shou,ld be sungleton
    //[Injection(Lifetime.Singleton )] 
    public class SimIncidentManager
    {
        IIncidentStore _store;

        public SimIncidentManager(IIncidentStore store)
        {
            _store = store;
        }

        public ObservableCollection<SimIncident> LiveIncidents { get; set; }

        public SimIncident FindIncident(long incidentId)
        {
            return LiveIncidents.FirstOrDefault(x => x.IncidentId == incidentId);
        }

        public List<SimulationIncident> GetIncidents(long fromIncidentId, int take, DateTime from, DateTime to)
        {
            return _store.GetIncidents(fromIncidentId, take, from, to);
        }
    }
}