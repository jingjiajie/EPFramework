Imports System.Linq
Imports EditPanelFramework

Public Class Model
    Implements IModel

    Private _selectionRange As Range() = New Range() {}
    Private _mode As String
    Private _metaData As EditPanelMetaData
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
        Return Me.AddRows(New Dictionary(Of String, Object)() {data})
    End Function

    Public Function AddRows(dataOfEachRow As Dictionary(Of String, Object)()) As Long Implements IModel.AddRows
        Dim addedRows As New List(Of Long)
        For Each curData In dataOfEachRow
            Dim newRow = Me.Data.NewRow
            'Dim newGuid = Guid.NewGuid '生成新添加行的GUID
            'Me.RowGuid.Add(newRow, newGuid)
            If curData IsNot Nothing Then
                For Each item In curData
                    newRow(item.Key) = item.Value
                Next
            End If
            Me.Data.Rows.Add(newRow)
            addedRows.Add(Me.Data.Rows.Count - 1)
        Next

        RaiseEvent RowAdded(New ModelRowAddedEventArgs() With {
                             .AddedRows = (From r In addedRows Select New IndexRowPair(r, Me.GetRowID(Me.Data.Rows(r)), Me.DataRowToDictionary(Me.Data.Rows(r)))).ToArray
                            })

        Return Me.Data.Rows.Count - 1
    End Function

    Public Sub RemoveRow(row As Long) Implements IModel.RemoveRow
        Me.RemoveRows(New Long() {row})
    End Sub

    Public Sub RemoveRows(startRow As Long, rowCount As Long) Implements IModel.RemoveRows
        Me.RemoveRows(Util.Range(startRow, startRow + rowCount))
    End Sub

    Public Sub RemoveRows(rows As Long()) Implements IModel.RemoveRows
        Try
            Dim indexRowList = New List(Of IndexRowPair)
            For Each row In rows
                Dim dataRow = Me.Data.Rows(row)
                'If Not Me.RowGuid.ContainsKey(Me.Data.Rows(row)) Then
                '    Me.RowGuid.Add(Me.Data.Rows(row), Guid.NewGuid)
                'End If
                Dim newIndexRowPair = New IndexRowPair(row, Me.GetRowID(Me.Data.Rows(row)), Me.DataRowToDictionary(Me.Data.Rows(row)))
                indexRowList.Add(newIndexRowPair)
                dataRow.Delete()
            Next
            Call Me.Data.AcceptChanges()

            RaiseEvent RowRemoved(New ModelRowRemovedEventArgs() With {
                                        .RemovedRows = indexRowList.ToArray
                                   })
        Catch ex As Exception
            Throw New Exception("RemoveRows failed: " & ex.Message)
        End Try
    End Sub

    Public Sub UpdateRow(row As Long, data As Dictionary(Of String, Object)) Implements IModel.UpdateRow
        Call Me.UpdateRows(
            New Long() {row},
            New Dictionary(Of String, Object)() {data}
        )
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

        Catch ex As Exception
            Throw New Exception("UpdateRows failed: " & ex.Message)
        End Try
    End Sub

    Public Sub UpdateCell(row As Long, columnName As String, data As Object) Implements IModel.UpdateCell
        Me.UpdateCells(New Long() {row}, New String() {columnName}, New Object() {data})
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
            'If Not Me.RowGuid.ContainsKey(Me.Data.Rows(rows(i))) Then
            '    Me.RowGuid.Add(Me.Data.Rows(rows(i)), Guid.NewGuid)
            'End If
            posCellPairs.Add(New PositionCellPair(rows(i), Me.GetRowID(Me.Data.Rows(rows(i))), columnName, dataOfEachCell(i)))
        Next

        RaiseEvent CellUpdated(New ModelCellUpdatedEventArgs() With {
                                    .UpdatedCells = posCellPairs.ToArray
                               })
    End Sub

    Public Sub Refresh(dataTable As DataTable, ranges As Range()) Implements IModel.Refresh
        Me._selectionRange = ranges
        For Each range In Me._selectionRange
            Me.BindRangeChangedEventToSelectionRangeChangedEvent(range)
        Next
        Me._Data = dataTable
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

    Protected Function GetRowID(row As DataRow) As Guid
        Static rowGuid As New Dictionary(Of DataRow, Guid)
        If Not rowGuid.ContainsKey(row) Then
            rowGuid.Add(row, Guid.NewGuid)
        End If
        Return rowGuid(row)
    End Function

    Public Event Refreshed(e As ModelRefreshedEventArgs) Implements IModel.Refreshed
    Public Event RowAdded(e As ModelRowAddedEventArgs) Implements IModel.RowAdded
    Public Event RowUpdated(e As ModelRowUpdatedEventArgs) Implements IModel.RowUpdated
    Public Event RowRemoved(e As ModelRowRemovedEventArgs) Implements IModel.RowRemoved
    Public Event CellUpdated(e As ModelCellUpdatedEventArgs) Implements IModel.CellUpdated
    Public Event SelectionRangeChanged(e As ModelSelectionRangeChangedEventArgs) Implements IModel.SelectionRangeChanged
End Class
