using Quest.Common.ServiceBus;
using Quest.Common.Utils;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     All messages derive from MessageBase
    /// </summary>
    public abstract class MessageBase : IServiceBusMessage
    {
        public MessageBase()
        {
            Timestamp = Time.CurrentUnixTime();
        }

        /// <summary>
        ///     A timestamp indicating when the message was created
        /// </summary>
        public long Timestamp { get; set; }
    }
}