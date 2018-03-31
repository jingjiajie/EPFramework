Public Class PositionCellPair
    Public Property Row As Long
    Public Property RowID As Guid
    Public Property ColumnName As String
    Public Property CellData As Object

    Public Sub New(row As Long, rowID As Guid, columnName As String, cellData As Object)
        Me.Row = row
        Me.CellData = cellData
        Me.ColumnName = columnName
        Me.RowID = rowID
    End Sub
End Class
