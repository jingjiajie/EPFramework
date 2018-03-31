Imports EditPanelFramework
Imports Jint.Native
Imports System.Linq
Imports unvell.ReoGrid
Imports unvell.ReoGrid.CellTypes
Imports unvell.ReoGrid.Events

Public Class ReoGridWorksheetView
    Inherits ViewBase(Of Worksheet)

    Private dicCellDataChangedEvent As New Dictionary(Of Integer, FieldMethod) 'CellDataChanged时触发的事件列表
    Private dicTextChangedEvent As New Dictionary(Of Integer, FieldMethod) '编辑框文本改变时触发的事件列表
    Private dicBeforeSelectionRangeChangeEvent As New Dictionary(Of Integer, FieldMethod) '选择编辑框改变时触发的事件列表
    Private dicNameColumn As New Dictionary(Of String, Integer)
    Private textBox As TextBox = Nothing
    Private RowInited As New List(Of Integer) '已经初始化过的行，保证每行只初始化一次
    Private Workbook As ReoGridControl = Nothing

    Public Sub New(reoGridControl As ReoGridControl)
        Me.Panel = reoGridControl.CurrentWorksheet
        Dim worksheet = Me.Panel
        Me.Workbook = reoGridControl
        worksheet.StartEdit()
        worksheet.EndEdit(EndEditReason.NormalFinish)
        For Each control As Control In reoGridControl.Controls
            If TypeOf (control) Is TextBox AndAlso control.Name = "" Then
                Me.textBox = control
                Exit For
            End If
        Next
        If Me.textBox Is Nothing Then
            Logger.SetMode(LogMode.INIT_VIEW)
            Logger.PutMessage("Table textbox not found")
        End If
    End Sub

    Protected Overrides Sub BindModel()
        AddHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
        AddHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
        AddHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
        AddHandler Me.Model.RowRemoved, AddressOf Me.ModelRowRemovedEvent
        AddHandler Me.Model.SelectionRangeChanged, AddressOf Me.ModelSelectionRangeChangedEvent
        AddHandler Me.Model.Refreshed, AddressOf Me.ModelRefreshedEvent

        Call Me.ImportData()
    End Sub

    Protected Overrides Sub UnbindModel()
        RemoveHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
        RemoveHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
        RemoveHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
        RemoveHandler Me.Model.RowRemoved, AddressOf Me.ModelRowRemovedEvent
        RemoveHandler Me.Model.SelectionRangeChanged, AddressOf Me.ModelSelectionRangeChangedEvent
        RemoveHandler Me.Model.Refreshed, AddressOf Me.ModelRefreshedEvent

    End Sub

    Protected Sub ModelSelectionRangeChangedEvent(e As ModelSelectionRangeChangedEventArgs)
        Logger.Debug("==ReoGrid ModelSelectionRangeChanged " & Str(Me.GetHashCode))
        Logger.SetMode(LogMode.REFRESH_VIEW)
        If e.NewSelectionRange.Length <= 0 Then
            Me.Panel.SelectionRange = RangePosition.Empty
            Return
        End If
        If e.NewSelectionRange.Length > 1 Then
            Logger.PutMessage("Multiple range selected, ReoGridView will only show range of the first one", LogLevel.WARNING)
        End If
        Dim range = e.NewSelectionRange(0)
        RemoveHandler Me.Panel.BeforeSelectionRangeChange, AddressOf Me.BeforeSelectionRangeChange
        Me.Panel.SelectionRange = New RangePosition(range.Row, range.Column, range.Rows, range.Columns)
        AddHandler Me.Panel.BeforeSelectionRangeChange, AddressOf Me.BeforeSelectionRangeChange
    End Sub

    Protected Sub ModelRefreshedEvent(e As ModelRefreshedEventArgs)
        Logger.Debug("==ReoGrid ModelRefreshedEvent")
        Call Me.ImportData()
    End Sub

    Protected Sub ModelRowUpdatedEvent(e As ModelRowUpdatedEventArgs)
        Logger.Debug("==ReoGrid ModelDataUpdatedEvent")
        Dim rows As Long() = (From item In e.UpdatedRows Select item.Index).ToArray
        Me.ImportData(rows)
    End Sub

    Protected Sub ModelRowAddedEvent(e As ModelRowAddedEventArgs)
        Dim rows As Long() = (From item In e.AddedRows Select item.Index).ToArray
        Logger.Debug("Reogrid Added Rows: " & rows.ToString)
        '只允许增加从最后一行开始连续的若干行
        For i = 0 To rows.Length - 1
            If i <> 0 AndAlso rows(i) - rows(i - 1) <> 1 Then
                Throw New Exception("DataAddedEvent参数错误，只允许增加从最后一行开始连续的若干行")
            End If
        Next
        Me.Panel.Rows += rows.Length
        Me.ImportData(rows)
    End Sub

    Protected Sub ModelCellUpdatedEvent(e As ModelCellUpdatedEventArgs)
        Logger.Debug("==ReoGrid ModelCellUpdatedEvent: " + Str(Me.GetHashCode))
        Dim rows As Long() = (From item In e.UpdatedCells Select item.Row).ToArray
        Me.ImportData(rows)
    End Sub

    Protected Sub ModelRowRemovedEvent(e As ModelRowRemovedEventArgs)
        '去掉选区变化事件，防止删除行时触发选区变化事件，造成无用刷新和警告
        RemoveHandler Me.Panel.BeforeSelectionRangeChange, AddressOf Me.BeforeSelectionRangeChange
        For Each indexDataRow In e.RemovedRows
            If Me.Panel.Rows > indexDataRow.Index Then
                Me.Panel.DeleteRows(indexDataRow.Index, 1)
            End If
        Next
        AddHandler Me.Panel.BeforeSelectionRangeChange, AddressOf Me.BeforeSelectionRangeChange
    End Sub

    Protected Overrides Sub InitEditPanel()
        Logger.SetMode(LogMode.INIT_VIEW)

        Dim fieldMetaData As FieldMetaData() = Me.MetaData.GetFieldMetaData(Me.Mode)
        If fieldMetaData Is Nothing Then
            Logger.PutMessage("Metadata of mode """ + Me.Mode + """ not found!")
            Return
        End If

        '设定表格初始有一行
        Me.Panel.Rows = 1
        '设定表头列数为metadata指定的字段数量
        Me.Panel.Columns = fieldMetaData.Length
        '禁止自动判断单元格格式
        Me.Panel.SetSettings(WorksheetSettings.Edit_AutoFormatCell, False)
        '清空列Name和列号的对应关系
        Me.dicNameColumn.Clear()
        '遍历FieldMetaData()
        For i = 0 To fieldMetaData.Length - 1
            Dim curField = fieldMetaData(i)
            Me.dicNameColumn.Add(curField.Name, i)
            '如果字段不可视，直接跳过
            If curField.Visible = False Then Continue For
            '否则开始初始化表头
            Me.Panel.ColumnHeaders.Item(i).Text = curField.DisplayName
            '给字段注册事件
            '内容改变事件
            If curField.ContentChanged IsNot Nothing Then
                If curField.Values Is Nothing Then '如果是文本框，同时绑定到文本改变事件和单元格内容变化事件
                    Me.dicTextChangedEvent.Add(i, curField.ContentChanged)
                    Me.dicCellDataChangedEvent.Add(i, curField.ContentChanged)
                Else '否则是ComboBox，仅绑定到CellDataChanged事件
                    Me.dicCellDataChangedEvent.Add(i, curField.ContentChanged)
                End If
            End If
            '编辑完成事件
            If curField.EditEnded IsNot Nothing Then
                Me.dicBeforeSelectionRangeChangeEvent.Add(i, curField.EditEnded)
            End If
        Next

        '给worksheet添加事件
        '换行初始化行，绑定JS变量
        AddHandler Me.Panel.BeforeSelectionRangeChange, AddressOf BeforeSelectionRangeChange
        '编辑事件
        AddHandler Me.Panel.CellDataChanged, AddressOf Me.CellDataChanged
        AddHandler Me.textBox.TextChanged, AddressOf Me.TextChanged
        AddHandler Me.textBox.Leave, Sub()
                                         Me.Workbook.Focus()
                                     End Sub

        Call Me.InitRow(Me.Panel.SelectionRange.Row)
        Call Me.BindRowToJsEngine(Me.Panel.SelectionRange.Row)
    End Sub

    Private Sub InitRow(row As Integer)
        Logger.SetMode(LogMode.INIT_VIEW)
        Dim fieldMetaData As FieldMetaData() = Me.MetaData.GetFieldMetaData(Me.Mode)
        If fieldMetaData Is Nothing Then
            Logger.PutMessage("Metadata of mode """ + Me.Mode + """ not found!")
            Return
        End If

        Dim worksheet = Me.Panel
        '遍历FieldMetaData()
        For i = 0 To fieldMetaData.Length - 1
            Dim col = i
            Dim curField = fieldMetaData(i)
            '如果字段不可视，直接跳过
            If curField.Visible = False Then Continue For
            '否则开始初始化当前格
            Dim curCell = worksheet.CreateAndGetCell(row, col)
            '如果设定了Values，则执行Values获取值
            If curField.Values IsNot Nothing Then
                Dim comboBox = New DropdownListCell(CType(curField.Values.Invoke(), IEnumerable(Of Object)))
                AddHandler comboBox.DropdownOpened, Sub()
                                                        worksheet.SelectionRange = New RangePosition(row, col, 1, 1)
                                                    End Sub
                worksheet(row, col) = comboBox
            End If

            If curField.Editable = False Then
                curCell.IsReadOnly = True
            End If
        Next
        RowInited.Add(row)
    End Sub

    Protected Sub RowAddedEvent()
        Call Me.ExportRows()
    End Sub

    Protected Sub RowUpdatedEvent()
        Call Me.ExportRows()
    End Sub

    Protected Sub CellUpdatedEvent()
        Call Me.ExportCellsAndAddedRows()
    End Sub

    '选择行改变时初始化新的行，只初始化选区首行
    Private Sub BeforeSelectionRangeChange(sender As Object, e As BeforeSelectionChangeEventArgs)
        Dim worksheet = Me.Panel
        Dim newRow = System.Math.Min(e.StartRow, e.EndRow)
        Dim newCol = System.Math.Min(e.StartCol, e.EndCol)
        Dim newRows = System.Math.Max(e.StartRow, e.EndRow) - newRow + 1
        Dim newCols = System.Math.Max(e.StartCol, e.EndCol) - newCol + 1

        '首先更新数据
        '如果是在Model已有行的变化，认为是Update，否则认为是Add
        If Me.Panel.SelectionRange.Row < Me.Model.RowCount Then
            '判断是单元格更新还是整行更新
            Dim selectionRange = Me.Panel.SelectionRange
            If selectionRange.IsSingleCell Then
                RemoveHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
                Call Me.CellUpdatedEvent()
                AddHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
            Else
                RemoveHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
                Call Me.RowUpdatedEvent()
                AddHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
            End If
        Else
            RemoveHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
            Call Me.RowAddedEvent()
            AddHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
        End If

        '同步Model的选区
        RemoveHandler Me.Model.SelectionRangeChanged, AddressOf Me.ModelSelectionRangeChangedEvent
        Me.Model.SelectionRange = New Range() {New Range(newRow, newCol, newRows, newCols)}
        AddHandler Me.Model.SelectionRangeChanged, AddressOf Me.ModelSelectionRangeChangedEvent

        '初始化新的选中行。如果选区首行没变，就不重新初始化行了
        If newRow = Me.Panel.SelectionRange.Row Then
            Exit Sub
        Else
            If RowInited.Contains(newRow) = False Then
                Me.InitRow(newRow)
            End If
            Me.BindRowToJsEngine(newRow)
        End If
    End Sub

    Private Sub TextChanged(sender As Object, e As EventArgs)
        Static switcher = True '事件开关，False为关，True为开
        If switcher = False Then Return '开关关掉则直接返回
        Logger.Debug("ReoGrid View textChanged: " & Str(Me.GetHashCode))
        Dim worksheet = Me.Panel
        Dim row = Me.Panel.SelectionRange.Row
        Dim col = Me.Panel.SelectionRange.Col
        If Not Me.RowInited.Contains(row) Then '如果本行未被初始化，不要触发事件
            Return
        End If
        '要是这个列没设置TextChanged事件，就不用刷了
        If Not Me.dicTextChangedEvent.ContainsKey(col) Then Exit Sub
        '否则执行设置的事件
        switcher = False
        worksheet.EditingCell.Data = Me.textBox.Text
        switcher = True
        Dim fieldMethod = Me.dicTextChangedEvent(col)
        fieldMethod.Invoke()
    End Sub

    Private Sub CellDataChanged(sender As Object, e As CellEventArgs)
        Logger.Debug("ReoGrid View CellDataChanged: " & Str(Me.GetHashCode))
        Dim worksheet = Me.Panel
        Dim row = e.Cell.Row
        Dim col = e.Cell.Column
        If Not Me.RowInited.Contains(row) Then '如果本行未被初始化，不要触发事件
            Return
        End If
        '要是这个列没设置ContentChanged事件，就不用刷了
        If Not Me.dicCellDataChangedEvent.ContainsKey(col) Then Exit Sub
        '否则执行设置的事件
        'If Me.textBox.Visible = True Then
        '    Me.eventSwitcher = False
        '    worksheet.EditingCell.Data = Me.textBox.Text
        '    Me.eventSwitcher = True
        'End If
        Dim fieldMethod = Me.dicCellDataChangedEvent(col)
        fieldMethod.Invoke()
    End Sub

    Private Sub BindRowToJsEngine(row As Integer)
        Dim jsEngine = Me.JsEngine
        Dim worksheet = Me.Panel
        Dim viewObj = jsEngine.Execute("view = {}").GetValue("view").AsObject
        Dim fieldMetaData = Me.MetaData.GetFieldMetaData(Me.Mode)
        jsEngine.SetValue("DropdownListCellItemsToArray", New Func(Of DropdownListCell.DropdownItemsCollection, Object())(AddressOf Me.DropdownListCellItemsToArray))
        For i = 0 To fieldMetaData.Length - 1
            Try
                Dim curField = fieldMetaData(i)
                Dim col = i
                If curField.Values IsNot Nothing Then '如果设置了Values，则是下拉框
                    viewObj.Put(curField.Name, JsValue.FromObject(jsEngine, worksheet.CreateAndGetCell(row, col)), True)
                    Dim tmp = String.Format(
                        <string>
                             {0} = undefined;
                             Object.defineProperty(
                                this,
                                "{0}",
                                {{get: function(){{
                                    return view.{0}.Data
                                }},
                                set: function(val){{
                                    if(val == undefined) return;
                                    var itemArray = DropdownListCellItemsToArray(view.{0}.Body.Items)
                                    for(var i=0;i &lt; itemArray.length;i++){{
                                        if(itemArray[i] == val){{
                                            view.{0}.Data = val;
                                            return;
                                        }}
                                    }}
                                }} }}
                            )
                         </string>.Value, curField.Name)
                    jsEngine.Execute(tmp)
                Else '否则是普通的文本格
                    viewObj.Put(curField.Name, JsValue.FromObject(jsEngine, worksheet.CreateAndGetCell(row, col)), True)
                    Dim tmp = String.Format(
                        <string>
                             {0} = undefined
                             Object.defineProperty(
                                this,
                                "{0}",
                                {{get: function(){{
                                    return view.{0}.Data
                                }},
                                set: function(val){{
                                    if(val == undefined) return;
                                    view.{0}.Data = val
                                }} }}
                            )
                         </string>.Value, curField.Name)
                    jsEngine.Execute(tmp)
                End If
            Catch ex As Exception
                Logger.SetMode(LogMode.INIT_VIEW)
                Logger.PutMessage(ex.Message)
            End Try
        Next
    End Sub

    Protected Function ImportData(Optional rows As Long() = Nothing) As Boolean
        Logger.Debug("==ReoGrid ImportData: " + Str(Me.GetHashCode))
        Logger.SetMode(LogMode.REFRESH_VIEW)
        Dim data As DataTable
        data = Me.Model.GetDataTable
        If data.Rows.Count = 0 Then
            Logger.PutMessage("Data is empty")
            Return False
        End If
        If Me.Mode Is Nothing Then
            Logger.PutMessage("Mode is not setted")
            Return False
        End If
        If Me.MetaData Is Nothing Then
            Logger.PutMessage("MetaData is not setted")
            Return False
        End If
        If Me.Panel Is Nothing Then
            Logger.PutMessage("Panel is not setted")
            Return False
        End If
        '获取当前的MetaData
        Dim fieldMetaData = Me.MetaData.GetFieldMetaData(Me.Mode)
        If fieldMetaData Is Nothing Then
            Logger.PutMessage("Mode """ + Me.Mode + """ not found!")
            Return False
        End If
        '清空ReoGrid相应行
        If rows Is Nothing Then
            Me.Panel.Rows = 0
            Me.Panel.Rows = data.Rows.Count
            Me.RowInited.Clear()
        Else
            Me.ClearRows(rows)
        End If
        '遍历传入数据
        For Each curDataRowNum In If(rows Is Nothing, Me.Range(data.Rows.Count), rows)
            Dim curDataRow = data.Rows(curDataRowNum)
            Dim curReoGridRowNum = curDataRowNum
            Me.InitRow(curReoGridRowNum)
            '遍历列（MetaData)
            For Each curField In fieldMetaData
                Dim curDataColumn As DataColumn = (From c As DataColumn In data.Columns
                                                   Where c.ColumnName.Equals(curField.Name, StringComparison.OrdinalIgnoreCase)
                                                   Select c).FirstOrDefault
                '在对象中找不到MetaData描述的字段，直接报错，并接着下一个字段
                If curDataColumn Is Nothing Then
                    Logger.PutMessage("Field """ + curField.Name + """ not found in model")
                    Continue For
                End If
                '否则开始Push值
                '先计算值，过一遍Mapper
                Dim value = curDataRow(curDataColumn)
                Dim text = If(value Is Nothing, "", value.ToString)
                If String.IsNullOrWhiteSpace(text) Then Continue For '如果推的内容是空白，就不显示在格里了，节约创建单元格的内存空间

                If Not curField.ForwardMapper Is Nothing Then
                    text = curField.ForwardMapper.Invoke(text)
                End If
                Logger.SetMode(LogMode.REFRESH_VIEW)
                '然后获取单元格
                If Me.dicNameColumn.ContainsKey(curField.Name) = False Then
                    Logger.PutMessage("Field """ & curField.Name & """ not found in view")
                    Continue For
                End If
                Dim curReoGridColumnNum = Me.dicNameColumn(curField.Name)
                Dim curReoGridCell = Panel.CreateAndGetCell(curReoGridRowNum, curReoGridColumnNum)
                '根据MetaData中的Field类型，处理View中的单元格
                If curField.Values Is Nothing Then '没有Values，是文本框
                    RemoveHandler Me.Panel.CellDataChanged, AddressOf CellDataChanged
                    curReoGridCell.Data = text
                    AddHandler Me.Panel.CellDataChanged, AddressOf CellDataChanged
                Else '有Values，是ComboBox框
                    Dim values = Util.ToArray(Of String)(curField.Values.Invoke())
                    If values.Contains(text) = False Then
                        Logger.PutMessage("Value """ + text + """" + " not found in comboBox """ + curField.Name + """")
                        Continue For
                    End If
                    RemoveHandler Me.Panel.CellDataChanged, AddressOf CellDataChanged
                    curReoGridCell.Data = text
                    AddHandler Me.Panel.CellDataChanged, AddressOf CellDataChanged
                End If
            Next
        Next
        Return True
    End Function

    ''' <summary>
    ''' 创建从0到n的数组，包含0不包含length
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Protected Function Range(n As Integer) As Long()
        Return Me.Range(0, n)
    End Function

    ''' <summary>
    ''' 创建从start到end的数组，包含start不包含end
    ''' </summary>
    ''' <param name="start"></param>
    ''' <param name="end"></param>
    ''' <returns></returns>
    Protected Function Range(start As Long, [end] As Long) As Long()
        Dim array([end] - start - 1) As Long
        For i = 0 To array.Length - 1
            array(i) = start + i
        Next
        Return array
    End Function


    Protected Sub ClearRows(rows As Long())
        RemoveHandler Me.Panel.CellDataChanged, AddressOf Me.CellDataChanged
        For Each row In rows
            For col = 0 To Me.Panel.Columns - 1
                Dim cell = Me.Panel.GetCell(row, col)
                If cell Is Nothing Then Continue For
                cell.Data = Nothing
            Next
        Next
        AddHandler Me.Panel.CellDataChanged, AddressOf Me.CellDataChanged
    End Sub

    Protected Sub ExportRows()
        Logger.Debug("==ReoGrid ExportData")
        Logger.SetMode(LogMode.SYNC_FROM_VIEW)

        Dim rowsUpdated As Long() = Me.Range(Me.Panel.SelectionRange.Row, System.Math.Min(Me.Model.RowCount, Me.Panel.SelectionRange.EndRow + 1))
        Dim updateData = New Dictionary(Of String, Object)() {}
        Dim rowsAdded = New Long() {}
        Dim addedData = New Dictionary(Of String, Object)() {}

        If rowsUpdated.Length > 0 Then
            ReDim updateData(rowsUpdated.Length - 1)
        End If

        If Me.Panel.Rows > Me.Model.RowCount Then
            rowsAdded = Me.Range(Me.Model.RowCount + 1, Me.Panel.Rows + 1)
            ReDim addedData(rowsAdded.Length - 1)
        End If

        Dim curUpdateDataIndex = 0 'rowsUpdated的每一项和updateData的每一项相对应
        For Each curReoGridRowNum In rowsUpdated
            updateData(curUpdateDataIndex) = Me.RowToDictionary(curReoGridRowNum)
            curUpdateDataIndex += 1
        Next

        Dim curAddDataIndex = 0 'rowsAdded的每一项和addedData的每一项相对应
        For Each curReoGridRowNum In rowsAdded
            addedData(curAddDataIndex) = Me.RowToDictionary(curReoGridRowNum)
            curAddDataIndex += 1
        Next

        If rowsAdded.Length > 0 Then
            RemoveHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
            Me.Model.AddRows(addedData)
            AddHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
        End If

        If rowsUpdated.Length > 0 Then
            RemoveHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
            Me.Model.UpdateRows(rowsUpdated, updateData)
            AddHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
        End If
    End Sub

    Protected Sub ExportCellsAndAddedRows()
        Logger.Debug("==ReoGrid ExportCell: " + Str(Me.GetHashCode))
        Logger.SetMode(LogMode.SYNC_FROM_VIEW)

        If Me.Panel.SelectionRange.Cols <> 1 Then
            Throw New Exception("ExportCells() can only be used when single column selected")
        End If

        Dim rowsUpdated As Long() = Me.Range(Me.Panel.SelectionRange.Row, System.Math.Min(Me.Model.RowCount, Me.Panel.SelectionRange.EndRow + 1))
        Dim colUpdated As Integer = Me.Panel.SelectionRange.Col
        Dim fieldName = (From item In Me.dicNameColumn Where item.Value = colUpdated Select item.Key).FirstOrDefault
        Dim fieldMetaData = (From fm In Me.MetaData.GetFieldMetaData(Me.Mode) Where fm.Name = fieldName Select fm).FirstOrDefault
        If fieldMetaData Is Nothing Then
            Logger.PutMessage("FieldMetaData not found of column index: " & Str(colUpdated))
            Return
        End If
        Dim updateCellData = New Object() {}
        Dim rowsAdded = New Long() {}
        Dim addedRowData = New Object() {}

        If rowsUpdated.Length > 0 Then
            ReDim updateCellData(rowsUpdated.Length - 1)
        End If

        If Me.Panel.Rows > Me.Model.RowCount Then
            rowsAdded = Me.Range(Me.Model.RowCount + 1, Me.Panel.Rows + 1)
            ReDim addedRowData(rowsAdded.Length - 1)
        End If

        Dim curUpdateDataIndex = 0 'rowsUpdated的每一项和updateData的每一项相对应
        For Each curReoGridRowNum In rowsUpdated
            updateCellData(curUpdateDataIndex) = Me.GetMappedCellData(curReoGridRowNum, colUpdated, fieldMetaData)
            curUpdateDataIndex += 1
        Next

        Dim curAddDataIndex = 0 'rowsAdded的每一项和addedData的每一项相对应
        For Each curReoGridRowNum In rowsAdded
            addedRowData(curAddDataIndex) = Me.RowToDictionary(curReoGridRowNum)
            curAddDataIndex += 1
        Next

        If rowsAdded.Length > 0 Then
            RemoveHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
            Me.Model.AddRows(addedRowData)
            AddHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
        End If

        If rowsUpdated.Length > 0 Then
            RemoveHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
            Me.Model.UpdateCells(rowsUpdated, Me.Times(fieldName, rowsUpdated.LongLength), updateCellData)
            AddHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
        End If
    End Sub

    Protected Function GetMappedCellData(row As Long, col As Integer, fieldMetaData As FieldMetaData)
        Dim curReoGridCell = Me.Panel.GetCell(row, col)
        '获取Cell中的文字
        Dim text As String
        If curReoGridCell Is Nothing Then
            text = ""
        Else
            text = If(Me.Panel.GetCell(row, col).Data, "")
        End If

        '将文字经过ReverseMapper映射成转换后的value
        Dim value As Object
        If Not fieldMetaData.ReverseMapper Is Nothing Then
            value = fieldMetaData.ReverseMapper.Invoke(text)
        Else
            value = text
        End If
        Return value
    End Function

    Protected Function RowToDictionary(row As Long) As Dictionary(Of String, Object)
        Dim dic = New Dictionary(Of String, Object)

        For Each curField As FieldMetaData In Me.MetaData.GetFieldMetaData(Me.Mode)
            '如果字段不可见，则不pull
            If Not curField.Visible Then Continue For
            '然后获取Cell
            If Me.dicNameColumn.ContainsKey(curField.Name) = False Then
                Logger.PutMessage("Field """ & curField.Name & """ not found in view")
                Continue For
            End If
            Dim curReoGridColumnNum = Me.dicNameColumn(curField.Name)
            Dim curReoGridCell = Panel.GetCell(row, curReoGridColumnNum)

            '获取DataTable的列
            Dim modelDataTable = Model.GetDataTable
            Dim curColumn As DataColumn = (From c As DataColumn In modelDataTable.Columns
                                           Where c.ColumnName = curField.Name
                                           Select c).FirstOrDefault
            If curColumn Is Nothing Then
                Logger.PutMessage("Field """ + curField.Name + """ not found in model!")
                Continue For
            End If
            '获取Cell中的内容
            Dim value = Me.GetMappedCellData(row, curReoGridColumnNum, curField)
            '将新的值赋予Model中的相应单元格
            dic.Add(curField.Name, value)
        Next
        Return dic
    End Function

    Protected Function Times(Of T)(data As T, repeatTimes As Long) As T()
        Dim result(repeatTimes - 1) As T
        For i = 0 To repeatTimes - 1
            result(i) = data
        Next
        Return result
    End Function

    Private Function DropdownListCellItemsToArray(items As DropdownListCell.DropdownItemsCollection) As Object()
        Return items.ToArray
    End Function
End Class
