using System;

namespace Quest.Common.Messages.System
{
    [Serializable]
    public class StopProcessingResponse : Response
    {
        public ProcessingUnitId Id;
        public ProcessorStatusCode Status;
    }

}
