using System;

namespace Quest.Common.Messages
{

    [Serializable]
    public class IntelTheme
    {
        
        public string name;
        
        public double probability;
    }
    
    [Serializable]
    public class IntelObservation
    {
        
        public string[] geometries;
        
        public IntelTheme[] themes;
    }
    
    [Serializable]
    public class ValidPeriod
    {
        
        public DateTime start;
        
        public DateTime end;
    }

    
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

    [Serializable]
    public class IntelIncidentDelete : MessageBase
    {
        
        public string id;
    }
}