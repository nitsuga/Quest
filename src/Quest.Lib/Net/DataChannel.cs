using System;

namespace Quest.Lib.Net
{
    public interface DataChannel
    {
        DisconnectAction ActionOnDisconnect { get; set; }

        bool IsConnected { get; }
        event EventHandler<DataEventArgs> Data;

        /// <summary>
        ///     Raised when the link is disconnected during a read
        /// </summary>
        event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        ///     raised when a client connects to the remote server or a client connects to this server
        /// </summary>
        event EventHandler<ConnectedEventArgs> Connected;

        /// <summary>
        ///     send data to all connected remote ends
        /// </summary>
        /// <param name="data"></param>
        void Send(string data);

        void Send(string data, Guid[] clients);

        void Send(byte[] data);

        void CloseChannel();
    }

    public enum DisconnectAction
    {
        ThrowError,
        RaiseDisconnectEvent,
        RetryConnection
    }
}