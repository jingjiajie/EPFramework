Imports System.Threading
Friend Enum LogMode As Integer
    DEFAULT_MODE
    PARSING_METADATA
    INIT_VIEW
    LOAD_MODE_METHODLISTENER
    REFRESH_VIEW
    SYNC_FROM_VIEW
End Enum

Friend Enum LogLevel As Integer
    FATAL_ERROR
    WARNING
    INFOMATION
    DEBUG
End Enum

Friend Class Logger
    Private Shared curErrorConfig As LogConfig = Nothing
    Private Shared dicModeErrorConfig As Dictionary(Of LogMode, LogConfig) = Nothing

    Shared Sub New()
        dicModeErrorConfig = New Dictionary(Of LogMode, LogConfig) From {
            {LogMode.DEFAULT_MODE, New LogConfig()},
            {LogMode.INIT_VIEW, New LogConfig("Initializing view")},
            {LogMode.REFRESH_VIEW, New LogConfig("Refreshing view")},
            {LogMode.SYNC_FROM_VIEW, New LogConfig("Synchronizing data from view")},
            {LogMode.PARSING_METADATA, New LogConfig("Parsing metadata")},
            {LogMode.LOAD_MODE_METHODLISTENER, New LogConfig("Loading mode MethodListener")}
        }
    End Sub

    Public Shared Sub SetMode(mode As LogMode)
        curErrorConfig = dicModeErrorConfig(mode)
    End Sub

    Public Shared Sub PutMessage(message As String, Optional level As LogLevel = LogLevel.FATAL_ERROR)
        Dim levelHint As String = ""
        Select Case level
            Case LogLevel.FATAL_ERROR
                levelHint = "Error"
            Case LogLevel.WARNING
                levelHint = "Warning"
            Case LogLevel.INFOMATION
                levelHint = "Info"
        End Select
        Console.WriteLine("[EPF][" + levelHint + "] " + curErrorConfig.Prefix + ": " + message)
    End Sub

    Public Shared Sub Debug(message As String)
        Console.WriteLine(message)
    End Sub
End Class


Friend Class LogConfig
    Public Property Prefix As String = Nothing

    Public Sub New()
    End Sub

    Public Sub New(prefix As String)
        Me.Prefix = prefix
    End Sub

End Class
