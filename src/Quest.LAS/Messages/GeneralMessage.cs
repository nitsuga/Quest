namespace Quest.LAS.Messages
{
    public class GeneralMessage: ICadMessage
    {
        public MdtCadMessageTypeEnum MessageType { get; set; }
        public int MsgPriority { get; set; }
        public string MessageText { get; set; }
    }

}
