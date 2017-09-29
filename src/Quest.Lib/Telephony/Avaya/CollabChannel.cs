using Quest.Lib.Net;
using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Quest.Lib.Telephony.AspectCTIPS
{
    public class CollabChannel : ICADChannel 
    {
        public event System.EventHandler<DialRequest> Dial;
        public event System.EventHandler<SetDataRequest> SetData;

        private Queue<string> _fifo;
        private System.Threading.ReaderWriterLock _locker;
        private System.Timers.Timer _timer;
        private TcpipConnection _tcpclient;
        private int _lastCallId;
        private Thread _readwriterworker;
        private CollabChannelConfig _cfg;

        /// <summary>
        /// maintain a list of inbound calls
        /// </summary>
        private HashSet<int> _tracker = new HashSet<int>();

        public override string ToString()
        {
            return _cfg == null ? "unconfigured channel" : _cfg.Name;
        }

        public void Initialise(CollabChannelConfig cfg)
        {
            _cfg = cfg;
            _timer = new System.Timers.Timer(5000);
            _fifo = new Queue<string>();
            _locker = new System.Threading.ReaderWriterLock();

            Logger.Write(string.Format("Channel {0} initialising", this.ToString()), TraceEventType.Information, "CollabChannel");
        }

        public void Start()
        {
            Logger.Write(string.Format("Channel {0} starting", this.ToString()), TraceEventType.Information, "CollabChannel");
            StartReaderWriter();
        }
        public void Stop()
        {
            Logger.Write(string.Format("Channel {0} stopping", this.ToString()), TraceEventType.Information, "CollabChannel");
        }

        private void QueueData(string Text)
        {
            try
            {
                Logger.Write("queued text " + Text + " for output to CAD", "Trace");
                _locker.AcquireWriterLock(1000);
                _fifo.Enqueue(Text);
                _locker.ReleaseWriterLock();

            }
            catch (Exception ex)
            {
                Logger.Write("couldn't queue text " + Text + " error:" + ex.Message, "Trace");

            }
        }

        private void StartReaderWriter()
        {
            _readwriterworker = new System.Threading.Thread(ReaderWriterWorker);
            _readwriterworker.IsBackground = true;
            _readwriterworker.Name = "ReadWriter Worker";
            _readwriterworker.Start();
        }

        public void SendLogoff(string extension)
        {
            QueueData(string.Format("logoff {0} ", extension));
        }

        public void SendLogon(string extension)
        {
            QueueData(string.Format("logon {0} ", extension));
        }

        public void Ring(int callid, string extension)
        {
            // only send if inbound call
            if (_tracker.Contains(callid))
                    QueueData(string.Format("ring {0} {1}", callid, extension));
        }
        
        public void EndCall(int callid)
        {
            // only send if inbound call
            if (_tracker.Contains(callid))
                {
                    QueueData(string.Format("end {0}", callid));
                    _tracker.Remove(callid);
                }

        }

        public void NewOutboundCall(int callid, string DDI, string Group)
        {
            if (_tracker.Contains(callid))
                _tracker.Remove(callid);
        }

        public void NewInboundCall(int callid, string DDI, string Group)
        {
            if (_lastCallId != callid)
            {
                if (_tracker.Contains(callid))
                    _tracker.Remove(callid);

                _tracker.Add(callid);

                QueueData(string.Format("new {0} {1} {2}", callid, DDI, Group));
                _lastCallId = callid;
            }
        }

        private void ReaderWriterWorker()
        {
            Logger.Write(string.Format("ReaderWriterWorker started", this.ToString()), TraceEventType.Information, "CollabChannel");
            do
            {

                try
                {
                    System.Threading.Thread.Sleep(250);
                    _locker.AcquireReaderLock(1000);
                    if (_fifo.Count > 0)
                    {
                        string Text = _fifo.Dequeue();
                        WriteDataToClient(Text);
                    }
                    _locker.ReleaseReaderLock();

                }
                catch (Exception ex)
                {
                    Logger.Write("couldn't dequeue text, error:" + ex.Message, "Trace");
                }
            } while (true);

        }

        private void WriteDataToClient(string Text)
        {
            try
            {
                if (_tcpclient == null)
                {
                    _tcpclient = new TcpipConnection();
                    _tcpclient.ActionOnDisconnect = DisconnectAction.RetryConnection;
                    _tcpclient.Data+=_tcpclient_Data;                    
                }

                if (_tcpclient.IsConnected == false)
                {
                    _tcpclient.Connect(typeof(LfCodec), Guid.Empty, _cfg.Hostname, _cfg.Port);
                }

                if (_tcpclient.IsConnected)
                {
                    Logger.Write("writing " + Text, "Trace");
                    byte[] sendBytes = System.Text.Encoding.ASCII.GetBytes(Text + "\n");
                    _tcpclient.Send(sendBytes);
                }
                else
                {
                    Logger.Write(string.Format("couldn't write {0}", Text), TraceEventType.Information, "CollabChannel");
                }

            }
            catch (Exception ex)
            {
                Logger.Write("couldn't write " + Text + " error:" + ex.Message, "Trace");

                _tcpclient = null;
            }
        }

        private void _tcpclient_Data(object sender, DataEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data.Trim()))
            {
                Logger.Write("Got: " + e.Data,"CollabChannel");
                string[] parts = e.Data.Split(' ');
                // format is dial id ext number
                switch (parts[0].ToLower())
                {
                    case "dial":
                        if (parts.Length == 4)
                        {
                            ThreadPool.QueueUserWorkItem(dialnumber, parts);
                        }
                        break;
                    case "setdata":
                        if (parts.Length == 4)
                        {
                            ThreadPool.QueueUserWorkItem(setdata, parts);
                        }
                        break;
                }
            }
        }

        private void dialnumber(object o)
        {
            try
            {
                string[] parts = o as string[];
                if (Dial != null)
                {
                    Logger.Write(string.Format("Channel {0} Raising Dial event", this.ToString()), TraceEventType.Information, "CollabChannel");
                    Dial(this, new DialRequest() { id = parts[1], destination = parts[2], station = parts[3] });
                }
                else
                    Logger.Write(string.Format("Channel {0} initialising", this.ToString()), TraceEventType.Information, "CollabChannel");

            }
            catch(Exception ex)
            {
                Logger.Write(String.Format("error raising dial event {0}: {1}", o, ex), "General");
            }
        }

        private void setdata(object o)
        {
            try
            {
                string[] parts = o as string[];
             //   if (SetData != null)
             //       SetData(this, new SetDataRequest() { callid = parts[1], data = parts[2],  station= parts[3], udf =parts[4] });
            }
            catch (Exception ex)
            {
                Logger.Write(String.Format("error raising setdata event {0}: {1}", o, ex), "General");
            }
        }
    }

}
