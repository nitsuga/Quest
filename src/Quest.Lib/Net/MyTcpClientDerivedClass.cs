using System.Net.Sockets;

namespace Quest.Lib.Net
{
    /// <summary>
    ///     This derived class lets us use protected properties belonging to the TcpClient Class.
    /// </summary>
    public class MyTcpClientDerivedClass : TcpClient, CodecSocket
    {
        private readonly TcpipConnection _parent;

        public MyTcpClientDerivedClass(TcpipConnection parent)
        {
            _parent = parent;
        }

        public bool IsConnected
        {
            get { return Client.Connected; }
        }

        public void Send(byte[] data)
        {
            _parent.Send(data);
        }

        public void Send(string data)
        {
            _parent.Send(data);
        }

        public override string ToString()
        {
            return _parent.ToString();
        }
    }
}