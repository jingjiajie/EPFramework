Imports System.IO
Imports System.Net
Imports System.Web.Script.Serialization
Imports Jint.Native

Public Class SearchViewJsonRESTAdapter
    Public ReadOnly Property Synchronizer As JsonRESTSynchronizer
    Public ReadOnly Property SearchWidget As SearchView

    Public Sub Bind(searchWidget As SearchView, jsonRESTSynchronizer As JsonRESTSynchronizer)
        If Me.SearchWidget IsNot Nothing Then
            RemoveHandler Me.SearchWidget.OnSearch, AddressOf Me.SearchWidgetOnSearch
        End If
        Me._SearchWidget = searchWidget
        Me._Synchronizer = jsonRESTSynchronizer
        AddHandler Me.SearchWidget.OnSearch, AddressOf Me.SearchWidgetOnSearch
        Me.Synchronizer.PullAPI.Callback = AddressOf Me.SynchronizerPullCallback
    End Sub

    Private Function SynchronizerPullCallback(res As HttpWebResponse, ex As WebException) As Boolean
        If res IsNot Nothing AndAlso res.StatusCode = 200 Then Return True
        If res IsNot Nothing Then
            Dim responseBodyReader = New StreamReader(res.GetResponseStream)
            Dim responseBody = responseBodyReader.ReadToEnd
            MessageBox.Show(responseBody, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        ElseIf ex IsNot Nothing Then
            MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
        Return False
    End Function

    Private Sub SearchWidgetOnSearch(sender As Object, args As OnSearchEventArgs)
        Dim jsEngine As New Jint.Engine
        Dim jsSerializer = New JavaScriptSerializer
        '添加搜索条件
        Dim conditions = args.Conditions
        jsEngine.Execute("var $conditions = [];")
        If conditions IsNot Nothing Then
            For Each condition In conditions
                Dim key = condition.Key
                Dim relation = condition.Relation
                Dim values = condition.Values
                Dim jsonValues = jsSerializer.Serialize(values)
                jsEngine.Execute($"$conditions.push({{""key"":""{key}"",""relation"":""{relation}"",""values"":{jsonValues} }});")
            Next
        End If

        Dim orders = args.Orders
        jsEngine.Execute("var $orders = [];")
        If orders IsNot Nothing Then
            For Each orderItem In orders
                Dim key = orderItem.Key
                Dim order = orderItem.Order
                jsEngine.Execute($"$orders.push({{""key"":""{key}"",""order"":""{order}"" }});")
            Next
        End If

        Me.Synchronizer.PullAPI.SetRequestParameter("$conditions", jsEngine.GetValue("$conditions"))
        Me.Synchronizer.PullAPI.SetRequestParameter("$orders", jsEngine.GetValue("$orders"))
        Call Me.Synchronizer.PullFromServer()
    End Sub
End Class
