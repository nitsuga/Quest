using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     All requests derive from this class. The AuthToken authenticates the requester. Only login requests do not need
    ///     and authtoken. A Request Id should also be set (.net class automatically generates new id's) to uniquely identify
    ///     this request
    /// </summary>
    public class Request : MessageBase
    {
        public Request()
        {
            SessionId = "0000-0000-0000-0000";

            RequestId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Session token passed back during login. (blank during login)
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        ///     Every request is stamped with a unique request id and every response will contain the corresponding
        ///     request id.
        /// </summary>
        public string RequestId { get; set; }

        public override string ToString()
        {
            return "Request";
        }
    }

    public class ApiRequest : MessageBase
    {
        public ApiRequest()
        {
            RequestId = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     Every request is stamped with a unique request id and every response will contain the corresponding
        ///     request id.
        /// </summary>
        public string RequestId { get; set; }

        public override string ToString()
        {
            return "Request";
        }
    }

}