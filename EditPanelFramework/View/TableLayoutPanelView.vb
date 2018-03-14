Imports EditPanelFramework
Imports Jint.Native
Imports System.Linq

Public Class TableLayoutPanelView
    Inherits ViewBase(Of TableLayoutPanel)

    Private _targetRow As Long
    Private switcherModelDataUpdatedEvent As Boolean = True
    Private switcherLocalEvents As Boolean = True '本View内部事件开关，包括文本框文字变化等。不包括外部，例如Model数据变化事件开关
    Private dicFieldNameColumn As New Dictionary(Of String, Integer)
    Private dicFieldUpdated As New Dictionary(Of String, Boolean)

    Public Sub New(tableLayout As TableLayoutPanel)
        Me.Panel = tableLayout
    End Sub

    Protected Overrides Sub BindModel()
        AddHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
        AddHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
        AddHandler Me.Model.RowRemoved, AddressOf Me.ModelRowRemovedEvent
        AddHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
        AddHandler Me.Model.SelectionRangeChanged, AddressOf Me.ModelSelectionRangeChangedEvent

        Me.ImportData()
    End Sub

    Protected Overrides Sub UnbindModel()
        RemoveHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
        RemoveHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
        RemoveHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
        RemoveHandler Me.Model.RowRemoved, AddressOf Me.ModelRowRemovedEvent
        RemoveHandler Me.Model.SelectionRangeChanged, AddressOf Me.ModelSelectionRangeChangedEvent

    End Sub

    Private Function GetModelSelectedRow() As Long
        Logger.SetMode(LogMode.REFRESH_VIEW)
        If Me.Model.SelectionRange.Length = 0 Then
            Return -1
        End If
        If Me.Model.SelectionRange.Length > 1 Then
            Logger.PutMessage("Multiple range selected, TableLayoutPanelView will only show data of the first range", LogLevel.WARNING)
        End If
        Dim range = Me.Model.SelectionRange(0)
        If range.Rows > 1 Then
            Logger.PutMessage("Multiple rows selected, TableLayoutPanelView will only show data of the first row", LogLevel.WARNING)
        End If
        Return range.Row
    End Function

    Private Sub ModelSelectionRangeChangedEvent(e As ModelSelectionRangeChangedEventArgs)
        Dim modelSelectedRow = Me.GetModelSelectedRow
        If modelSelectedRow < 0 Then
            Call Me.ClearPanelData()
            Return
        Else
            Me.ImportData()
        End If
    End Sub

    Private Sub ModelRowAddedEvent(e As ModelRowAddedEventArgs)
        Logger.Debug("TableLayoutView ModelRowAddedEvent: " & Str(Me.GetHashCode))
    End Sub

    Private Sub ModelRowRemovedEvent(e As ModelRowRemovedEventArgs)
        Logger.Debug("TableLayoutView ModelRowRemovedEvent: " & Str(Me.GetHashCode))
        Me.ImportData()
    End Sub

    Private Sub ModelCellUpdatedEvent(e As ModelCellUpdatedEventArgs)
        Logger.Debug("TableLayoutView ModelCellUpdatedEvent: " & Str(Me.GetHashCode))
        Logger.SetMode(LogMode.REFRESH_VIEW)
        Dim modelSelectedRow = Me.GetModelSelectedRow
        If modelSelectedRow < 0 Then
            Logger.PutMessage("Unvalid selection range in model")
            Return
        End If
        '如果更新的行不包括本View的目标行，则不刷新
        For Each posCell In e.UpdatedCells
            If modelSelectedRow = posCell.Row Then
                Call Me.ImportData()
                Return
            End If
        Next
    End Sub

    Private Sub ModelRowUpdatedEvent(e As ModelRowUpdatedEventArgs)
        If switcherModelDataUpdatedEvent = False Then Return '开关被关闭则不执行事件
        Logger.Debug("TableLayoutView ModelRowUpdatedEvent: " & Str(Me.GetHashCode))
        Dim modelSelectedRow = Me.GetModelSelectedRow
        If modelSelectedRow < 0 Then
            Logger.PutMessage("Unvalid selection range in model")
            Return
        End If
        Dim needToUpdate As Boolean = (From indexRow In e.UpdatedRows
                                       Where indexRow.Index = modelSelectedRow
                                       Select indexRow.Index).Count > 0
        If needToUpdate Then
            Call Me.ImportData()
        End If
    End Sub

    Private Sub CellUpdateEvent(fieldName As String)
        If Me.switcherLocalEvents = False Then Return
        Logger.Debug("TableLayoutView CellUpdateEvent: " & Str(Me.GetHashCode))
        Call Me.ExportField(fieldName)
    End Sub

    Private Sub ClearPanelData()
        For Each control As Control In Me.Panel.Controls
            Select Case control.GetType
                Case GetType(TextBox)
                    control.Text = String.Empty
                Case GetType(ComboBox)
                    Dim comboBox As ComboBox = CType(control, ComboBox)
                    comboBox.SelectedIndex = -1
            End Select
        Next
    End Sub

    Protected Overrides Sub InitEditPanel()
        '如果基本信息不足，则直接返回
        Logger.SetMode(LogMode.INIT_VIEW)
        If Me.Mode Is Nothing Then
            Logger.PutMessage("Mode is not setted")
            Return
        End If
        If Me.MetaData Is Nothing Then
            Logger.PutMessage("MetaData is not setted")
            Return
        End If
        If Me.Panel Is Nothing Then
            Logger.PutMessage("Panel is not setted")
            Return
        End If
        Me.Panel.Controls.Clear()
        Dim fieldMetaData As FieldMetaData() = Me.MetaData.GetFieldMetaData(Me.Mode)

        If fieldMetaData Is Nothing Then
            Logger.PutMessage("Metadata of mode """ + Me.Mode + """ not found!")
            Return
        End If

        '遍历FieldMetaData()
        Call Me.dicFieldNameColumn.Clear()
        Call Me.dicFieldUpdated.Clear()
        Dim i As Integer = -1 'i从0开始循环
        For Each curField As FieldMetaData In fieldMetaData
            i += 1
            Me.dicFieldNameColumn.Add(curField.Name, i)
            Me.dicFieldUpdated.Add(curField.Name, False)
            '如果字段不可视，直接跳过
            If curField.Visible = False Then Continue For
            '创建标签
            Dim label As New Label With {
                .Text = curField.DisplayName
            }
            Me.Panel.Controls.Add(label)
            '如果没有设定Values字段，认为可以用编辑框体现
            If curField.Values Is Nothing Then
                '创建编辑框
                Dim textBox As New TextBox With {
                    .Name = curField.Name,
                    .ReadOnly = Not curField.Editable
                }
                '绑定用户事件
                If curField.ContentChanged IsNot Nothing Then
                    AddHandler textBox.TextChanged, Sub()
                                                        If Me.switcherLocalEvents = False Then Return
                                                        Logger.Debug("TableLayoutView TextBox TextChanged User Event: " & Str(Me.GetHashCode))
                                                        curField.ContentChanged.Invoke()
                                                    End Sub
                End If
                If curField.EditEnded IsNot Nothing Then
                    AddHandler textBox.Leave, Sub()
                                                  If Me.switcherLocalEvents = False Then Return
                                                  Logger.Debug("TableLayoutView TextBox Leave User Event: " & Str(Me.GetHashCode))
                                                  curField.EditEnded.Invoke()
                                              End Sub
                End If
                '绑定焦点离开自动保存事件
                AddHandler textBox.Leave, Sub()
                                              If Me.switcherLocalEvents = False Then Return
                                              Logger.Debug("TableLayoutView TextBox Leave Save Data: " & Str(Me.GetHashCode))
                                              Call Me.CellUpdateEvent(textBox.Name)
                                          End Sub
                Me.Panel.Controls.Add(textBox)
            Else '否则可以用ComboBox体现
                Dim comboBox As New ComboBox With {
                    .Name = curField.Name,
                    .Enabled = curField.Editable,
                    .DropDownStyle = ComboBoxStyle.DropDownList
                }
                Dim values As Object() = Util.ToArray(Of Object)(curField.Values.Invoke())
                If Not values Is Nothing Then
                    comboBox.Items.AddRange(values)
                End If
                '绑定用户事件
                If curField.ContentChanged IsNot Nothing Then
                    AddHandler comboBox.SelectedIndexChanged, Sub()
                                                                  If Me.switcherLocalEvents = False Then Return
                                                                  Logger.Debug("TableLayoutView ComboBox SelectedIndexChanged User Event: " & Str(Me.GetHashCode))
                                                                  curField.ContentChanged.Invoke()
                                                              End Sub
                End If
                If curField.EditEnded IsNot Nothing Then
                    AddHandler comboBox.Leave, Sub()
                                                   If Me.switcherLocalEvents = False Then Return
                                                   Logger.Debug("TableLayoutView ComboBox Leave User Event: " & Str(Me.GetHashCode))
                                                   curField.EditEnded.Invoke()
                                               End Sub
                End If
                '绑定焦点离开自动保存事件
                AddHandler comboBox.Leave, Sub()
                                               If Me.switcherLocalEvents = False Then Return
                                               Logger.Debug("TableLayoutView ComboBox Leave Save Data: " & Str(Me.GetHashCode))
                                               Call Me.CellUpdateEvent(comboBox.Name)
                                           End Sub
                Me.Panel.Controls.Add(comboBox)
            End If
        Next

        AddHandler Me.Panel.Leave, Sub()
                                       Logger.Debug("TableLayoutView Panel Leave: " & Str(Me.GetHashCode))
                                       Me.switcherLocalEvents = False
                                   End Sub

        AddHandler Me.Panel.Enter, Sub()
                                       Logger.Debug("TableLayoutView Panel Enter: " & Str(Me.GetHashCode))
                                       Me.switcherLocalEvents = True
                                   End Sub

        Call Me.BindViewToJsEngine()
    End Sub

    Public Function ImportData() As Boolean
        Logger.SetMode(LogMode.REFRESH_VIEW)
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
        Dim modelSelectedRow = Me.GetModelSelectedRow
        If modelSelectedRow < 0 Then
            Logger.PutMessage("Unvalid selection range in model")
            Return False
        End If
        If modelSelectedRow < 0 Then
            Call Me.ClearPanelData()
            Return True
        End If
        If modelSelectedRow >= Me.Model.RowCount Then
            Logger.PutMessage("TargetRow(" & Str(modelSelectedRow) & ") exceeded the max row of model: " & Me.Model.RowCount - 1)
            Return False
        End If
        '清空面板
        Call Me.ClearPanelData()
        '获取数据
        Dim data = Me.Model.GetRows(New Long() {modelSelectedRow})
        '遍历MetaData的字段
        Dim fieldMetaData = Me.MetaData.GetFieldMetaData(Me.Mode)
        If fieldMetaData Is Nothing Then
            Logger.PutMessage("Mode """ + Me.Mode + """ not found!")
            Return False
        End If
        For Each curField In fieldMetaData
            Dim curColumn As DataColumn = (From c As DataColumn In data.Columns
                                           Where c.ColumnName = curField.Name
                                           Select c).FirstOrDefault
            '在对象中找不到MetaData描述的字段，直接报错，并接着下一个字段
            If curColumn Is Nothing Then
                Logger.PutMessage("Field """ + curField.Name + """ not found in model")
                Continue For
            End If
            '否则开始Push值
            '先计算值，过一遍Mapper
            Dim value = data.Rows(0)(curColumn)
            Dim text = If(value Is Nothing, "", value.ToString)

            If Not curField.ForwardMapper Is Nothing Then
                text = curField.ForwardMapper.Invoke(text)
            End If
            Logger.SetMode(LogMode.REFRESH_VIEW)
            '然后获取Control
            Dim curControl = (From control As Control In Me.Panel.Controls
                              Where control.Name = curField.Name
                              Select control).FirstOrDefault()
            If curControl Is Nothing Then
                Logger.PutMessage(curField.Name + " not found in view!")
                Continue For
            End If
            '根据Control是文本框还是ComboBox，有不一样的行为
            Me.switcherLocalEvents = False '关闭本地事件开关， 放置连锁事件
            Select Case curControl.GetType()
                Case GetType(TextBox)
                    Dim textBox = CType(curControl, TextBox)
                    textBox.Text = text
                Case GetType(ComboBox)
                    Dim comboBox = CType(curControl, ComboBox)
                    Dim found = False
                    For i As Integer = 0 To comboBox.Items.Count - 1
                        If comboBox.Items(i).ToString = text Then
                            found = True
                            comboBox.SelectedIndex = i
                        End If
                    Next
                    If found = False Then
                        Logger.PutMessage("Value """ + text + """" + " not found in comboBox """ + curField.Name + """")
                    End If
            End Select
            Me.switcherLocalEvents = True
        Next
        Return True
    End Function

    Public Sub ExportField(fieldName As String)
        Logger.SetMode(LogMode.SYNC_FROM_VIEW)
        Dim modelSelectedRow = Me.GetModelSelectedRow
        If modelSelectedRow < 0 Then
            Logger.PutMessage("Unvalid selection range in model")
            Return
        End If
        If modelSelectedRow < 0 Then '如果目标行为负，则认为未指向确定行，故不导出数据
            Return
        End If
        If modelSelectedRow >= Me.Model.RowCount Then '如果目标行超过Model的最大行，提示错误并返回
            Logger.PutMessage("TargetRow(" & Str(modelSelectedRow) & ") exceeded the max row of model: " & Me.Model.RowCount - 1)
            Return
        End If
        Dim metaData = (From m As FieldMetaData In Me.MetaData.GetFieldMetaData(Me.Mode)
                        Where m.Name = fieldName
                        Select m).FirstOrDefault
        Dim value = Me.GetMappedValue(fieldName, metaData)
        RemoveHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
        Me.Model.UpdateCell(modelSelectedRow, Me.dicFieldNameColumn(fieldName), value)
        AddHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
    End Sub

    Public Sub ExportData()
        Logger.SetMode(LogMode.SYNC_FROM_VIEW)
        Dim modelSelectedRow = Me.GetModelSelectedRow
        If modelSelectedRow < 0 Then
            Logger.PutMessage("Unvalid selection range in model")
            Return
        End If
        If modelSelectedRow < 0 Then '如果目标行为负，则认为未指向确定行，故不导出数据
            Return
        End If
        If modelSelectedRow >= Me.Model.RowCount Then '如果目标行超过Model的最大行，提示错误并返回
            Logger.PutMessage("TargetRow(" & Str(modelSelectedRow) & ") exceeded the max row of model: " & Me.Model.RowCount - 1)
            Return
        End If
        Dim dicData As New Dictionary(Of String, Object)
        For Each curField As FieldMetaData In Me.MetaData.GetFieldMetaData(Me.Mode)
            '如果字段不可见，则不pull
            If Not curField.Visible Then Continue For
            Dim value = Me.GetMappedValue(curField.Name, curField)
            If value Is Nothing Then Continue For

            '将新的值加入更新字典中
            dicData.Add(curField.Name, value)
        Next
        Me.Model.UpdateRow(modelSelectedRow, dicData)
    End Sub

    Private Function GetMappedValue(fieldName As String, fieldMetaData As FieldMetaData) As Object
        '获取Control
        Dim curControl = (From control As Control In Me.Panel.Controls
                          Where control.Name = fieldName
                          Select control).FirstOrDefault
        If curControl Is Nothing Then
            Logger.PutMessage(fieldName + " not found in view!")
            Return Nothing
        End If

        '获取Control中的文字
        Dim text As String = Nothing
        Select Case curControl.GetType
            Case GetType(TextBox)
                text = CType(curControl, TextBox).Text
            Case GetType(ComboBox)
                Dim comboBox = CType(curControl, ComboBox)
                Dim selectedItem = comboBox.SelectedItem
                If selectedItem Is Nothing Then
                    text = comboBox.Text
                Else
                    text = selectedItem.ToString
                End If
        End Select
        '将文字经过ReverseMapper映射成转换后的value
        Dim value As Object
        If Not fieldMetaData.ReverseMapper Is Nothing Then
            value = fieldMetaData.ReverseMapper.Invoke(text)
        Else
            value = text
        End If
        Return value
    End Function

    Private Sub BindViewToJsEngine()
        Dim jsEngine = Me.JsEngine
        Dim viewObj = jsEngine.Execute("view = {}").GetValue("view").AsObject
        For Each control In Me.Panel.Controls
            If TypeOf control Is Label Then Continue For '提示标签就不用加到js里了
            Try
                viewObj.Put(control.Name, JsValue.FromObject(jsEngine, control), True)

                If TypeOf control Is TextBox Then
                    Dim tmp = String.Format(
                         <string>
                             {0} = undefined
                             Object.defineProperty(
                                this,
                                "{0}",
                                {{get: function(){{
                                    return view.{0}.Text
                                }},
                                set: function(val){{
                                    view.{0}.Text = val
                                }} }}
                            )
                         </string>.Value, control.Name)
                    jsEngine.Execute(tmp)
                ElseIf TypeOf control Is ComboBox Then
                    Dim tmp = String.Format(
                         <string>
                             {0} = undefined
                             Object.defineProperty(
                                this,
                                "{0}",
                                {{get: function(){{
                                    return view.{0}.SelectedItem
                                }},
                                set: function(val){{
                                    for(var i=0;i &lt; view.{0}.Items.Count;i++){{
                                        if(view.{0}.Items[i] == val){{
                                            view.{0}.SelectedIndex = i; 
                                            return;
                                        }}
                                    }}
                                }} }}
                            )
                         </string>.Value, control.Name)
                    jsEngine.Execute(tmp)
                End If
            Catch ex As Exception
                Logger.SetMode(LogMode.INIT_VIEW)
                Logger.PutMessage(ex.Message)
            End Try
        Next
    End Sub
End Class
