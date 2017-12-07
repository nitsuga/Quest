namespace Quest.LAS.Messages
{
    public class ManualNavigation : IDeviceMessage
    {
        public string Destination;
        public int Easting;
        public int Northing;
    }

}
