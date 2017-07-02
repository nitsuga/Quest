using Nest;
using System;
using System.Collections.Generic;

namespace Quest.Common.Messages
{
    [Serializable]
    public class CallLogon : MessageBase
    {
        public string Extension { get; set; }
    }

    public class CallLogoff : MessageBase
    {
        public string Extension { get; set; }
    }

    [Serializable]
    public class CallLookupRequest : MessageBase
    {
        public int CallId { get; set; }

        public string CLI { get; set; }

        public string DDI { get; set; }


        public override string ToString()
        {
            return $"New Call Callid={CallId} CLI={CLI} DDI={DDI}";
        }
    }

    public enum CallerDetailsStatusCode
    {
        Searching,
        LocationFound,
    }

    [Serializable]
    public class CallLookupResponse : MessageBase
    {

        public bool IsMobile;

        public string Status { get; set; }

        public CallerDetailsStatusCode StatusCode { get; set; }

        /// <summary>
        /// single line address that produces a search hit
        /// </summary>

        public string SearchableAddress { get; set; }


        public string[] Address { get; set; }

        public int Requery { get; set; }

        public int Altitude { get; set; }

        public int Angle { get; set; }

        public int CallId { get; set; }

        public int Confidence { get; set; } // 0-99

        public int Direction { get; set; }

        public int Eastings { get; set; } // (metres)

        public string Name { get; set; }

        public int Northings { get; set; } // (metres)

        public int SemiMajor { get; set; }

        public int SemiMinor { get; set; }

        public int Speed { get; set; }

        public string TelephoneNumber { get; set; }

        public int RejectCode { get; set; }


        public PolygonGeoShape Shape;
    }

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

    [Serializable]
    public class CallEnd : MessageBase
    {
        public int CallId;
        public string Extension;
    }

    [Serializable]
    public class CallDisconnectStatusList : Request
    {
        /// <summary>
        ///     The event Id / CAD number
        /// </summary>
        public List<CallDisconnectStatus> Items { get; set; }

        public override string ToString()
        {
            if (Items != null)
                return $"Call Disconnect Status List count = {Items.Count}";
            return "Call Disconnect Status List Empty";
        }
    }

    [Serializable]
    public class CallDisconnectStatus
    {
        /// <summary>
        ///     The event Id / CAD number
        /// </summary>
        public string Serial { get; set; }

        public DateTime DisconnectTime { get; set; }

        public override string ToString()
        {
            return $"Call Disconnect Status {Serial} @ {DisconnectTime}";
        }
    }
}
