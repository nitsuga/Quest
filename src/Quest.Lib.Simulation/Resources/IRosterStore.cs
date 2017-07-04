using Quest.Common.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Lib.Simulation.Resources
{
    
    public interface IRosterStore
    {
        /// <summary>
        /// return a list of vehicles that are on duty at this time
        /// </summary>
        /// <param name="validAt"></param>
        /// <returns></returns>
        List<VehicleRoster> GetRoster(DateTime validAt);

        /// <summary>
        /// load the roster for the simulation period
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void LoadRoster(DateTime from, DateTime to);
    }
}
