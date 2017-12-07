using System;

namespace Quest.LAS.Messages
{
    public class CadLogin : IDeviceMessage
    {
        public UInt16 ProtocolVersion;
        public string ConfigVersion;
        public string ImageVersion;
        public string G18Info;
        public string OtherInfo;
    }

}
