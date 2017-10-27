using System;

namespace Quest.Common.Messages.Telephony
{
    [Serializable]
    public class CallEvent : MessageBase
    {
        /// <summary>
        ///     The type of event that has occurred
        /// </summary>
        /// <remarks></remarks>
        public enum CallEventType
        {
            Alerting,
            Connected
        }


        /// <summary>
        ///     the unique reference of the call
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int CallId;

        /// <summary>
        ///     the extension involved in the event if known
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Extension;


        /// <summary>
        /// The callers' number if known
        /// </summary>
        public string CLI;

        /// <summary>
        ///     the event type that has just occurred
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public CallEventType EventType;

        public override string ToString()
        {
            return $"Call Details Callid={CallId} Ext={Extension} Call Event={EventType}";
        }
    }
}
