using System;
using System.Collections.Generic;

namespace Quest.Common.Messages
{
    [Serializable]
    public class MDTMessage
    {
        public long Id { get; set; }
        // Additional Items required for the object to contain all required Data
        public Dictionary<string, string> DecodedMessageParts { get; set; }
        public int FleetNo { get; set; }
        public string MPCNo { get; set; }
        public string CallSign { get; set; }
        public string SourceGateName { get; set; }
        public string SourceServer { get; set; }
        public DateTime Created { get; set; }
        public DateTime Processed { get; set; }
        public DateTime MDTTime { get; set; }
        public DateTime EQTimeStamp { get; set; }
        public string MsgType { get; set; }
        public int MsgTypeId { get; set; }
        public string FileUniqueId { get; set; }

        // Any Errors found while decoding the message body will set this flag true
        // Check the DecodedMessageParts Dictionary for specifics (ERROR_1 ERROR_2 etc)
        public bool HasErrors { get; set; }
    }
}