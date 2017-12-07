using System.Collections.Generic;

namespace Quest.LAS.Messages
{
    public class MdtParameters : ICadMessage
    {
        public List<MdtParameter> Items;
    }

    public class MdtParameter
    {
        public int Identifier { get; set; }
        public int DataLength { get; set; }
        public object ParameterValue { get; set; }
        public byte[] Data { get; set; }
    }

}
