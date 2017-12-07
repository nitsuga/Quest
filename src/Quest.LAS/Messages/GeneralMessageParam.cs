namespace Quest.LAS.Messages
{
    public class GeneralMessageParam
    {
        public MdtCadMessageTypeEnum MessageType { get; set; }
        public int MsgPriority { get; set; }
        public string MessageText { get; set; }
    }

}
