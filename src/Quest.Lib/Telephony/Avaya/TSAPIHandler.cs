using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Quest.Lib.Telephony.Avaya
{

    public class CallArgs : System.EventArgs
    {
        public uint callid;
        public string cli;
        public string vdn;
        public string extension;
    }

    public class TSAPIHandler
    {
        public event EventHandler<CallArgs> NewCall;
		public event EventHandler<CallArgs> Ringing;
		public event EventHandler<CallArgs> Connected;
		public event EventHandler<CallArgs> Disconnected;

        // Define the instance variables referenced throughout the class
        private UInt32 acsHandle = 0;
        private UInt32 numInvokeId;
        private char[] chDevice;
        private Csta.EventBuf_t eventBuf = new Csta.EventBuf_t();
        private UInt32 activeCallId;
        private char[] activeDeviceId;
        private int activeDeviceIdType;
        private string activeConnectionCallId;
        private string activeConnectionDeviceId;
        private string activeConnectionDeviceIdType;
        private string activeCallingDeviceId;
        private bool m_messagesWaiting;

        public TSAPIHandler()
        {
            // Add the delegate event for checking the TServer buffer
            TServerBufferPoll += TsHandler;
        }

        // The public method to open the ACS stream
        public bool open(string loginId, string passwd, string serverId)
        {
            // Convert the parameters to character arrays

            // Define the initial set of variables used for opening the ACS Stream
            int invokeIdType = 1;
            UInt32 invokeId = 0;
            int streamType = 1;
            char[] appName = "Mojo".ToCharArray();
            int acsLevelReq = 1;
            char[] apiVer = "TS1-2".ToCharArray();
            ushort sendQSize = 0;
            ushort sendExtraBufs = 0;
            ushort recvQSize = 0;
            ushort recvExtraBufs = 0;

            // Define the mandatory (but unused) private data structure
            Csta.PrivateData_t privData = new Csta.PrivateData_t();
            privData.vendor = "MERLIN                          ".ToCharArray();
            privData.length = 4;
            privData.data = "N".ToCharArray();

            // Define the event buffer pointer that gets data back from the TServer
            ushort numEvents = 0;
            Csta.EventBuf_t eventBuf = new Csta.EventBuf_t();
            ushort eventBufSize = Convert.ToUInt16(Csta.CSTA_MAX_HEAP);

            // Open the ACS stream
            try
            {
                char[] serverIdc = serverId.ToCharArray();
                char[] loginIdc = loginId.ToCharArray();
                char[] passwdc = passwd.ToCharArray();

                int openStream = Csta.acsOpenStream(ref acsHandle, invokeIdType, invokeId, streamType, serverIdc, loginIdc, passwdc, appName, acsLevelReq, apiVer,
                sendQSize, sendExtraBufs, recvQSize, recvExtraBufs, ref privData);
            }
            catch (System.Exception eOpenStream)
            {
                Logger.Write("There was a TServer error. " + eOpenStream.Message,TraceEventType.Error);
                return false;
            }

            // Wait a second to poll the event buffer
            System.Threading.Thread.Sleep(100);

            // Poll the event buffer
            try
            {
                int openStreamConf = Csta.acsGetEventPoll(acsHandle, ref eventBuf, ref eventBufSize, ref privData, ref numEvents);
            }
            catch (System.Exception eOpenStreamConf)
            {
                // If we can't get back a confirmation record the error and inform the user
                Logger.Write("There was a TServer error. " + eOpenStreamConf.Message, TraceEventType.Error);
                return false;
            }

            // Parse out the data elements in the event buffer...

            // The event header
            UInt32 numAcsHandle = BitConverter.ToUInt32(eventBuf.data, 0);
            ushort numEventClass = BitConverter.ToUInt16(eventBuf.data, 4);
            ushort numEventType = BitConverter.ToUInt16(eventBuf.data, 6);

            // The remainder of the open stream conf structure
            numInvokeId = BitConverter.ToUInt32(eventBuf.data, 8);
            string strApiVer = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 12, 21);
            string strLibVer = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 33, 21);
            string strTsrvVer = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 54, 21);
            string strDrvrVer = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 75, 21);

            if (numEventClass == Csta.ACSCONFIRMATION && numEventType == Csta.ACS_OPEN_STREAM_CONF)
            {
                // The stream has been successfully opened           
                return true;
            }
            else
            {
                // If we can't get back the open stream confirmation record the error and inform the user
                string strStreamOpenFailed = "The stream was not opened. The Event Class code returned was " + numEventClass.ToString() + " and the Event Type returned was " + numEventType.ToString() + ".";
                Logger.Write(strStreamOpenFailed, TraceEventType.Error);
                return false;
            }
        }

        // The public method to monitor the provided extension
        public bool monitor(string device)
        {
            // Convert the extension string to a character array
            chDevice = device.ToCharArray();

            // Define the mandatory (unused) private data structure
            Csta.PrivateData_t privData = new Csta.PrivateData_t();
            privData.vendor = "MERLIN                          ".ToCharArray();
            privData.length = 4;
            privData.data = "N".ToCharArray();

            // Define the various event monitor filters...

            // Any filters NOT added will allow those events to be monitored
            Csta.CSTAMonitorFilter_t monitorFilter = new Csta.CSTAMonitorFilter_t();
            monitorFilter.call = 65535 - Csta.CF_DELIVERED - Csta.CF_CONNECTION_CLEARED - Csta.CF_ESTABLISHED - Csta.CF_NETWORK_REACHED;
            // Monitor these call events 
            monitorFilter.feature = 0;
            // Monitor everything feature-wise
            monitorFilter.agent = 0;
            // Monitor everything agent-wise
            monitorFilter.maintenance = 0;
            // Monitor everything maintenance-wise
            monitorFilter.privateFilter = 1;
            // Mandatory but unused
            try
            {
                int monitorDevice = Csta.cstaMonitorCallsViaDevice(acsHandle, numInvokeId, chDevice, ref monitorFilter, ref privData);
            }
            catch (System.Exception eMonitor)
            {
                Logger.Write("Failed to monitor device - " + eMonitor.Message.ToString(), TraceEventType.Error);
                return false;
            }

            // Wait a second before polling the event queue
            System.Threading.Thread.Sleep(100);

            // Define the event buffer that contains data passed back from TServer
            ushort numEvents2 = 0;
            Csta.EventBuf_t eventBuf2 = new Csta.EventBuf_t();
            ushort eventBufSize2 = Convert.ToUInt16(Csta.CSTA_MAX_HEAP);

            try
            {
                int monitorDeviceConf = Csta.acsGetEventPoll(acsHandle, ref eventBuf2, ref eventBufSize2, ref privData, ref numEvents2);
            }
            catch (System.Exception eMonitorConf)
            {
                Logger.Write("Failed to monitor device - " + eMonitorConf.Message.ToString(), TraceEventType.Error);
                return false;
            }

            // Parse out the data elements in the event buffer...

            // The event header
            UInt32 numAcsHandle3 = BitConverter.ToUInt32(eventBuf2.data, 0);
            ushort numEventClass3 = BitConverter.ToUInt16(eventBuf2.data, 4);
            ushort numEventType3 = BitConverter.ToUInt16(eventBuf2.data, 6);

            // The various elements contained in the rest of the event buffer
            UInt32 numInvokeId3 = default(UInt32);
            UInt32 numMonitorCrossRefId3 = default(UInt32);
            ushort numCallFilter3 = 0;
            byte numFeatureFilter3 = 0;
            byte numAgentFilter3 = 0;
            byte numMaintenanceFilter3 = 0;
            UInt32 numPrivateFilter3 = default(UInt32);

            // If the device monitor was successful...
            if (numEventClass3 == Csta.CSTACONFIRMATION && numEventType3 == Csta.CSTA_MONITOR_CONF)
            {
                // Parse the elements in the event buffer
                numInvokeId3 = BitConverter.ToUInt32(eventBuf2.data, 8);
                numMonitorCrossRefId3 = BitConverter.ToUInt32(eventBuf2.data, 12);
                numCallFilter3 = BitConverter.ToUInt16(eventBuf2.data, 16);
                numFeatureFilter3 = Convert.ToByte(eventBuf2.data.GetValue(18));
                numAgentFilter3 = Convert.ToByte(eventBuf2.data.GetValue(20));
                numMaintenanceFilter3 = Convert.ToByte(eventBuf2.data.GetValue(22));
                numPrivateFilter3 = BitConverter.ToUInt32(eventBuf2.data, 24);
                return true;
            }
            else
            {
                string strMonitorDeviceFailed = "The device was not monitored. The Event Class code returned was " + numEventClass3.ToString() + " and the Event Type returned was " + numEventType3.ToString() + ".";
                Logger.Write(strMonitorDeviceFailed, TraceEventType.Error);
                return false;
            }
        }

        // The private method to fire if a CSTA_DELIVERED event type is received
        private void isRinging()
        {
            // Parse out the data elements in the event buffer...

            // The remainder of the delivered call structure
            UInt32 numMonitorCrossRefId = default(UInt32);
            UInt32 connectionCallId = default(UInt32);
            string connectionDeviceId = null;
            char[] chConnectionDeviceId = null;
            int connectionDeviceIdType = 0;
            string alertingDeviceId = null;
            char[] chAlertingDeviceId = null;
            int alertingDeviceIdType = 0;
            int alertingDeviceIdStatus = 0;
            string callingDeviceId = null;
            char[] chCallingDeviceId = null;
            int callingDeviceIdType = 0;
            int callingDeviceIdStatus = 0;
            string calledDeviceId = null;
            char[] chCalledDeviceId = null;
            int calledDeviceIdType = 0;
            int calledDeviceIdStatus = 0;
            string lastDeviceId = null;
            char[] chLastDeviceId = null;
            int lastDeviceIdType = 0;
            int lastDeviceIdStatus = 0;
            int localConnectionState = 0;
            int eventCause = 0;

            // The cross reference ID
            numMonitorCrossRefId = BitConverter.ToUInt32(eventBuf.data, 8);

            // The connection ID
            connectionCallId = BitConverter.ToUInt32(eventBuf.data, 12);
            connectionDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 16, 64);
            chConnectionDeviceId = connectionDeviceId.ToCharArray();
            connectionDeviceIdType = BitConverter.ToInt32(eventBuf.data, 80);

            // Define the active call to be referenced elsewhere
            activeCallId = connectionCallId;
            activeDeviceId = chConnectionDeviceId;
            activeDeviceIdType = connectionDeviceIdType;

            // The alerting device
            alertingDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 84, 64);
            chAlertingDeviceId = alertingDeviceId.ToCharArray();
            alertingDeviceIdType = BitConverter.ToInt32(eventBuf.data, 148);
            alertingDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 152);

            // The calling device
            callingDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 156, 64);
            chCallingDeviceId = callingDeviceId.ToCharArray();
            callingDeviceIdType = BitConverter.ToInt32(eventBuf.data, 220);
            callingDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 224);

            // The called device
            calledDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 228, 64);
            chCalledDeviceId = calledDeviceId.ToCharArray();
            calledDeviceIdType = BitConverter.ToInt32(eventBuf.data, 292);
            calledDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 296);

            // The last redirection device
            lastDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 300, 64);
            chLastDeviceId = lastDeviceId.ToCharArray();
            lastDeviceIdType = BitConverter.ToInt32(eventBuf.data, 364);
            lastDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 368);

            // A couple of other state and event describers
            localConnectionState = BitConverter.ToInt32(eventBuf.data, 372);
            eventCause = BitConverter.ToInt32(eventBuf.data, 376);

            activeConnectionCallId = connectionCallId.ToString();
            activeConnectionDeviceId = connectionDeviceId;
            activeConnectionDeviceIdType = connectionDeviceIdType.ToString();
            activeCallingDeviceId = callingDeviceId.Trim('\0');

            calledDeviceId = NumericsOnly(calledDeviceId);
            callingDeviceId = NumericsOnly(callingDeviceId);
            connectionDeviceId = NumericsOnly(connectionDeviceId);

            calledDeviceId = calledDeviceId.Substring(calledDeviceId.Length - 4);

            CallArgs callDetails = new CallArgs();
            callDetails.callid = connectionCallId;
            callDetails.cli = callingDeviceId;
            callDetails.vdn = connectionDeviceId;
            callDetails.extension = connectionDeviceId;

            string msg = string.Format("CSTA_DELIVERED id={0} cause={1} dest={2} destType={3} cli={4}", connectionCallId, eventCause, calledDeviceId, connectionDeviceId, callingDeviceId);
            Logger.Write(msg, TraceEventType.Information, "TSAPI");

            if (eventCause == 22)
            {
                if (NewCall != null)
                {
                    NewCall(this, callDetails);
                }
            }
            else
            {
                if (Ringing != null)
                {
                    Ringing(this, callDetails);
                }
            }
        }

        public string NumericsOnly(string source)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char ch in source)
            {
                if (char.IsNumber(ch))
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        // The private method to fire if a CSTA_ESTABLISHED event type is received
        private void isConnected()
        {
            // Parse out the data elements in the event buffer...

            // The remainder of the delivered call structure
            UInt32 monitorCrossRefId = default(UInt32);
            UInt32 connectionCallId = default(UInt32);
            string connectionDeviceId = null;
            char[] chConnectionDeviceId = null;
            int connectionDeviceIdType = 0;
            string answeringDeviceId = null;
            char[] chAnsweringDeviceId = null;
            int answeringDeviceIdType = 0;
            int answeringDeviceIdStatus = 0;
            string callingDeviceId = null;
            char[] chCallingDeviceId = null;
            int callingDeviceIdType = 0;
            int callingDeviceIdStatus = 0;
            string calledDeviceId = null;
            char[] chCalledDeviceId = null;
            int calledDeviceIdType = 0;
            int calledDeviceIdStatus = 0;
            string lastDeviceId = null;
            char[] chLastDeviceId = null;
            int lastDeviceIdType = 0;
            int lastDeviceIdStatus = 0;
            int localConnectionState = 0;
            int eventCause = 0;

            // The cross reference ID
            monitorCrossRefId = BitConverter.ToUInt32(eventBuf.data, 8);

            // The connection ID
            connectionCallId = BitConverter.ToUInt32(eventBuf.data, 12);
            connectionDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 16, 64);
            chConnectionDeviceId = connectionDeviceId.ToCharArray();
            connectionDeviceIdType = BitConverter.ToInt32(eventBuf.data, 80);

            // The answering device
            answeringDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 84, 64);
            chAnsweringDeviceId = answeringDeviceId.ToCharArray();
            answeringDeviceIdType = BitConverter.ToInt32(eventBuf.data, 148);
            answeringDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 152);

            // The calling device
            callingDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 156, 64);
            chCallingDeviceId = callingDeviceId.ToCharArray();
            callingDeviceIdType = BitConverter.ToInt32(eventBuf.data, 220);
            callingDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 224);

            // The called device
            calledDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 228, 64);
            chCalledDeviceId = calledDeviceId.ToCharArray();
            calledDeviceIdType = BitConverter.ToInt32(eventBuf.data, 292);
            calledDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 296);

            // The last redirection device
            lastDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 300, 64);
            chLastDeviceId = lastDeviceId.ToCharArray();
            lastDeviceIdType = BitConverter.ToInt32(eventBuf.data, 364);
            lastDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 368);

            // A couple of other state and event describers
            localConnectionState = BitConverter.ToInt32(eventBuf.data, 372);
            eventCause = BitConverter.ToInt32(eventBuf.data, 376);

            activeConnectionCallId = connectionCallId.ToString();
            activeConnectionDeviceId = connectionDeviceId;
            activeConnectionDeviceIdType = connectionDeviceIdType.ToString();

            calledDeviceId = NumericsOnly(calledDeviceId);
            callingDeviceId = NumericsOnly(callingDeviceId);
            connectionDeviceId = NumericsOnly(connectionDeviceId);

            CallArgs callDetails = new CallArgs();
            callDetails.callid = connectionCallId;
            callDetails.cli = callingDeviceId;
            callDetails.vdn = calledDeviceId;
            callDetails.extension = connectionDeviceId;

            string msg = string.Format("CSTA_ESTABLISHED id={0} cli={1} vdn={2} ext={3} cause={4}", connectionCallId, callingDeviceId, calledDeviceId, connectionDeviceId, eventCause);

            Logger.Write(msg, TraceEventType.Information, "TSAPI");

            if (eventCause == 22)
            {
                if (Connected != null)
                {
                    Connected(this, callDetails);
                }
            }

        }

        // The private method to fire if a CSTA_CONNECTION_CLEARED event type is received
        private void isDisconnected()
        {
            // Parse out the data elements in the event buffer...

            // The remainder of the cleared call structure
            UInt32 numMonitorCrossRefId = default(UInt32);
            UInt32 connectionCallId = default(UInt32);
            string connectionDeviceId = null;
            char[] chConnectionDeviceId = null;
            int connectionDeviceIdType = 0;
            string releasingDeviceId = null;
            char[] chReleasingDeviceId = null;
            int releasingDeviceIdType = 0;
            int releasingDeviceIdStatus = 0;
            int localConnectionState = 0;
            int eventCause = 0;

            // The cross reference ID
            numMonitorCrossRefId = BitConverter.ToUInt32(eventBuf.data, 8);

            // The connection ID
            connectionCallId = BitConverter.ToUInt32(eventBuf.data, 12);
            connectionDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 16, 64);
            chConnectionDeviceId = connectionDeviceId.ToCharArray();
            connectionDeviceIdType = BitConverter.ToInt32(eventBuf.data, 80);

            // The releasing device
            releasingDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 84, 64);
            chReleasingDeviceId = releasingDeviceId.ToCharArray();
            releasingDeviceIdType = BitConverter.ToInt32(eventBuf.data, 148);
            releasingDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 152);

            // A couple of other state and event describers
            localConnectionState = BitConverter.ToInt32(eventBuf.data, 156);
            eventCause = BitConverter.ToInt32(eventBuf.data, 160);

            releasingDeviceId = NumericsOnly(releasingDeviceId);

            CallArgs callDetails = new CallArgs();
            callDetails.callid = connectionCallId;
            callDetails.cli = releasingDeviceId;
            callDetails.vdn = "";
            callDetails.extension = "";

            string msg = string.Format("CSTA_CONNECTION_CLEARED id={0} device={1} type={2} type={3} cause={4}", connectionCallId, releasingDeviceId, releasingDeviceIdType, connectionDeviceIdType, eventCause);
            Logger.Write(msg, TraceEventType.Information, "TSAPI");

            if (eventCause == -1)
            {
                if (Disconnected != null)
                {
                    Disconnected(this, callDetails);
                }
            }

        }

        // The private method to fire if a CSTA_NETWORK_REACHED event type is received
        private void isDialed()
        {
            // Parse out the data elements in the event buffer...

            // The remainder of the network reached call structure
            UInt32 monitorCrossRefId = default(UInt32);
            UInt32 connectionCallId = default(UInt32);
            string connectionDeviceId = null;
            char[] chConnectionDeviceId = null;
            int connectionDeviceIdType = 0;
            string trunkDeviceId = null;
            char[] chTrunkDeviceId = null;
            int trunkDeviceIdType = 0;
            int trunkDeviceIdStatus = 0;
            string calledDeviceId = null;
            char[] chCalledDeviceId = null;
            int calledDeviceIdType = 0;
            int calledDeviceIdStatus = 0;
            int localConnectionState = 0;
            int eventCause = 0;

            // The cross reference ID
            monitorCrossRefId = BitConverter.ToUInt32(eventBuf.data, 8);

            // The connection ID
            connectionCallId = BitConverter.ToUInt32(eventBuf.data, 12);
            connectionDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 16, 64);
            chConnectionDeviceId = connectionDeviceId.ToCharArray();
            connectionDeviceIdType = BitConverter.ToInt32(eventBuf.data, 80);

            // The trunk ID
            trunkDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 84, 64);
            chTrunkDeviceId = trunkDeviceId.ToCharArray();
            trunkDeviceIdType = BitConverter.ToInt32(eventBuf.data, 148);
            trunkDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 152);

            // The called device
            calledDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 156, 64);
            chCalledDeviceId = calledDeviceId.ToCharArray();
            calledDeviceIdType = BitConverter.ToInt32(eventBuf.data, 220);
            calledDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 224);

            // A couple of other state and event describers
            localConnectionState = BitConverter.ToInt32(eventBuf.data, 228);
            eventCause = BitConverter.ToInt32(eventBuf.data, 232);

            activeConnectionCallId = connectionCallId.ToString();
            activeConnectionDeviceId = connectionDeviceId;
            activeConnectionDeviceIdType = connectionDeviceIdType.ToString();

            string msg = string.Format("CSTA_NETWORK_REACHED id={0} cli={1} vdn={2} ext={3}", connectionCallId, CallingDeviceId, calledDeviceId, connectionDeviceId);
            Logger.Write(msg, TraceEventType.Information, "TSAPI");

        }

        // The private method to fire if a CSTA_QUERY_MWI_CONF event type is received
        private void mwiStatus()
        {
            // Parse out the data elements in the event buffer...
            UInt32 mwiInvokeId = default(UInt32);
            bool boolMwi = false;

            // Parse the elements in the event buffer
            mwiInvokeId = BitConverter.ToUInt32(eventBuf.data, 8);
            boolMwi = BitConverter.ToBoolean(eventBuf.data, 12);

            m_messagesWaiting = boolMwi;
        }

        // The custom handler for interpreting various event types from TServer
        private void TsHandler(object sender, TServerBufferEventArgs e)
        {
            // Ringing call
            Debug.Print(e.EventClass.ToString());
            if (e.EventClass == Csta.CSTAUNSOLICITED && e.EventType == Csta.CSTA_DELIVERED)
            {
                isRinging();
            }
            else if (e.EventClass == Csta.CSTAUNSOLICITED && e.EventType == Csta.CSTA_CONNECTION_CLEARED)
            {
                // Disconnected call
                isDisconnected();
            }
            else if (e.EventClass == Csta.CSTAUNSOLICITED && e.EventType == Csta.CSTA_ESTABLISHED)
            {
                // Connected call
                isConnected();
            }
            else if (e.EventClass == Csta.CSTAUNSOLICITED && e.EventType == Csta.CSTA_NETWORK_REACHED)
            {
                // Dialed call 
                isDialed();
            }
            else if (e.EventClass == Csta.CSTACONFIRMATION && e.EventType == Csta.CSTA_QUERY_MWI_CONF)
            {
                // Message waiting indicator update
                mwiStatus();
            }
        }

        // Define the two TServer event buffer elements of interest
        public class TServerBufferEventArgs : EventArgs
        {
            private int m_eventClass;
            private int m_eventType;
            public int EventClass
            {
                get { return m_eventClass; }
            }
            public int EventType
            {
                get { return m_eventType; }
            }

            public TServerBufferEventArgs(int EventClass, int EventType)
            {
                m_eventClass = EventClass;
                m_eventType = EventType;
            }
        }

        // The public method to place an active call on hold
        public void holdCall(UInt32 callId, char[] device, int deviceType)
        {
            // Define the mandatory (unused) private data buffer
            Csta.PrivateData_t privData = new Csta.PrivateData_t();
            privData.vendor = "MERLIN                          ".ToCharArray();
            privData.length = 4;
            privData.data = "N".ToCharArray();

            // Populate a ConnectionID_t struct with the active call elements

            Csta.ConnectionID_t activeCall = new Csta.ConnectionID_t();
            activeCall.callID = callId;
            activeCall.deviceID.device = device;
            activeCall.devIDType = (Csta.ConnectionID_Device_t)deviceType;

            int holdCall = Csta.cstaHoldCall(acsHandle, numInvokeId, ref activeCall, false, ref privData);
        }

        // The public method to retrieve a held call
        public void retrieveCall(uint callId, char[] device, int deviceType)
        {
            // Define the mandatory (unused) private data buffer
            Csta.PrivateData_t privData = new Csta.PrivateData_t();
            privData.vendor = "MERLIN                          ".ToCharArray();
            privData.length = 4;
            privData.data = "N".ToCharArray();

            // Populate a ConnectionID_t struct with the active call elements


            Csta.ConnectionID_t activeCall = new Csta.ConnectionID_t();
            activeCall.callID = callId;
            activeCall.deviceID.device = device;
            activeCall.devIDType = (Csta.ConnectionID_Device_t)deviceType;

            int retrieveCall = Csta.cstaRetrieveCall(acsHandle, numInvokeId, ref activeCall, ref privData);
        }

        // The public method to pick up an delivered call
        public void answerCall(uint callId, char[] device, int deviceType)
        {
            // Define the mandatory (unused) private data buffer
            Csta.PrivateData_t privData = new Csta.PrivateData_t();
            privData.vendor = "MERLIN                          ".ToCharArray();
            privData.length = 4;
            privData.data = "N".ToCharArray();

            // Populate a ConnectionID_t struct with the active call elements
            Csta.ConnectionID_t activeCall = new Csta.ConnectionID_t();
            activeCall.callID = callId;
            activeCall.deviceID.device = device;
            activeCall.devIDType = (Csta.ConnectionID_Device_t)deviceType;

            int answerCall = Csta.cstaAnswerCall(acsHandle, numInvokeId, ref activeCall, ref privData);
        }

        // The public method to clear an active call
        public void hangupCall(uint callId, char[] device, int deviceType)
        {
            // Define the mandatory (unused) private data buffer
            Csta.PrivateData_t privData = new Csta.PrivateData_t();
            privData.vendor = "MERLIN                          ".ToCharArray();
            privData.length = 4;
            privData.data = "N".ToCharArray();

            // Populate a ConnectionID_t struct with the active call elements
            Csta.ConnectionID_t activeCall = new Csta.ConnectionID_t();
            activeCall.callID = callId;
            activeCall.deviceID.device = device;
            activeCall.devIDType = (Csta.ConnectionID_Device_t)deviceType;

            int clearConnection = Csta.cstaClearConnection(acsHandle, numInvokeId, ref activeCall, ref privData);
        }

        // The public method to initiate an outgoing call
        public void makeCall(string callee)
        {
            // Define the mandatory (unused) private data buffer 
            Csta.PrivateData_t privData = new Csta.PrivateData_t();
            privData.vendor = "MERLIN                          ".ToCharArray();
            privData.length = 4;
            privData.data = "N".ToCharArray();

            char[] calledDevice = callee.ToCharArray();

            try
            {
                int makeCall = Csta.cstaMakeCall(acsHandle, numInvokeId, chDevice, calledDevice, ref privData);
            }
            catch (System.Exception eMakeCall)
            {
                return;
            }
        }

        // The public method to check the message waiting indicator (MWI) status
        public void checkMwi()
        {
            // Define the mandatory (unused) private data buffer
            Csta.PrivateData_t privData = new Csta.PrivateData_t();
            privData.vendor = "MERLIN                          ".ToCharArray();
            privData.length = 4;
            privData.data = "N".ToCharArray();

            int pollMwi = Csta.cstaQueryMsgWaitingInd(acsHandle, numInvokeId, chDevice, ref privData);
        }

        // Define the generic TServer buffer event handler
        public delegate void TServerBufferEventHandler(object sender, TServerBufferEventArgs e);

        // Define the event for polling the TServer event buffer
        public event TServerBufferEventHandler TServerBufferPoll;

        // Check the TServer event buffer
        public void checkTServer()
        {
            // Define the mandatory (unused) private data buffer
            Csta.PrivateData_t privData = new Csta.PrivateData_t();
            privData.vendor = "MERLIN                          ".ToCharArray();
            privData.length = 4;
            privData.data = "N".ToCharArray();

            // Define the event buffer that contains data passed back from TServer
            eventBuf = new Csta.EventBuf_t();
            ushort numEvents = 0;
            ushort eventBufSize = Convert.ToUInt16(Csta.CSTA_MAX_HEAP);

            // Poll the event queue to see if any call events are occurring
            int polledEvent = 0;
            try
            {
                polledEvent = Csta.acsGetEventPoll(acsHandle, ref eventBuf, ref eventBufSize, ref privData, ref numEvents);
            }
            catch (System.Exception eEventPoll)
            {
                return;
            }

            if (polledEvent == -8)
            {
                return;
            }

            // Parse out the data elements in the event buffer...

            // The event header
            UInt32 numAcsHandle = BitConverter.ToUInt32(eventBuf.data, 0);
            ushort numEventClass = BitConverter.ToUInt16(eventBuf.data, 4);
            ushort numEventType = BitConverter.ToUInt16(eventBuf.data, 6);

            TServerBufferEventArgs args = new TServerBufferEventArgs(numEventClass, numEventType);
            if (TServerBufferPoll != null)
            {
                TServerBufferPoll(this, args);
            }
        }

        // Define the public properties available for the class
        public string ConnectionCallId
        {
            get { return activeConnectionCallId; }
        }
        public string ConnectionDeviceId
        {
            get { return activeConnectionDeviceId; }
        }
        public string ConnectionDeviceIdType
        {
            get { return activeConnectionDeviceIdType; }
        }
        public string CallingDeviceId
        {
            get { return activeCallingDeviceId; }
        }
        public bool MessagesWaiting
        {
            get { return m_messagesWaiting; }
        }

    }
}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
