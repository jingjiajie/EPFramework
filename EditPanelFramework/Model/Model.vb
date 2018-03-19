Imports System.Linq
Imports EditPanelFramework

Public Class Model
    Implements IModel

    Private _selectionRange As Range() = New Range() {}
    Private Property Data As DataTable

    Public Sub New(data As DataTable)
        Me.Data = data
        '选区默认第一行
        If Me.SelectionRange.Length = 0 Then
            Me.SelectionRange = New Range() {
                    New Range(0, 0, 1, Me.Data.Columns.Count)
                }
        End If
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

    Private Sub BindRangeChangedEventToSelectionRangeChangedEvent(range As Range)
        AddHandler range.RangeChanged, Sub()
                                           RaiseEvent SelectionRangeChanged(New ModelSelectionRangeChangedEventArgs() With {
                                                                       .NewSelectionRange = Me.SelectionRange
                                                                   })
                                       End Sub
    End Sub

    Public Function ToDataTable() As DataTable Implements IModel.ToDataTable
        ToDataTable = Me.Data
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
            For Each item In curData
                newRow(item.Key) = item.Value
            Next
            Me.Data.Rows.Add(newRow)
            addedRows.Add(Me.Data.Rows.Count - 1)
        Next

        RaiseEvent RowAdded(New ModelRowAddedEventArgs() With {
                             .AddedRows = (From r In addedRows Select New IndexRowPair(r, Me.DataRowToDictionary(Me.Data.Rows(r)))).ToArray
                            })

        Return Me.Data.Rows.Count - 1
    End Function

    Public Sub RemoveRow(row As Long) Implements IModel.RemoveRow
        Try
            Dim newRow = Me.Data.NewRow
            newRow.ItemArray = Me.Data.Rows(row).ItemArray
            Me.Data.Rows.RemoveAt(row)
            RaiseEvent RowRemoved(New ModelRowRemovedEventArgs() With {
                                        .RemovedRows = New IndexRowPair() {
                                            New IndexRowPair(row, DataRowToDictionary(newRow))
                                        }
                                   })
        Catch ex As Exception
            Throw New Exception("RemoveRow failed: " & ex.Message)
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
                updatedRows(i) = New IndexRowPair(rows(i), Me.DataRowToDictionary(Me.Data.Rows(rows(i))))
            Next

            Dim tmp = New ModelRowUpdatedEventArgs() With {
                                        .UpdatedRows = updatedRows
                                   }
            RaiseEvent RowUpdated(tmp)

        Catch ex As Exception
            Throw New Exception("UpdateRows failed: " & ex.Message)
        End Try
    End Sub

    Public Sub UpdateCell(row As Long, column As Integer, data As Object) Implements IModel.UpdateCell
        Me.UpdateCells(New Long() {row}, New Integer() {column}, New Object() {data})
    End Sub

    Public Sub UpdateCells(rows As Long(), columns As Integer(), dataOfEachCell As Object()) Implements IModel.UpdateCells
        Dim posCellPairs As New List(Of PositionCellPair)
        For i = 0 To rows.Length - 1
            Me.Data.Rows(rows(i))(columns(i)) = dataOfEachCell(i)
            posCellPairs.Add(New PositionCellPair(rows(i), columns(i), Me.Data.Columns(columns(i)).ColumnName, dataOfEachCell(i)))
        Next

        RaiseEvent CellUpdated(New ModelCellUpdatedEventArgs() With {
                                    .UpdatedCells = posCellPairs.ToArray
                               })
    End Sub

    Protected Function DataRowToDictionary(dataRow As DataRow) As Dictionary(Of String, Object)
        Dim result As New Dictionary(Of String, Object)
        Dim columns = dataRow.Table.Columns
        For Each column As DataColumn In columns
            result.Add(column.ColumnName, dataRow(column))
        Next
        Return result
    End Function

    Public Event RowAdded(e As ModelRowAddedEventArgs) Implements IModel.RowAdded
    Public Event RowUpdated(e As ModelRowUpdatedEventArgs) Implements IModel.RowUpdated
    Public Event RowRemoved(e As ModelRowRemovedEventArgs) Implements IModel.RowRemoved
    Public Event CellUpdated(e As ModelCellUpdatedEventArgs) Implements IModel.CellUpdated
    Public Event SelectionRangeChanged(e As ModelSelectionRangeChangedEventArgs) Implements IModel.SelectionRangeChanged
End Class
