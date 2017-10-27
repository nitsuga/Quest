using System;

namespace Quest.Common.Messages.Gazetteer.Gazetteer
{
    [Serializable]
    public class IndexGroupRequest : Request
    {
        public bool NotUsed { get; set; }

        public override string ToString()
        {
            return $"IndexGroupRequest";
        }
    }
}