using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.LAS.Codec;

namespace Quest.LAS.Messages
{
    /// <summary>
    /// Message From Device
    /// </summary>
    public interface IDeviceMessage 
    { }

    public class DeviceMessage : MessageBase
    {
        public IDeviceMessage Message;
        public MessageHeader Metadata;
    }
}
