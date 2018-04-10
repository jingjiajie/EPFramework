Public Class IndexRowPair
    Public Property Index As Long
    Public Property RowID As Guid
    Public Property RowData As Dictionary(Of String, Object)

    Public Sub New(index As Long, rowID As Guid, dataRow As Dictionary(Of String, Object))
        Me.Index = index
        Me.RowData = dataRow
        Me.RowID = rowID
    End Sub

    Public Sub New()

    End Sub
End Class
