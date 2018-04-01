Imports System.Net

Public Class Form2
    Private tableLayoutView As TableLayoutPanelView
    Private tableLayoutView2 As TableLayoutPanelView
    Private reoGridView1 As ReoGridWorksheetView
    Private reoGridView2 As ReoGridWorksheetView
    Private reoGridView3 As ReoGridWorksheetView
    Dim adapter As JsonWebAPIModelAdapter
    Dim model As Model

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim metaDataStr = <string>[{
                                      mode:'default',
                                      fields:[{name:'id',displayName:'ID',visible:false},
                                              {name:'name',displayName:'姓名'},
                                              {name:'password',displayName:'密码'},
                                              {name:'role',displayName:'角色'},
                                              {name:'authorityString',displayName:'权限字符串'}]
                                  }]
                          </string>.Value

        Me.model = New Model(metaDataStr)

        Me.tableLayoutView = New TableLayoutPanelView(Me.TableLayoutPanel1)
        Me.tableLayoutView.SetMetaDataFromJson(metaDataStr, New MyMethods)
        Me.tableLayoutView.Model = model

        Me.tableLayoutView2 = New TableLayoutPanelView(Me.TableLayoutPanel2)
        Me.tableLayoutView2.SetMetaDataFromJson(metaDataStr, New MyMethods)
        Me.tableLayoutView2.Model = model

        Me.reoGridView1 = New ReoGridWorksheetView(Me.ReoGridControl1)
        Me.reoGridView1.SetMetaDataFromJson(metaDataStr, New MyMethods)
        Me.reoGridView1.Model = model

        Me.reoGridView2 = New ReoGridWorksheetView(Me.ReoGridControl2)
        Me.reoGridView2.SetMetaDataFromJson(metaDataStr, New MyMethods)
        Me.reoGridView2.Model = model

        Me.reoGridView3 = New ReoGridWorksheetView(Me.ReoGridControl3)
        Me.reoGridView3.SetMetaDataFromJson(metaDataStr, New MyMethods)
        Me.reoGridView3.Model = model

        Me.adapter = New JsonWebAPIModelAdapter()
        adapter.Model = model

        adapter.SetPullAPI("http://localhost.fiddler:9000/ledger/WMS_Template/person/{}", HTTPMethod.GET, "$data", Sub(res As HttpWebResponse, ex As WebException)
                                                                                                                       If ex IsNot Nothing Then
                                                                                                                           Console.WriteLine("Add请求失败：" & ex.Message)
                                                                                                                           Return
                                                                                                                       End If
                                                                                                                       Console.WriteLine("Add请求返回：" & Str(res.StatusCode))
                                                                                                                   End Sub)
        adapter.SetUpdateAPI("http://localhost.fiddler:9000/ledger/WMS_Template/person/", HTTPMethod.PUT, "$data", Sub(res As HttpWebResponse, ex As WebException)
                                                                                                                       If ex IsNot Nothing Then
                                                                                                                           Console.WriteLine("Update请求失败：" & ex.Message)
                                                                                                                           Return
                                                                                                                       End If
                                                                                                                       Console.WriteLine("Update请求返回：" & Str(res.StatusCode))
                                                                                                                   End Sub)
        adapter.SetRemoveAPI("http://localhost.fiddler:9000/ledger/WMS_Template/person/{mapProperty($data,'id')}", HTTPMethod.DELETE, Nothing, Sub(res As HttpWebResponse, ex As WebException)
                                                                                                                                                   If ex IsNot Nothing Then
                                                                                                                                                       Console.WriteLine("Remove请求失败：" & ex.Message)
                                                                                                                                                       Return
                                                                                                                                                   End If
                                                                                                                                                   Console.WriteLine("Remove请求返回：" & Str(res.StatusCode))
                                                                                                                                               End Sub)
        adapter.SetAddAPI("http://localhost.fiddler:9000/ledger/WMS_Template/person/", HTTPMethod.POST, "$data", Sub(res As HttpWebResponse, ex As WebException)
                                                                                                                     If ex IsNot Nothing Then
                                                                                                                         Console.WriteLine("Add请求失败：" & ex.Message)
                                                                                                                         Return
                                                                                                                     End If
                                                                                                                     Console.WriteLine("Add请求返回：" & Str(res.StatusCode))
                                                                                                                 End Sub)
    End Sub

    Private Sub ButtonSwitchToTableLayout_Click(sender As Object, e As EventArgs)
        'Me.controller.View = Me.tableLayoutView
    End Sub

    Private Sub ButtonSwitchToReoGrid_Click(sender As Object, e As EventArgs)
        'Me.controller.View = Me.reoGridView
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.model.UpdateRow(0, New Dictionary(Of String, Object)() From
                           {{"Name", "小明"}, {"Age", 10}, {"Type", "学生"}})
    End Sub

    Private Sub TableLayoutPanel1_Leave(sender As Object, e As EventArgs) Handles TableLayoutPanel1.Leave
        'Console.WriteLine("Leave")
    End Sub

    Private Sub TableLayoutPanel1_Enter(sender As Object, e As EventArgs) Handles TableLayoutPanel1.Enter
        'Console.WriteLine("Enter")
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.model.AddRow(New Dictionary(Of String, Object)() From
                           {{"Name", "小华"}, {"Age", 18}, {"Type", "学生"}})
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim selectionRange = Me.model.FirstSelectionRange
        Call Me.model.RemoveRows(selectionRange.Row, selectionRange.Rows)
    End Sub

    Private Sub ButtonPush_Click(sender As Object, e As EventArgs) Handles ButtonPull.Click
        Call adapter.PullFromServer()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles ButtonPush.Click
        Call Me.adapter.PushToServer()
    End Sub

    Private Sub Button4_Click_1(sender As Object, e As EventArgs) Handles Button4.Click
        Me.model.AddRow(Nothing)
    End Sub
End Class

Class MyMethods
    Implements IMethodListener
    Public Sub NameChanged()
        MessageBox.Show("这是本地函数弹出的编辑框")
    End Sub
End Class