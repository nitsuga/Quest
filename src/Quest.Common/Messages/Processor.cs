using System;
using System.Diagnostics;

namespace Quest.Common.Messages
{
    public enum ProcessorStatusCode
    {
        Preparing = 1,
        Ready = 2,
        Running = 3,
        Stopped = 4,
        Failed = 5
    }

    [Serializable]
    public class ProcessingUnitId
    {
        public string Session;
        public string Name;
        public int Instance;
    }

    /// <summary>
    /// Start a new processor and pass it configuration information in JSON
    /// </summary>
    [Serializable]
    public class StartProcessingRequest : Request
    {
        public ProcessingUnitId Id;
    }

    [Serializable]
    public class StartProcessingResponse : Response
    {
        public ProcessingUnitId Id;
        public string Configuration;
        public string QueueName;
    }

    [Serializable]
    public class ProcessorStatus: MessageBase
    {
        public ProcessingUnitId Id;
        public ProcessorStatusCode Status;
    }

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

    [Serializable]
    public class ProcessorPrepareStatus : MessageBase
    {
        public ProcessingUnitId Id;
        public string Message;
        public int PercentComplete;
    }


    [Serializable]
    public class StopProcessingRequest : Request
    {
        public ProcessingUnitId Id;
    }

    [Serializable]
    public class StopProcessingResponse : Response
    {
        public ProcessingUnitId Id;
        public ProcessorStatusCode Status;
    }

}
