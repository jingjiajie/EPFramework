Public Class RangeChangedEventArgs
    Inherits EventArgs

    Public ReadOnly Property NewRange As Range
    Public Sub New(newRange As Range)
        Me.NewRange = newRange
    End Sub
End Class
