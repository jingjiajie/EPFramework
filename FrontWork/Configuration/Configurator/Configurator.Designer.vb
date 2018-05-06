<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Configurator
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Configurator))
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.TextBoxDescription = New System.Windows.Forms.TextBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.TableLayoutPanel3 = New System.Windows.Forms.TableLayoutPanel()
        Me.ButtonAddField = New System.Windows.Forms.Button()
        Me.ButtonRemoveField = New System.Windows.Forms.Button()
        Me.ButtonGenerateJson = New System.Windows.Forms.Button()
        Me.ConfigurationFields = New FrontWork.Configuration()
        Me.ModelFields = New FrontWork.Model()
        Me.BasicViewFields = New FrontWork.BasicView()
        Me.ReoGridViewFields = New FrontWork.ReoGridView()
        Me.TabControlBottom = New System.Windows.Forms.TabControl()
        Me.TabPageDescription = New System.Windows.Forms.TabPage()
        Me.TabPageResult = New System.Windows.Forms.TabPage()
        Me.TextBoxResult = New System.Windows.Forms.TextBox()
        Me.ButtonLoad = New System.Windows.Forms.Button()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.TableLayoutPanel2.SuspendLayout()
        Me.TableLayoutPanel3.SuspendLayout()
        Me.TabControlBottom.SuspendLayout()
        Me.TabPageDescription.SuspendLayout()
        Me.TabPageResult.SuspendLayout()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 500.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.Panel1, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.GroupBox1, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.GroupBox2, 1, 0)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 620.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(1574, 1029)
        Me.TableLayoutPanel1.TabIndex = 4
        '
        'Panel1
        '
        Me.TableLayoutPanel1.SetColumnSpan(Me.Panel1, 2)
        Me.Panel1.Controls.Add(Me.TabControlBottom)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel1.Location = New System.Drawing.Point(3, 623)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(1568, 403)
        Me.Panel1.TabIndex = 5
        '
        'TextBoxDescription
        '
        Me.TextBoxDescription.BackColor = System.Drawing.Color.White
        Me.TextBoxDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.TextBoxDescription.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TextBoxDescription.Location = New System.Drawing.Point(3, 3)
        Me.TextBoxDescription.Margin = New System.Windows.Forms.Padding(0)
        Me.TextBoxDescription.Multiline = True
        Me.TextBoxDescription.Name = "TextBoxDescription"
        Me.TextBoxDescription.ReadOnly = True
        Me.TextBoxDescription.Size = New System.Drawing.Size(1546, 347)
        Me.TextBoxDescription.TabIndex = 2
        Me.TextBoxDescription.Text = "字段的说明"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.BasicViewFields)
        Me.GroupBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.GroupBox1.Font = New System.Drawing.Font("黑体", 10.0!)
        Me.GroupBox1.Location = New System.Drawing.Point(3, 3)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(3, 10, 3, 3)
        Me.GroupBox1.Size = New System.Drawing.Size(494, 614)
        Me.GroupBox1.TabIndex = 6
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "字段配置"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.TableLayoutPanel2)
        Me.GroupBox2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.GroupBox2.Font = New System.Drawing.Font("黑体", 10.0!)
        Me.GroupBox2.Location = New System.Drawing.Point(503, 3)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(1068, 614)
        Me.GroupBox2.TabIndex = 7
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "所有字段"
        '
        'TableLayoutPanel2
        '
        Me.TableLayoutPanel2.ColumnCount = 1
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel2.Controls.Add(Me.ReoGridViewFields, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.TableLayoutPanel3, 0, 1)
        Me.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel2.Location = New System.Drawing.Point(3, 34)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        Me.TableLayoutPanel2.RowCount = 2
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70.0!))
        Me.TableLayoutPanel2.Size = New System.Drawing.Size(1062, 577)
        Me.TableLayoutPanel2.TabIndex = 4
        '
        'TableLayoutPanel3
        '
        Me.TableLayoutPanel3.ColumnCount = 9
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel3.Controls.Add(Me.ButtonAddField, 1, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.ButtonRemoveField, 3, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.ButtonGenerateJson, 5, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.ButtonLoad, 7, 0)
        Me.TableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel3.Location = New System.Drawing.Point(0, 507)
        Me.TableLayoutPanel3.Margin = New System.Windows.Forms.Padding(0)
        Me.TableLayoutPanel3.Name = "TableLayoutPanel3"
        Me.TableLayoutPanel3.RowCount = 1
        Me.TableLayoutPanel3.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel3.Size = New System.Drawing.Size(1062, 70)
        Me.TableLayoutPanel3.TabIndex = 4
        '
        'ButtonAddField
        '
        Me.ButtonAddField.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ButtonAddField.Location = New System.Drawing.Point(153, 3)
        Me.ButtonAddField.Name = "ButtonAddField"
        Me.ButtonAddField.Size = New System.Drawing.Size(154, 64)
        Me.ButtonAddField.TabIndex = 0
        Me.ButtonAddField.Text = "添加字段"
        Me.ButtonAddField.UseVisualStyleBackColor = True
        '
        'ButtonRemoveField
        '
        Me.ButtonRemoveField.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ButtonRemoveField.Location = New System.Drawing.Point(343, 3)
        Me.ButtonRemoveField.Name = "ButtonRemoveField"
        Me.ButtonRemoveField.Size = New System.Drawing.Size(154, 64)
        Me.ButtonRemoveField.TabIndex = 1
        Me.ButtonRemoveField.Text = "删除字段"
        Me.ButtonRemoveField.UseVisualStyleBackColor = True
        '
        'ButtonGenerateJson
        '
        Me.ButtonGenerateJson.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ButtonGenerateJson.Location = New System.Drawing.Point(533, 3)
        Me.ButtonGenerateJson.Name = "ButtonGenerateJson"
        Me.ButtonGenerateJson.Size = New System.Drawing.Size(154, 64)
        Me.ButtonGenerateJson.TabIndex = 2
        Me.ButtonGenerateJson.Text = "生成Json"
        Me.ButtonGenerateJson.UseVisualStyleBackColor = True
        '
        'ConfigurationFields
        '
        Me.ConfigurationFields.BackColor = System.Drawing.SystemColors.Control
        Me.ConfigurationFields.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.ConfigurationFields.ConfigurationString = resources.GetString("ConfigurationFields.ConfigurationString")
        Me.ConfigurationFields.Location = New System.Drawing.Point(182, 67)
        Me.ConfigurationFields.MethodListeners = New FrontWork.ModeMethodListenerNamePair(-1) {}
        Me.ConfigurationFields.Mode = "default"
        Me.ConfigurationFields.Name = "ConfigurationFields"
        Me.ConfigurationFields.Size = New System.Drawing.Size(180, 180)
        Me.ConfigurationFields.TabIndex = 1
        '
        'ModelFields
        '
        Me.ModelFields.BackColor = System.Drawing.SystemColors.Control
        Me.ModelFields.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.ModelFields.Configuration = Me.ConfigurationFields
        Me.ModelFields.FirstSelectionRange = Nothing
        Me.ModelFields.Location = New System.Drawing.Point(6, 67)
        Me.ModelFields.Name = "ModelFields"
        Me.ModelFields.SelectionRange = New FrontWork.Range(-1) {}
        Me.ModelFields.Size = New System.Drawing.Size(180, 180)
        Me.ModelFields.TabIndex = 0
        '
        'BasicViewFields
        '
        Me.BasicViewFields.Configuration = Me.ConfigurationFields
        Me.BasicViewFields.Dock = System.Windows.Forms.DockStyle.Fill
        Me.BasicViewFields.Font = New System.Drawing.Font("黑体", 10.0!)
        Me.BasicViewFields.ItemsPerRow = 1
        Me.BasicViewFields.Location = New System.Drawing.Point(3, 41)
        Me.BasicViewFields.Margin = New System.Windows.Forms.Padding(0)
        Me.BasicViewFields.Model = Me.ModelFields
        Me.BasicViewFields.Name = "BasicViewFields"
        Me.BasicViewFields.Size = New System.Drawing.Size(488, 570)
        Me.BasicViewFields.TabIndex = 0
        '
        'ReoGridViewFields
        '
        Me.ReoGridViewFields.Configuration = Me.ConfigurationFields
        Me.ReoGridViewFields.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ReoGridViewFields.Font = New System.Drawing.Font("黑体", 11.0!)
        Me.ReoGridViewFields.Location = New System.Drawing.Point(0, 0)
        Me.ReoGridViewFields.Margin = New System.Windows.Forms.Padding(0)
        Me.ReoGridViewFields.Model = Me.ModelFields
        Me.ReoGridViewFields.Name = "ReoGridViewFields"
        Me.ReoGridViewFields.Size = New System.Drawing.Size(1062, 507)
        Me.ReoGridViewFields.TabIndex = 3
        '
        'TabControlBottom
        '
        Me.TabControlBottom.Controls.Add(Me.TabPageDescription)
        Me.TabControlBottom.Controls.Add(Me.TabPageResult)
        Me.TabControlBottom.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TabControlBottom.Font = New System.Drawing.Font("黑体", 10.0!)
        Me.TabControlBottom.Location = New System.Drawing.Point(0, 0)
        Me.TabControlBottom.Name = "TabControlBottom"
        Me.TabControlBottom.SelectedIndex = 0
        Me.TabControlBottom.Size = New System.Drawing.Size(1568, 403)
        Me.TabControlBottom.TabIndex = 3
        '
        'TabPageDescription
        '
        Me.TabPageDescription.BackColor = System.Drawing.SystemColors.Control
        Me.TabPageDescription.Controls.Add(Me.ConfigurationFields)
        Me.TabPageDescription.Controls.Add(Me.ModelFields)
        Me.TabPageDescription.Controls.Add(Me.TextBoxDescription)
        Me.TabPageDescription.Location = New System.Drawing.Point(8, 42)
        Me.TabPageDescription.Name = "TabPageDescription"
        Me.TabPageDescription.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageDescription.Size = New System.Drawing.Size(1552, 353)
        Me.TabPageDescription.TabIndex = 0
        Me.TabPageDescription.Text = "说明"
        '
        'TabPageResult
        '
        Me.TabPageResult.BackColor = System.Drawing.SystemColors.Control
        Me.TabPageResult.Controls.Add(Me.TextBoxResult)
        Me.TabPageResult.Location = New System.Drawing.Point(8, 42)
        Me.TabPageResult.Name = "TabPageResult"
        Me.TabPageResult.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageResult.Size = New System.Drawing.Size(1552, 353)
        Me.TabPageResult.TabIndex = 1
        Me.TabPageResult.Text = "生成结果"
        '
        'TextBoxResult
        '
        Me.TextBoxResult.BackColor = System.Drawing.Color.White
        Me.TextBoxResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.TextBoxResult.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TextBoxResult.Location = New System.Drawing.Point(3, 3)
        Me.TextBoxResult.Margin = New System.Windows.Forms.Padding(0)
        Me.TextBoxResult.Multiline = True
        Me.TextBoxResult.Name = "TextBoxResult"
        Me.TextBoxResult.Size = New System.Drawing.Size(1546, 347)
        Me.TextBoxResult.TabIndex = 3
        '
        'ButtonLoad
        '
        Me.ButtonLoad.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ButtonLoad.Location = New System.Drawing.Point(723, 3)
        Me.ButtonLoad.Name = "ButtonLoad"
        Me.ButtonLoad.Size = New System.Drawing.Size(174, 64)
        Me.ButtonLoad.TabIndex = 3
        Me.ButtonLoad.Text = "从Json读取"
        Me.ButtonLoad.UseVisualStyleBackColor = True
        '
        'Configurator
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(12.0!, 24.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1574, 1029)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "Configurator"
        Me.Text = "Configurator"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.TableLayoutPanel2.ResumeLayout(False)
        Me.TableLayoutPanel3.ResumeLayout(False)
        Me.TabControlBottom.ResumeLayout(False)
        Me.TabPageDescription.ResumeLayout(False)
        Me.TabPageDescription.PerformLayout()
        Me.TabPageResult.ResumeLayout(False)
        Me.TabPageResult.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ModelFields As Model
    Friend WithEvents ConfigurationFields As Configuration
    Friend WithEvents BasicViewFields As BasicView
    Friend WithEvents ReoGridViewFields As ReoGridView
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents Panel1 As Panel
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents TableLayoutPanel2 As TableLayoutPanel
    Friend WithEvents TableLayoutPanel3 As TableLayoutPanel
    Friend WithEvents ButtonAddField As Button
    Friend WithEvents ButtonRemoveField As Button
    Friend WithEvents ButtonGenerateJson As Button
    Friend WithEvents TextBoxDescription As TextBox
    Friend WithEvents TabControlBottom As TabControl
    Friend WithEvents TabPageDescription As TabPage
    Friend WithEvents TabPageResult As TabPage
    Friend WithEvents TextBoxResult As TextBox
    Friend WithEvents ButtonLoad As Button
End Class
