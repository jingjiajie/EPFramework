Public Interface IModel
    '数据操作方法
    Function AddRow(data As Dictionary(Of String, Object)) As Long
    Function AddRows(data As Dictionary(Of String, Object)()) As Long()
    Sub InsertRow(row As Long, data As Dictionary(Of String, Object))
    Sub InsertRows(rows As Long(), data As Dictionary(Of String, Object)())
    Sub RemoveRow(row As Long)
    Sub RemoveRow(rowID As Guid)
    Sub RemoveRows(rows As Long())
    Sub RemoveRows(rowIDs As Guid())
    Sub RemoveRows(startRow As Long, rowCount As Long)
    Sub UpdateRow(row As Long, data As Dictionary(Of String, Object))
    Sub UpdateRow(rowID As Guid, data As Dictionary(Of String, Object))
    Sub UpdateRows(rows As Long(), dataOfEachRow As Dictionary(Of String, Object)())
    Sub UpdateRows(rowIDs As Guid(), dataOfEachRow As Dictionary(Of String, Object)())
    Sub UpdateCells(rows As Long(), columnNames As String(), dataOfEachCell As Object())
    Sub UpdateCell(row As Long, columnName As String, data As Object)
    Sub UpdateCells(rowID As Guid(), columnNames As String(), dataOfEachCell As Object())
    Sub UpdateCell(rowID As Guid, columnName As String, data As Object)
    Sub UpdateRowSynchronizationStates(rows As Guid(), syncStates As SynchronizationState())
    Sub UpdateRowSynchronizationStates(rows As Long(), syncStates As SynchronizationState())
    Sub UpdateRowSynchronizationState(row As Guid, syncState As SynchronizationState)
    Sub UpdateRowSynchronizationState(row As Long, syncState As SynchronizationState)
    Function GetRowSynchronizationStates(rows As Guid()) As SynchronizationState()
    Function GetRowSynchronizationStates(rows As Long()) As SynchronizationState()
    Function GetRowSynchronizationState(row As Long) As SynchronizationState
    Function GetRowSynchronizationState(row As Guid) As SynchronizationState
    Function GetRowID(rowNum As Long) As Guid
    Function GetRowIDs(rowNums As Long()) As Guid()
    Function GetRows(rows As Long()) As DataTable
    Function GetRows(rowIDs As Guid()) As DataTable
    Function GetDataTable() As DataTable

    Sub Refresh(dataTable As DataTable, selectionRange As Range(), syncStates As SynchronizationState())

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
    Event RowSynchronizationStateChanged(e As ModelRowSynchronizationStateChangedEventArgs)
End Interface
