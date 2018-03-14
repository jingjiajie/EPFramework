Public Class EditPanelModeMetaData
    Public Property Mode As String
    Public Property Fields As FieldMetaData()

    Public Sub SetMethodListener(methodListener As IMethodListener)
        For Each field In Me.Fields
            field.MethodListener = methodListener
        Next
    End Sub

    Public Shared Function FromJsValue(jsEngine As Jint.Engine, jsValue As Jint.Native.JsValue) As EditPanelModeMetaData()
        Logger.SetMode(LogMode.PARSING_METADATA)
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        '如果是数组，则遍历解析
        If jsValue.IsArray Then
            Dim newEditPanelModeMetaDataArray(jsValue.AsArray.GetLength - 1) As EditPanelModeMetaData
            For i As Integer = 0 To jsValue.AsArray.GetLength - 1
                newEditPanelModeMetaDataArray(i) = MakeEditPanelModeMetaDataFromJsValue(jsEngine, jsValue.AsArray.Get(i))
                If newEditPanelModeMetaDataArray(i) Is Nothing Then Return Nothing
            Next
            Return newEditPanelModeMetaDataArray
        ElseIf jsValue.IsObject Then
            '如果是对象，则直接解析
            Dim newEditPanelModeMetaData = MakeEditPanelModeMetaDataFromJsValue(jsEngine, jsValue)
            If newEditPanelModeMetaData Is Nothing Then Return Nothing
            Return New EditPanelModeMetaData() {newEditPanelModeMetaData}
        Else '既不是数组又不是对象，报错返回空
            Logger.PutMessage("Only js object or array is accepted to generate EditPanelModeMetaData!")
            Return Nothing
        End If
    End Function

    Private Shared Function MakeEditPanelModeMetaDataFromJsValue(jsEngine As Jint.Engine, jsValue As Jint.Native.JsValue) As EditPanelModeMetaData
        Logger.SetMode(LogMode.PARSING_METADATA)
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        If Not jsValue.IsObject Then
            Logger.PutMessage("Not a valid JsObject!")
            Return Nothing
        End If

        Dim jsObject = jsValue.AsObject

        '把字段，赋值给EditPanelModeMetaData
        Dim newEditPanelModeMetaData As New EditPanelModeMetaData
        If jsObject.HasOwnProperty("mode") Then
            newEditPanelModeMetaData.Mode = jsObject.GetOwnProperty("mode").Value.ToObject
        Else
            newEditPanelModeMetaData.Mode = "default"
            Logger.PutMessage("""mode"" property not found, set as ""default"" automatically", LogLevel.WARNING)
        End If

        If jsObject.HasOwnProperty("fields") Then
            newEditPanelModeMetaData.Fields = FieldMetaData.FromJsValue(jsEngine, jsObject.GetOwnProperty("fields").Value)
        Else
            Logger.PutMessage("""fields"" property is necessary!")
            Return Nothing
        End If

        Return newEditPanelModeMetaData
    End Function
End Class
