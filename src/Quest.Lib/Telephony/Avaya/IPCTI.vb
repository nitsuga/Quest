Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Diba.Tcpip
Imports System.Windows.Forms
Imports System.Diagnostics
Imports Microsoft.Practices.EnterpriseLibrary.Logging
Imports Microsoft.Practices.EnterpriseLibrary.Logging.ExtraInformation
Imports Microsoft.Practices.EnterpriseLibrary.Logging.Filters
Imports Microsoft.Practices.EnterpriseLibrary.ExceptionHandling


Namespace CTI

    ''' <summary>
    ''' This class allows the user to generate events similar to the TSAPI layer. use TELNET localhost 5023 to connect
    ''' to thi stest server
    ''' </summary>                
    ''' <remarks></remarks>
    Public Class IPCTI
        Implements ITsapi

        Private _parent As Object
        Private WithEvents _listener As Diba.Tcpip.TcpipListener
        Private _buffer As String = ""
        Private _lastbuffer As New Generic.List(Of String)
        Private _index As Integer = 0

        Public Event NewCall As System.EventHandler(Of CallArgs) Implements ITsapi.NewCall
        Public Event Ringing As System.EventHandler(Of CallArgs) Implements ITsapi.Ringing
        Public Event Connected As System.EventHandler(Of CallArgs) Implements ITsapi.Connected
        Public Event Disconnected As System.EventHandler(Of CallArgs) Implements ITsapi.Disconnected

        Public Sub New(ByVal parent As Object)
            _parent = parent
        End Sub

        ' The public method to open the ACS stream
        Public Function open(ByVal loginId As String, ByVal passwd As String, ByVal serverId As String) As Boolean Implements ITsapi.open
            _listener = New TcpipListener()
            _listener.StartListening(GetType(RawStreamCodec), 5023)
        End Function

        ' The public method to monitor the provided extension
        Public Function monitor(ByVal device As String) As Boolean Implements ITsapi.monitor
            ' no action
        End Function

        ' Check the TServer event buffer
        Public Sub checkTServer() Implements ITsapi.checkTServer
        End Sub

        Private Sub _listener_Connected(ByVal sender As Object, ByVal e As Diba.Tcpip.ConnectedEventArgs) Handles _listener.Connected
#If VERBOSE Then
            e.RemoteTcpipConnection.Send("Welcome to the Telephone simulator" + vbCrLf)
            e.RemoteTcpipConnection.Send("type 'help' for help" + vbCrLf)
            e.RemoteTcpipConnection.Send(">")
#End If
        End Sub

        Private Sub _listener_Data(ByVal sender As Object, ByVal e As Diba.Tcpip.DataEventArgs) Handles _listener.Data
            For Each ch As Char In e.Message.ToCharArray()
                ProcessCharacter(ch, e.remoteTcpipConnection)
            Next
        End Sub

        Private Enum CHARMODE
            Normal
            Escape1
            Escape2
            EOL
        End Enum

        Private InputMode As CHARMODE = CHARMODE.Normal

        Private Sub ProcessCharacter(ByVal ch As Char, ByVal remoteTcpipConnection As RemoteTcpipConnection)
            Dim cmdok As Boolean

            Try

                Select Case InputMode
                    Case CHARMODE.Normal

                        Select Case ch
                            Case Chr(27)
                                InputMode = CHARMODE.Escape1
                            Case Chr(8)
                                If _buffer.Length > 0 Then
                                    _buffer = _buffer.Substring(0, _buffer.Length - 1)
#If VERBOSE Then
                                    remoteTcpipConnection.Send(" " + ch)
#End If
                                    Return
                                End If
                            Case vbCr.Chars(0)

                            Case vbLf.Chars(0)
                                InputMode = CHARMODE.EOL
                            Case Else
                                _buffer += ch
                        End Select
                    Case CHARMODE.Escape1
                        Select Case ch
                            Case "[".Chars(0)
                                InputMode = CHARMODE.Escape2
                            Case Else
                                InputMode = CHARMODE.Normal
                        End Select

                    Case CHARMODE.Escape2
                        Select Case ch
                            Case "A".Chars(0)
                                If _index = 0 Then _index = _lastbuffer.Count - 1
                                If _index <> -1 Then
                                    _buffer = _lastbuffer(_index)
#If VERBOSE Then
                                    remoteTcpipConnection.Send(vbCr + "                                                      " + vbCr + ">" + _buffer)
#End If
                                End If
                            Case "B".Chars(0)
                                If _lastbuffer.Count > 0 Then
                                    _index += 1
                                    If _index > _lastbuffer.Count - 1 Then
                                        _index = 0
                                    End If
                                    _buffer = _lastbuffer(_index)
#If VERBOSE Then
                                    remoteTcpipConnection.Send(vbCr + "                                                      " + vbCr + ">" + _buffer)
#End If
                                End If
                            Case Else
                                InputMode = CHARMODE.Normal
                        End Select
                End Select

                If InputMode <> CHARMODE.EOL Then Return

                InputMode = CHARMODE.Normal

                Dim parts() As String = _buffer.Split(CChar(" "))
                Logger.Write("TSAPI command: """ + _buffer + """", Category.Trace.ToString(), 0, 0, TraceEventType.Information, "IPCTI")

                cmdok = False
                Select Case parts(0).ToLower()
                    Case "help"
#If VERBOSE Then
                        remoteTcpipConnection.Send("use the following commands:" + vbCrLf)
                        remoteTcpipConnection.Send("new {id} {cli} {vdn}" + vbCrLf)
                        remoteTcpipConnection.Send("ring {id} {extension}" + vbCrLf)
                        remoteTcpipConnection.Send("connect {id} {extension}" + vbCrLf)
                        remoteTcpipConnection.Send("disconnect {id}" + vbCrLf)
                        remoteTcpipConnection.Send("delay {seconds}" + vbCrLf)
#End If
                        cmdok = True
                    Case "delay"
                        If parts.Length <> 2 Then
#If VERBOSE Then
                            remoteTcpipConnection.Send("incorrect syntax, use: delay {seconds}" + vbCrLf)
#End If
                        Else
                            Threading.Thread.Sleep(CInt(parts(1)) * 1000)
                            cmdok = True
                        End If
                    Case "new"
                        If parts.Length <> 4 Then
#If VERBOSE Then
                            remoteTcpipConnection.Send("incorrect syntax, use: new {id} {cli} {vdn}" + vbCrLf)
#End If
                        Else
                            Dim callDetails As New CallArgs()
                            callDetails.callid = CUInt(parts(1))
                            callDetails.cli = parts(2)
                            callDetails.vdn = parts(3)
                            callDetails.parent = _parent
                            RaiseEvent NewCall(_parent, callDetails)
                            cmdok = True
                        End If

                    Case "ring"
                        If parts.Length <> 3 Then
#If VERBOSE Then
                            remoteTcpipConnection.Send("incorrect syntax, use: ring {id} {extension}" + vbCrLf)
#End If
                        Else
                            Dim callDetails As New CallArgs()
                            callDetails.callid = CUInt(parts(1))
                            callDetails.extension = parts(2)
                            callDetails.parent = _parent
                            RaiseEvent Ringing(_parent, callDetails)
                            cmdok = True
                        End If

                    Case "connect"
                        If parts.Length <> 3 Then
#If VERBOSE Then
                            remoteTcpipConnection.Send("incorrect syntax, use: connect {id} {extension}" + vbCrLf)
#End If
                        Else
                            Dim callDetails As New CallArgs()
                            callDetails.callid = CUInt(parts(1))
                            callDetails.extension = parts(2)
                            callDetails.parent = _parent
                            RaiseEvent Connected(_parent, callDetails)
                            cmdok = True
                        End If

                    Case "disconnect"

                        If parts.Length <> 2 Then
#If VERBOSE Then
                            remoteTcpipConnection.Send("incorrect syntax, use: disconnect {id}" + vbCrLf)
#End If
                        Else
                            Dim callDetails As New CallArgs()
                            Dim id As Integer
                            Int32.TryParse(parts(1), id)
                            callDetails.callid = CUInt(id)
                            callDetails.parent = _parent
                            RaiseEvent Disconnected(_parent, callDetails)
                            cmdok = True
                        End If

                    Case "quit"
                        remoteTcpipConnection.Socket.Close()

                End Select

#If VERBOSE Then
                remoteTcpipConnection.Send(vbCrLf + ">")
#End If

                '** parse for simulation commands
                If cmdok Then
                    _lastbuffer.Add(_buffer)
                    _index = 0
                End If
                _buffer = ""


            Catch ex As Exception
                If (True = ExceptionPolicy.HandleException(ex, Policy.TracePolicy.ToString())) Then
                    Throw
                End If
            End Try

        End Sub
    End Class
End Namespace
