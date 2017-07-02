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
            AuthToken = "0000-0000-0000-0000";

            RequestId = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     authorisation token passed during login. (blank during login)
        /// </summary>
        public string AuthToken { get; set; }

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