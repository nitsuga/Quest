using System;

namespace Quest.Common.Messages.System
{
    [Serializable]
    public class ProcessorStatus: MessageBase
    {
        public ProcessingUnitId Id;
        public ProcessorStatusCode Status;
    }

}
