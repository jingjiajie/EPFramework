Public Class ModeConfiguration
    Public Property Mode As String
    Public Property Fields As FieldConfiguration()
    Public Property HTTPAPIs As HTTPAPIConfiguration()

    Public Sub SetMethodListener(methodListener As IMethodListener)
        For Each field In Me.Fields
            field.MethodListener = methodListener
        Next
    End Sub

    Public Shared Function FromJsValue(jsEngine As Jint.Engine, jsValue As Jint.Native.JsValue) As ModeConfiguration()
        Logger.SetMode(LogMode.PARSING_Configuration)
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        '如果是数组，则遍历解析
        If jsValue.IsArray Then
            Dim newModeConfigurationArray(jsValue.AsArray.GetLength - 1) As ModeConfiguration
            For i As Integer = 0 To jsValue.AsArray.GetLength - 1
                newModeConfigurationArray(i) = MakeEditPanelModeConfigurationFromJsValue(jsEngine, jsValue.AsArray.Get(i))
                If newModeConfigurationArray(i) Is Nothing Then Return Nothing
            Next
            Return newModeConfigurationArray
        ElseIf jsValue.IsObject Then
            '如果是对象，则直接解析
            Dim newModeConfiguration = MakeEditPanelModeConfigurationFromJsValue(jsEngine, jsValue)
            If newModeConfiguration Is Nothing Then Return Nothing
            Return New ModeConfiguration() {newModeConfiguration}
        Else '既不是数组又不是对象，报错返回空
            Logger.PutMessage("Only js object or array is accepted to generate EditPanelModeConfiguration!")
            Return Nothing
        End If
    End Function

    Private Shared Function MakeEditPanelModeConfigurationFromJsValue(jsEngine As Jint.Engine, jsValue As Jint.Native.JsValue) As ModeConfiguration
        Logger.SetMode(LogMode.PARSING_Configuration)
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        If Not jsValue.IsObject Then
            Logger.PutMessage("Not a valid JsObject!")
            Return Nothing
        End If

        Dim jsObject = jsValue.AsObject

        '把字段，赋值给EditPanelModeConfiguration
        Dim newEditPanelModeConfiguration As New ModeConfiguration
        If jsObject.HasOwnProperty("mode") Then
            newEditPanelModeConfiguration.Mode = jsObject.GetOwnProperty("mode").Value.ToObject
        Else
            newEditPanelModeConfiguration.Mode = "default"
            Logger.PutMessage("""mode"" property not found, set as ""default"" automatically", LogLevel.WARNING)
        End If

        If jsObject.HasOwnProperty("fields") Then
            newEditPanelModeConfiguration.Fields = FieldConfiguration.FromJsValue(jsEngine, jsObject.GetOwnProperty("fields").Value)
        Else
            Logger.PutMessage("""fields"" property is necessary!")
            Return Nothing
        End If

        If jsObject.HasOwnProperty("httpAPIs") Then
            newEditPanelModeConfiguration.HTTPAPIs = HTTPAPIConfiguration.FromJSValue(jsEngine, jsObject.GetOwnProperty("httpAPIs").Value)
        End If

        Return newEditPanelModeConfiguration
    End Function
End Class
