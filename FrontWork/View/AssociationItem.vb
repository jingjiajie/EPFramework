Public Class AssociationItem
    Public Word As String
    Public Hint As String

    Public Overrides Function ToString() As String
        Dim str As String = String.Format("{0,-15}", Word)
        If String.IsNullOrEmpty(Hint) = False Then
            str += " -" & Hint
        End If

        Return str
    End Function
End Class
