#pragma warning disable 0169,649
using System;
using System.Text;

namespace Quest.Lib.Net
{
    public class CtiCodec : ICodec
    {
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<DataToSendEventArgs> DataToSend;

        public virtual string Description => "Client Codec";

        public virtual void Send(object sender, byte[] data)
        {
            DataToSend?.Invoke(sender, new DataToSendEventArgs(data));
        }

        public virtual void Send(object sender, string data)
        {
            var body = Encoding.ASCII.GetBytes(data);

            DataToSend?.Invoke(sender, new DataToSendEventArgs(body));
        }

        public virtual long Receive(object sender, byte[] data, int count)
        {
            var body = Encoding.ASCII.GetString(data, 0, count);
            DataReceived?.Invoke(sender, new DataReceivedEventArgs(body));
            return 999;
        }
    }
}