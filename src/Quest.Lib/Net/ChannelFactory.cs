using System;
using System.Diagnostics;
using Quest.Lib.Trace;

namespace Quest.Lib.Net
{
    /// <summary>
    ///     make a TCP channel. The factory can make either a server or client TCP channel
    /// </summary>
    public class ChannelFactory
    {
        public static DataChannel MakeTcPchannel(string connsettings)
        {
            DataChannel result;
            string host;
            Type codecType;
            int port;

            ParseHostandPort(connsettings, out host, out port, out codecType);
            Logger.Write($"Trying {connsettings}", 
                TraceEventType.Information, "TCPchannel factory");

            if (host.Length == 0)
            {
                // construct a server as no host was specified.
                var conn = new TcpipListener();
                conn.StartListening(codecType, port);
                Logger.Write($"Listening on {port}", 
                    TraceEventType.Information, "TCPchannel factory");
                result = conn;
            }
            else
            {
                var conn = new TcpipConnection();
                var connected = conn.Connect(codecType, Guid.NewGuid(), host, port);
                Logger.Write(connected ? $"Connected to {host}:{port}" : $"Failed to connect to {host}:{port}",
                    TraceEventType.Information, "TCPchannel factory");
                result = conn;
            }

            return result;
        }

        /// <summary>
        ///     parse the connection string
        ///     for clients use the format HOST=xxx,PORT=999,CODEC=mycodec
        ///     for servers use the format PORT=999,CODEC=mycodec
        /// </summary>
        /// <param name="text"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="codecType"></param>
        private static void ParseHostandPort(string text, out string host, out int port, out Type codecType)
        {
            var parts = text.Split(';');
            host = "";
            port = 0;
            codecType = null;

            foreach (var s in parts)
            {
                var parms = s.Split('=');

                if (parms.Length == 2)
                {
                    switch (parms[0].ToUpper())
                    {
                        case "HOST":
                            host = parms[1];
                            break;
                        case "PORT":
                            int.TryParse(parms[1], out port);
                            break;
                        case "CODEC":
                            codecType = Type.GetType(parms[1]);
                            break;
                    }
                }
            }
        }
    }
}