using Quest.LAS.Messages;

namespace Quest.LAS.Messages
{
    public class StatusUpdate : IDeviceMessage
    {
        public int ShortStatusCode;
        public int? StatusEasting;
        public int? StatusNorthing;
        public int? NonConveyCode;
        public string DestinationHospital;
    }

}
