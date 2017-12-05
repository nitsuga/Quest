using System.Collections.Generic;

namespace Quest.Common.Messages.Resource
{
    public class GetResourceAssignmentsResponse : Response
    {
        public List<DestinationStatus> Destinations;

        public List<DestinationHistory> History;


    }
}
