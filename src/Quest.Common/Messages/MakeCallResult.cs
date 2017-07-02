using System;

namespace Quest.Common.Messages
{
    [Serializable]
    public class MakeCallResult
    {
        public MakeCallResult(int requestId, int result)
        {
            RequestId = requestId;
            Result = result;
        }

        public int RequestId { get; set; }
        public int Result { get; set; }

        public override string ToString()
        {
            return $"MakeCallResult {RequestId} {Result}";
        }
    }
}