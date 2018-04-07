Public Interface IModel
    '数据操作方法
    Function AddRow(data As Dictionary(Of String, Object)) As Long
    Function AddRows(data As Dictionary(Of String, Object)()) As Long()
    Sub InsertRow(row As Long, data As Dictionary(Of String, Object))
    Sub InsertRows(rows As Long(), data As Dictionary(Of String, Object)())
    Sub RemoveRow(row As Long)
    Sub RemoveRows(rows As Long())
    Sub RemoveRows(startRow As Long, rowCount As Long)
    Sub UpdateRow(row As Long, data As Dictionary(Of String, Object))
    Sub UpdateRows(rows As Long(), dataOfEachRow As Dictionary(Of String, Object)())
    Sub UpdateCells(rows As Long(), columnNames As String(), dataOfEachCell As Object())
    Sub UpdateCell(row As Long, columnName As String, data As Object)
    Function GetRows(rows As Long()) As DataTable
    Function GetDataTable() As DataTable

    Sub Refresh(dataTable As DataTable, selectionRange As Range())

    '属性相关
    ReadOnly Property RowCount As Long
    ReadOnly Property ColumnCount As Long
    Property SelectionRange As Range()
    Property FirstSelectionRange As Range

    '事件
    Event Refreshed(e As ModelRefreshedEventArgs)
    Event RowAdded(e As ModelRowAddedEventArgs)
    Event RowUpdated(e As ModelRowUpdatedEventArgs)
    Event RowRemoved(e As ModelRowRemovedEventArgs)
    Event CellUpdated(e As ModelCellUpdatedEventArgs)
    Event SelectionRangeChanged(e As ModelSelectionRangeChangedEventArgs)
End Interface
