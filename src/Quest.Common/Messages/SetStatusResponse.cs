using System;

namespace Quest.Common.Messages
{
    [Serializable]
    
    public class SetStatusResponse : Response
    {
        
        public StatusCode NewStatusCode { get; set; }

        
        public StatusCode OldStatusCode { get; set; }

        
        public string Callsign { get; set; }
    }

    
}