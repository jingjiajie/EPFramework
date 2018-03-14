Public Interface IModel
    '数据操作方法
    Function AddRow(data As Dictionary(Of String, Object)) As Long
    Function AddRows(data As Dictionary(Of String, Object)()) As Long
    Sub RemoveRow(rows As Long)
    Sub UpdateRow(row As Long, data As Dictionary(Of String, Object))
    Sub UpdateRows(rows As Long(), dataOfEachRow As Dictionary(Of String, Object)())
    Sub UpdateCells(rows As Long(), columns As Integer(), dataOfEachCell As Object())
    Sub UpdateCell(row As Long, column As Integer, data As Object)
    Function GetRows(rows As Long()) As DataTable
    Function ToDataTable() As DataTable

    '属性相关
    ReadOnly Property RowCount As Long
    ReadOnly Property ColumnCount As Long
    Property SelectionRange As Range()

    '事件
    Event RowAdded(e As ModelRowAddedEventArgs)
    Event RowUpdated(e As ModelRowUpdatedEventArgs)
    Event RowRemoved(e As ModelRowRemovedEventArgs)
    Event CellUpdated(e As ModelCellUpdatedEventArgs)
    Event SelectionRangeChanged(e As ModelSelectionRangeChangedEventArgs)
End Interface
