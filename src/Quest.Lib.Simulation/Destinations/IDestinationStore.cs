using Quest.Common.Simulation;
using System.Collections.Generic;

namespace Quest.Lib.Simulation.Destinations
{
    public interface IDestinationStore
    {
        List<SimDestination> GetDestinations(bool hospitals, bool stations, bool standby);
    }
}