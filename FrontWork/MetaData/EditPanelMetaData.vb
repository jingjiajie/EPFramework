Imports System.Linq
Imports Jint.Native

Public Class EditPanelMetaData
    Private modeMetaData As New List(Of EditPanelModeMetaData)

    Public Function ContainsMode(mode As String) As Boolean
        Dim foundMetaData = (From m In Me.modeMetaData Where m.Mode = mode Select m).FirstOrDefault
        If foundMetaData IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Set the method Listener for the field metadata
    ''' </summary>
    ''' <param name="methodListener">the method Listener for the mode specified</param>
    ''' <param name="mode">null for all mode</param>
    Public Sub SetMethodListener(methodListener As IMethodListener, Optional mode As String = Nothing)
        Logger.SetMode(LogMode.LOAD_MODE_METHODLISTENER)
        If mode Is Nothing Then
            For Each metaData In Me.modeMetaData
                Call metaData.SetMethodListener(methodListener)
            Next
        Else
            Dim foundModeMetaData = (From m In Me.modeMetaData Where m.Mode = mode Select m).FirstOrDefault
            If foundModeMetaData Is Nothing Then
                Call Logger.PutMessage("mode """ + mode + """ not found!")
                Return
            End If
            Call foundModeMetaData.SetMethodListener(methodListener)
        End If
    End Sub

    Public Sub SetFieldMetaData(mode As String, fieldMetaData As FieldMetaData())
        Dim foundModeMetaData = (From m In modeMetaData Where m.Mode = mode Select m).FirstOrDefault
        If foundModeMetaData Is Nothing Then
            Me.modeMetaData.Add(New EditPanelModeMetaData() With {
                .Mode = mode,
                .Fields = fieldMetaData
            })
        Else
            foundModeMetaData.Fields = fieldMetaData
        End If
    End Sub

    Public Function GetFieldMetaData(mode As String) As FieldMetaData()
        Dim foundModeMetaData = (From m In modeMetaData Where m.Mode = mode Select m).FirstOrDefault
        If foundModeMetaData Is Nothing Then
            Return Nothing
        Else
            Return foundModeMetaData.Fields
        End If
    End Function

    Public Shared Function FromJson(jsEngine As Jint.Engine, jsonStr As String) As EditPanelMetaData
        Logger.SetMode(LogMode.PARSING_METADATA)
        Dim jsValue As JsValue = Nothing
        Try
            jsValue = jsEngine.Execute("$_EPFJsonResult = " + jsonStr).GetValue("$_EPFJsonResult")
        Catch ex As Exception
            Logger.PutMessage("Evaluate json expression failed: " + ex.Message)
            Return Nothing
        End Try
        Return EditPanelMetaData.FromJsValue(jsEngine, jsValue)
    End Function

    Public Shared Function FromJsValue(jsEngine As Jint.Engine, jsValue As JsValue) As EditPanelMetaData
        Dim newEditPanelMetaData As New EditPanelMetaData
        Dim newEditPanelModeMetaData = EditPanelModeMetaData.FromJsValue(jsEngine, jsValue)
        If newEditPanelModeMetaData Is Nothing Then Return Nothing
        newEditPanelMetaData.modeMetaData.AddRange(newEditPanelModeMetaData)
        Return newEditPanelMetaData
    End Function
End Class
