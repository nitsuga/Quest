﻿namespace Quest.LAS.Codec
{
    public class EqMessageSettings
    {
        public string Destination;
        public string Source;
        public int Sequence;
        public int Priority = 5;
        public int Lifetime = 60;
        public string Opt = "N";
        public int S1 = 9;
        public int S2 = 9;
        public int OutboundTimestampDelta;
    }
}
