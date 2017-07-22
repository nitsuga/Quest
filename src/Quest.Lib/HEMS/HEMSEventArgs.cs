using Quest.Lib.HEMS.Message;
using System;


namespace Quest.Lib.HEMS
{
    public class HEMSEventArgs : System.EventArgs
    {
        public HEMSMessage HEMSMessage { get; set; }
        public String RawMessage { get; set; }
        public String ErrorMessage { get; set; }
    }
}
