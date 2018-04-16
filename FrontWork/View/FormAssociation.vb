Imports System.Threading

Partial Public Class FormAssociation
    Inherits Form

    Public Enum MoveDirection
        UP
        DOWN
    End Enum

    Private textBox As TextBox = Nothing

    Private Property AssociationFunc As Func(Of String, AssociationItem())

    Public Property Selected As Boolean = False
    Public Property StayVisible As Boolean = False '是否保持可视
    Public Property StayUnvisible As Boolean = False '是否保持不可视

    Public Sub SetAssociationFunc(func As Func(Of String, AssociationItem()))
        Me.AssociationFunc = func
    End Sub

    Public Sub New(ByVal textBox As TextBox)
        InitializeComponent()
        Call MyBase.Show()
        Call MyBase.Hide()
        Me.textBox = textBox
        AddHandler textBox.PreviewKeyDown, AddressOf textBox_PreviewKeyDown
        AddHandler textBox.TextChanged, AddressOf textBox_TextChanged
        AddHandler textBox.Leave, AddressOf textBox_Leave
        AddHandler textBox.VisibleChanged, AddressOf textBox_VisibleChanged
        AddHandler Me.FindTopParentControl(textBox).LocationChanged, AddressOf textBoxBaseForm_LocationChanged
        AddHandler Me.GotFocus, AddressOf formAssociate_GotFocus
    End Sub

    Private Sub textBoxBaseForm_LocationChanged(ByVal sender As Object, ByVal e As EventArgs)
        If Me.Visible <> True Then Return
        Me.AdjustPosition()
    End Sub

    Private Sub textBox_VisibleChanged(ByVal sender As Object, ByVal e As EventArgs)
        If Not Me.textBox.Visible Then
            Call Me.Hide()
        End If
    End Sub

    Private Sub textBox_Leave(ByVal sender As Object, ByVal e As EventArgs)
        If Me.Focused <> True Then
            Call Me.Hide()
        End If
    End Sub

    Private Function FindTopParentControl(ByVal c As Control) As Control
        If c.Parent Is Nothing Then
            Return c
        Else
            Return FindTopParentControl(c.Parent)
        End If
    End Function

    Private Sub textBox_TextChanged(ByVal sender As Object, ByVal e As EventArgs)
        If Me.textBox.Focused And Me.AssociationFunc IsNot Nothing Then
            Call Me.Show()
            Call Me.RefreshAssociation()
        End If
        Me.Selected = False
    End Sub

    Public Sub RefreshAssociation()
        Static newestListBoxDataTime = DateTime.Now
        If Me.StayUnvisible Then Return
        If String.IsNullOrEmpty(textBox.Text) OrElse Me.AssociationFunc Is Nothing Then
            Me.Hide()
            Return
        End If
        Call Me.listBox.Items.Clear()
        Me.listBox.Items.Add(New AssociationItem() With {.Word = "加载中..."})
        Dim threadGetItems = New Thread(
            Sub()
                Dim threadStartTime As DateTime = DateTime.Now
                newestListBoxDataTime = threadStartTime
                Try
                    Dim data = Me.AssociationFunc.Invoke(textBox.Text)
                    If newestListBoxDataTime > threadStartTime Then '如果已经有更新的联想返回了，本次联想就废弃
                        Return
                    End If

                    Me.Invoke(New Action(Sub()
                                             Me.listBox.Items.Clear()
                                             Me.listBox.Items.AddRange(data)
                                             If data.Length = 0 Then
                                                 Me.Hide()
                                             ElseIf Me.Visible = False AndAlso textBox.Visible = True Then
                                                 Me.Show()
                                             End If
                                         End Sub))
                Catch
                    Return
                End Try
            End Sub)
        Call threadGetItems.Start()
    End Sub

    Private Sub textBox_PreviewKeyDown(ByVal sender As Object, ByVal e As PreviewKeyDownEventArgs)
        If e.KeyCode = Keys.Up Then
            Me.MoveSelection(MoveDirection.UP)
        ElseIf e.KeyCode = Keys.Down Then
            Me.MoveSelection(MoveDirection.DOWN)
        ElseIf e.KeyCode = Keys.Enter Then
            Me.SelectItem()
        End If
    End Sub

    Private Sub SelectItem()
        If Me.Visible = False Then
            Return
        End If

        If Me.listBox.SelectedItem IsNot Nothing Then
            Me.StayVisible = True
            RemoveHandler textBox.TextChanged, AddressOf Me.textBox_TextChanged
            textBox.Text = (TryCast(Me.listBox.SelectedItem, AssociationItem)).Word
            AddHandler textBox.TextChanged, AddressOf Me.textBox_TextChanged
            Me.Selected = True
            Me.StayVisible = False
            Me.Hide()
        End If
    End Sub

    Private Sub formAssociate_GotFocus(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.GotFocus
        Me.GiveBackFocus()
    End Sub

    Public Sub MoveSelection(ByVal direction As MoveDirection)
        If direction = MoveDirection.UP Then
            If Me.listBox.SelectedIndex > 0 Then
                Me.listBox.SelectedIndex -= 1
            End If
        ElseIf direction = MoveDirection.DOWN Then
            If Me.listBox.SelectedIndex < Me.listBox.Items.Count - 1 Then
                Me.listBox.SelectedIndex += 1
            End If
        End If
    End Sub

    Private Sub AdjustPosition()
        Dim textBoxScreenPosition As Point = textBox.PointToScreen(New Point(0, 0))
        Dim x = textBoxScreenPosition.X - Me.Padding.Left + textBox.Padding.Left - 2
        Dim y = textBoxScreenPosition.Y + textBox.Height - Me.Padding.Top - 3
        Me.SetPosition(x, y)
        Me.GiveBackFocus()
    End Sub

    Private Sub GiveBackFocus()
        If Me.textBox IsNot Nothing Then
            Me.textBox.Focus()
            If textBox.SelectionLength > 0 Then
                textBox.SelectionLength = 0
                textBox.SelectionStart = textBox.Text.Length
            End If
        End If
    End Sub

    Public Shadows Sub Show()
        If Me.StayUnvisible Then Return
        If Me.Visible = False Then
            MyBase.Show()
        End If

        Me.AdjustPosition()
    End Sub

    Private Sub SetPosition(ByVal x As Integer, ByVal y As Integer)
        Me.Location = New Point(x, y)
    End Sub

    Private Sub FormAssociate_Load(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Const WS_EX_NOACTIVATE As Integer = 134217728

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_NOACTIVATE
            Return cp
        End Get
    End Property


    Public Shadows Sub Hide()
        If Me.StayVisible Then
            Return
        Else
            MyBase.Hide()
        End If
    End Sub

    Private Sub listBox_DoubleClick(ByVal sender As Object, ByVal e As EventArgs) Handles listBox.DoubleClick
        Me.SelectItem()
    End Sub

    Private Sub listBox_Click(ByVal sender As Object, ByVal e As EventArgs) Handles listBox.Click
        GiveBackFocus()
    End Sub

    Private Sub listBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles listBox.SelectedIndexChanged

    End Sub
End Class