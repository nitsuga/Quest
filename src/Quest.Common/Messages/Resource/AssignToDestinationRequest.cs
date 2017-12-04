namespace Quest.Common.Messages.Resource
{
    public class AssignToDestinationRequest: Request
    {
        /// <summary>
        /// callsign to assign
        /// </summary>
        public string Callsign;

        /// <summary>
        /// destination to assign to
        /// </summary>
        public string DestinationCode;

        /// <summary>
        /// additional message to send to crew
        /// </summary>
        public string Message;

        /// <summary>
        /// include all available vehicles within this distance (km) in the response
        /// </summary>
        public double NearbyDistance = 1;

    }
}
