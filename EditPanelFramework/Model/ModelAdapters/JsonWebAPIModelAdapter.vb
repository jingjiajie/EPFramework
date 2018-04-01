Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Text
Imports System.Web.Script.Serialization
Imports Jint.Native

Public Class JsonWebAPIModelAdapter
    Private _model As IModel

    Private jsEngine As New Jint.Engine

    Public Property AddAPI As JsonWebAPIInfo
    Public Property UpdateAPI As JsonWebAPIInfo
    Public Property RemoveAPI As JsonWebAPIInfo
    Public Property PullAPI As JsonWebAPIInfo

    Public Property AddCallback As Action(Of HttpWebResponse, WebException)
    Public Property UpdateCallback As Action(Of HttpWebResponse, WebException)
    Public Property RemoveCallback As Action(Of HttpWebResponse, WebException)
    Public Property PullCallback As Action(Of HttpWebResponse, WebException)

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

            Dim action = New UpdateRowAction(Me.UpdateAPI, indexFullRows, Me.UpdateCallback)
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
        Dim action = New RemoveRowAction(Me.RemoveAPI, e.RemovedRows, Me.RemoveCallback)
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
        Dim action = New UpdateRowAction(Me.UpdateAPI, indexFullRows, Me.UpdateCallback)
        modelActions.Add(action)
    End Sub

    Private Sub ModelRowAddedEvent(e As ModelRowAddedEventArgs)
        If Me.AddAPI Is Nothing Then
            Logger.PutMessage("Add API not setted!")
            Return
        End If
        Dim rows = (From indexRow In e.AddedRows
                    Select indexRow.RowData).ToArray
        Dim action = New AddRowAction(Me.AddAPI, e.AddedRows, Me.AddCallback)
        modelActions.Add(action)
    End Sub

    Public Function PullFromServer()
        Logger.SetMode(LogMode.MODEL_ADAPTER)
        Try
            Console.WriteLine(Me.PullAPI.HTTPMethod.ToString & " " & Me.PullAPI.GetURL)

            Dim httpWebRequest = CType(WebRequest.Create(Me.PullAPI.GetURL), HttpWebRequest)
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
            Dim data = Me.PullAPI.GetParamValuesFromResponse(responseStreamReader.ReadToEnd, {"$data"})(0)
            If data Is Nothing Then Return False

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
            Call Me.Model.Refresh(dataTable, selectionRanges.ToArray)

            Call Me.PullCallback.Invoke(response, Nothing)
        Catch ex As WebException
            Call Me.PullCallback.Invoke(CType(ex.Response, HttpWebResponse), ex)
        End Try
        Return True
    End Function

    Public Function PushToServer()
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
            Dim ret = action.DoSync()
            If ret = False Then
                Me.modelActions.Add(action)
                Continue For
            End If
        Next
        Return True
    End Function

    Public Sub SetAddAPI(url As String, method As HTTPMethod, bodyJsonTemplate As String, callback As Action(Of HttpWebResponse, WebException))
        Dim apiInfo = New JsonWebAPIInfo()
        apiInfo.URLTemplate = url
        apiInfo.HTTPMethod = method
        apiInfo.RequestBodyTemplate = bodyJsonTemplate
        Me.AddAPI = apiInfo
        Me.AddCallback = callback
    End Sub

    Public Sub SetUpdateAPI(url As String, method As HTTPMethod, bodyJsonTemplate As String, callback As Action(Of HttpWebResponse, WebException))
        Dim apiInfo = New JsonWebAPIInfo()
        apiInfo.URLTemplate = url
        apiInfo.HTTPMethod = method
        apiInfo.RequestBodyTemplate = bodyJsonTemplate
        Me.UpdateAPI = apiInfo
        Me.UpdateCallback = callback
    End Sub

    Public Sub SetRemoveAPI(url As String, method As HTTPMethod, bodyJsonTemplate As String, callback As Action(Of HttpWebResponse, WebException))
        Dim apiInfo = New JsonWebAPIInfo()
        apiInfo.URLTemplate = url
        apiInfo.HTTPMethod = method
        apiInfo.RequestBodyTemplate = bodyJsonTemplate
        Me.RemoveAPI = apiInfo
        Me.RemoveCallback = callback
    End Sub

    Public Sub SetPullAPI(url As String, method As HTTPMethod, responseJsonTemplate As String, callback As Action(Of HttpWebResponse, WebException))
        Dim apiInfo = New JsonWebAPIInfo()
        apiInfo.URLTemplate = url
        apiInfo.HTTPMethod = method
        apiInfo.ResponseBodyTemplate = responseJsonTemplate
        Me.PullAPI = apiInfo
        Me.PullCallback = callback
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
        Public Property APIInfo As JsonWebAPIInfo
        Public Overridable Property ResponseCallback As Action(Of HttpWebResponse, WebException)

        Public Overridable Function DoSync() As Boolean
            Dim response As HttpWebResponse
            Try
                If TypeOf Me Is AddRowAction Then

                End If
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

                response = httpWebRequest.GetResponse
            Catch ex As WebException
                Call Me.ResponseCallback.Invoke(CType(ex.Response, HttpWebResponse), ex)
                Return False
            End Try

            Call Me.ResponseCallback.Invoke(response, Nothing)
            '返回200认为成功，否则认为失败。//TODO 这儿应该能让用户定制
            If response.StatusCode = HttpStatusCode.OK Then
                Return True
            Else
                Return False
            End If
        End Function

        Protected Shared Function IndexRowPairsToJson(indexRowPairs As IndexRowPair()) As String
            Dim dics = (From indexRowPair In indexRowPairs
                        Select indexRowPair.RowData).ToArray
            Return New JavaScriptSerializer().Serialize(dics)
        End Function

    End Class

    Private Class AddRowAction
        Inherits ModelAdapterAction

        Public Property IndexRowPairs As IndexRowPair()

        Public Sub New(apiInfo As JsonWebAPIInfo, indexRowPairs As IndexRowPair(), callback As Action(Of HttpWebResponse, WebException))
            Me.APIInfo = apiInfo
            Me.IndexRowPairs = indexRowPairs
            Me.ResponseCallback = callback
        End Sub

        Public Overrides Function DoSync() As Boolean
            Dim dataJson = IndexRowPairsToJson(Me.IndexRowPairs)
            Me.APIInfo.SetJsonRequestParameter("$data", dataJson)
            Return MyBase.DoSync()
        End Function
    End Class

    Private Class UpdateRowAction
        Inherits ModelAdapterAction

        Public Property IndexRowPairs As IndexRowPair()

        Public Sub New(apiInfo As JsonWebAPIInfo, indexRowPairs As IndexRowPair(), callback As Action(Of HttpWebResponse, WebException))
            Me.APIInfo = apiInfo
            Me.IndexRowPairs = indexRowPairs
            Me.ResponseCallback = callback
        End Sub


        Public Overrides Function DoSync() As Boolean
            Dim dataJson = IndexRowPairsToJson(Me.IndexRowPairs)
            Me.APIInfo.SetJsonRequestParameter("$data", dataJson)
            Return MyBase.DoSync()
        End Function
    End Class

    Private Class RemoveRowAction
        Inherits ModelAdapterAction

        Public Property IndexRowPairs As IndexRowPair()

        Public Sub New(apiInfo As JsonWebAPIInfo, indexRowPairs As IndexRowPair(), callback As Action(Of HttpWebResponse, WebException))
            Me.APIInfo = apiInfo
            Me.IndexRowPairs = indexRowPairs
            Me.ResponseCallback = callback
        End Sub


        Public Overrides Function DoSync() As Boolean
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
                                dicRowIDActions(indexRowPair.RowID) = New AddRowAction(addRowAction.APIInfo, {indexRowPair}, addRowAction.ResponseCallback)
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
                                dicRowIDActions(indexRowPair.RowID) = New AddRowAction(addRowAction.APIInfo, {indexRowPair}, addRowAction.ResponseCallback)
                            End If
                        Next

                    Case GetType(RemoveRowAction)
                        Dim removeRowAction = CType(action, RemoveRowAction)
                        For Each indexRowPair In removeRowAction.IndexRowPairs
                            If Not dicRowIDActions.ContainsKey(indexRowPair.RowID) Then
                                dicRowIDActions.Add(indexRowPair.RowID, New RemoveRowAction(removeRowAction.APIInfo, {indexRowPair}, removeRowAction.ResponseCallback))
                            Else
                                dicRowIDActions(indexRowPair.RowID) = New RemoveRowAction(removeRowAction.APIInfo, {indexRowPair}, removeRowAction.ResponseCallback)
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
                                dicRowIDActions(indexRowPair.RowID) = New UpdateRowAction(updateRowAction.APIInfo, {indexRowPair}, updateRowAction.ResponseCallback)
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
End Class
