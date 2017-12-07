using Quest.Common.Messages;
using Quest.LAS.Codec;
using System;

namespace Quest.LAS.Messages
{
    /// <summary>
    /// full decoded message from Cad outbound to devices
    /// </summary>
    public class CadMessage : MessageBase
    {
        public ICadMessage Message;
        public MessageHeader Metadata;
    }
}
