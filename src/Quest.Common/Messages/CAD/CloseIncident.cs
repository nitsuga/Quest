﻿using System;

namespace Quest.Common.Messages.CAD
{
    [Serializable]
    public class CloseIncident : Request
    {
        public string Serial { get; set; }

        public override string ToString()
        {
            return "CloseIncident " + Serial;
        }
    }
}