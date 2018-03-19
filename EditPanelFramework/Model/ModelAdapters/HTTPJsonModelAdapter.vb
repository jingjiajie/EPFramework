Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Text
Imports System.Web.Script.Serialization
Imports Jint.Native

Public Class HTTPJsonModelAdapter
    Private _model As IModel

    Private jsEngine As New Jint.Engine

    Public Property AddAPIURL As String
    Public Property AddAPIMethod As HTTPMethod = HTTPMethod.POST
    Public Property AddJsonBodyTemplate As String
    Public Property AddedCallback As Action(Of HttpWebResponse, WebException)

    Public Property UpdateAPIURL As String
    Public Property UpdateAPIMethod As HTTPMethod = HTTPMethod.PUT
    Public Property UpdateJsonBodyTemplate As String
    Public Property UpdatedCallback As Action(Of HttpWebResponse, WebException)

    Public Property RemoveAPIURL As String
    Public Property RemoveAPIMethod As HTTPMethod = HTTPMethod.DELETE
    Public Property RemoveJsonBodyTemplate As String
    Public Property RemovedCallback As Action(Of HttpWebResponse, WebException)

    Public Property PullAPIURL
    Public Property PullAPIMethod As HTTPMethod = HTTPMethod.GET
    Public Property PullJsonTemplate As String
    Public Property PulledCallback As Action(Of HttpWebResponse, WebException)
    '分析过的Json关键变量位置。例如$data放在哪个对象的哪个字段里。Key是变量名$data，Value是存放路径
    Private Property AnalyzedPullJsonVariablePositions As Dictionary(Of String, String())

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
    End Sub

    Private Sub UnbindModel()
        RemoveHandler Me.Model.RowAdded, AddressOf Me.ModelRowAddedEvent
        RemoveHandler Me.Model.RowUpdated, AddressOf Me.ModelRowUpdatedEvent
        RemoveHandler Me.Model.CellUpdated, AddressOf Me.ModelCellUpdatedEvent
    End Sub

    Private Sub ModelCellUpdatedEvent(e As ModelCellUpdatedEventArgs)
        Dim updatedCells = e.UpdatedCells
        Dim indexRowPairs = New List(Of IndexRowPair)
        For Each posCell In updatedCells
            Dim row = posCell.Row
            Dim data = posCell.CellData
            Dim fieldName = posCell.FieldName
            indexRowPairs.Add(New IndexRowPair(row, New Dictionary(Of String, Object) From {
                                                        {fieldName, data}
                                                    }))
            Dim action = New UpdateRowAction(Me.UpdateAPIURL, Me.UpdateAPIMethod, indexRowPairs.ToArray, Me.UpdateJsonBodyTemplate, Me.UpdatedCallback)
            '尝试merge并添加action
            Dim lastAction As ModelAdapterAction = Nothing
            If modelActions.Count <> 0 Then
                lastAction = modelActions.Last
            End If
            Dim mergeAction As ModelAdapterAction = Nothing
            If lastAction IsNot Nothing AndAlso lastAction.TryMerge(action, mergeAction) Then
                modelActions(modelActions.Count - 1) = mergeAction
            Else
                modelActions.Add(action)
            End If
        Next
    End Sub

    Private Sub ModelRowUpdatedEvent(e As ModelRowUpdatedEventArgs)
        Dim action = New UpdateRowAction(Me.UpdateAPIMethod.ToString, Me.UpdateAPIMethod, e.UpdatedRows, Me.UpdateJsonBodyTemplate, Me.UpdatedCallback)
        '尝试merge并添加action
        Dim lastAction As ModelAdapterAction = Nothing
        If modelActions.Count <> 0 Then
            lastAction = modelActions.Last
        End If
        Dim mergeAction As ModelAdapterAction = Nothing
        If lastAction IsNot Nothing AndAlso lastAction.TryMerge(action, mergeAction) Then
            modelActions(modelActions.Count - 1) = mergeAction
        Else
            modelActions.Add(action)
        End If
    End Sub

    Private Sub ModelRowAddedEvent(e As ModelRowAddedEventArgs)
        Dim rows = (From indexRow In e.AddedRows
                    Select indexRow.RowData).ToArray
        Dim action = New AddRowAction(Me.AddAPIURL, Me.AddAPIMethod, e.AddedRows, Me.AddJsonBodyTemplate, Me.AddedCallback)
        Dim lastAction As ModelAdapterAction = Nothing
        If modelActions.Count <> 0 Then
            lastAction = modelActions.Last
        End If
        Dim mergeAction As ModelAdapterAction = Nothing
        If lastAction IsNot Nothing AndAlso lastAction.TryMerge(action, mergeAction) Then
            modelActions(modelActions.Count - 1) = mergeAction
        Else
            modelActions.Add(action)
        End If
    End Sub

    Public Function PullFromServer()

    End Function

    Public Function PushToServer()
        Logger.SetMode(LogMode.MODEL_ADAPTER)
        If Me.Model Is Nothing Then
            Logger.PutMessage("Model not setted")
            Return False
        End If

        For Each action In modelActions
            Call action.DoSync()
        Next
        Call Me.modelActions.Clear()
        Return True
    End Function

    Public Sub SetAddAPI(url As String, method As HTTPMethod, bodyJsonTemplate As String, callback As Action(Of HttpWebResponse, WebException))
        Me.AddAPIURL = url
        Me.AddAPIMethod = method
        Me.AddJsonBodyTemplate = bodyJsonTemplate
        Me.AddedCallback = callback
    End Sub

    Public Sub SetUpdateAPI(url As String, method As HTTPMethod, bodyJsonTemplate As String, callback As Action(Of HttpWebResponse, WebException))
        Me.UpdateAPIURL = url
        Me.UpdateAPIMethod = method
        Me.UpdateJsonBodyTemplate = bodyJsonTemplate
        Me.UpdatedCallback = callback
    End Sub

    Public Sub SetRemoveAPI(url As String, method As HTTPMethod, bodyJsonTemplate As String, callback As Action(Of HttpWebResponse, WebException))
        Me.RemoveAPIURL = url
        Me.RemoveAPIMethod = method
        Me.RemoveJsonBodyTemplate = bodyJsonTemplate
        Me.RemovedCallback = callback
    End Sub

    Public Sub SetPullAPI(url As String, method As HTTPMethod, responseJsonTemplate As String, callback As Action(Of HttpWebResponse, WebException))
        Logger.SetMode(LogMode.MODEL_ADAPTER)
        Me.PullAPIURL = url
        Me.PullAPIMethod = method
        Me.PullJsonTemplate = responseJsonTemplate
        Me.PulledCallback = callback
        Me.AnalyzedPullJsonVariablePositions = New Dictionary(Of String, String())
        Me.jsEngine.SetValue("variablePaths", Me.AnalyzedPullJsonVariablePositions)
        Dim strFindData = '从给定jsonTemplate中寻找$data变量
            <string>
                var $data = new Object()
                var objsToFind = {'$data':$data}
                var objPaths = {}

                //填充objPaths，将从curObj中找到的对象的路径填充到objPaths里
                function fillObjPath(curObj){
                  if(!fillObjPath.stackInited){
                    fillObjPath.stack = []
                    fillObjPath.stackInited = true
                  }
                  var foundName = findKeyInObject(curObj,objsToFind)
                  if(foundName){
                    objPaths[foundName] = copyArray(fillObjPath.stack)
                    return
                  }
                  if(typeof(curObj) == 'object'){
                      for(var key in curObj){
                        fillObjPath.stack.push(key)
                        fillObjPath(curObj[key])
                        fillObjPath.stack.pop()
                      }
                  }
                }

                //在对象中根据值搜索key，找到返回key，找不到返回null
                function findKeyInObject(value,obj){
                  for(var key in obj){
                    if(obj[key] == value){
                      return key
                    }
                  }
                  return null
                }

                //数组拷贝
                function copyArray(arr) {
                    let res = []
                    for (let i = 0; i &lt; arr.length; i++) {
                     res.push(arr[i])
                    }
                    return res
                }
            </string>.Value
        jsEngine.Execute(strFindData)
        jsEngine.Execute(String.Format("var $_EPFTargetObject = {0}", responseJsonTemplate))
        jsEngine.Execute("fillObjPath($_EPFTargetObject)")
        Dim dataPath = jsEngine.Execute("objPaths['$data']").GetCompletionValue
        If dataPath.IsUndefined OrElse dataPath.IsNull Then
            Logger.PutMessage("$data not found!")
            Return
        End If
        '把Object()转为String()
        Dim dataPathArray = Util.ToArray(Of String)(dataPath.ToObject)
        Me.AnalyzedPullJsonVariablePositions.Add("$data", dataPathArray)
    End Sub

    '=======================================================================================
    Private MustInherit Class ModelAdapterAction

        Public Overridable Property URL As String
        Public Overridable Property Method As HTTPMethod
        Public Overridable ReadOnly Property Body As String
        Public Overridable Property ResponseCallback As Action(Of HttpWebResponse, WebException)

        Public MustOverride Function TryMerge(action As ModelAdapterAction, ByRef result As ModelAdapterAction) As Boolean

        Public Sub DoSync()
            Try
                Console.WriteLine(Me.Method.ToString & " " & Me.URL & vbCrLf & Me.Body)

                Dim httpWebRequest = CType(WebRequest.Create(Me.URL), HttpWebRequest)
                httpWebRequest.Method = Me.Method.ToString
                httpWebRequest.ContentType = "application/json"
                httpWebRequest.ContentLength = Body.Length
                Dim streamWrite As StreamWriter = New StreamWriter(httpWebRequest.GetRequestStream)
                streamWrite.WriteLine(Me.Body)

                Dim response As HttpWebResponse = httpWebRequest.GetResponse
                Call Me.ResponseCallback.Invoke(response, Nothing)
            Catch ex As WebException
                Call Me.ResponseCallback.Invoke(CType(ex.Response, HttpWebResponse), ex)
            End Try
        End Sub

        Protected Shared Function IndexRowPairsToJson(indexRowPairs As IndexRowPair()) As String
            Dim dics = (From indexRowPair In indexRowPairs
                        Select indexRowPair.RowData).ToArray
            Return New JavaScriptSerializer().Serialize(dics)
        End Function

    End Class

    Private Class AddRowAction
        Inherits ModelAdapterAction

        Private Property JsonTemplate As String
        Public Property IndexRowPairs As IndexRowPair()

        Public Overrides ReadOnly Property Body As String
            Get
                Static jsEngine As New Jint.Engine
                Dim dataJson = IndexRowPairsToJson(Me.IndexRowPairs)
                jsEngine.Execute(String.Format("$data = {0};", dataJson))
                Dim json = jsEngine.Execute(String.Format("JSON.stringify({0})", Me.JsonTemplate)).GetCompletionValue.ToString
                Return json
            End Get
        End Property

        Public Sub New(URL As String, method As HTTPMethod, indexRowPairs As IndexRowPair(), pushJsonTemplate As String, callback As Action(Of HttpWebResponse, WebException))
            Me.URL = URL
            Me.Method = method
            Me.JsonTemplate = pushJsonTemplate
            Me.IndexRowPairs = indexRowPairs
            Me.ResponseCallback = callback
        End Sub

        Public Overrides Function TryMerge(action As ModelAdapterAction, ByRef result As ModelAdapterAction) As Boolean
            If action Is Nothing Then
                result = Nothing
                Return False
            End If

            Select Case action.GetType
                Case GetType(UpdateRowAction)
                    Dim updateAction = CType(action, UpdateRowAction)
                    For Each indexRow In updateAction.IndexRowPairs
                        Dim row = indexRow.Index
                        Dim rowData = indexRow.RowData
                        Dim myIndexRow = (From ir In Me.IndexRowPairs
                                          Where ir.Index = row
                                          Select ir).FirstOrDefault
                        If myIndexRow Is Nothing Then Continue For
                        Dim myRowData = myIndexRow.RowData
                        For Each field In rowData
                            If myRowData.ContainsKey(field.Key) Then
                                myRowData(field.Key) = field.Value
                            Else
                                myRowData.Add(field.Key, field.Value)
                            End If
                        Next
                    Next
                    result = Me
                    Return True
            End Select

            result = Nothing
            Return False
        End Function
    End Class

    Private Class UpdateRowAction
        Inherits ModelAdapterAction

        Private Property JsonTemplate As String
        Public Property IndexRowPairs As IndexRowPair()

        Public Overrides ReadOnly Property Body As String
            Get
                Static jsEngine As New Jint.Engine
                Dim dataJson = IndexRowPairsToJson(Me.IndexRowPairs)
                jsEngine.Execute(String.Format("$data = {0};", dataJson))
                Dim json = jsEngine.Execute(String.Format("JSON.stringify({0});", Me.JsonTemplate)).GetCompletionValue.ToString
                Return json
            End Get
        End Property

        Public Sub New(URL As String, method As HTTPMethod, indexRowPairs As IndexRowPair(), pushJsonTemplate As String, callback As Action(Of HttpWebResponse, WebException))
            Me.URL = URL
            Me.Method = method
            Me.JsonTemplate = pushJsonTemplate
            Me.IndexRowPairs = indexRowPairs
            Me.ResponseCallback = callback
        End Sub

        Public Overrides Function TryMerge(action As ModelAdapterAction, ByRef result As ModelAdapterAction) As Boolean
            If action Is Nothing Then
                result = Nothing
                Return False
            End If

            Select Case action.GetType
                Case GetType(UpdateRowAction)
                    Dim updateAction = CType(action, UpdateRowAction)
                    For Each indexRow In updateAction.IndexRowPairs
                        Dim row = indexRow.Index
                        Dim rowData = indexRow.RowData
                        Dim myIndexRow = (From ir In Me.IndexRowPairs
                                          Where ir.Index = row
                                          Select ir).FirstOrDefault
                        If myIndexRow Is Nothing Then Continue For
                        Dim myRowData = myIndexRow.RowData
                        For Each field In rowData
                            If myRowData.ContainsKey(field.Key) Then
                                myRowData(field.Key) = field.Value
                            Else
                                myRowData.Add(field.Key, field.Value)
                            End If
                        Next
                    Next
                    result = Me
                    Return True
            End Select

            result = Nothing
            Return False
        End Function
    End Class
End Class
