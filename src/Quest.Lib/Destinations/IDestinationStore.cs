using System.Collections.Generic;
using Quest.Common.Messages.Destination;

namespace Quest.Lib.Destinations
{
    public interface IDestinationStore
    {
        List<QuestDestination> GetDestinations(bool hospitals, bool stations, bool standby);
        QuestDestination GetDestination(string code);
    }
}