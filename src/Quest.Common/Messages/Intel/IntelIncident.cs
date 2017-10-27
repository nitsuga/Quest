using System;

namespace Quest.Common.Messages.Intel
{
    [Serializable]
    public class IntelIncident : MessageBase
    {
        
        public string id;
        
        public string[] geometries;
        
        public IntelTheme[] themes;
        
        public string[] keywords;
        
        public ValidPeriod times;
        
        public IntelObservation[] observations;
    }
}