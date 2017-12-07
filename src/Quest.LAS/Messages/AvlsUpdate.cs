namespace Quest.LAS.Messages
{
    public class AvlsUpdate : IDeviceMessage
    {
        public int Easting;
        public int Northing;
        public float Speed;
        public float Direction;
        public float? EtaDistance;
        public float? EtaMinutes;
    }

}
