using System;

namespace Quest.WebCore.Interfaces
{
    public class InvalidPropertyValueException : Exception
    {
        public InvalidPropertyValueException(string pluginName, string propertyName)
            : base($"Unable to initialize plugin [{pluginName}]. The value of the following property was invalid: {propertyName}")
        {
            
        }
    }
}