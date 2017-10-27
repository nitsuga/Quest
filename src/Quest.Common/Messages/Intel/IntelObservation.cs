using System;

namespace Quest.Common.Messages.Intel
{

    [Serializable]
    public class IntelObservation
    {
        
        public string[] geometries;
        
        public IntelTheme[] themes;
    }
}