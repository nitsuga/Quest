Namespace CTI

    Public Interface ITsapi

        Event NewCall As System.EventHandler(Of CallArgs)
        Event Ringing As System.EventHandler(Of CallArgs)
        Event Connected As System.EventHandler(Of CallArgs)
        Event Disconnected As System.EventHandler(Of CallArgs)

        Function open(ByVal loginId As String, ByVal passwd As String, ByVal serverId As String) As Boolean
        Function monitor(ByVal device As String) As Boolean
        Sub checkTServer()

    End Interface

End Namespace
