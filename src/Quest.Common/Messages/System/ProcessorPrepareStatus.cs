using System;

namespace Quest.Common.Messages.System
{
    [Serializable]
    public class ProcessorPrepareStatus : MessageBase
    {
        public ProcessingUnitId Id;
        public string Message;
        public int PercentComplete;
    }

}
