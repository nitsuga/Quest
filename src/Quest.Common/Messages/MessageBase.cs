using Quest.Common.ServiceBus;
using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     All messages derive from MessageBase
    /// </summary>
    public abstract class MessageBase : IServiceBusMessage
    {
        public MessageBase()
        {
            Timestamp = DateTime.UtcNow.Ticks/10000000 - 62135596800;
                                                         
        }

        /// <summary>
        ///     A timestamp indicating when the message was created
        /// </summary>
        public long Timestamp { get; set; }
    }
}