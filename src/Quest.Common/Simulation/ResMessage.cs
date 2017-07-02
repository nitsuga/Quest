using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    public class ResMessage : MessageBase
    {
        public int ResourceId;
        public string Text;
        public object Priority;
    }

}
