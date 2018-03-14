Public Class PositionCellPair
    Public Property Row As Long
    Public Property Column As Long
    Public Property CellData As Object

    Public Sub New(row As Long, col As Long, cellData As Object)
        Me.Row = row
        Me.Column = col
        Me.CellData = cellData
    End Sub
End Class
