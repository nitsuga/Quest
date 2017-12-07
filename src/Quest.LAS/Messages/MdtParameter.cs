namespace Quest.LAS.Messages
{
    public class MdtParameter
    {
        public int Identifier { get; set; }
        public int DataLength { get; set; }
        public object ParameterValue { get; set; }
        public byte[] Data { get; set; }
    }

}
