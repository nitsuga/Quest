using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     permits the device to query which event it is assigned to. This should normally be called
    ///     after and eventassign push or eventupdate push
    /// </summary>
    [Serializable]
    
    public class RefreshStateRequest : Request
    {
         public string Dummy;

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

    
}