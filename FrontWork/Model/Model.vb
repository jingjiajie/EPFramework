Imports System.Linq
Imports FrontWork

Public Class Model
    Implements IModel

    Private _selectionRange As Range() = New Range() {}
    Private _mode As String
    Private _metaData As EditPanelMetaData
    Private _dicRowGuid As New Dictionary(Of DataRow, Guid)
    Private _dicRowSyncState As New Dictionary(Of DataRow, SynchronizationState)

    Public Property Mode As String
        Get
            Return Me._mode
        End Get
        Set(value As String)
            Me._mode = value
            If Me._metaData IsNot Nothing Then
                Call Me.RefreshDataTable()
            End If
        End Set
    End Property
    Public Property MetaData As EditPanelMetaData
        Get
            Return Me._metaData
        End Get
        Set(value As EditPanelMetaData)
            Me._metaData = value
            If Me._mode IsNot Nothing Then
                Call Me.RefreshDataTable()
            End If
        End Set
    End Property
    Private Property Data As New DataTable

    Public Sub New(metaData As String, Optional mode As String = "default")
        Me.MetaData = EditPanelMetaData.FromJson(New Jint.Engine, metaData)
        If Me.MetaData Is Nothing Then
            Throw New Exception("Invalid metaData")
        End If
        Me.Mode = mode
    End Sub

    Public ReadOnly Property RowCount As Long Implements IModel.RowCount
        Get
            Return Me.Data.Rows.Count
        End Get
    End Property

    Public ReadOnly Property ColumnCount As Long Implements IModel.ColumnCount
        Get
            Return Me.Data.Columns.Count
        End Get
    End Property

    Public Property SelectionRange As Range() Implements IModel.SelectionRange
        Get
            Return Me._selectionRange
        End Get
        Set(value As Range())
            Me._selectionRange = value
            For Each range In value
                Me.BindRangeChangedEventToSelectionRangeChangedEvent(range)
            Next
            RaiseEvent SelectionRangeChanged(New ModelSelectionRangeChangedEventArgs() With {
                                         .NewSelectionRange = value
                                        })
        End Set
    End Property

    Public Property FirstSelectionRange As Range Implements IModel.FirstSelectionRange
        Get
            If Me.SelectionRange Is Nothing Then Return Nothing
            If Me.SelectionRange.Length = 0 Then Return Nothing
            Return Me.SelectionRange(0)
        End Get
        Set(value As Range)
            If Me.SelectionRange Is Nothing Then
                Me.SelectionRange = {value}
            ElseIf Me.SelectionRange.Length = 0 Then
                Me.SelectionRange = {value}
            Else
                Me.SelectionRange(0) = value
            End If
        End Set
    End Property

    Public Property SelectionRange(i) As Range
        Get
            Return Me._selectionRange(i)
        End Get
        Set(value As Range)
            Me._selectionRange(i) = value
            Me.BindRangeChangedEventToSelectionRangeChangedEvent(value)
            RaiseEvent SelectionRangeChanged(New ModelSelectionRangeChangedEventArgs() With {
                                         .NewSelectionRange = Me._selectionRange
                                        })
        End Set
    End Property

    Private Sub RefreshDataTable()
        If Me.MetaData Is Nothing Then Return
        If Me.Mode Is Nothing Then Return

        Call Me.Data.Columns.Clear()
        Dim fieldMetaData = Me.MetaData.GetFieldMetaData(Me.Mode)
        For Each curField In fieldMetaData
            Me.Data.Columns.Add(curField.Name)
        Next
    End Sub

    Private Sub BindRangeChangedEventToSelectionRangeChangedEvent(range As Range)
        AddHandler range.RangeChanged, Sub()
                                           RaiseEvent SelectionRangeChanged(New ModelSelectionRangeChangedEventArgs() With {
                                                                       .NewSelectionRange = Me.SelectionRange
                                                                   })
                                       End Sub
    End Sub

    Public Function GetDataTable() As DataTable Implements IModel.GetDataTable
        GetDataTable = Me.Data
        Exit Function
    End Function

    Public Function GetRows(rowIDs As Guid()) As DataTable Implements IModel.GetRows
        Dim rowNums(rowIDs.Length - 1) As Long
        For i = 0 To rowIDs.Length - 1
            Dim rowID = rowIDs(i)
            Dim rowNum = Me.GetRowNum(rowID)
            If rowNum = -1 Then
                Throw New Exception($"Invalid RowID: {rowID}")
            End If
            rowNums(i) = rowNum
        Next
        Return Me.GetRows(rowNums)
    End Function

    Public Function GetRows(rows As Long()) As DataTable Implements IModel.GetRows
        Dim dataTable = Me.Data.Clone
        Try
            For Each row In rows
                Dim newRow = dataTable.NewRow
                newRow.ItemArray = Me.Data.Rows(row).ItemArray
                dataTable.Rows.Add(newRow)
            Next
        Catch ex As Exception
            Throw New Exception("GetRows failed: " & ex.Message)
        End Try

        Return dataTable
    End Function

    Public Function AddRow(data As Dictionary(Of String, Object)) As Long Implements IModel.AddRow
        Return Me.AddRows({data})(0)
    End Function

    Public Function AddRows(dataOfEachRow As Dictionary(Of String, Object)()) As Long() Implements IModel.AddRows
        Dim addRowCount = dataOfEachRow.Length
        Dim oriRowCount = Me.Data.Rows.Count
        Dim insertRows = Util.Range(RowCount, RowCount + addRowCount)
        Call Me.InsertRows(insertRows, dataOfEachRow)
        Return insertRows
    End Function

    Public Sub InsertRow(row As Long, data As Dictionary(Of String, Object)) Implements IModel.InsertRow
        Call Me.InsertRows({row}, {data})
    End Sub

    Public Sub InsertRows(rows As Long(), dataOfEachRow As Dictionary(Of String, Object)()) Implements IModel.InsertRows
        Dim indexRowPairs As New List(Of IndexRowPair)
        '原始行每次插入之后，行号会变，所以做调整
        Dim realRowsASC = (From r In rows Order By r Ascending Select r).ToArray
        For i = 0 To realRowsASC.Length - 1
            realRowsASC(i) = realRowsASC(i) + i
        Next
        For i = 0 To realRowsASC.Length - 1
            Dim realRow = realRowsASC(i)
            Dim curData = dataOfEachRow(i)
            Dim newRow = Me.Data.NewRow
            If curData IsNot Nothing Then
                For Each item In curData
                    newRow(item.Key) = item.Value
                Next
            End If
            Me.Data.Rows.InsertAt(newRow, realRow)
            Dim newIndexRowPair As New IndexRowPair(realRow, Me.GetRowID(Me.Data.Rows(realRow)), If(curData, New Dictionary(Of String, Object)))
            indexRowPairs.Add(newIndexRowPair)
        Next

        RaiseEvent RowAdded(New ModelRowAddedEventArgs() With {
                             .AddedRows = indexRowPairs.ToArray
                            })

        Dim columnCount = Me.Data.Columns.Count
        Dim selectionRanges As New List(Of Range)
        For Each curRow In realRowsASC
            If selectionRanges.Count = 0 OrElse selectionRanges.Last.Row + 1 <> curRow Then
                selectionRanges.Add(New Range(curRow, 0, 1, columnCount))
                Continue For
            End If
            If selectionRanges.Last.Row + 1 = curRow Then
                selectionRanges.Last.Rows += 1
            End If
        Next
        Me.SelectionRange = selectionRanges.ToArray

        Me.UpdateRowSynchronizationStates(realRowsASC, Util.Times(SynchronizationState.UNSYNCHRONIZED, realRowsASC.Length))
    End Sub

    Public Sub RemoveRow(rowID As Guid) Implements IModel.RemoveRow
        Me.RemoveRows({rowID})
    End Sub

    Public Sub RemoveRow(row As Long) Implements IModel.RemoveRow
        Me.RemoveRows({row})
    End Sub

    Public Sub RemoveRows(startRow As Long, rowCount As Long) Implements IModel.RemoveRows
        Me.RemoveRows(Util.Range(startRow, startRow + rowCount))
    End Sub

    Public Sub RemoveRows(rowIDs As Guid()) Implements IModel.RemoveRows
        Dim rowNums(rowIDs.Length - 1) As Long
        For i = 0 To rowIDs.Length - 1
            Dim rowID = rowIDs(i)
            Dim rowNum = Me.GetRowNum(rowID)
            If rowNum = -1 Then
                Throw New Exception($"Invalid RowID: {rowID}")
            End If
            rowNums(i) = rowNum
        Next
        Me.RemoveRows(rowNums)
    End Sub

    Public Sub RemoveRows(rows As Long()) Implements IModel.RemoveRows
        Try
            Dim indexRowList = New List(Of IndexRowPair)
            '每次删除行后行号会变，所以要做调整
            Dim realRows(rows.Length - 1) As Long
            For i = 0 To rows.Length - 1
                realRows(i) = rows(i) - i
            Next
            For Each curRowNum In realRows
                Dim newIndexRowPair = New IndexRowPair(curRowNum, Me.GetRowID(Me.Data.Rows(curRowNum)), Me.DataRowToDictionary(Me.Data.Rows(curRowNum)))
                indexRowList.Add(newIndexRowPair)
                Me.Data.Rows.RemoveAt(curRowNum)
            Next

            RaiseEvent RowRemoved(New ModelRowRemovedEventArgs() With {
                                        .RemovedRows = indexRowList.ToArray
                                   })
            If Me.Data.Rows.Count = 0 Then
                Me.SelectionRange = {}
            Else
                Me.SelectionRange = {New Range(Math.Min(rows.Min, Me.Data.Rows.Count - 1), 0, 1, Me.Data.Columns.Count)}
            End If
        Catch ex As Exception
            Throw New Exception("RemoveRows failed: " & ex.Message)
        End Try
    End Sub

    Public Sub UpdateRow(rowID As Guid, data As Dictionary(Of String, Object)) Implements IModel.UpdateRow
        Me.UpdateRows({rowID}, {data})
    End Sub

    Public Sub UpdateRow(row As Long, data As Dictionary(Of String, Object)) Implements IModel.UpdateRow
        Call Me.UpdateRows(
            New Long() {row},
            New Dictionary(Of String, Object)() {data}
        )
    End Sub

    Public Sub UpdateRows(rowIDs As Guid(), dataOfEachRow As Dictionary(Of String, Object)()) Implements IModel.UpdateRows
        Dim rowNums(rowIDs.Length - 1) As Long
        For i = 0 To rowIDs.Length - 1
            Dim rowID = rowIDs(i)
            Dim rowNum = Me.GetRowNum(rowID)
            If rowNum = -1 Then
                Throw New Exception($"Invalid RowID: {rowID}")
            End If
            rowNums(i) = rowNum
        Next
        Me.UpdateRows(rowNums, dataOfEachRow)
    End Sub

    Public Sub UpdateRows(rows As Long(), dataOfEachRow As Dictionary(Of String, Object)()) Implements IModel.UpdateRows
        Try
            Dim i = 0
            For Each row In rows
                For Each item In dataOfEachRow(i)
                    Dim key = item.Key
                    Dim value = item.Value
                    Me.Data.Rows(row)(key) = value
                Next
                i += 1
            Next

            Dim updatedRows(rows.Length - 1) As IndexRowPair
            For i = 0 To rows.Length - 1
                updatedRows(i) = New IndexRowPair(rows(i), Me.GetRowID(Me.Data.Rows(rows(i))), Me.DataRowToDictionary(Me.Data.Rows(rows(i))))
            Next

            Dim tmp = New ModelRowUpdatedEventArgs() With {
                                        .UpdatedRows = updatedRows
                                   }

            RaiseEvent RowUpdated(tmp)
            '将被更新的行的同步状态修改为未同步
            Me.UpdateRowSynchronizationStates(rows, Util.Times(SynchronizationState.UNSYNCHRONIZED, rows.Length))

        Catch ex As Exception
            Throw New Exception("UpdateRows failed: " & ex.Message)
        End Try
    End Sub

    Public Sub UpdateCell(row As Guid, columnName As String, data As Object) Implements IModel.UpdateCell
        Me.UpdateCells({row}, {columnName}, {data})
    End Sub

    Public Sub UpdateCell(row As Long, columnName As String, data As Object) Implements IModel.UpdateCell
        Me.UpdateCells(New Long() {row}, New String() {columnName}, New Object() {data})
    End Sub

    Public Sub UpdateCells(rowIDs As Guid(), columnNames As String(), dataOfEachCell As Object()) Implements IModel.UpdateCells
        Dim rowNums(rowIDs.Length - 1) As Long
        For i = 0 To rowIDs.Length - 1
            Dim rowID = rowIDs(i)
            Dim rowNum = Me.GetRowNum(rowID)
            If rowNum = -1 Then
                Throw New Exception($"Invalid RowID: {rowID}")
            End If
            rowNums(i) = rowNum
        Next
        Me.UpdateCells(rowNums, columnNames, dataOfEachCell)
    End Sub

    Public Sub UpdateCells(rows As Long(), columnNames As String(), dataOfEachCell As Object()) Implements IModel.UpdateCells
        Dim posCellPairs As New List(Of PositionCellPair)
        For i = 0 To rows.Length - 1
            Dim columnName = columnNames(i)
            Dim dataColumn = (From col As DataColumn In Me.Data.Columns
                              Where col.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                              Select col).FirstOrDefault
            If dataColumn Is Nothing Then
                Throw New Exception("UpdateCells failed: column """ & columnName & """ not found in model")
            End If
            Me.Data.Rows(rows(i))(dataColumn) = dataOfEachCell(i)
            posCellPairs.Add(New PositionCellPair(rows(i), Me.GetRowID(Me.Data.Rows(rows(i))), columnName, dataOfEachCell(i)))
        Next

        RaiseEvent CellUpdated(New ModelCellUpdatedEventArgs() With {
                                    .UpdatedCells = posCellPairs.ToArray
                               })
        Me.UpdateRowSynchronizationStates(rows, Util.Times(SynchronizationState.UNSYNCHRONIZED, rows.Length))
    End Sub

    Public Sub Refresh(dataTable As DataTable, ranges As Range(), syncStates As SynchronizationState()) Implements IModel.Refresh
        '刷新选区
        If ranges Is Nothing Then ranges = {}
        Me._selectionRange = ranges
        For Each range In Me._selectionRange
            Me.BindRangeChangedEventToSelectionRangeChangedEvent(range)
        Next
        '刷新数据
        Me._Data = dataTable
        '刷新同步状态字典
        Call Me._dicRowSyncState.Clear()
        For i = 0 To syncStates.Length - 1
            If dataTable.Rows.Count <= i Then
                Throw New Exception("Length of syncStates exceeded the max row of dataTable")
            End If
            Dim row = dataTable.Rows(i)
            Dim syncState = syncStates(i)
            Me._dicRowSyncState.Add(row, syncState)
        Next
        '触发刷新事件
        RaiseEvent Refreshed(New ModelRefreshedEventArgs)
    End Sub

    Protected Function DataRowToDictionary(dataRow As DataRow) As Dictionary(Of String, Object)
        Dim result As New Dictionary(Of String, Object)
        Dim columns = dataRow.Table.Columns
        For Each column As DataColumn In columns
            result.Add(column.ColumnName, dataRow(column))
        Next
        Return result
    End Function

    Private Function GetRowID(row As DataRow) As Guid
        If Not Me._dicRowGuid.ContainsKey(row) Then
            Me._dicRowGuid.Add(row, Guid.NewGuid)
        End If
        Return Me._dicRowGuid(row)
    End Function

    Public Function GetRowID(rowNum As Long) As Guid Implements IModel.GetRowID
        Return Me.GetRowIDs({rowNum})(0)
    End Function

    Public Function GetRowIDs(rowNums As Long()) As Guid() Implements IModel.GetRowIDs
        Dim dataRows(rowNums.Length - 1) As DataRow
        Dim rowIDs(rowNums.Length - 1) As Guid
        For i = 0 To rowNums.Length - 1
            Dim rowNum = rowNums(i)
            If Me.Data.Rows.Count <= rowNum Then
                Throw New Exception($"Row {rowNum} exceeded the max row of model")
            End If
            Dim dataRow = Me.Data.Rows(rowNum)
            rowIDs(i) = Me.GetRowID(dataRow)
        Next
        Return rowIDs
    End Function

    Protected Function GetRowNum(rowID As Guid) As Long
        Dim dataRow = (From rg In Me._dicRowGuid Where rg.Value = rowID Select rg.Key).FirstOrDefault
        If dataRow Is Nothing Then Return -1
        Return Me.Data.Rows.IndexOf(dataRow)
    End Function

    Protected Function GetDataRow(rowID As Guid) As DataRow
        Return (From rowGuid In Me._dicRowGuid Where rowGuid.Value = rowID Select rowGuid.Key).FirstOrDefault
    End Function

    Public Sub UpdateRowSynchronizationStates(rowIDs As Guid(), syncStates As SynchronizationState()) Implements IModel.UpdateRowSynchronizationStates
        If rowIDs.Length <> syncStates.Length Then
            Throw New Exception("Length of rows must be same of the length of syncStates")
        End If
        Dim rowNums(rowIDs.Length - 1) As Long
        For i = 0 To rowIDs.Length - 1
            Dim rowID = rowIDs(i)
            Dim rowNum = Me.GetRowNum(rowID)
            If rowNum < 0 Then
                Throw New Exception($"Row ID:{rowID} not found!")
            End If
            rowNums(i) = rowNum
        Next
        Call Me.UpdateRowSynchronizationStates(rowNums, syncStates)
    End Sub

    Public Sub UpdateRowSynchronizationStates(rows As Long(), syncStates As SynchronizationState()) Implements IModel.UpdateRowSynchronizationStates
        If rows.Length <> syncStates.Length Then
            Throw New Exception("Length of rows must be same of the length of syncStates")
        End If
        Dim updatedRows = New List(Of IndexRowSynchronizationStatePair)
        For i = 0 To rows.Length - 1
            Dim row = rows(i)
            Dim syncState = syncStates(i)
            If row >= Me.Data.Rows.Count Then
                Throw New Exception($"Row {row} exceeded the max row of model")
            End If
            Me.SetRowSynchronizationState(Me.Data.Rows(row), syncState)
            Dim newIndexRowSynchronizationStatePair = New IndexRowSynchronizationStatePair(row, Me.GetRowID(row), syncState)
            updatedRows.Add(newIndexRowSynchronizationStatePair)
        Next
        Dim eventArgs = New ModelRowSynchronizationStateChangedEventArgs(updatedRows.ToArray)
        RaiseEvent RowSynchronizationStateChanged(eventArgs)
    End Sub

    Public Sub UpdateRowSynchronizationState(row As Long, syncState As SynchronizationState) Implements IModel.UpdateRowSynchronizationState
        Call Me.UpdateRowSynchronizationStates({row}, {syncState})
    End Sub

    Public Sub UpdateRowSynchronizationState(rowID As Guid, syncState As SynchronizationState) Implements IModel.UpdateRowSynchronizationState
        Call Me.UpdateRowSynchronizationStates({rowID}, {syncState})
    End Sub

    Private Sub SetRowSynchronizationState(row As DataRow, state As SynchronizationState)
        If Me._dicRowSyncState.ContainsKey(row) Then
            Me._dicRowSyncState(row) = state
        Else
            Me._dicRowSyncState.Add(row, state)
        End If
    End Sub

    Private Function GetRowSynchronizationState(row As DataRow) As SynchronizationState
        If Not Me._dicRowSyncState.ContainsKey(row) Then
            Me._dicRowSyncState.Add(row, SynchronizationState.UNSYNCHRONIZED)
        End If
        Return Me._dicRowSyncState(row)
    End Function

    Public Function GetRowSynchronizationStates(rows As Long()) As SynchronizationState() Implements IModel.GetRowSynchronizationStates
        Dim states(rows.Length - 1) As SynchronizationState
        For i = 0 To rows.Length - 1
            Dim rowNum = rows(i)
            If Me.Data.Rows.Count <= rowNum Then
                Throw New Exception($"Row {rowNum} exceeded max row of model!")
            End If
            Dim dataRow = Me.Data.Rows(rowNum)
            states(i) = Me.GetRowSynchronizationState(dataRow)
        Next
        Return states
    End Function

    Public Function GetRowSynchronizationStates(rowIDs As Guid()) As SynchronizationState() Implements IModel.GetRowSynchronizationStates
        Dim rows(rowIDs.Length - 1) As Long
        For i = 0 To rowIDs.Length - 1
            Dim row = Me.GetRowNum(rowIDs(i))
            If row < 0 Then
                Throw New Exception($"Row ID:{rowIDs(i)} not found!")
            End If
            rows(i) = row
        Next
        Return Me.GetRowSynchronizationStates(rows)
    End Function

    Public Function GetRowSynchronizationState(row As Long) As SynchronizationState Implements IModel.GetRowSynchronizationState
        Return Me.GetRowSynchronizationStates({row})(0)
    End Function

    Public Function GetRowSynchronizationStates(rowID As Guid) As SynchronizationState Implements IModel.GetRowSynchronizationState
        Return Me.GetRowSynchronizationStates({rowID})(0)
    End Function

    Public Event Refreshed(e As ModelRefreshedEventArgs) Implements IModel.Refreshed
    Public Event RowAdded(e As ModelRowAddedEventArgs) Implements IModel.RowAdded
    Public Event RowUpdated(e As ModelRowUpdatedEventArgs) Implements IModel.RowUpdated
    Public Event RowRemoved(e As ModelRowRemovedEventArgs) Implements IModel.RowRemoved
    Public Event CellUpdated(e As ModelCellUpdatedEventArgs) Implements IModel.CellUpdated
    Public Event SelectionRangeChanged(e As ModelSelectionRangeChangedEventArgs) Implements IModel.SelectionRangeChanged
    Public Event RowSynchronizationStateChanged(e As ModelRowSynchronizationStateChangedEventArgs) Implements IModel.RowSynchronizationStateChanged
End Class
