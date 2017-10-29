using System;
using System.Diagnostics;

namespace Quest.Common.Messages.System
{
    [Serializable]
    public class ProcessorMessage : MessageBase
    {
        public string Message;
        public TraceEventType Severity = TraceEventType.Information;
        public ProcessingUnitId Id;

        public override string ToString()
        {
            return $"{Message}";
        }

    }

}
