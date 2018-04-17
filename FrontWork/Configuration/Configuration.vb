Imports System.ComponentModel
Imports System.Drawing.Design
Imports System.Linq
Imports Jint.Native

Public Class Configuration
    Inherits UserControl

    Public Event ConfigurationChanged As EventHandler(Of ConfigurationChangedEventArgs)

    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Label1 As Label

    Private _configurationString As String
    Private _mode As String = "default"
    Private modeConfigurations As New List(Of ModeConfiguration)
    Private jsEngine As New Jint.Engine

    <Description("当前的模式，默认为default"), Category("FrontWork")>
    Public Property Mode As String
        Get
            Return Me._mode
        End Get
        Set(value As String)
            Me._mode = value
            RaiseEvent ConfigurationChanged(Me, New ConfigurationChangedEventArgs)
        End Set
    End Property

    <Description("配置字符串"), Category("FrontWork")>
    <Editor(GetType(ConfigurationEditor), GetType(UITypeEditor))>
    Public Property ConfigurationString As String
        Get
            Return Me._configurationString
        End Get
        Set(value As String)
            Me._configurationString = value
            Me.Configurate(Me._configurationString)
            RaiseEvent ConfigurationChanged(Me, New ConfigurationChangedEventArgs)
        End Set
    End Property

    Public Function ContainsMode(mode As String) As Boolean
        Dim foundConfiguration = (From m In Me.modeConfigurations Where m.Mode = mode Select m).FirstOrDefault
        If foundConfiguration IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Set the method Listener for the field Configuration
    ''' </summary>
    ''' <param name="methodListener">the method Listener for the mode specified</param>
    ''' <param name="mode">null for all mode</param>
    Public Sub SetMethodListener(methodListener As IMethodListener, Optional mode As String = Nothing)
        Logger.SetMode(LogMode.LOAD_MODE_METHODLISTENER)
        If mode Is Nothing Then
            For Each Configuration In Me.modeConfigurations
                Call Configuration.SetMethodListener(methodListener)
            Next
        Else
            Dim foundModeConfiguration = (From m In Me.modeConfigurations Where m.Mode = mode Select m).FirstOrDefault
            If foundModeConfiguration Is Nothing Then
                Call Logger.PutMessage("mode """ + mode + """ not found!")
                Return
            End If
            Call foundModeConfiguration.SetMethodListener(methodListener)
        End If
    End Sub

    'Public Sub SetFieldConfiguration(mode As String, fieldConfiguration As FieldConfiguration())
    '    Dim foundModeConfiguration = (From m In modeConfigurations Where m.Mode = mode Select m).FirstOrDefault
    '    If foundModeConfiguration Is Nothing Then
    '        Me.modeConfigurations.Add(New ModeConfiguration() With {
    '            .Mode = mode,
    '            .Fields = fieldConfiguration
    '        })
    '    Else
    '        foundModeConfiguration.Fields = fieldConfiguration
    '    End If
    'End Sub

    Public Function GetFieldConfigurations() As FieldConfiguration()
        Dim foundModeConfiguration = (From m In modeConfigurations Where m.Mode = Me.Mode Select m).FirstOrDefault
        If foundModeConfiguration Is Nothing Then
            Return {}
        Else
            Return foundModeConfiguration.Fields
        End If
    End Function

    Public Function GetHTTPAPIConfigurations() As HTTPAPIConfiguration()
        Dim foundModeConfiguration = (From m In modeConfigurations Where m.Mode = Me.Mode Select m).FirstOrDefault
        If foundModeConfiguration Is Nothing Then
            Return {}
        Else
            Return foundModeConfiguration.HTTPAPIs
        End If
    End Function

    Public Function Configurate(jsonStr As String) As Boolean
        Logger.SetMode(LogMode.PARSING_Configuration)
        Dim jsValue As JsValue = Nothing
        Try
            jsValue = jsEngine.Execute("$_FWJsonResult = " + jsonStr).GetValue("$_FWJsonResult")
        Catch ex As Exception
            Logger.PutMessage("Evaluate json expression failed: " + ex.Message)
            Return False
        End Try
        Dim newModeConfigurations = ModeConfiguration.FromJsValue(Me.jsEngine, jsValue)
        If newModeConfigurations Is Nothing Then Return False
        Me.modeConfigurations.Clear()
        Me.modeConfigurations.AddRange(newModeConfigurations)
        Return True
    End Function

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Configuration))
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
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
        Me.TableLayoutPanel1.Controls.Add(Me.PictureBox1, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Label1, 0, 1)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(180, 180)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'PictureBox1
        '
        Me.PictureBox1.BackgroundImage = CType(resources.GetObject("PictureBox1.BackgroundImage"), System.Drawing.Image)
        Me.PictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.PictureBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox1.Location = New System.Drawing.Point(0, 0)
        Me.PictureBox1.Margin = New System.Windows.Forms.Padding(0)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(180, 140)
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label1.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.5!)
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.Label1.Location = New System.Drawing.Point(0, 140)
        Me.Label1.Margin = New System.Windows.Forms.Padding(0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(180, 40)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Config"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Configuration
        '
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.DoubleBuffered = True
        Me.Name = "Configuration"
        Me.Size = New System.Drawing.Size(180, 180)
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Private Sub Configuration_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Me.DesignMode Then Me.Visible = False
        Call InitializeComponent()
    End Sub
End Class
