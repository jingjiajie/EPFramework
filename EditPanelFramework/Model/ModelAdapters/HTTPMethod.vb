﻿Public Class HTTPMethod
    Public Shared Property [GET] As New HTTPMethod
    Public Shared Property POST As New HTTPMethod
    Public Shared Property PUT As New HTTPMethod
    Public Shared Property DELETE As New HTTPMethod

    Public Overrides Function ToString() As String
        If Me Is HTTPMethod.GET Then
            Return "GET"
        ElseIf Me Is HTTPMethod.POST Then
            Return "POST"
        ElseIf Me Is HTTPMethod.PUT Then
            Return "PUT"
        ElseIf Me Is HTTPMethod.DELETE Then
            Return "DELETE"
        Else
            Throw New Exception("Unknown HTTPMethod!")
        End If
    End Function

    Public Shared Operator =(method1 As HTTPMethod, method2 As HTTPMethod) As Boolean
        Return method1 Is method2
    End Operator

    Public Shared Operator <>(method1 As HTTPMethod, method2 As HTTPMethod) As Boolean
        Return Not method1 = method2
    End Operator
End Class
