using System;

namespace Quest.Common.Messages.System
{
    [Serializable]
    public class StopProcessingRequest : Request
    {
        public ProcessingUnitId Id;
    }

}
