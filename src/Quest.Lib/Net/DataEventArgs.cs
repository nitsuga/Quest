using System;

namespace Quest.Lib.Net
{
    [Serializable]
    public class DataEventArgs : EventArgs
    {
        private Guid _connectionId;

        public DataEventArgs(Guid instanceId, string data, Guid connectionId)
        {
            Data = data;
            _connectionId = connectionId;
        }

        public Guid ConnectionId
        {
            get { return _connectionId; }
            set { _connectionId = value; }
        }

        public string Data { get; }
    }


    /// <summary>
    ///     Class used for passing disconnection messages
    /// </summary>
    /// <remarks></remarks>
    public class DisconnectedEventArgs : EventArgs
    {
        public RemoteTcpipConnection Remoteconnection { get; set; }

        public Guid Queue { get; set; }
    }

    public class ConnectedEventArgs : EventArgs
    {
        public ConnectedEventArgs(RemoteTcpipConnection remoteTcpipConnection, Guid queue)
        {
            RemoteTcpipConnection = remoteTcpipConnection;
            Queue = queue;
        }

        public RemoteTcpipConnection RemoteTcpipConnection { get; set; }

        public Guid Queue { get; set; }
    }
}