Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Text
Imports System.Web.Script.Serialization
Imports Jint.Native

Public Class JsonRESTSynchronizer
    Inherits UserControl
    Implements ISynchronizer
    Private _model As IModel
    Private _configuration As Configuration

    Private jsEngine As New Jint.Engine
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label

    <Browsable(False)>
    Public Property AddAPI As JsonRESTAPIInfo

    <Browsable(False)>
    Public Property UpdateAPI As JsonRESTAPIInfo

    <Browsable(False)>
    Public Property RemoveAPI As JsonRESTAPIInfo

    <Browsable(False)>
    Public Property PullAPI As JsonRESTAPIInfo

    <Browsable(False)>
    Public Property PushFinishedCallback As Action

    Public Property Model As IModel
        Get
            Return Me._model
        End Get
        Set(value As IModel)
            If Me._model IsNot Nothing Then
                Call Me.UnbindModel()
            End If
            Me._model = value
            If Me._model IsNot Nothing Then
                Call Me.BindModel()
            End If
        End Set
    End Property

    Public Property Configuration As Configuration Implements ISynchronizer.Configuration
        Get
            Return Me._configuration
        End Get
        Set(value As Configuration)
            If Me._configuration IsNot Nothing Then
                RemoveHandler Me._configuration.ConfigurationChanged, AddressOf Me.ConfigurationChanged
            End If
            Me._configuration = value
            If Me._configuration IsNot Nothing Then
                AddHandler Me._configuration.ConfigurationChanged, AddressOf Me.ConfigurationChanged
                Call Me.InitSynchronizer()
            End If
        End Set
    End Property

    Private modelActions As New List(Of ModelAdapterAction)

    Private Sub BindModel()
        AddHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
        AddHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
        AddHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
        AddHandler Me.Model.RowRemoved, AddressOf Me.ModelRowRemovedEvent
    End Sub

    Private Sub UnbindModel()
        RemoveHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
        RemoveHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
        RemoveHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
        RemoveHandler Me.Model.RowRemoved, AddressOf Me.ModelRowRemovedEvent
    End Sub

    Private Sub InitSynchronizer()
        If Me._configuration Is Nothing Then Return
        Dim httpAPIConfigs = Me._configuration.GetHTTPAPIConfigurations
        For Each apiConfig In httpAPIConfigs
            If apiConfig.APIType.Equals("pushFinishedCallback", StringComparison.OrdinalIgnoreCase) Then
                Me.PushFinishedCallback =
                Sub()
                    Call apiConfig.Callback.Invoke()
                End Sub
                Continue For
            End If

            Dim newAPIInfo As New JsonRESTAPIInfo
            newAPIInfo.URLTemplate = apiConfig.URL
            If Not String.IsNullOrWhiteSpace(apiConfig.Method) Then
                newAPIInfo.HTTPMethod = HTTPMethod.Parse(apiConfig.Method)
            End If
            newAPIInfo.RequestBodyTemplate = apiConfig.RequestBody
            newAPIInfo.ResponseBodyTemplate = apiConfig.ResponseBody
            newAPIInfo.Callback =
                Function(res, ex) As Boolean
                    Return apiConfig.Callback.Invoke(res, ex)
                End Function
            If apiConfig.APIType.Equals("pull", StringComparison.OrdinalIgnoreCase) Then
                Me.PullAPI = newAPIInfo
            ElseIf apiConfig.APIType.Equals("add", StringComparison.OrdinalIgnoreCase) Then
                Me.AddAPI = newAPIInfo
            ElseIf apiConfig.APIType.Equals("update", StringComparison.OrdinalIgnoreCase) Then
                Me.UpdateAPI = newAPIInfo
            ElseIf apiConfig.APIType.Equals("remove", StringComparison.OrdinalIgnoreCase) Then
                Me.RemoveAPI = newAPIInfo
            End If
        Next
    End Sub

    Private Sub ConfigurationChanged(sender As Object, e As EventArgs)
        Call Me.InitSynchronizer()
    End Sub

    Private Sub ModelCellUpdatedEvent(e As ModelCellUpdatedEventArgs)
        If Me.UpdateAPI Is Nothing Then
            Logger.PutMessage("Update API not setted!")
            Return
        End If
        Dim updatedCells = e.UpdatedCells
        Dim indexFullRows(updatedCells.Length - 1) As IndexRowPair
        For i = 0 To updatedCells.Length - 1
            Dim posCell = updatedCells(i)
            Dim row = posCell.Row
            Dim rowID = posCell.RowID
            '获取整列数据
            Dim fullRow As Dictionary(Of String, Object) = Me.DataRowToDictionary(Me.Model.GetRows({row}).Rows(0))
            indexFullRows(i) = New IndexRowPair(row, rowID, fullRow)

            Dim action = New UpdateRowAction(Me.UpdateAPI, indexFullRows)
            modelActions.Add(action)
        Next
    End Sub

    Private Sub ModelRowRemovedEvent(e As ModelRowRemovedEventArgs)
        If Me.RemoveAPI Is Nothing Then
            Logger.PutMessage("Remove API not setted!")
            Return
        End If
        Dim rows = (From indexRow In e.RemovedRows
                    Select indexRow.RowData).ToArray
        Dim action = New RemoveRowAction(Me.RemoveAPI, e.RemovedRows)
        modelActions.Add(action)
    End Sub

    Private Sub ModelRowUpdatedEvent(e As ModelRowUpdatedEventArgs)
        If Me.UpdateAPI Is Nothing Then
            Logger.PutMessage("Update API not setted!")
            Return
        End If
        Dim indexFullRows(e.UpdatedRows.Length - 1) As IndexRowPair
        For i = 0 To e.UpdatedRows.Length - 1
            Dim index = e.UpdatedRows(i).Index
            Dim rowID = e.UpdatedRows(i).RowID
            Dim fullRow As Dictionary(Of String, Object) = Me.DataRowToDictionary(Me.Model.GetRows({index}).Rows(0))
            indexFullRows(i) = New IndexRowPair(index, rowID, fullRow)
        Next
        Dim action = New UpdateRowAction(Me.UpdateAPI, indexFullRows)
        modelActions.Add(action)
    End Sub

    Private Sub ModelRowAddedEvent(e As ModelRowAddedEventArgs)
        If Me.AddAPI Is Nothing Then
            Logger.PutMessage("Add API not setted!")
            Return
        End If
        Dim rows = (From indexRow In e.AddedRows
                    Select indexRow.RowData).ToArray
        Dim action = New AddRowAction(Me.AddAPI, e.AddedRows)
        modelActions.Add(action)
    End Sub

    Public Function PullFromServer() As Boolean Implements ISynchronizer.PullFromServer
        Logger.SetMode(LogMode.MODEL_ADAPTER)
        Try
            Console.WriteLine(Me.PullAPI.HTTPMethod.ToString & " " & Me.PullAPI.GetURL)

            Dim url = Me.PullAPI.GetURL
            Dim httpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
            httpWebRequest.Timeout = 5000
            httpWebRequest.Method = Me.PullAPI.HTTPMethod.ToString
            If Me.PullAPI.HTTPMethod = HTTPMethod.PUT OrElse Me.PullAPI.HTTPMethod = HTTPMethod.POST Then
                httpWebRequest.ContentType = "application/json"
                Dim body = Me.PullAPI.GetRequestBody
                httpWebRequest.ContentLength = body.Length
                Dim streamWrite As StreamWriter = New StreamWriter(httpWebRequest.GetRequestStream)
                streamWrite.WriteLine(body)
            End If

            Dim response As HttpWebResponse = httpWebRequest.GetResponse
            Dim responseStreamReader = New StreamReader(response.GetResponseStream())
            Me.PullAPI.SetResponseParameter("$data")
            Dim data = Me.PullAPI.GetResponseParameters(responseStreamReader.ReadToEnd, {"$data"})(0)
            If data Is Nothing Then Return False

            '清空actions
            Call Me.modelActions.Clear()
            '更新Model
            Dim resultList As New List(Of IDictionary(Of String, Object))
            '判断是对象还是对象数组
            If TypeOf (data) Is IDictionary(Of String, Object) Then '如果是对象，则作为数组的第一项
                Dim value = CType(data, IDictionary(Of String, Object))
                resultList.Add(value)
            Else '否则是数组
                Dim valueArray = CType(data, Object())
                For Each value In valueArray
                    resultList.Add(value)
                Next
            End If
            '直接操作源数据，不触发事件
            Dim dataTable = Me.Model.GetDataTable
            Call dataTable.Rows.Clear()
            For Each resultRow In resultList
                Dim newRow = dataTable.NewRow
                For Each item In resultRow
                    Dim key = item.Key
                    Dim value = item.Value
                    If Not dataTable.Columns.Contains(key) Then
                        Logger.PutMessage("Column """ & key & """ not found in model", LogLevel.WARNING)
                        Continue For
                    Else
                        newRow(key) = value
                    End If
                Next
                dataTable.Rows.Add(newRow)
            Next
            '修改完成后整体触发刷新事件
            Dim selectionRanges As New List(Of Range)
            For Each oriRange In Me.Model.SelectionRange
                '截取选区，如果原选区超过了数据表的范围，则进行截取
                If oriRange.Row >= dataTable.Rows.Count Then Continue For
                If oriRange.Column >= dataTable.Columns.Count Then Continue For
                Dim newRow = oriRange.Row
                Dim newCol = oriRange.Column
                Dim newRows = oriRange.Rows
                Dim newCols = oriRange.Columns
                If oriRange.Row + oriRange.Rows >= dataTable.Rows.Count Then
                    newRows = dataTable.Rows.Count - newRow
                End If
                If oriRange.Column + oriRange.Columns >= dataTable.Columns.Count Then
                    newRows = dataTable.Columns.Count - newCol
                End If
                selectionRanges.Add(New Range(newRow, newCol, newRows, newCols))
            Next
            '如果实在没有选区了，就自动选第一行第一列
            If selectionRanges.Count = 0 AndAlso dataTable.Rows.Count > 0 Then
                selectionRanges.Add(New Range(0, 0, 1, 1))
            End If
            Call Me.Model.Refresh(dataTable, selectionRanges.ToArray, Util.Times(SynchronizationState.SYNCHRONIZED, dataTable.Rows.Count))

            Call Me.PullAPI.Callback?.Invoke(response, Nothing)
        Catch ex As WebException
            Call Me.PullAPI.Callback?.Invoke(CType(ex.Response, HttpWebResponse), ex)
        End Try
        Return True
    End Function

    Public Function PushToServer() As Boolean Implements ISynchronizer.PushToServer
        Logger.SetMode(LogMode.MODEL_ADAPTER)
        If Me.Model Is Nothing Then
            Logger.PutMessage("Model not setted")
            Return False
        End If

        '将Actions进行优化
        Dim optimizer As New ActionOptimizer
        Dim optimizedActions = optimizer.Optimize(Me.modelActions.ToArray)
        Me.modelActions.Clear()

        For Each action In optimizedActions
            Dim rowGuids = (From indexRowPair In action.IndexRowPairs Select indexRowPair.RowID).ToArray
            Try
                Dim response = action.DoSync()
                'TODO 不等于200就认为失败吗？
                If response.StatusCode <> 200 Then
                    Me.modelActions.Add(action)
                    Me.Model.UpdateRowSynchronizationStates(rowGuids, Util.Times(SynchronizationState.UNSYNCHRONIZED, rowGuids.Length))
                End If
            Catch ex As WebException
                Me.modelActions.Add(action)
                Me.Model.UpdateRowSynchronizationStates(rowGuids, Util.Times(SynchronizationState.UNSYNCHRONIZED, rowGuids.Length))
                If action.APIInfo.Callback Is Nothing Then Continue For
                Dim ifContinue = action.APIInfo.Callback.Invoke(ex.Response, ex)
                If Not ifContinue Then
                    Return False
                End If
            End Try

            '将相应行的同步状态更新为已同步，删除行就不用同步了，因为行已经被删了。
            If TypeOf (action) IsNot RemoveRowAction Then
                Me.Model.UpdateRowSynchronizationStates(rowGuids, Util.Times(SynchronizationState.SYNCHRONIZED, rowGuids.Length))
            End If
        Next

        Call Me.PushFinishedCallback?.Invoke
        Return True
    End Function

    Public Sub SetAddAPI(url As String, method As HTTPMethod, bodyJsonTemplate As String)
        Dim apiInfo = New JsonRESTAPIInfo()
        apiInfo.URLTemplate = url
        apiInfo.HTTPMethod = method
        apiInfo.RequestBodyTemplate = bodyJsonTemplate
        Me.AddAPI = apiInfo
    End Sub

    Public Sub SetUpdateAPI(url As String, method As HTTPMethod, bodyJsonTemplate As String)
        Dim apiInfo = New JsonRESTAPIInfo()
        apiInfo.URLTemplate = url
        apiInfo.HTTPMethod = method
        apiInfo.RequestBodyTemplate = bodyJsonTemplate
        Me.UpdateAPI = apiInfo
    End Sub

    Public Sub SetRemoveAPI(url As String, method As HTTPMethod, bodyJsonTemplate As String)
        Dim apiInfo = New JsonRESTAPIInfo()
        apiInfo.URLTemplate = url
        apiInfo.HTTPMethod = method
        apiInfo.RequestBodyTemplate = bodyJsonTemplate
        Me.RemoveAPI = apiInfo
    End Sub

    Public Sub SetPullAPI(url As String, method As HTTPMethod, responseJsonTemplate As String)
        Dim apiInfo = New JsonRESTAPIInfo()
        apiInfo.URLTemplate = url
        apiInfo.HTTPMethod = method
        apiInfo.ResponseBodyTemplate = responseJsonTemplate
        Me.PullAPI = apiInfo
    End Sub

    Protected Function DataRowToDictionary(dataRow As DataRow) As Dictionary(Of String, Object)
        Dim result As New Dictionary(Of String, Object)
        Dim columns = dataRow.Table.Columns
        For Each column As DataColumn In columns
            result.Add(column.ColumnName, dataRow(column))
        Next
        Return result
    End Function

    '=======================================================================================
    Private MustInherit Class ModelAdapterAction
        Public Property APIInfo As JsonRESTAPIInfo
        Public Property IndexRowPairs As IndexRowPair()

        Public Overridable Function DoSync() As HttpWebResponse
            Console.WriteLine(Me.APIInfo.HTTPMethod.ToString & " " & Me.APIInfo.GetURL & vbCrLf & Me.APIInfo.GetRequestBody)

            Dim httpWebRequest = CType(WebRequest.Create(Me.APIInfo.GetURL), HttpWebRequest)
            httpWebRequest.Method = Me.APIInfo.HTTPMethod.ToString
            If Me.APIInfo.HTTPMethod = HTTPMethod.POST OrElse Me.APIInfo.HTTPMethod = HTTPMethod.PUT Then
                httpWebRequest.ContentType = "application/json"
                Dim requestBody = Me.APIInfo.GetRequestBody
                Dim bytes = Encoding.UTF8.GetBytes(requestBody)
                Dim stream = httpWebRequest.GetRequestStream
                stream.Write(bytes, 0, bytes.Length)
            End If

            Dim response = httpWebRequest.GetResponse
            Return response
        End Function

        Protected Shared Function IndexRowPairsToJson(indexRowPairs As IndexRowPair()) As String
            Dim dics = (From indexRowPair In indexRowPairs
                        Select indexRowPair.RowData).ToArray
            Return New JavaScriptSerializer().Serialize(dics)
        End Function

    End Class

    Private Class AddRowAction
        Inherits ModelAdapterAction

        Public Sub New(apiInfo As JsonRESTAPIInfo, indexRowPairs As IndexRowPair())
            Me.APIInfo = apiInfo
            Me.IndexRowPairs = indexRowPairs
        End Sub

        Public Overrides Function DoSync() As HttpWebResponse
            Dim dataJson = IndexRowPairsToJson(Me.IndexRowPairs)
            Me.APIInfo.SetJsonRequestParameter("$data", dataJson)
            Return MyBase.DoSync()
        End Function
    End Class

    Private Class UpdateRowAction
        Inherits ModelAdapterAction

        Public Sub New(apiInfo As JsonRESTAPIInfo, indexRowPairs As IndexRowPair())
            Me.APIInfo = apiInfo
            Me.IndexRowPairs = indexRowPairs
        End Sub


        Public Overrides Function DoSync() As HttpWebResponse
            Dim dataJson = IndexRowPairsToJson(Me.IndexRowPairs)
            Me.APIInfo.SetJsonRequestParameter("$data", dataJson)
            Return MyBase.DoSync()
        End Function
    End Class

    Private Class RemoveRowAction
        Inherits ModelAdapterAction

        Public Sub New(apiInfo As JsonRESTAPIInfo, indexRowPairs As IndexRowPair())
            Me.APIInfo = apiInfo
            Me.IndexRowPairs = indexRowPairs
        End Sub


        Public Overrides Function DoSync() As HttpWebResponse
            Dim dataJson = IndexRowPairsToJson(Me.IndexRowPairs)
            Me.APIInfo.SetJsonRequestParameter("$data", dataJson)
            Return MyBase.DoSync()
        End Function
    End Class

    Private Class ActionOptimizer

        Public Function Optimize(actions As ModelAdapterAction()) As ModelAdapterAction()
            Dim dicRowIDActions As New Dictionary(Of Guid, ModelAdapterAction)
            For Each action In actions
                Select Case action.GetType
                    Case GetType(AddRowAction)
                        Dim addRowAction = CType(action, AddRowAction)
                        For Each indexRowPair In addRowAction.IndexRowPairs
                            If Not dicRowIDActions.ContainsKey(indexRowPair.RowID) Then
                                dicRowIDActions.Add(indexRowPair.RowID, Nothing)
                            End If
                            Dim lastAction = dicRowIDActions(indexRowPair.RowID)
                            If lastAction Is Nothing Then
                                dicRowIDActions(indexRowPair.RowID) = New AddRowAction(addRowAction.APIInfo, {indexRowPair})
                            ElseIf lastAction.GetType() = GetType(RemoveRowAction) Then
                                Continue For
                            ElseIf lastAction.GetType = GetType(UpdateRowAction) Then
                                Dim lastUpdateAction = CType(lastAction, UpdateRowAction)
                                For Each field In lastUpdateAction.IndexRowPairs(0).RowData
                                    If indexRowPair.RowData.ContainsKey(field.Key) Then
                                        indexRowPair.RowData(field.Key) = field.Value
                                    Else
                                        indexRowPair.RowData.Add(field.Key, field.Value)
                                    End If
                                Next
                                dicRowIDActions(indexRowPair.RowID) = New AddRowAction(addRowAction.APIInfo, {indexRowPair})
                            End If
                        Next

                    Case GetType(RemoveRowAction)
                        Dim removeRowAction = CType(action, RemoveRowAction)
                        For Each indexRowPair In removeRowAction.IndexRowPairs
                            If Not dicRowIDActions.ContainsKey(indexRowPair.RowID) Then
                                dicRowIDActions.Add(indexRowPair.RowID, New RemoveRowAction(removeRowAction.APIInfo, {indexRowPair}))
                            Else
                                Dim lastAction = dicRowIDActions(indexRowPair.RowID)
                                If TypeOf (lastAction) Is AddRowAction Then
                                    dicRowIDActions.Remove(indexRowPair.RowID)
                                Else
                                    dicRowIDActions(indexRowPair.RowID) = New RemoveRowAction(removeRowAction.APIInfo, {indexRowPair})
                                End If
                            End If
                        Next

                    Case GetType(UpdateRowAction)
                        Dim updateRowAction = CType(action, UpdateRowAction)
                        For Each indexRowPair In updateRowAction.IndexRowPairs
                            If Not dicRowIDActions.ContainsKey(indexRowPair.RowID) Then
                                dicRowIDActions.Add(indexRowPair.RowID, Nothing)
                            End If
                            Dim lastAction = dicRowIDActions(indexRowPair.RowID)
                            If lastAction Is Nothing Then
                                dicRowIDActions(indexRowPair.RowID) = New UpdateRowAction(updateRowAction.APIInfo, {indexRowPair})
                            ElseIf lastAction.GetType() = GetType(RemoveRowAction) Then
                                Continue For
                            ElseIf lastAction.GetType = GetType(UpdateRowAction) Then
                                Dim lastUpdateAction = CType(lastAction, UpdateRowAction)
                                '后来的更新之前的
                                For Each field In indexRowPair.RowData
                                    If lastUpdateAction.IndexRowPairs(0).RowData.ContainsKey(field.Key) Then
                                        lastUpdateAction.IndexRowPairs(0).RowData(field.Key) = field.Value
                                    Else
                                        lastUpdateAction.IndexRowPairs(0).RowData.Add(field.Key, field.Value)
                                    End If
                                Next
                            ElseIf lastAction.GetType = GetType(AddRowAction) Then
                                Dim lastAddAction = CType(lastAction, AddRowAction)
                                '后来的更新之前的
                                For Each field In indexRowPair.RowData
                                    If lastAddAction.IndexRowPairs(0).RowData.ContainsKey(field.Key) Then
                                        lastAddAction.IndexRowPairs(0).RowData(field.Key) = field.Value
                                    Else
                                        lastAddAction.IndexRowPairs(0).RowData.Add(field.Key, field.Value)
                                    End If
                                Next
                            End If
                        Next
                End Select
            Next
            Return dicRowIDActions.Values.ToArray
        End Function
    End Class

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(JsonRESTSynchronizer))
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 1
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.PictureBox1, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Label1, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.Label2, 0, 1)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 3
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(180, 180)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'PictureBox1
        '
        Me.PictureBox1.BackgroundImage = CType(resources.GetObject("PictureBox1.BackgroundImage"), System.Drawing.Image)
        Me.PictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.PictureBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox1.Location = New System.Drawing.Point(0, 0)
        Me.PictureBox1.Margin = New System.Windows.Forms.Padding(0)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(180, 120)
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label1.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!)
        Me.Label1.Location = New System.Drawing.Point(3, 150)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(174, 30)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Synchronizer"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label2.Font = New System.Drawing.Font("Microsoft YaHei UI", 8.0!)
        Me.Label2.Location = New System.Drawing.Point(0, 120)
        Me.Label2.Margin = New System.Windows.Forms.Padding(0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(180, 30)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "REST&&JSON"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'JsonRESTSynchronizer
        '
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Margin = New System.Windows.Forms.Padding(0)
        Me.Name = "JsonRESTSynchronizer"
        Me.Size = New System.Drawing.Size(180, 180)
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Private Sub JsonRESTSynchronizer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Me.DesignMode Then Me.Visible = False
        Call Me.InitializeComponent()
    End Sub
End Class
