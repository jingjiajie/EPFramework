Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Web.Script.Serialization
Imports Jint.Native

''' <summary>
''' SearchView和JsonRESTSynchronizer的适配器
''' </summary>
Public Class SearchViewJsonRESTAdapter
    Inherits UserControl

    Private _synchronizer As JsonRESTSynchronizer
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label1 As Label
    Private _searchView As SearchView

    ''' <summary>
    ''' JsonRESTSynchronizer对象
    ''' </summary>
    ''' <returns></returns>
    <Description("JsonREST同步器对象"), Category("FrontWork")>
    Public Property Synchronizer As JsonRESTSynchronizer
        Get
            Return Me._synchronizer
        End Get
        Set(value As JsonRESTSynchronizer)
            Me._synchronizer = value
        End Set
    End Property

    ''' <summary>
    ''' 搜索视图对象
    ''' </summary>
    ''' <returns></returns>
    <Description("搜索视图（SearchView）对象"), Category("FrontWork")>
    Public Property SearchView As SearchView
        Get
            Return Me._searchView
        End Get
        Set(value As SearchView)
            If Me._searchView IsNot Nothing Then
                RemoveHandler Me._searchView.OnSearch, AddressOf Me.SearchViewOnSearch
            End If
            Me._searchView = value
            If Me.SearchView IsNot Nothing Then
                AddHandler Me._searchView.OnSearch, AddressOf Me.SearchViewOnSearch
            End If
        End Set
    End Property

    Public Sub New()
        If Not Me.DesignMode Then Me.Visible = False
        Call Me.InitializeComponent()
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

    Private Sub SearchViewOnSearch(sender As Object, args As OnSearchEventArgs)
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

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SearchViewJsonRESTAdapter))
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 1
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.Label3, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.PictureBox1, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Label1, 0, 1)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 3
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(180, 180)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label3.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label3.Location = New System.Drawing.Point(0, 150)
        Me.Label3.Margin = New System.Windows.Forms.Padding(0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(180, 30)
        Me.Label3.TabIndex = 3
        Me.Label3.Text = "Adapter"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'PictureBox1
        '
        Me.PictureBox1.BackgroundImage = CType(resources.GetObject("PictureBox1.BackgroundImage"), System.Drawing.Image)
        Me.PictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.PictureBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox1.Location = New System.Drawing.Point(0, 0)
        Me.PictureBox1.Margin = New System.Windows.Forms.Padding(0)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(180, 120)
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label1.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label1.Location = New System.Drawing.Point(0, 120)
        Me.Label1.Margin = New System.Windows.Forms.Padding(0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(180, 30)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "SearchView" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'SearchViewJsonRESTAdapter
        '
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "SearchViewJsonRESTAdapter"
        Me.Size = New System.Drawing.Size(180, 180)
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Private Sub SearchViewJsonRESTAdapter_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click

    End Sub
End Class
