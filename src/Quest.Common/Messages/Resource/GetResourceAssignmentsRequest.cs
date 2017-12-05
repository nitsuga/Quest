namespace Quest.Common.Messages.Resource
{
    public class GetResourceAssignmentsRequest : Request
    {
        /// <summary>
        /// include all available vehicles within this distance (km)
        /// </summary>
        public double NearbyDistance=1;
    }
}
