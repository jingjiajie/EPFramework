Public Class PositionCellPair
    Public Property Row As Long
    Public Property Column As Long
    Public Property FieldName As String
    Public Property CellData As Object

    Public Sub New(row As Long, col As Long, fieldName As String, cellData As Object)
        Me.Row = row
        Me.Column = col
        Me.CellData = cellData
        Me.FieldName = fieldName
    End Sub
End Class
