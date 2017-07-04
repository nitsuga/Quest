using System;
using System.Collections.Generic;
using System.Text;

namespace Quest.XC
{

    //************************************************
    //** 
    //** Handle streams from and from CTAK
    //** both directions are runlength encoded. The same Codec can be used at each end 
    //** Little Endian
    public class CTAKCodec : ICodec
    {

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<DataToSendEventArgs> DataToSend;
        public event EventHandler<MessageEventArgs> Message;

        /// <summary>
        /// Used to hold partial packets
        /// </summary>
        /// <remarks></remarks>
        private byte[] _buffer = new byte[4097];

        /// <summary>
        /// Amount of characters to read
        /// </summary>
        /// <remarks></remarks>
        private long _bytesWanted;
        private long _bytesWantedTotal;

        public CTAKCodec()
        {
        }

        public virtual string Description
        {
            get { return "Client Codec"; }
        }

        public virtual void Send(object sender, byte[] data)
        {
            byte[] header = new byte[2];

            //** Calculate run length
            long iBytes = (long)data.Length;

            header[0] = (byte)((iBytes) & 255);
            header[1] = (byte)((iBytes >> 8) & 255);

            if (DataToSend != null)
            {
                DataToSend(sender, new DataToSendEventArgs(header));
            }
            if (DataToSend != null)
            {
                DataToSend(sender, new DataToSendEventArgs(data));
            }
        }

        public virtual void Send(object sender, string data)
        {

            byte[] header = new byte[2];
            byte[] body = null;

            //** Calculate run length
            long iBytes = (long)data.Length;

            header[0] = (byte)((iBytes) & 255);
            header[1] = (byte)((iBytes >> 8) & 255);

            body = Encoding.ASCII.GetBytes(data);

            if (DataToSend != null)
            {
                DataToSend(sender, new DataToSendEventArgs(header));
            }
            if (DataToSend != null)
            {
                DataToSend(sender, new DataToSendEventArgs(body));

            }
        }

        public virtual long Receive(object sender, byte[] data, int count)
        {
            lock (this)
            {

                //** inbound data is length encoded.
                //** extract first 4 bytes as a length and wait until all the data has arrived
                //** before processing. The first time in the Counter should be zero

                int bytesAvailable = 0;

                //** used when calculating how much of the buffer to extract
                int bytesToCopy = 0;
                int copyOffset = 0;

                //Debug.Write("Got " + CStr(count) + " bytes ")
                //For i As Integer = 0 To count - 1
                // Debug.Write(Conversion.Hex(data(i)) + " ")
                //Next
                //Debug.WriteLine("")

                while (count > 0)
                {

                    //** see whether this is the first time in..
                    if (_bytesWanted == 0)
                    {

                        //** extract the number of characters to read from the data stream
                        //** This is the first four bytes of the data
                        _bytesWanted = ((long)data[1] * 256) + ((long)data[0]);
                        _bytesWantedTotal = _bytesWanted;

                        //** make sure we dont copy too much data across
                        bytesToCopy = count - 2;

                        copyOffset = 2;
                    }
                    else
                    {
                        // Debug.WriteLine("Got Header , waiting for " + CStr(_bytesWanted) + " bytes, bytes to copy=" + bytesToCopy.ToString())


                        //Debug.WriteLine("Got Body")

                        //** Get here if the last block that arrived didn't have enough data in it
                        //** so this packet doesn't have a length block at the beginning
                        //** make sure we dont copy too much data across
                        bytesToCopy = count;
                        copyOffset = 0;
                    }

                    if (_bytesWanted == 0)
                    {
                        return 0;
                    }

                    //** Make sure we dont take too much out of the buffer in one go.
                    if (bytesToCopy > _bytesWanted)
                    {
                        //** recalculate the number of valid bytes
                        Array.Copy(data, copyOffset, _buffer, 0, (int)_bytesWanted);

                        //** Copy extra data back to the beginning
                        long iStartPos = copyOffset + _bytesWanted;
                        count -= (int)iStartPos;
                        Array.Copy(data, iStartPos, data, 0, count);

                        //** we had too much data 
                        _bytesWanted = 0;
                    }
                    else
                    {
                        Array.Copy(data, copyOffset, _buffer, 0, bytesToCopy);

                        count = 0;
                        //** we had exactly or too little data
                        _bytesWanted -= bytesToCopy;
                    }

                    //** sConvertedData now holds in raw data from the stream without a length if there
                    //** was one attached. so the number of bytes available for processing is calculated..
                    bytesAvailable = _buffer.Length;

                    //** This is going to be nearly alaways the case that 
                    //** the number of bytes required is present
                    //** in the newly arrived packet + the save buffer
                    if (_bytesWanted == 0)
                    {
                        byte[] final = new byte[_bytesWantedTotal];
                        Array.Copy(_buffer, final, _bytesWantedTotal);

                        //Debug.Write("Emit " + CStr(final.Length) + " bytes ")
                        //For i As Integer = 0 To final.Length - 1
                        // Debug.Write(Conversion.Hex(final(i)) + " ")
                        //Next
                        //Debug.WriteLine("")

                        OnDataReceived(sender, Convert.ToBase64String(final));
                    }
                    else
                    {
                    }
                    //Debug.WriteLine("Still need " + CStr(_bytesWanted) + " bytes ")
                }

                return _bytesWanted;

            }
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