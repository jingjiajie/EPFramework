Imports EditPanelFramework
Imports Jint.Native
Imports System.Linq
Imports System.Threading
Imports unvell.ReoGrid
Imports unvell.ReoGrid.CellTypes
Imports unvell.ReoGrid.Events

Public Class ReoGridWorksheetView
    Inherits ViewBase(Of Worksheet)

    Private Enum SyncMode
        SYNC
        NOT_SYNC
    End Enum

    Private COLOR_UNSYNCHRONIZED As Color = Color.AliceBlue
    Private COLOR_SYNCHRONIZED As Color = Color.Transparent

    Private dicCellDataChangedEvent As New Dictionary(Of Integer, FieldMethod) 'CellDataChanged时触发的事件列表
    Private dicTextChangedEvent As New Dictionary(Of Integer, FieldMethod) '编辑框文本改变时触发的事件列表
    Private dicBeforeSelectionRangeChangeEvent As New Dictionary(Of Integer, FieldMethod) '选择编辑框改变时触发的事件列表
    Private dicNameColumn As New Dictionary(Of String, Integer)
    Private textBox As TextBox = Nothing
    Private formAssociation As FormAssociation
    Private Workbook As ReoGridControl = Nothing

    Private RowInited As New List(Of Integer) '已经初始化过的行，保证每行只初始化一次
    Private dicCellEdited As New Dictionary(Of CellPosition, Boolean)
    Private _curSyncMode = SyncMode.NOT_SYNC
    Private Property CurSyncMode As SyncMode
        Get
            Return Me._curSyncMode
        End Get
        Set(value As SyncMode)
            If Me._curSyncMode = value Then Return
            Console.WriteLine("CurSyncMode Changing:" & CStr(value))
            Me._curSyncMode = value
            If value = SyncMode.SYNC Then
                Me.formAssociation.StayUnvisible = False
            Else
                Me.formAssociation.StayUnvisible = True
            End If
        End Set
    End Property


    Private canChangeSelectionRange As Boolean = True

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
        Me.formAssociation = New FormAssociation(Me.textBox)
        If Me.textBox Is Nothing Then
            Logger.SetMode(LogMode.INIT_VIEW)
            Logger.PutMessage("Table textbox not found")
        End If
        AddHandler Me.textBox.PreviewKeyDown, AddressOf Me.TextboxPreviewKeyDown
        AddHandler Me.Panel.CellMouseDown, AddressOf Me.CellMouseDown
    End Sub

    Protected Overrides Sub BindModel()
        AddHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
        AddHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
        AddHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
        AddHandler Me.Model.RowRemoved, AddressOf Me.ModelRowRemovedEvent
        AddHandler Me.Model.SelectionRangeChanged, AddressOf Me.ModelSelectionRangeChangedEvent
        AddHandler Me.Model.Refreshed, AddressOf Me.ModelRefreshedEvent
        AddHandler Me.Model.RowSynchronizationStateChanged, AddressOf Me.ModelRowSynchronizationStateChangedEvent

        Call Me.ImportData()
    End Sub

    Protected Overrides Sub UnbindModel()
        RemoveHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
        RemoveHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
        RemoveHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
        RemoveHandler Me.Model.RowRemoved, AddressOf Me.ModelRowRemovedEvent
        RemoveHandler Me.Model.SelectionRangeChanged, AddressOf Me.ModelSelectionRangeChangedEvent
        RemoveHandler Me.Model.Refreshed, AddressOf Me.ModelRefreshedEvent
        RemoveHandler Me.Model.RowSynchronizationStateChanged, AddressOf Me.ModelRowSynchronizationStateChangedEvent
    End Sub

    Protected Sub ModelRowSynchronizationStateChangedEvent(e As ModelRowSynchronizationStateChangedEventArgs)
        Dim rows = (From r In e.SynchronizationStateUpdatedRows Select r.Index).ToArray
        Call Me.RefreshRowSynchronizationStates(rows)
    End Sub

    Protected Sub ModelSelectionRangeChangedEvent(e As ModelSelectionRangeChangedEventArgs)
        Logger.Debug("==ReoGrid ModelSelectionRangeChanged " & Str(Me.GetHashCode))
        If Me.Model.RowCount = 0 Then
            Me.CurSyncMode = SyncMode.NOT_SYNC
            Call Me.ShowDefaultPage()
            Return
        End If
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then
            Me.CurSyncMode = SyncMode.SYNC
            Call Me.ImportData()
            Return
        End If
        Call Me.RefreshSelectionRange()
    End Sub

    Protected Sub ModelRefreshedEvent(e As ModelRefreshedEventArgs)
        Logger.Debug("==ReoGrid ModelRefreshedEvent")
        If Me.Model.RowCount = 0 Then
            Me.CurSyncMode = SyncMode.NOT_SYNC
            Call Me.ShowDefaultPage()
            Return
        End If
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then
            Me.CurSyncMode = SyncMode.SYNC
            Call Me.ImportData()
            Return
        End If
        Call Me.ImportData()
        Call Me.RefreshSelectionRange()
        Call Me.RefreshRowSynchronizationStates()
    End Sub

    Protected Sub ModelRowUpdatedEvent(e As ModelRowUpdatedEventArgs)
        Logger.Debug("==ReoGrid ModelDataUpdatedEvent")
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then
            Call Me.ImportData()
            Return
        End If
        Dim rows As Long() = (From item In e.UpdatedRows Select item.Index).ToArray
        Me.ImportData(rows)
    End Sub

    Protected Sub ModelRowAddedEvent(e As ModelRowAddedEventArgs)
        Dim oriRows As Long() = (From item In e.AddedRows Select item.Index).ToArray
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then
            Call Me.ImportData()
            Return
        End If
        Logger.Debug("Reogrid Added Rows: " & oriRows.ToString)
        '原始行每次插入之后，行号会变，所以做调整
        Dim realRowsASC = (From r In oriRows Order By r Ascending Select r).ToArray
        For i = 0 To realRowsASC.Length - 1
            realRowsASC(i) = realRowsASC(i) + i
        Next
        '将调整后的行分别插入表格中
        For i = 0 To realRowsASC.Length - 1
            Me.Panel.InsertRows(realRowsASC(i), 1)
        Next
        '刷新数据
        Call Me.ImportData(realRowsASC)
    End Sub

    Protected Sub ModelCellUpdatedEvent(e As ModelCellUpdatedEventArgs)
        Logger.Debug("==ReoGrid ModelCellUpdatedEvent: " + Str(Me.GetHashCode))
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then
            Call Me.ImportData()
            Return
        End If
        Dim rows As Long() = (From item In e.UpdatedCells Select item.Row).ToArray
        Me.ImportData(rows)
    End Sub

    Protected Sub ModelRowRemovedEvent(e As ModelRowRemovedEventArgs)
        If Me.Model.RowCount = 0 Then
            Me.CurSyncMode = SyncMode.NOT_SYNC
            Call Me.ShowDefaultPage()
            Return
        End If
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then
            Call Me.ImportData()
            Return
        End If
        '去掉选区变化事件，防止删除行时触发选区变化事件，造成无用刷新和警告
        RemoveHandler Me.Panel.BeforeSelectionRangeChange, AddressOf Me.BeforeSelectionRangeChange
        For Each indexDataRow In e.RemovedRows
            If Me.Panel.Rows > indexDataRow.Index Then
                Me.Panel.DeleteRows(indexDataRow.Index, 1)
            End If
        Next
        AddHandler Me.Panel.BeforeSelectionRangeChange, AddressOf Me.BeforeSelectionRangeChange
    End Sub

    Protected Sub RefreshSelectionRange()
        Logger.SetMode(LogMode.REFRESH_VIEW)
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then Return
        If Me.Model.SelectionRange.Length <= 0 Then
            Me.Panel.SelectionRange = RangePosition.Empty
            Return
        End If
        If Me.Model.SelectionRange.Length > 1 Then
            Logger.PutMessage("Multiple range selected, ReoGridView will only show range of the first one", LogLevel.WARNING)
        End If
        Dim range = Me.Model.SelectionRange(0)
        RemoveHandler Me.Panel.BeforeSelectionRangeChange, AddressOf Me.BeforeSelectionRangeChange
        Me.Panel.SelectionRange = New RangePosition(range.Row, range.Column, range.Rows, range.Columns)
        AddHandler Me.Panel.BeforeSelectionRangeChange, AddressOf Me.BeforeSelectionRangeChange
    End Sub

    Protected Sub RefreshRowSynchronizationStates(Optional rows As Long() = Nothing)
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then Return
        Logger.SetMode(LogMode.REFRESH_VIEW)
        If rows Is Nothing Then
            rows = Me.Range(0, Me.Model.RowCount)
        End If
        For Each row In rows
            Dim state = Me.Model.GetRowSynchronizationState(row)
            If row >= Me.Panel.RowCount Then
                Logger.PutMessage($"Row number {row} exceeded max row in the ReoGridView")
                Return
            End If
            If state = SynchronizationState.SYNCHRONIZED Then
                Me.Panel.SetRangeBorders(row, 0, 1, Me.Panel.ColumnCount, BorderPositions.All, RangeBorderStyle.Empty)
                Me.Panel.SetRangeStyles(row, 0, 1, Me.Panel.ColumnCount, New WorksheetRangeStyle() With {
                    .Flag = PlainStyleFlag.BackColor,
                    .BackColor = Me.COLOR_SYNCHRONIZED
                   })
            ElseIf state = SynchronizationState.UNSYNCHRONIZED Then
                Me.Panel.SetRangeBorders(row, 0, 1, Me.Panel.ColumnCount, BorderPositions.All, RangeBorderStyle.SilverSolid)
                Me.Panel.SetRangeStyles(row, 0, 1, Me.Panel.ColumnCount, New WorksheetRangeStyle() With {
                    .Flag = PlainStyleFlag.BackColor,
                    .BackColor = Me.COLOR_UNSYNCHRONIZED
                                    })
            End If
        Next
    End Sub

    Protected Sub ShowDefaultPage()
        Me.CurSyncMode = SyncMode.NOT_SYNC
        '设定表格初始有10行，非同步模式
        Me.Panel.DeleteRangeData(RangePosition.EntireRange)
        Me.Panel.Rows = 10
        '设定提示文本
        Me.Panel.Item(0, 0) = "暂无数据"
    End Sub

    Protected Overrides Sub InitEditPanel()
        Logger.SetMode(LogMode.INIT_VIEW)

        Dim fieldMetaData As FieldMetaData() = Me.MetaData.GetFieldMetaData(Me.Mode)
        If fieldMetaData Is Nothing Then
            Logger.PutMessage("Metadata of mode """ + Me.Mode + """ not found!")
            Return
        End If

        '禁止自动判断单元格格式
        Me.Panel.SetSettings(WorksheetSettings.Edit_AutoFormatCell, False)
        '清空列Name和列号的对应关系
        Me.dicNameColumn.Clear()
        Dim curColumn = 0
        '遍历FieldMetaData()
        For i = 0 To fieldMetaData.Length - 1
            Dim curField = fieldMetaData(i)
            '如果字段不可视，直接跳过
            If curField.Visible = False Then Continue For
            '否则开始初始化表头
            Me.dicNameColumn.Add(curField.Name, curColumn)
            Me.Panel.ColumnHeaders.Item(curColumn).Text = curField.DisplayName
            '给字段注册事件
            '内容改变事件
            If curField.ContentChanged IsNot Nothing Then
                If curField.Values Is Nothing Then '如果是文本框，同时绑定到文本改变事件和单元格内容变化事件
                    Me.dicTextChangedEvent.Add(curColumn, curField.ContentChanged)
                    Me.dicCellDataChangedEvent.Add(curColumn, curField.ContentChanged)
                Else '否则是ComboBox，仅绑定到CellDataChanged事件
                    Me.dicCellDataChangedEvent.Add(curColumn, curField.ContentChanged)
                End If
            End If
            '编辑完成事件
            If curField.EditEnded IsNot Nothing Then
                Me.dicBeforeSelectionRangeChangeEvent.Add(curColumn, curField.EditEnded)
            End If
            curColumn += 1
        Next
        '设定表头列数
        Me.Panel.Columns = curColumn

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
        Call Me.BindAssociation(0) '最开始默认0,0的时候，不会触发选取更改。所以手动绑定一下单元格联想
        Call Me.ShowDefaultPage() '显示默认页
    End Sub

    Private Sub InitRow(row As Integer)
        Logger.SetMode(LogMode.INIT_VIEW)
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then Return
        Dim fieldMetaData As FieldMetaData() = Me.MetaData.GetFieldMetaData(Me.Mode)
        If fieldMetaData Is Nothing Then
            Logger.PutMessage("Metadata of mode """ + Me.Mode + """ not found!")
            Return
        End If

        Dim worksheet = Me.Panel
        '遍历FieldMetaData()
        For i = 0 To fieldMetaData.Length - 1
            Dim curField = fieldMetaData(i)
            If Not curField.Visible Then Continue For
            Dim col = Me.dicNameColumn(curField.Name)
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
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then Return
        Call Me.ExportRows()
    End Sub

    Protected Sub RowUpdatedEvent()
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then Return
        Call Me.ExportRows()
    End Sub

    Protected Sub CellUpdatedEvent()
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then Return
        Call Me.ExportCells()
    End Sub

    Private Sub CellMouseDown(sender As Object, e As EventArgs)
        Me.canChangeSelectionRange = True
        Me.formAssociation.StayVisible = False
    End Sub

    Private Sub TextboxPreviewKeyDown(sender As Object, e As PreviewKeyDownEventArgs)
        If (e.KeyCode = Keys.Up OrElse e.KeyCode = Keys.Down) AndAlso Me.formAssociation IsNot Nothing AndAlso Me.formAssociation.Visible = True Then
            Me.canChangeSelectionRange = False
            Me.formAssociation.StayVisible = True
            Dim threadRestartEdit = New Thread(
                Sub()
                    Call Thread.Sleep(10)
                    Call Me.Workbook.Invoke(
                    Sub()
                        Call Me.Panel.StartEdit()
                    End Sub)
                End Sub)
            Call threadRestartEdit.Start()
        ElseIf e.KeyCode = Keys.Enter And Me.formAssociation.Selected Then
            Me.canChangeSelectionRange = False
            Dim threadEnableChangeSelectionRange = New Thread(
                Sub()
                    Call Thread.Sleep(100)
                    Me.canChangeSelectionRange = True
                End Sub)
            Call threadEnableChangeSelectionRange.Start()
        Else
            Me.canChangeSelectionRange = True
            Me.formAssociation.StayVisible = False
        End If
    End Sub

    '选择行改变时初始化新的行，只初始化选区首行
    Private Sub BeforeSelectionRangeChange(sender As Object, e As BeforeSelectionChangeEventArgs)
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then Return
        If Me.canChangeSelectionRange = False Then
            e.IsCancelled = True
            Return
        End If
        Dim worksheet = Me.Panel
        Dim newRow = System.Math.Min(e.StartRow, e.EndRow)
        Dim newCol = System.Math.Min(e.StartCol, e.EndCol)
        Dim newRows = System.Math.Max(e.StartRow, e.EndRow) - newRow + 1
        Dim newCols = System.Math.Max(e.StartCol, e.EndCol) - newCol + 1

        '隐藏联想
        Call Me.formAssociation.Hide()

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
        If Not newRow = Me.Panel.SelectionRange.Row Then
            If Not RowInited.Contains(newRow) Then
                Me.InitRow(newRow)
            End If
            Me.BindRowToJsEngine(newRow)
        End If

        Me.BindAssociation(newCol)
    End Sub

    Private Sub BindAssociation(col As Integer)
        If Me.Panel.SelectionRange.IsSingleCell Then
            Dim newColName = (From mc In Me.dicNameColumn Where mc.Value = col Select mc.Key).First
            Dim modeMetaData = Me.MetaData.GetFieldMetaData(Me.Mode)
            Dim curField = (From m In modeMetaData Where m.Name = newColName Select m).First
            If curField.Association Is Nothing Then
                formAssociation.SetAssociationFunc(Nothing)
            Else
                formAssociation.SetAssociationFunc(Function(str As String)
                                                       Dim ret = curField.Association.Invoke({str})
                                                       Return Util.ToArray(Of AssociationItem)(ret)
                                                   End Function)
            End If
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
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then Return
        If Not Me.dicCellEdited.ContainsKey(e.Cell.Position) Then
            Me.dicCellEdited.Add(e.Cell.Position, True)
        End If
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
        Dim col = -1
        For i = 0 To fieldMetaData.Length - 1
            Try
                Dim curField = fieldMetaData(i)
                If Not curField.Visible Then Continue For
                col += 1
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
        If Me.Model.RowCount = 0 Then
            Me.CurSyncMode = SyncMode.NOT_SYNC
            Call Me.ShowDefaultPage()
            Return True
        ElseIf Me.CurSyncMode = SyncMode.NOT_SYNC Then
            Me.Panel.Rows = 1
            Me.CurSyncMode = SyncMode.SYNC
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
        Dim dataTable = Me.Model.GetDataTable
        '清空ReoGrid相应行
        If rows Is Nothing Then
            Me.Panel.DeleteRangeData(RangePosition.EntireRange)
            Me.Panel.Rows = dataTable.Rows.Count
            Me.RowInited.Clear()
        Else
            Me.ClearRows(rows)
        End If
        '遍历传入数据
        For Each curDataRowNum In If(rows Is Nothing, Me.Range(dataTable.Rows.Count), rows)
            Dim curDataRow = dataTable.Rows(curDataRowNum)
            Dim curReoGridRowNum = curDataRowNum
            Me.InitRow(curReoGridRowNum)
            '遍历列（MetaData)
            For Each curField In fieldMetaData
                Dim curDataColumn As DataColumn = (From c As DataColumn In dataTable.Columns
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
                If Not curField.ForwardMapper Is Nothing Then
                    text = curField.ForwardMapper.Invoke(text)
                End If

                If String.IsNullOrWhiteSpace(text) Then Continue For '如果推的内容是空白，就不显示在格里了，节约创建单元格的内存空间
                Logger.SetMode(LogMode.REFRESH_VIEW)
                '然后获取单元格
                If Me.dicNameColumn.ContainsKey(curField.Name) = False Then
                    'Logger.PutMessage("Field """ & curField.Name & """ not found in view")
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
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then Return

        Dim rowsUpdated As List(Of Long) = Me.Range(Me.Panel.SelectionRange.Row, System.Math.Min(Me.Model.RowCount, Me.Panel.SelectionRange.EndRow + 1)).ToList
        Dim updateData = New List(Of Dictionary(Of String, Object))

        '删除掉没有真正修改内容的行
        rowsUpdated.RemoveAll(Function(row)
                                  For i = 0 To Me.Model.ColumnCount - 1
                                      If Me.dicCellEdited.ContainsKey(New CellPosition(row, i)) Then Return False
                                  Next
                                  Return True
                              End Function)
        'rowsUpdated的每一项和updateData的每一项相对应
        For Each curReoGridRowNum In rowsUpdated
            updateData.Add(Me.RowToDictionary(curReoGridRowNum))
        Next

        If rowsUpdated.Count > 0 Then
            RemoveHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
            Me.Model.UpdateRows(rowsUpdated.ToArray, updateData.ToArray)
            AddHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
        End If

        Call Me.dicCellEdited.Clear()
    End Sub

    Protected Sub ExportCells()
        Logger.Debug("==ReoGrid ExportCell: " + Str(Me.GetHashCode))
        Logger.SetMode(LogMode.SYNC_FROM_VIEW)
        If Me.CurSyncMode = SyncMode.NOT_SYNC Then Return

        If Me.Panel.SelectionRange.Cols <> 1 Then
            Throw New Exception("ExportCells() can only be used when single column selected")
        End If

        Dim rowsUpdated As List(Of Long) = Me.Range(Me.Panel.SelectionRange.Row, System.Math.Min(Me.Model.RowCount, Me.Panel.SelectionRange.EndRow + 1)).ToList
        Dim colUpdated As Integer = Me.Panel.SelectionRange.Col
        Dim fieldName = (From item In Me.dicNameColumn Where item.Value = colUpdated Select item.Key).FirstOrDefault
        Dim fieldMetaData = (From fm In Me.MetaData.GetFieldMetaData(Me.Mode) Where fm.Name = fieldName Select fm).FirstOrDefault
        If fieldMetaData Is Nothing Then
            Logger.PutMessage("FieldMetaData not found of column index: " & Str(colUpdated))
            Return
        End If
        Dim updateCellData = New List(Of Object)

        '删除掉没有真正修改内容的行
        rowsUpdated.RemoveAll(Function(row)
                                  If Me.dicCellEdited.ContainsKey(New CellPosition(row, colUpdated)) Then
                                      Return False
                                  Else
                                      Return True
                                  End If
                              End Function)

        'rowsUpdated的每一项和updateData的每一项相对应
        For Each curReoGridRowNum In rowsUpdated
            Call updateCellData.Add(Me.GetMappedCellData(curReoGridRowNum, colUpdated, fieldMetaData))
        Next

        If rowsUpdated.Count > 0 Then
            RemoveHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
            Me.Model.UpdateCells(rowsUpdated.ToArray, Util.Times(fieldName, rowsUpdated.LongCount), updateCellData.ToArray)
            AddHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
        End If

        Call Me.dicCellEdited.Clear()
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
        If Not fieldMetaData.BackwordMapper Is Nothing Then
            value = fieldMetaData.BackwordMapper.Invoke(text)
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

    Private Function DropdownListCellItemsToArray(items As DropdownListCell.DropdownItemsCollection) As Object()
        Return items.ToArray
    End Function
End Class
