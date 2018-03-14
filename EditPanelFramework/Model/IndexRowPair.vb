Public Class IndexRowPair
    Public Property Index As Long
    Public Property RowData As Dictionary(Of String, Object)

    Public Sub New(index As Long, dataRow As Dictionary(Of String, Object))
        Me.Index = index
        Me.RowData = dataRow
    End Sub
End Class
