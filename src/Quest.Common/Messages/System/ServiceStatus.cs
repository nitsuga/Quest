using System;

namespace Quest.Common.Messages.System
{
    [Serializable]
    public class ServiceStatus : MessageBase
    {
        public string ServiceName { get; set; }
        public string Instance { get; set; }        
        public string Status { get; set; }
        public string Reason { get; set; }
        public string Server { get; set; }

        public override string ToString()
        {
            return $"ServiceStatus {ServiceName} {Instance} {Status} {Reason} {Server} ";
        }
    }
}