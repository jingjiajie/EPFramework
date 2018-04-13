Imports System.Linq

Public Class SearchWidget
    Private _metaData As EditPanelMetaData
    Private _mode As String = "default"

    Public Event OnSearch(args As SearchArgs)

    Public Property MetaData As EditPanelMetaData
        Get
            Return Me._metaData
        End Get
        Set(value As EditPanelMetaData)
            If Me._metaData Is value Then Return
            Me._metaData = value
            If Not String.IsNullOrEmpty(Me._mode) Then
                Call Me.InitPanel()
            End If
        End Set
    End Property

    Public Property Mode As String
        Get
            Return Me._mode
        End Get
        Set(value As String)
            Me._mode = value
            If Me.MetaData IsNot Nothing Then
                Call Me.InitPanel()
            End If
        End Set
    End Property

    Public Sub SetMetaData(strMetaData As String)
        Logger.SetMode(LogMode.INIT_VIEW)
        Dim jsEngine As New Jint.Engine
        Dim metaData = EditPanelMetaData.FromJson(jsEngine, strMetaData)
        If metaData Is Nothing Then Return
        Me.MetaData = metaData
    End Sub

    Private Sub InitPanel()
        Dim fieldMetaData = Me.MetaData.GetFieldMetaData(Me.Mode)
        Dim fieldNames = (From field In fieldMetaData
                          Where field.Visible
                          Select field.DisplayName).ToArray
        Call Me.ComboBoxSearchKey.Items.Clear()
        Call Me.ComboBoxOrderKey.Items.Clear()
        Call Me.ComboBoxOrderKey.Items.Add("无")
        Call Me.ComboBoxSearchKey.Items.AddRange(fieldNames)
        Call Me.ComboBoxOrderKey.Items.AddRange(fieldNames)
        If Me.ComboBoxSearchKey.Items.Count > 0 Then Me.ComboBoxSearchKey.SelectedIndex = 0
        If Me.ComboBoxOrderKey.Items.Count > 0 Then Me.ComboBoxOrderKey.SelectedIndex = 0
    End Sub

    Private Sub SearchWidget_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.ComboBoxSearchRelation.SelectedIndex = 0
        Me.ComboBoxOrder.SelectedIndex = 0
    End Sub

    Private Sub TableLayoutPanel1_Paint(sender As Object, e As PaintEventArgs) Handles TableLayoutPanel1.Paint

    End Sub

    Private Sub ButtonSearch_Click(sender As Object, e As EventArgs) Handles ButtonSearch.Click
        Dim searchArgs = Me.GetSearchArgs
        RaiseEvent OnSearch(searchArgs)
    End Sub

    Public Function GetSearchArgs() As SearchArgs
        If Me.MetaData Is Nothing Then
            Throw New Exception("MetaData not set in SearchWidget")
        End If
        If String.IsNullOrEmpty(Me.Mode) Then
            Throw New Exception("Mode not set in SearchWidget")
            Return Nothing
        End If
        Dim newSearchArgs = New SearchArgs

        Dim searchDisplayName = Me.ComboBoxSearchKey.SelectedItem?.ToString
        Dim relation As SearchArgs.Relation
        Dim searchValue = Me.TextBoxSearchCondition.Text
        Dim searchName = (From m In Me.MetaData.GetFieldMetaData(Me.Mode)
                          Where m.DisplayName = searchDisplayName
                          Select m.Name).First
        Select Case Me.ComboBoxSearchRelation.SelectedIndex
            Case 0
                relation = SearchArgs.Relation.EQUAL
            Case 1
                relation = SearchArgs.Relation.GREATER_THAN
            Case 2
                relation = SearchArgs.Relation.LESS_THAN
        End Select

        If Me.ComboBoxOrderKey.SelectedIndex <> 0 Then
            Dim orderDisplayName = Me.ComboBoxOrderKey.SelectedItem?.ToString
            Dim orderName = (From m In Me.MetaData.GetFieldMetaData(Me.Mode)
                             Where m.DisplayName = orderDisplayName
                             Select m.Name).First
            Dim order As SearchArgs.Order
            Select Case Me.ComboBoxOrder.SelectedIndex
                Case 0
                    order = SearchArgs.Order.ASC
                Case 1
                    order = SearchArgs.Order.DESC
            End Select
            newSearchArgs.Orders = {New SearchArgs.OrderConditionItem(orderName, order)}
        End If
        newSearchArgs.Conditions = {New SearchArgs.SearchConditionItem(searchName, relation, {searchValue})}
        Return newSearchArgs
    End Function

    Private Sub ComboBoxOrderKey_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxOrderKey.SelectedIndexChanged
        If Me.ComboBoxOrderKey.SelectedIndex = 0 Then
            Me.ComboBoxOrder.Enabled = False
        Else
            Me.ComboBoxOrder.Enabled = True
        End If
    End Sub
End Class
