﻿using Quest.Common.Simulation;
using Quest.Lib.Simulation.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        List<Avl> GetHistoricResources(DateTime lastResourceId, int take, DateTime from, DateTime to);
    }
}