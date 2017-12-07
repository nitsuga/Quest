namespace Quest.LAS.Messages
{
    public class StationResources : IDeviceMessage
    {
        public string StationCode;
        public int? StatusEasting;
        public int? StatusNorthing;
    }

}
