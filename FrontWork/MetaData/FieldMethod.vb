Public Class FieldMethod
    Public Property Parameters As Object() = New Object() {}
    Public Property ReturnValue As Object
    Public Property Func As Action

    Public Function Invoke(ParamArray params As Object()) As Object
        If params IsNot Nothing Then
            Me.Parameters = params
        End If
        Me.ReturnValue = Nothing
        Me.Func?.Invoke()
        Return ReturnValue
    End Function
End Class
