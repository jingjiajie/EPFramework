﻿Public Class FrontWorkException
    Inherits Exception

    Public Sub New(message As String)
        Call MyBase.New(message)
    End Sub

End Class
