using Quest.Common.Simulation;
using System;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.Resources
{
    public interface IResourceStore
    {
        /// <summary>
        /// Get a list of fleet vehicles
        /// </summary>
        /// <returns></returns>
        List<SimVehicle> GetVehicles();

        /// <summary>
        /// retrieve a list of historic resource updates
        /// </summary>
        /// <param name="lastResourceId"></param>
        /// <param name="take"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        //List<Avls> GetHistoricResources(DateTime lastResourceId, int take, DateTime from, DateTime to);
    }
}
