using System;

namespace Quest.Lib.Net
{
    /// <summary>
    ///     Provides a framework for coding and decoding different streams of data
    /// </summary>
    /// <remarks></remarks>
    public interface ICodec
    {
        /// <summary>
        ///     Get a full description for this Codec. its used in trace messages
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        string Description { get; }

        /// <summary>
        ///     Implementers must have a send function that formats data ready to be
        ///     sent to the other side
        ///     The Send function must raise DataToSend events for complete message blocks
        /// </summary>
        /// <remarks></remarks>
        void Send(object sender, string data);

        void Send(object sender, byte[] data);

        /// <summary>
        ///     Implementers must have a receive function that unpacks data from other side
        ///     The recieve function must raise DataReceived events for complete message blocks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        long Receive(object sender, byte[] data, int count);

        /// <summary>
        ///     raised when a complete packet has been received
        /// </summary>
        /// <remarks></remarks>
        event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        ///     raised when a complete packet is ready to be sent by the transport
        /// </summary>
        /// <remarks></remarks>
        event EventHandler<DataToSendEventArgs> DataToSend;

    }

    /// <summary>
    ///     contains data arguments when data packet as been received
    /// </summary>
    /// <remarks></remarks>
    public class DataReceivedEventArgs : EventArgs
    {
        public DataReceivedEventArgs(string message)
        {
            Message = message;
        }

        /// <summary>
        ///     message contaiing the complete packet
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Message { get; set; }
    }

    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }

        public bool IsError { get; set; }
    }

    /// <summary>
    ///     contains data that must be sent to the transport layer for transmission
    /// </summary>
    /// <remarks></remarks>
    public class DataToSendEventArgs : EventArgs
    {
        private readonly byte[] _message;

        public DataToSendEventArgs(byte[] message)
        {
            _message = message;
        }

        public byte[] GetMessage()
        {
            return _message;
        }
    }
}