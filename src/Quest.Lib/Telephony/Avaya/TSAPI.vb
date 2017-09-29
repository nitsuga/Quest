Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Windows.Forms
Imports System.Diagnostics
Imports Microsoft.Practices.EnterpriseLibrary.Logging
Imports Microsoft.Practices.EnterpriseLibrary.Logging.ExtraInformation
Imports Microsoft.Practices.EnterpriseLibrary.Logging.Filters
Imports Microsoft.Practices.EnterpriseLibrary.ExceptionHandling

Namespace CTI

    Public Class CallArgs
        Inherits System.EventArgs
        Public parent As Object
        Public callid As UInteger
        Public cli As String
        Public vdn As String
        Public extension As String
    End Class

    Public Class Tsapi
        Implements ITsapi

        ' Define the instance variables referenced throughout the class
        Private acsHandle As UInt32 = 0
        Private numInvokeId As UInt32
        Private chDevice As Char()
        Private strSource As String = "Diba"
        Private strLog As String = "Application"
        Private eventBuf As New Csta.EventBuf_t()
        Private activeCallId As UInt32
        Private activeDeviceId As Char()
        Private activeDeviceIdType As Integer
        Private activeConnectionCallId As String
        Private activeConnectionDeviceId As String
        Private activeConnectionDeviceIdType As String
        Private activeCallingDeviceId As String
        Private m_messagesWaiting As Boolean
        Private _parent As Object

        Public Event NewCall As System.EventHandler(Of CallArgs) Implements ITsapi.NewCall
        Public Event Ringing As System.EventHandler(Of CallArgs) Implements ITsapi.Ringing
        Public Event Connected As System.EventHandler(Of CallArgs) Implements ITsapi.Connected
        Public Event Disconnected As System.EventHandler(Of CallArgs) Implements ITsapi.Disconnected

        Public Sub New(ByVal parent As Object)
            ' Add the delegate event for checking the TServer buffer
            AddHandler Me.TServerBufferPoll, AddressOf TsHandler
            _parent = parent
        End Sub


        ' The public method to open the ACS stream
        Public Function open(ByVal loginId As String, ByVal passwd As String, ByVal serverId As String) As Boolean Implements ITsapi.open
            ' Convert the parameters to character arrays

            ' Define the initial set of variables used for opening the ACS Stream
            Dim invokeIdType As Integer = 1
            Dim invokeId As UInt32 = 0
            Dim streamType As Integer = 1
            Dim appName As Char() = "Mojo".ToCharArray()
            Dim acsLevelReq As Integer = 1
            Dim apiVer As Char() = "TS1-2".ToCharArray()
            Dim sendQSize As UShort = 0
            Dim sendExtraBufs As UShort = 0
            Dim recvQSize As UShort = 0
            Dim recvExtraBufs As UShort = 0

            ' Define the mandatory (but unused) private data structure
            Dim privData As New Csta.PrivateData_t()
            privData.vendor = "MERLIN                          ".ToCharArray()
            privData.length = 4
            privData.data = "N".ToCharArray()

            ' Define the event buffer pointer that gets data back from the TServer
            Dim numEvents As UShort = 0
            Dim eventBuf As New Csta.EventBuf_t()
            Dim eventBufSize As UShort = CUShort(Csta.CSTA_MAX_HEAP)

            ' Open the ACS stream
            Try
                Dim serverIdc As Char() = serverId.ToCharArray()
                Dim loginIdc As Char() = loginId.ToCharArray()
                Dim passwdc As Char() = passwd.ToCharArray()

                Dim openStream As Integer = Csta.acsOpenStream(acsHandle, invokeIdType, invokeId, streamType, serverIdc, loginIdc, _
                 passwdc, appName, acsLevelReq, apiVer, sendQSize, sendExtraBufs, _
                 recvQSize, recvExtraBufs, privData)
            Catch eOpenStream As System.Exception
                Logger.Write("There was a TServer error. " + eOpenStream.Message, Category.Trace.ToString(), 0, 0, TraceEventType.Error)
                Return False
            End Try

            ' Wait a second to poll the event buffer
            System.Threading.Thread.Sleep(100)

            ' Poll the event buffer
            Try
                Dim openStreamConf As Integer = Csta.acsGetEventPoll(acsHandle, eventBuf, eventBufSize, privData, numEvents)
            Catch eOpenStreamConf As System.Exception
                ' If we can't get back a confirmation record the error and inform the user
                Logger.Write("There was a TServer error. " + eOpenStreamConf.Message, Category.Trace.ToString(), 0, 0, TraceEventType.Error)
                Return False
            End Try

            ' Parse out the data elements in the event buffer...

            ' The event header
            Dim numAcsHandle As UInt32 = BitConverter.ToUInt32(eventBuf.data, 0)
            Dim numEventClass As UShort = BitConverter.ToUInt16(eventBuf.data, 4)
            Dim numEventType As UShort = BitConverter.ToUInt16(eventBuf.data, 6)

            ' The remainder of the open stream conf structure
            numInvokeId = BitConverter.ToUInt32(eventBuf.data, 8)
            Dim strApiVer As String = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 12, 21)
            Dim strLibVer As String = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 33, 21)
            Dim strTsrvVer As String = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 54, 21)
            Dim strDrvrVer As String = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 75, 21)

            If numEventClass = Csta.ACSCONFIRMATION AndAlso numEventType = Csta.ACS_OPEN_STREAM_CONF Then
                ' The stream has been successfully opened           
                Return True
            Else
                ' If we can't get back the open stream confirmation record the error and inform the user
                Dim strStreamOpenFailed As String = "The stream was not opened. The Event Class code returned was " + numEventClass.ToString() + " and the Event Type returned was " + numEventType.ToString() + "."
                Logger.Write(strStreamOpenFailed, Category.Trace.ToString(), 0, 0, TraceEventType.Error)
                Return False
            End If
        End Function

        ' The public method to monitor the provided extension
        Public Function monitor(ByVal device As String) As Boolean Implements ITsapi.monitor
            ' Convert the extension string to a character array
            chDevice = device.ToCharArray()

            ' Define the mandatory (unused) private data structure
            Dim privData As New Csta.PrivateData_t()
            privData.vendor = "MERLIN                          ".ToCharArray()
            privData.length = 4
            privData.data = "N".ToCharArray()

            ' Define the various event monitor filters...

            ' Any filters NOT added will allow those events to be monitored
            Dim monitorFilter As New Csta.CSTAMonitorFilter_t()
            monitorFilter.[call] = 65535 - Csta.CF_DELIVERED - Csta.CF_CONNECTION_CLEARED - Csta.CF_ESTABLISHED - Csta.CF_NETWORK_REACHED
            ' Monitor these call events 
            monitorFilter.feature = 0
            ' Monitor everything feature-wise
            monitorFilter.agent = 0
            ' Monitor everything agent-wise
            monitorFilter.maintenance = 0
            ' Monitor everything maintenance-wise
            monitorFilter.privateFilter = 1
            ' Mandatory but unused
            Try
                Dim monitorDevice As Integer = Csta.cstaMonitorCallsViaDevice(acsHandle, numInvokeId, chDevice, monitorFilter, privData)
            Catch eMonitor As System.Exception
                Logger.Write("Failed to monitor device - " + eMonitor.Message.ToString(), Category.Trace.ToString(), 0, 0, TraceEventType.Error)
                Return False
            End Try

            ' Wait a second before polling the event queue
            System.Threading.Thread.Sleep(100)

            ' Define the event buffer that contains data passed back from TServer
            Dim numEvents2 As UShort = 0
            Dim eventBuf2 As New Csta.EventBuf_t()
            Dim eventBufSize2 As UShort = CUShort(Csta.CSTA_MAX_HEAP)

            Try
                Dim monitorDeviceConf As Integer = Csta.acsGetEventPoll(acsHandle, eventBuf2, eventBufSize2, privData, numEvents2)
            Catch eMonitorConf As System.Exception
                Logger.Write("Failed to monitor device - " + eMonitorConf.Message.ToString(), Category.Trace.ToString(), 0, 0, TraceEventType.Error)
                Return False
            End Try

            ' Parse out the data elements in the event buffer...

            ' The event header
            Dim numAcsHandle3 As UInt32 = BitConverter.ToUInt32(eventBuf2.data, 0)
            Dim numEventClass3 As UShort = BitConverter.ToUInt16(eventBuf2.data, 4)
            Dim numEventType3 As UShort = BitConverter.ToUInt16(eventBuf2.data, 6)

            ' The various elements contained in the rest of the event buffer
            Dim numInvokeId3 As UInt32
            Dim numMonitorCrossRefId3 As UInt32
            Dim numCallFilter3 As UShort
            Dim numFeatureFilter3 As Byte
            Dim numAgentFilter3 As Byte
            Dim numMaintenanceFilter3 As Byte
            Dim numPrivateFilter3 As UInt32

            ' If the device monitor was successful...
            If numEventClass3 = Csta.CSTACONFIRMATION AndAlso numEventType3 = Csta.CSTA_MONITOR_CONF Then
                ' Parse the elements in the event buffer
                numInvokeId3 = BitConverter.ToUInt32(eventBuf2.data, 8)
                numMonitorCrossRefId3 = BitConverter.ToUInt32(eventBuf2.data, 12)
                numCallFilter3 = BitConverter.ToUInt16(eventBuf2.data, 16)
                numFeatureFilter3 = CByte(eventBuf2.data.GetValue(18))
                numAgentFilter3 = CByte(eventBuf2.data.GetValue(20))
                numMaintenanceFilter3 = CByte(eventBuf2.data.GetValue(22))
                numPrivateFilter3 = BitConverter.ToUInt32(eventBuf2.data, 24)
                Return True
            Else
                Dim strMonitorDeviceFailed As String = "The device was not monitored. The Event Class code returned was " + numEventClass3.ToString() + " and the Event Type returned was " + numEventType3.ToString() + "."
                Logger.Write(strMonitorDeviceFailed, Category.Trace.ToString(), 0, 0, TraceEventType.Error)
                Return False
            End If
        End Function

        ' The private method to fire if a CSTA_DELIVERED event type is received
        Private Sub isRinging()
            ' Parse out the data elements in the event buffer...

            ' The remainder of the delivered call structure
            Dim numMonitorCrossRefId As UInt32
            Dim connectionCallId As UInt32
            Dim connectionDeviceId As String
            Dim chConnectionDeviceId As Char()
            Dim connectionDeviceIdType As Integer
            Dim alertingDeviceId As String
            Dim chAlertingDeviceId As Char()
            Dim alertingDeviceIdType As Integer
            Dim alertingDeviceIdStatus As Integer
            Dim callingDeviceId As String
            Dim chCallingDeviceId As Char()
            Dim callingDeviceIdType As Integer
            Dim callingDeviceIdStatus As Integer
            Dim calledDeviceId As String
            Dim chCalledDeviceId As Char()
            Dim calledDeviceIdType As Integer
            Dim calledDeviceIdStatus As Integer
            Dim lastDeviceId As String
            Dim chLastDeviceId As Char()
            Dim lastDeviceIdType As Integer
            Dim lastDeviceIdStatus As Integer
            Dim localConnectionState As Integer
            Dim eventCause As Integer

            ' The cross reference ID
            numMonitorCrossRefId = BitConverter.ToUInt32(eventBuf.data, 8)

            ' The connection ID
            connectionCallId = BitConverter.ToUInt32(eventBuf.data, 12)
            connectionDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 16, 64)
            chConnectionDeviceId = connectionDeviceId.ToCharArray()
            connectionDeviceIdType = BitConverter.ToInt32(eventBuf.data, 80)

            ' Define the active call to be referenced elsewhere
            activeCallId = connectionCallId
            activeDeviceId = chConnectionDeviceId
            activeDeviceIdType = connectionDeviceIdType

            ' The alerting device
            alertingDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 84, 64)
            chAlertingDeviceId = alertingDeviceId.ToCharArray()
            alertingDeviceIdType = BitConverter.ToInt32(eventBuf.data, 148)
            alertingDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 152)

            ' The calling device
            callingDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 156, 64)
            chCallingDeviceId = callingDeviceId.ToCharArray()
            callingDeviceIdType = BitConverter.ToInt32(eventBuf.data, 220)
            callingDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 224)

            ' The called device
            calledDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 228, 64)
            chCalledDeviceId = calledDeviceId.ToCharArray()
            calledDeviceIdType = BitConverter.ToInt32(eventBuf.data, 292)
            calledDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 296)

            ' The last redirection device
            lastDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 300, 64)
            chLastDeviceId = lastDeviceId.ToCharArray()
            lastDeviceIdType = BitConverter.ToInt32(eventBuf.data, 364)
            lastDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 368)

            ' A couple of other state and event describers
            localConnectionState = BitConverter.ToInt32(eventBuf.data, 372)
            eventCause = BitConverter.ToInt32(eventBuf.data, 376)

            activeConnectionCallId = connectionCallId.ToString()
            activeConnectionDeviceId = connectionDeviceId
            activeConnectionDeviceIdType = connectionDeviceIdType.ToString()
            activeCallingDeviceId = callingDeviceId.Trim(Chr(0))

            calledDeviceId = NumericsOnly(calledDeviceId)
            callingDeviceId = NumericsOnly(callingDeviceId)
            connectionDeviceId = NumericsOnly(connectionDeviceId)

            calledDeviceId = Right(calledDeviceId, 4)

            Dim callDetails As New CallArgs()
            callDetails.callid = connectionCallId
            callDetails.cli = callingDeviceId
            callDetails.vdn = connectionDeviceId
            callDetails.extension = connectionDeviceId
            callDetails.parent = _parent

            Dim msg As String = String.Format("CSTA_DELIVERED id={0} cause={1} dest={2} destType={3} cli={4}", _
                    connectionCallId, eventCause, calledDeviceId, connectionDeviceId, callingDeviceId)
            Logger.Write(msg, Category.Trace.ToString(), 0, 0, TraceEventType.Information, "TSAPI")

            If eventCause = 22 Then
                RaiseEvent NewCall(Me, callDetails)
            Else
                RaiseEvent Ringing(Me, callDetails)
            End If
        End Sub

        Function NumericsOnly(ByVal source As String) As String
            Dim sb As New StringBuilder
            For Each ch As Char In source
                If Char.IsNumber(ch) Then
                    sb.Append(ch)
                End If
            Next
            Return sb.ToString()
        End Function


        ' The private method to fire if a CSTA_ESTABLISHED event type is received
        Private Sub isConnected()
            ' Parse out the data elements in the event buffer...

            ' The remainder of the delivered call structure
            Dim monitorCrossRefId As UInt32
            Dim connectionCallId As UInt32
            Dim connectionDeviceId As String
            Dim chConnectionDeviceId As Char()
            Dim connectionDeviceIdType As Integer
            Dim answeringDeviceId As String
            Dim chAnsweringDeviceId As Char()
            Dim answeringDeviceIdType As Integer
            Dim answeringDeviceIdStatus As Integer
            Dim callingDeviceId As String
            Dim chCallingDeviceId As Char()
            Dim callingDeviceIdType As Integer
            Dim callingDeviceIdStatus As Integer
            Dim calledDeviceId As String
            Dim chCalledDeviceId As Char()
            Dim calledDeviceIdType As Integer
            Dim calledDeviceIdStatus As Integer
            Dim lastDeviceId As String
            Dim chLastDeviceId As Char()
            Dim lastDeviceIdType As Integer
            Dim lastDeviceIdStatus As Integer
            Dim localConnectionState As Integer
            Dim eventCause As Integer

            ' The cross reference ID
            monitorCrossRefId = BitConverter.ToUInt32(eventBuf.data, 8)

            ' The connection ID
            connectionCallId = BitConverter.ToUInt32(eventBuf.data, 12)
            connectionDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 16, 64)
            chConnectionDeviceId = connectionDeviceId.ToCharArray()
            connectionDeviceIdType = BitConverter.ToInt32(eventBuf.data, 80)

            ' The answering device
            answeringDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 84, 64)
            chAnsweringDeviceId = answeringDeviceId.ToCharArray()
            answeringDeviceIdType = BitConverter.ToInt32(eventBuf.data, 148)
            answeringDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 152)

            ' The calling device
            callingDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 156, 64)
            chCallingDeviceId = callingDeviceId.ToCharArray()
            callingDeviceIdType = BitConverter.ToInt32(eventBuf.data, 220)
            callingDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 224)

            ' The called device
            calledDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 228, 64)
            chCalledDeviceId = calledDeviceId.ToCharArray()
            calledDeviceIdType = BitConverter.ToInt32(eventBuf.data, 292)
            calledDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 296)

            ' The last redirection device
            lastDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 300, 64)
            chLastDeviceId = lastDeviceId.ToCharArray()
            lastDeviceIdType = BitConverter.ToInt32(eventBuf.data, 364)
            lastDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 368)

            ' A couple of other state and event describers
            localConnectionState = BitConverter.ToInt32(eventBuf.data, 372)
            eventCause = BitConverter.ToInt32(eventBuf.data, 376)

            activeConnectionCallId = connectionCallId.ToString()
            activeConnectionDeviceId = connectionDeviceId
            activeConnectionDeviceIdType = connectionDeviceIdType.ToString()

            calledDeviceId = NumericsOnly(calledDeviceId)
            callingDeviceId = NumericsOnly(callingDeviceId)
            connectionDeviceId = NumericsOnly(connectionDeviceId)

            Dim callDetails As New CallArgs()
            callDetails.callid = connectionCallId
            callDetails.cli = callingDeviceId
            callDetails.vdn = calledDeviceId
            callDetails.extension = connectionDeviceId
            callDetails.parent = _parent

            Dim msg As String = String.Format("CSTA_ESTABLISHED id={0} cli={1} vdn={2} ext={3} cause={4}", _
                            connectionCallId, callingDeviceId, calledDeviceId, connectionDeviceId, eventCause)

            Logger.Write(msg, Category.Trace.ToString(), 0, 0, TraceEventType.Information, "TSAPI")

            If eventCause = 22 Then
                RaiseEvent Connected(Me, callDetails)
            End If

        End Sub

        ' The private method to fire if a CSTA_CONNECTION_CLEARED event type is received
        Private Sub isDisconnected()
            ' Parse out the data elements in the event buffer...

            ' The remainder of the cleared call structure
            Dim numMonitorCrossRefId As UInt32
            Dim connectionCallId As UInt32
            Dim connectionDeviceId As String
            Dim chConnectionDeviceId As Char()
            Dim connectionDeviceIdType As Integer
            Dim releasingDeviceId As String
            Dim chReleasingDeviceId As Char()
            Dim releasingDeviceIdType As Integer
            Dim releasingDeviceIdStatus As Integer
            Dim localConnectionState As Integer
            Dim eventCause As Integer

            ' The cross reference ID
            numMonitorCrossRefId = BitConverter.ToUInt32(eventBuf.data, 8)

            ' The connection ID
            connectionCallId = BitConverter.ToUInt32(eventBuf.data, 12)
            connectionDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 16, 64)
            chConnectionDeviceId = connectionDeviceId.ToCharArray()
            connectionDeviceIdType = BitConverter.ToInt32(eventBuf.data, 80)

            ' The releasing device
            releasingDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 84, 64)
            chReleasingDeviceId = releasingDeviceId.ToCharArray()
            releasingDeviceIdType = BitConverter.ToInt32(eventBuf.data, 148)
            releasingDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 152)

            ' A couple of other state and event describers
            localConnectionState = BitConverter.ToInt32(eventBuf.data, 156)
            eventCause = BitConverter.ToInt32(eventBuf.data, 160)

            releasingDeviceId = NumericsOnly(releasingDeviceId)

            Dim callDetails As New CallArgs()
            callDetails.callid = connectionCallId
            callDetails.cli = releasingDeviceId
            callDetails.vdn = ""
            callDetails.extension = ""
            callDetails.parent = _parent

            Dim msg As String = String.Format("CSTA_CONNECTION_CLEARED id={0} device={1} type={2} type={3} cause={4}", _
                                connectionCallId, releasingDeviceId, releasingDeviceIdType, connectionDeviceIdType, eventCause)
            Logger.Write(msg, Category.Trace.ToString(), 0, 0, TraceEventType.Information, "TSAPI")

            If eventCause = -1 Then
                RaiseEvent Disconnected(Me, callDetails)
            End If

        End Sub

        ' The private method to fire if a CSTA_NETWORK_REACHED event type is received
        Private Sub isDialed()
            ' Parse out the data elements in the event buffer...

            ' The remainder of the network reached call structure
            Dim monitorCrossRefId As UInt32
            Dim connectionCallId As UInt32
            Dim connectionDeviceId As String
            Dim chConnectionDeviceId As Char()
            Dim connectionDeviceIdType As Integer
            Dim trunkDeviceId As String
            Dim chTrunkDeviceId As Char()
            Dim trunkDeviceIdType As Integer
            Dim trunkDeviceIdStatus As Integer
            Dim calledDeviceId As String
            Dim chCalledDeviceId As Char()
            Dim calledDeviceIdType As Integer
            Dim calledDeviceIdStatus As Integer
            Dim localConnectionState As Integer
            Dim eventCause As Integer

            ' The cross reference ID
            monitorCrossRefId = BitConverter.ToUInt32(eventBuf.data, 8)

            ' The connection ID
            connectionCallId = BitConverter.ToUInt32(eventBuf.data, 12)
            connectionDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 16, 64)
            chConnectionDeviceId = connectionDeviceId.ToCharArray()
            connectionDeviceIdType = BitConverter.ToInt32(eventBuf.data, 80)

            ' The trunk ID
            trunkDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 84, 64)
            chTrunkDeviceId = trunkDeviceId.ToCharArray()
            trunkDeviceIdType = BitConverter.ToInt32(eventBuf.data, 148)
            trunkDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 152)

            ' The called device
            calledDeviceId = System.Text.ASCIIEncoding.ASCII.GetString(eventBuf.data, 156, 64)
            chCalledDeviceId = calledDeviceId.ToCharArray()
            calledDeviceIdType = BitConverter.ToInt32(eventBuf.data, 220)
            calledDeviceIdStatus = BitConverter.ToInt32(eventBuf.data, 224)

            ' A couple of other state and event describers
            localConnectionState = BitConverter.ToInt32(eventBuf.data, 228)
            eventCause = BitConverter.ToInt32(eventBuf.data, 232)

            activeConnectionCallId = connectionCallId.ToString()
            activeConnectionDeviceId = connectionDeviceId
            activeConnectionDeviceIdType = connectionDeviceIdType.ToString()

            Dim msg As String = String.Format("CSTA_NETWORK_REACHED id={0} cli={1} vdn={2} ext={3}", connectionCallId, CallingDeviceId, calledDeviceId, connectionDeviceId)
            Logger.Write(msg, Category.Trace.ToString(), 0, 0, TraceEventType.Information, "TSAPI")

        End Sub

        ' The private method to fire if a CSTA_QUERY_MWI_CONF event type is received
        Private Sub mwiStatus()
            ' Parse out the data elements in the event buffer...
            Dim mwiInvokeId As UInt32
            Dim boolMwi As Boolean

            ' Parse the elements in the event buffer
            mwiInvokeId = BitConverter.ToUInt32(eventBuf.data, 8)
            boolMwi = BitConverter.ToBoolean(eventBuf.data, 12)

            m_messagesWaiting = boolMwi
        End Sub

        ' The custom handler for interpreting various event types from TServer
        Private Sub TsHandler(ByVal sender As Object, ByVal e As TServerBufferEventArgs)
            ' Ringing call
            Debug.Print(e.EventClass.ToString())
            If e.EventClass = Csta.CSTAUNSOLICITED AndAlso e.EventType = Csta.CSTA_DELIVERED Then
                isRinging()
            ElseIf e.EventClass = Csta.CSTAUNSOLICITED AndAlso e.EventType = Csta.CSTA_CONNECTION_CLEARED Then
                ' Disconnected call
                isDisconnected()
            ElseIf e.EventClass = Csta.CSTAUNSOLICITED AndAlso e.EventType = Csta.CSTA_ESTABLISHED Then
                ' Connected call
                isConnected()
            ElseIf e.EventClass = Csta.CSTAUNSOLICITED AndAlso e.EventType = Csta.CSTA_NETWORK_REACHED Then
                ' Dialed call 
                isDialed()
            ElseIf e.EventClass = Csta.CSTACONFIRMATION AndAlso e.EventType = Csta.CSTA_QUERY_MWI_CONF Then
                ' Message waiting indicator update
                mwiStatus()
            End If
        End Sub

        ' Define the two TServer event buffer elements of interest
        Public Class TServerBufferEventArgs
            Inherits EventArgs
            Private m_eventClass As Integer, m_eventType As Integer
            Public ReadOnly Property EventClass() As Integer
                Get
                    Return m_eventClass
                End Get
            End Property
            Public ReadOnly Property EventType() As Integer
                Get
                    Return m_eventType
                End Get
            End Property

            Public Sub New(ByVal EventClass As Integer, ByVal EventType As Integer)
                m_eventClass = EventClass
                m_eventType = EventType
            End Sub
        End Class

        ' The public method to place an active call on hold
        Public Sub holdCall(ByVal callId As UInt32, ByVal device As Char(), ByVal deviceType As Integer)
            ' Define the mandatory (unused) private data buffer
            Dim privData As New Csta.PrivateData_t()
            privData.vendor = "MERLIN                          ".ToCharArray()
            privData.length = 4
            privData.data = "N".ToCharArray()

            ' Populate a ConnectionID_t struct with the active call elements

            Dim activeCall As New Csta.ConnectionID_t()
            activeCall.callID = callId
            activeCall.deviceID.device = device
            activeCall.devIDType = DirectCast(deviceType, Csta.ConnectionID_Device_t)

            Dim holdCall As Integer = Csta.cstaHoldCall(acsHandle, numInvokeId, activeCall, False, privData)
        End Sub

        ' The public method to retrieve a held call
        Public Sub retrieveCall(ByVal callId As UInteger, ByVal device As Char(), ByVal deviceType As Integer)
            ' Define the mandatory (unused) private data buffer
            Dim privData As New Csta.PrivateData_t()
            privData.vendor = "MERLIN                          ".ToCharArray()
            privData.length = 4
            privData.data = "N".ToCharArray()

            ' Populate a ConnectionID_t struct with the active call elements


            Dim activeCall As New Csta.ConnectionID_t()
            activeCall.callID = callId
            activeCall.deviceID.device = device
            activeCall.devIDType = DirectCast(deviceType, Csta.ConnectionID_Device_t)

            Dim retrieveCall As Integer = Csta.cstaRetrieveCall(acsHandle, numInvokeId, activeCall, privData)
        End Sub

        ' The public method to pick up an delivered call
        Public Sub answerCall(ByVal callId As UInteger, ByVal device As Char(), ByVal deviceType As Integer)
            ' Define the mandatory (unused) private data buffer
            Dim privData As New Csta.PrivateData_t()
            privData.vendor = "MERLIN                          ".ToCharArray()
            privData.length = 4
            privData.data = "N".ToCharArray()

            ' Populate a ConnectionID_t struct with the active call elements
            Dim activeCall As New Csta.ConnectionID_t()
            activeCall.callID = callId
            activeCall.deviceID.device = device
            activeCall.devIDType = DirectCast(deviceType, Csta.ConnectionID_Device_t)

            Dim answerCall As Integer = Csta.cstaAnswerCall(acsHandle, numInvokeId, activeCall, privData)
        End Sub

        ' The public method to clear an active call
        Public Sub hangupCall(ByVal callId As UInteger, ByVal device As Char(), ByVal deviceType As Integer)
            ' Define the mandatory (unused) private data buffer
            Dim privData As New Csta.PrivateData_t()
            privData.vendor = "MERLIN                          ".ToCharArray()
            privData.length = 4
            privData.data = "N".ToCharArray()

            ' Populate a ConnectionID_t struct with the active call elements
            Dim activeCall As New Csta.ConnectionID_t()
            activeCall.callID = callId
            activeCall.deviceID.device = device
            activeCall.devIDType = DirectCast(deviceType, Csta.ConnectionID_Device_t)

            Dim clearConnection As Integer = Csta.cstaClearConnection(acsHandle, numInvokeId, activeCall, privData)
        End Sub

        ' The public method to initiate an outgoing call
        Public Sub makeCall(ByVal callee As String)
            ' Define the mandatory (unused) private data buffer 
            Dim privData As New Csta.PrivateData_t()
            privData.vendor = "MERLIN                          ".ToCharArray()
            privData.length = 4
            privData.data = "N".ToCharArray()

            Dim calledDevice As Char() = callee.ToCharArray()

            Try
                Dim makeCall As Integer = Csta.cstaMakeCall(acsHandle, numInvokeId, chDevice, calledDevice, privData)
            Catch eMakeCall As System.Exception
                ' If we can't get back a confirmation record the error and inform the user
                'MessageBox.Show("There was a TServer error. Logging the incident and continuing the application.", "TServer Error")
                If Not EventLog.SourceExists(strSource) Then
                    EventLog.CreateEventSource(strSource, strLog)
                End If
                EventLog.WriteEntry(strSource, eMakeCall.Message.ToString(), EventLogEntryType.Warning, 234)
                Return
            End Try
        End Sub

        ' The public method to check the message waiting indicator (MWI) status
        Public Sub checkMwi()
            ' Define the mandatory (unused) private data buffer
            Dim privData As New Csta.PrivateData_t()
            privData.vendor = "MERLIN                          ".ToCharArray()
            privData.length = 4
            privData.data = "N".ToCharArray()

            Dim pollMwi As Integer = Csta.cstaQueryMsgWaitingInd(acsHandle, numInvokeId, chDevice, privData)
        End Sub

        ' Define the generic TServer buffer event handler
        Public Delegate Sub TServerBufferEventHandler(ByVal sender As Object, ByVal e As TServerBufferEventArgs)

        ' Define the event for polling the TServer event buffer
        Public Event TServerBufferPoll As TServerBufferEventHandler

        ' Check the TServer event buffer
        Public Sub checkTServer() Implements ITsapi.checkTServer
            ' Define the mandatory (unused) private data buffer
            Dim privData As New Csta.PrivateData_t()
            privData.vendor = "MERLIN                          ".ToCharArray()
            privData.length = 4
            privData.data = "N".ToCharArray()

            ' Define the event buffer that contains data passed back from TServer
            eventBuf = New Csta.EventBuf_t()
            Dim numEvents As UShort = 0
            Dim eventBufSize As UShort = CUShort(Csta.CSTA_MAX_HEAP)

            ' Poll the event queue to see if any call events are occurring
            Dim polledEvent As Integer
            Try
                polledEvent = Csta.acsGetEventPoll(acsHandle, eventBuf, eventBufSize, privData, numEvents)
            Catch eEventPoll As System.Exception
                ' If we can't get back a confirmation record the error and inform the user
                'MessageBox.Show("There was a TServer error. Logging the incident and continuing the application.", "TServer Error")
                If Not EventLog.SourceExists(strSource) Then
                    EventLog.CreateEventSource(strSource, strLog)
                End If
                EventLog.WriteEntry(strSource, eEventPoll.Message.ToString(), EventLogEntryType.Warning, 234)
                Return
            End Try

            If polledEvent = -8 Then
                Exit Sub
            End If

            ' Parse out the data elements in the event buffer...

            ' The event header
            Dim numAcsHandle As UInt32 = BitConverter.ToUInt32(eventBuf.data, 0)
            Dim numEventClass As UShort = BitConverter.ToUInt16(eventBuf.data, 4)
            Dim numEventType As UShort = BitConverter.ToUInt16(eventBuf.data, 6)

            Dim args As New TServerBufferEventArgs(numEventClass, numEventType)
            RaiseEvent TServerBufferPoll(Me, args)
        End Sub

        ' Define the public properties available for the class
        Public ReadOnly Property ConnectionCallId() As String
            Get
                Return activeConnectionCallId
            End Get
        End Property
        Public ReadOnly Property ConnectionDeviceId() As String
            Get
                Return activeConnectionDeviceId
            End Get
        End Property
        Public ReadOnly Property ConnectionDeviceIdType() As String
            Get
                Return activeConnectionDeviceIdType
            End Get
        End Property
        Public ReadOnly Property CallingDeviceId() As String
            Get
                Return activeCallingDeviceId
            End Get
        End Property
        Public ReadOnly Property MessagesWaiting() As Boolean
            Get
                Return m_messagesWaiting
            End Get
        End Property

    End Class
End Namespace
