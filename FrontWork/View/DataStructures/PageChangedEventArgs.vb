Public Class PageChangedEventArgs
    Public Property CurrentPage As Long

    Public Sub New(currentPage As Long)
        Me.CurrentPage = currentPage
    End Sub

    Public Sub New()

    End Sub
End Class
