Public Class Form2
    Private tableLayoutView As TableLayoutPanelView
    Private tableLayoutView2 As TableLayoutPanelView
    Private reoGridView1 As ReoGridWorksheetView
    Private reoGridView2 As ReoGridWorksheetView
    Private reoGridView3 As ReoGridWorksheetView
    Dim model As Model

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim dataTable = New DataTable()
        dataTable.Columns.AddRange(New DataColumn() {New DataColumn("Name"), New DataColumn("Age"), New DataColumn("Type")})
        dataTable.Rows.Add("小明", 5)
        dataTable.Rows.Add("小红", 10)
        Me.model = New Model(dataTable)
        'AddHandler model.DataAdded, Sub(dt As DataTable)
        '                                Console.Write("Added rows:")
        '                                For Each row In dt.Rows
        '                                    Call Console.Write(row(0) & " ")
        '                                Next
        '                                Call Console.WriteLine()
        '                            End Sub

        AddHandler model.RowUpdated, Sub(ev As ModelRowUpdatedEventArgs)
                                         Console.Write("Updated rows:")
                                         For Each item In ev.UpdatedRows
                                             Console.Write(Str(item.Index) & " " & CStr(item.RowData("Name")) & " ")
                                         Next
                                         Call Console.WriteLine()
                                     End Sub

        'AddHandler model.DataRemoved, Sub(dt As DataTable)
        '                                  Console.Write("Removed rows:")
        '                                  For Each row In dt.Rows
        '                                      Console.Write(row(0) & " ")
        '                                  Next
        '                                  Call Console.WriteLine()
        '                              End Sub

        Dim metaDataStr = <string>[{
                                      mode:'default',
                                      fields:[{name:'Name',displayName:'姓名'},
                                              {name:'Age',displayName:'年龄'},
                                              {name:'Type',displayName:'类型'}]
                                  }]
                          </string>.Value

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
        Me.model.RemoveRow(0)
    End Sub
End Class

Class MyMethods
    Implements IMethodListener
    Public Sub NameChanged()
        MessageBox.Show("这是本地函数弹出的编辑框")
    End Sub
End Class