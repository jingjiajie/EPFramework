Imports System.ComponentModel
Imports System.Linq

''' <summary>
''' 搜索视图。提供基本的搜索条件，比较条件，排序条件等功能。
''' 配合各种适配器来适配不同的同步器，从而实现按搜索条件将数据从后端检索并同步到Model中
''' </summary>
Public Class SearchView
    Inherits UserControl
    Implements IView
    Private _configuration As Configuration

    ''' <summary>
    ''' 用户按下查询按键触发的事件
    ''' </summary>
    Public Event OnSearch As EventHandler(Of OnSearchEventArgs)

    ''' <summary>
    ''' 配置中心对象，用来获取配置
    ''' </summary>
    ''' <returns></returns>
    <Description("配置中心对象"), Category("FrontWork")>
    Public Property Configuration As Configuration
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

    ''' <summary>
    ''' 初始化搜索视图，允许重复调用
    ''' </summary>
    Protected Sub InitEditPanel()
        Call Me.ComboBoxSearchKey.Items.Clear()
        Call Me.ComboBoxSearchKey.Items.Add("无")
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
        Dim eventArgs = Me.GetSearchEventArgs
        RaiseEvent OnSearch(Me, eventArgs)
    End Sub

    Private Sub ConfigurationChanged(sender As Object, e As ConfigurationChangedEventArgs)
        Call Me.InitEditPanel()
    End Sub

    ''' <summary>
    ''' 根据用户的搜索条件设置，生成OnSearchEventArgs
    ''' </summary>
    ''' <returns>返回生成的OnSearchEventArgs</returns>
    Protected Function GetSearchEventArgs() As OnSearchEventArgs
        If Me.Configuration Is Nothing Then
            Throw New Exception("Configuration not set in SearchWidget")
        End If
        Dim newSearchArgs = New OnSearchEventArgs

        If Me.ComboBoxSearchKey.SelectedIndex <> 0 Then
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
            newSearchArgs.Conditions = {New OnSearchEventArgs.SearchConditionItem(searchName, relation, {searchValue})}
        End If

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
        Return newSearchArgs
    End Function

    Private Sub ComboBoxOrderKey_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxOrderKey.SelectedIndexChanged
        If Me.ComboBoxOrderKey.SelectedIndex = 0 Then
            Me.ComboBoxOrder.Enabled = False
        Else
            Me.ComboBoxOrder.Enabled = True
        End If
    End Sub

    Private Sub ComboBoxSearchKey_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxSearchKey.SelectedIndexChanged
        If Me.ComboBoxSearchKey.SelectedIndex = 0 Then
            Me.ComboBoxSearchRelation.Enabled = False
            Me.TextBoxSearchCondition.Enabled = False
        Else
            Me.ComboBoxSearchRelation.Enabled = True
            Me.TextBoxSearchCondition.Enabled = True
        End If
    End Sub
End Class
