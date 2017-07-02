using System;
using System.Text;

namespace Quest.Lib.Net
{
    public class STORMCodec : ICodec
    {

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<DataToSendEventArgs> DataToSend;

        private const string STX = "<STORMTelServer>";

        private const string ETX = "</STORMTelServer>";
        //** Used to hold partial packets

        private string buffer;
        //** Amount of characters to read

        private void STORMCodec_Message(object sender, MessageEventArgs e)
        {
        }

        public virtual string Description
        {
            get { return "STORM CODEC"; }
        }

        public virtual void Send(object sender, string data)
        {
            byte[] body = Encoding.ASCII.GetBytes(data);
            if (DataToSend != null)
            {
                DataToSend(sender, new DataToSendEventArgs(body));
            }
        }

        public virtual long Receive(object sender, byte[] data, int count)
        {

            string msg = Encoding.ASCII.GetString(data, 0, count);
            //** We dont use stringbuilder as it is slower for just two joins.
            buffer = buffer + msg;

            do
            {
                int iStart = buffer.IndexOf(STX);

                //** The data does not have an STX marker, keep flushing the buffer
                //** until we get one
                if (iStart < 0)
                {
                    //buffer = ""
                    return 0;
                }

                //** we have an STX, make sure we discard stuff at the beginning. That way we
                //** cant have an ETX before the STX.
                buffer = buffer.Substring(iStart);

                //**now look for an ETX
                int iEnd = buffer.IndexOf(ETX);

                //** The data does not have an ETX marker, keep the buffer
                //** until we get one
                if (iEnd < 0)
                {
                    return 1024;
                }

                //** We get here if we have an STX and an ETX - so send the data
                //** without the STX/ETX markers
                if (DataReceived != null)
                {
                    DataReceived(sender, new DataReceivedEventArgs(buffer.Substring(0, iEnd + ETX.Length)));
                }

                if (iEnd + ETX.Length == buffer.Length)
                {
                    //** clear buffer if we know the ETX was at the end
                    buffer = "";
                }
                else
                {
                    //** There is still some stuff in the packet.. so leave partial data
                    //** alone
                    buffer = buffer.Substring(iEnd + ETX.Length);
                }

                //** Keep going until all packets have been extracted
            } while (buffer.Length != 0);

            return 0;

        }

        public void Send(object sender, byte[] data)
        {
            if (DataToSend != null)
            {
                DataToSend(sender, new DataToSendEventArgs(data));
            }
        }
    }
}
