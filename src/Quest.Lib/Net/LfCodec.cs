using System;
using System.Text;

namespace Quest.Lib.Net
{

    //************************************************
    //** 
    //** Handle streams from CRLF sources
    //** 
    //** 
    public class LfCodec : ICodec
    {
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<DataToSendEventArgs> DataToSend;

        const string vbLf = "\r";

        /// <summary>
        /// Used to hold partial packets
        /// </summary>
        /// <remarks></remarks>

        private string _buffer;
        /// <summary>
        /// Amount of characters to read
        /// </summary>
        /// <remarks></remarks>

        public LfCodec()
        {
        }

        public virtual string Description
        {
            get { return "Client Codec"; }
        }


        public virtual void Send(object sender, byte[] data)
        {
            //** Send as one packet
            if (DataToSend != null)
            {
                DataToSend(sender, new DataToSendEventArgs(data));
            }
        }

        public virtual void Send(object sender, string data)
        {
            Send(sender, System.Text.Encoding.ASCII.GetBytes(data + vbLf));
        }

        public virtual long Receive(object sender, byte[] data, int count)
        {
            long functionReturnValue = 0;

            //** We dont use stringbuilder as it is slower for just two joins.
            _buffer = _buffer + Encoding.ASCII.GetString(data, 0, count);

            do
            {
                int iStart = _buffer.IndexOf(vbLf);

                //** The data does not have an STX marker, keep flushing the buffer
                //** until we get one
                if (iStart < 0)
                {
                    _buffer = "";
                    return functionReturnValue;
                }

                //**now look for an ETX
                int iEnd = _buffer.IndexOf(vbLf);

                //** The data does not have an ETX marker, keep the buffer
                //** until we get one
                if (iEnd < 0)
                {
                    return 1024;
                }

                DataReceivedEventArgs args = new DataReceivedEventArgs(_buffer.Substring(0, iEnd));

                if (DataReceived != null)
                {
                    DataReceived(sender, args);
                }

                if (iEnd + 1 == _buffer.Length)
                {
                    //** clear buffer if we know the ETX was at the end
                    _buffer = "";
                }
                else
                {
                    //** There is still some stuff in the packet.. so leave partial data
                    //** alone
                    _buffer = _buffer.Substring(iEnd + 1);
                }

                //** Keep going until all packets have been extracted
            } while (_buffer.Length != 0);
            return functionReturnValue;

        }

        protected void OnDataReceived(object sender, string message)
        {
            if (DataReceived != null)
            {
                DataReceived(sender, new DataReceivedEventArgs(message));
            }
        }

    }

}
