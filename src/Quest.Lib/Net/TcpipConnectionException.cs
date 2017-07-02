using System;
using System.Runtime.Serialization;

namespace Quest.Lib.Net
{
    /// <summary>
    ///     Exception class specifically for this class
    /// </summary>
    /// <remarks></remarks>
    [Serializable]
    public class TcpipConnectionException : Exception
    {
        public TcpipConnectionException(string message)
            : base(message)
        {
        }

        public TcpipConnectionException()
        {
        }

        public TcpipConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected TcpipConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}