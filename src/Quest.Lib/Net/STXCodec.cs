using System;
using System.Text;

namespace Quest.Lib.Net
{
    /// <summary>
    ///     Handle streams that have STX and ETX markers
    /// </summary>
    /// <remarks></remarks>
    public class StxStreamCodec : ICodec
    {
        //** Used to hold partial packets

        private StringBuilder _buffer = new StringBuilder();

        protected string Etx = "\x03";

        protected string Stx = "\x02";

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<DataToSendEventArgs> DataToSend;

        public virtual string Description => "Generic STX/ETX Client CODEC";

        public virtual void Send(object sender, byte[] data)
        {
            //** Simply pre and postfix STX and ETX markers
            var dataArgs = new DataToSendEventArgs(Encoding.ASCII.GetBytes(Stx));
            DataToSend?.Invoke(sender, dataArgs);
            dataArgs = new DataToSendEventArgs(data);
            DataToSend?.Invoke(sender, dataArgs);
            dataArgs = new DataToSendEventArgs(Encoding.ASCII.GetBytes(Etx));
            DataToSend?.Invoke(sender, dataArgs);
        }


        public void Send(object sender, string data)
        {
            //** Simply pre and postfix STX and ETX markers
            var dataArgs = new DataToSendEventArgs(Encoding.ASCII.GetBytes(Stx + data + Etx));
            DataToSend?.Invoke(sender, dataArgs);
        }

        public long Receive(object sender, byte[] data, int count)
        {
            long functionReturnValue = 0;


            lock (_buffer)
            {
                //** We dont use stringbuilder as it is slower for just two joins.
                _buffer = _buffer.Append(Encoding.ASCII.GetString(data, 0, count));

                do
                {
                    var iStart = _buffer.ToString().IndexOf(Stx, StringComparison.Ordinal);

                    //** The data does not have an STX marker, keep flushing the buffer
                    //** until we get one
                    if (iStart < 0)
                    {
                        _buffer = new StringBuilder();
                        return functionReturnValue;
                    }

                    //** we have an STX, make sure we discard stuff at the beginning. That way we
                    //** cant have an ETX before the STX.
                    _buffer = new StringBuilder(_buffer.ToString(), iStart);

                    //**now look for an ETX (or STX
                    var iEnd = _buffer.ToString().IndexOf(Etx, StringComparison.Ordinal);

                    if (iEnd == 0)
                        iEnd = _buffer.ToString().IndexOf(Stx, StringComparison.Ordinal);

                    //** The data does not have an ETX marker, keep the buffer
                    //** until we get one
                    if (iEnd < 0)
                    {
                        return 1024;
                    }

                    //** We get here if we have an STX and an ETX - so send the data
                    //** without the STX/ETX markers
                    var args = new DataReceivedEventArgs(_buffer.ToString().Substring(Stx.Length, iEnd - Etx.Length));
                    DataReceived?.Invoke(sender, args);

                    _buffer = iEnd + Etx.Length == _buffer.Length ? new StringBuilder() : new StringBuilder(_buffer.ToString().Substring(iEnd + Etx.Length));

                    //** Keep going until all packets have been extracted
                } while (_buffer.Length != 0);
            }
            return functionReturnValue;
        }
    }
}