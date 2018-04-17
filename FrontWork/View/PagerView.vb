Imports System.ComponentModel

Public Class PagerView
    Implements IView
    Private _currentPage As Long = 1
    Private _totalPage As Long

    ''' <summary>
    ''' 配置中心对象，用来获取配置
    ''' </summary>
    ''' <returns>配置中心对象</returns>
    <Description("配置中心对象"), Category("FrontWork")>
    Public Property Configuration As Configuration Implements IView.Configuration

    ''' <summary>
    ''' 当前页码，从1开始
    ''' </summary>
    ''' <returns>当前页码</returns>
    <Description("当前页（从1开始）"), Category("FrontWork")>
    Public Property CurrentPage As Long
        Get
            Return Me._currentPage
        End Get
        Set(value As Long)
            If value > Me.TotalPage Then
                Throw New Exception($"CurrentPage:{value} exceeded TotalPage:{Me.TotalPage}")
            End If
            Me._currentPage = value
            Me.TextBoxCurrentPage.Text = CStr(value)
            Dim eventArgs As New PageChangedEventArgs(value)
            RaiseEvent OnCurrentPageChanged(eventArgs)
        End Set
    End Property

    ''' <summary>
    ''' 总页码，从1开始
    ''' </summary>
    ''' <returns>总页码</returns>
    <Description("总页码（从1开始）"), Category("FrontWork")>
    Public Property TotalPage As Long
        Get
            Return Me._totalPage
        End Get
        Set(value As Long)
            If value < Me.CurrentPage Then
                Throw New Exception($"TotalPage:{value} cannot be less than CurrentPage:{Me.CurrentPage}")
            End If
            Me._totalPage = value
            Me.LabelTotalPage.Text = CStr(value)
        End Set
    End Property

    ''' <summary>
    ''' 当前页码改变事件（可能由于用户点击下一页，或者程序改变CurrentPage触发）
    ''' </summary>
    ''' <param name="args">页码改变事件参数</param>
    Public Event OnCurrentPageChanged(args As PageChangedEventArgs)

    Private Sub TableLayoutPanel1_Paint(sender As Object, e As PaintEventArgs) Handles TableLayoutPanel1.Paint

    End Sub

    Private Sub TextBoxCurrentPage_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBoxCurrentPage.KeyPress
        If e.KeyChar = vbBack Then Return
        If Not Char.IsNumber(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

    Private Sub ButtonGo_Click(sender As Object, e As EventArgs) Handles ButtonGo.Click
        If Not (Me.CurrentPage >= 1 And Me.CurrentPage <= Me.TotalPage) Then
            MessageBox.Show($"请输入{1}到{Me.TotalPage}之间的页码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
        RaiseEvent OnCurrentPageChanged(New PageChangedEventArgs(Me.CurrentPage))
    End Sub

    Private Sub TextBoxCurrentPage_TextChanged(sender As Object, e As EventArgs) Handles TextBoxCurrentPage.TextChanged
        If String.IsNullOrEmpty(Me.TextBoxCurrentPage.Text) Then
            Me._currentPage = -1
            Return
        End If
        Me._currentPage = CInt(Me.TextBoxCurrentPage.Text)
    End Sub

    Private Sub ButtonNextPage_Click(sender As Object, e As EventArgs) Handles ButtonNextPage.Click
        If Me.CurrentPage = -1 Then
            Me.CurrentPage = 1
            Return
        End If
        If Me.CurrentPage >= Me.TotalPage Then Return
        Me.CurrentPage += 1
    End Sub

    Private Sub ButtonEndPage_Click(sender As Object, e As EventArgs) Handles ButtonEndPage.Click
        Me.CurrentPage = Me.TotalPage
    End Sub

    Private Sub ButtonPreviousPage_Click(sender As Object, e As EventArgs) Handles ButtonPreviousPage.Click
        If Me.CurrentPage = -1 Then
            Me.CurrentPage = 1
            Return
        End If
        If Me.CurrentPage <= 1 Then Return
        Me.CurrentPage -= 1
    End Sub

    Private Sub ButtonStartPage_Click(sender As Object, e As EventArgs) Handles ButtonStartPage.Click
        Me.CurrentPage = 1
    End Sub
End Class
