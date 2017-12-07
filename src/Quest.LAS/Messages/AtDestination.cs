namespace Quest.LAS.Messages
{
    public class AtDestination : IDeviceMessage
    {
        public int AtDestinationType;
        public int? NonConveyCode;
        public string DestinationHospital;
        public int? StatusEasting;
        public int? StatusNorthing;
    }

}
