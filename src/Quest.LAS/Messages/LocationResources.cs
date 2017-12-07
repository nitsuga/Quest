namespace Quest.LAS.Messages
{
    public class LocationResources : IDeviceMessage
    {
        public string RequestEasting;
        public string RequestNorthing;
        public int? StatusEasting;
        public int? StatusNorthing;
    }

}
