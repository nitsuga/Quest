using System;

namespace Quest.WebCore.Interfaces
{
    public class MissingPropertyException : Exception
    {
        public MissingPropertyException(string pluginName, string propertyName) 
            : base($"Unable to initialize plugin [{pluginName}]. The following property was missing: {propertyName}")
        { }
    }
}
