Imports System.Linq

Public Class SearchView
    Inherits UserControl
    Implements IView
    Private _configuration As Configuration

    Public Event OnSearch As EventHandler(Of OnSearchEventArgs)

    Public Property Configuration As Configuration Implements IView.Configuration
        Get
            Return Me._configuration
        End Get
        Set(value As Configuration)
            If Me._configuration IsNot Nothing Then
                RemoveHandler Me._configuration.ConfigurationChanged, AddressOf Me.ConfigurationChanged
            End If
            Me._configuration = value
            Call Me.InitEditPanel()
            If Me._configuration IsNot Nothing Then
                AddHandler Me._configuration.ConfigurationChanged, AddressOf Me.ConfigurationChanged
            End If
        End Set
    End Property

    Protected Sub InitEditPanel()
        Call Me.ComboBoxSearchKey.Items.Clear()
        Call Me.ComboBoxOrderKey.Items.Clear()
        Call Me.ComboBoxOrderKey.Items.Add("无")

        If Me.Configuration Is Nothing Then Return
        Dim fieldConfiguration = Me.Configuration.GetFieldConfigurations
        If fieldConfiguration Is Nothing Then Return
        Dim fieldNames = (From field In fieldConfiguration
                          Where field.Visible
                          Select field.DisplayName).ToArray
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
        Dim eventArgs = Me.GetSearchArgs
        RaiseEvent OnSearch(Me, eventArgs)
    End Sub

    Private Sub ConfigurationChanged(sender As Object, e As ConfigurationChangedEventArgs)
        Call Me.InitEditPanel()
    End Sub

    Public Function GetSearchArgs() As OnSearchEventArgs
        If Me.Configuration Is Nothing Then
            Throw New Exception("Configuration not set in SearchWidget")
        End If
        Dim newSearchArgs = New OnSearchEventArgs

        Dim searchDisplayName = Me.ComboBoxSearchKey.SelectedItem?.ToString
        Dim relation As OnSearchEventArgs.Relation
        Dim searchValue = Me.TextBoxSearchCondition.Text
        Dim searchName = (From m In Me.Configuration.GetFieldConfigurations()
                          Where m.DisplayName = searchDisplayName
                          Select m.Name).First
        Select Case Me.ComboBoxSearchRelation.SelectedIndex
            Case 0
                relation = OnSearchEventArgs.Relation.EQUAL
            Case 1
                relation = OnSearchEventArgs.Relation.GREATER_THAN
            Case 2
                relation = OnSearchEventArgs.Relation.LESS_THAN
        End Select

        If Me.ComboBoxOrderKey.SelectedIndex <> 0 Then
            Dim orderDisplayName = Me.ComboBoxOrderKey.SelectedItem?.ToString
            Dim orderName = (From m In Me.Configuration.GetFieldConfigurations
                             Where m.DisplayName = orderDisplayName
                             Select m.Name).First
            Dim order As OnSearchEventArgs.Order
            Select Case Me.ComboBoxOrder.SelectedIndex
                Case 0
                    order = OnSearchEventArgs.Order.ASC
                Case 1
                    order = OnSearchEventArgs.Order.DESC
            End Select
            newSearchArgs.Orders = {New OnSearchEventArgs.OrderConditionItem(orderName, order)}
        End If
        newSearchArgs.Conditions = {New OnSearchEventArgs.SearchConditionItem(searchName, relation, {searchValue})}
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
