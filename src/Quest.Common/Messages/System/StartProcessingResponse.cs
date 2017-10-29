using System;

namespace Quest.Common.Messages.System
{
    [Serializable]
    public class StartProcessingResponse : Response
    {
        public ProcessingUnitId Id;
        public string Configuration;
        public string QueueName;
    }

}
