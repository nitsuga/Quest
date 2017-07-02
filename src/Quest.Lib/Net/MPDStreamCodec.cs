#pragma warning disable 0169
using System;
using System.Text;

namespace Quest.Lib.Net
{
    /// <summary>
    ///     Handle streams that have STX and ETX markers
    /// </summary>
    /// <remarks></remarks>
    public class MpdStreamCodec : ICodec
    {
        //** Used to hold partial packets

        private string _buffer = "";

        protected string Etx = "</MPD>";

        protected string Stx = "<MPD";

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<DataToSendEventArgs> DataToSend;

        public string Description => "Met Police CODEC";

        public virtual void Send(object sender, byte[] data)
        {
            var dataArgs = new DataToSendEventArgs(data);
            DataToSend?.Invoke(sender, dataArgs);
        }


        public void Send(object sender, string data)
        {
            //** Simply pre and postfix STX and ETX markers
            var dataArgs = new DataToSendEventArgs(Encoding.ASCII.GetBytes(data));
            DataToSend?.Invoke(sender, dataArgs);
        }


        public long Receive(object sender, byte[] data, int count)
        {
            lock (_buffer)
            {
                //** We dont use stringbuilder as it is slower for just two joins.
                _buffer = _buffer + Encoding.ASCII.GetString(data, 0, count);

                do
                {
                    var iStart = _buffer.IndexOf(Stx, StringComparison.Ordinal);

                    //** The data does not have an STX marker, keep flushing the buffer
                    //** until we get one
                    if (iStart < 0)
                    {
                        _buffer = "";
                        return 0;
                    }

                    //** we have an STX, make sure we discard stuff at the beginning. That way we
                    //** cant have an ETX before the STX.
                    _buffer = _buffer.Substring(iStart);

                    //**now look for an ETX (or STX
                    var iEnd = _buffer.IndexOf(Etx, StringComparison.Ordinal);

                    if (iEnd == 0)
                        iEnd = _buffer.IndexOf(Stx, StringComparison.Ordinal);

                    //** The data does not have an ETX marker, keep the buffer
                    //** until we get one
                    if (iEnd < 0)
                    {
                        return 1024;
                    }

                    //** We get here if we have an STX and an ETX - so send the data
                    //** with the STX/ETX markers
                    var args = new DataReceivedEventArgs(_buffer.Substring(0, iEnd + Etx.Length));
                    DataReceived?.Invoke(sender, args);

                    _buffer = iEnd + Etx.Length == _buffer.Length ? "" : _buffer.Substring(iEnd + Etx.Length);

                    //** Keep going until all packets have been extracted
                } while (_buffer.Length != 0);
            }

            return 0;
        }
    }
}