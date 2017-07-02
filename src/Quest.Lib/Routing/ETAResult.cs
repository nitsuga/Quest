using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Quest.Lib.Routing
{
    [DataContract]
    [Serializable]
    public class EtaResult
    {
        [DataMember] public string Callsign;

        [DataMember] public DateTime Eta;
    }

    [DataContract]
    [Serializable]
    public class EtaResults
    {
        [DataMember] public List<EtaResult> Results;

        [DataMember] public DateTime TimeNow;
    }
}