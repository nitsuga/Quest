using System;

namespace Quest.Common.Messages.Device
{
    /// <summary>
    ///     request a list of status codes used by the organisation. Each status code belongs to
    ///     a status group. The status groups can be derived from the GetStatusCodesResponse response
    /// </summary>
    [Serializable]
    
    public class GetStatusCodesRequest : Request
    {
        [Serializable]
        
        public enum Mode
        {
            AllCodes,
            Context, // only those in context to my current status
            Specified // only those in context to the given status
        }

        
        public Mode SearchMode { get; set; }

        
        public string FromStatus { get; set; }

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

    
}