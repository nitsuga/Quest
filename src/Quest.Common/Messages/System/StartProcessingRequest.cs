using System;

namespace Quest.Common.Messages.System
{
    /// <summary>
    /// Start a new processor and pass it configuration information in JSON
    /// </summary>
    [Serializable]
    public class StartProcessingRequest : Request
    {
        public ProcessingUnitId Id;
    }

}
