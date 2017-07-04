using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quest.Lib.HEMS.Message;

namespace Quest.HEMSLinkTest
{
    internal class HEMSEventArgs : System.EventArgs
    {
        public HEMSMessage HEMSMessage { get; set; }
        public String RawMessage { get; set; }
        public String ErrorMessage { get; set; }
    }
}
