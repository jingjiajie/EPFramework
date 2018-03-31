<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form2
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.ReoGridControl1 = New unvell.ReoGrid.ReoGridControl()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.ReoGridControl2 = New unvell.ReoGrid.ReoGridControl()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.ReoGridControl3 = New unvell.ReoGrid.ReoGridControl()
        Me.ButtonPull = New System.Windows.Forms.Button()
        Me.ButtonPush = New System.Windows.Forms.Button()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(560, 383)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'ReoGridControl1
        '
        Me.ReoGridControl1.BackColor = System.Drawing.Color.White
        Me.ReoGridControl1.ColumnHeaderContextMenuStrip = Nothing
        Me.ReoGridControl1.LeadHeaderContextMenuStrip = Nothing
        Me.ReoGridControl1.Location = New System.Drawing.Point(0, 430)
        Me.ReoGridControl1.Name = "ReoGridControl1"
        Me.ReoGridControl1.RowHeaderContextMenuStrip = Nothing
        Me.ReoGridControl1.Script = Nothing
        Me.ReoGridControl1.SheetTabContextMenuStrip = Nothing
        Me.ReoGridControl1.SheetTabNewButtonVisible = True
        Me.ReoGridControl1.SheetTabVisible = True
        Me.ReoGridControl1.SheetTabWidth = 60
        Me.ReoGridControl1.ShowScrollEndSpacing = True
        Me.ReoGridControl1.Size = New System.Drawing.Size(560, 398)
        Me.ReoGridControl1.TabIndex = 3
        Me.ReoGridControl1.Text = "ReoGridControl1"
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(1012, 43)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(164, 48)
        Me.Button1.TabIndex = 6
        Me.Button1.Text = "testUpdate"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(1012, 109)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(164, 48)
        Me.Button2.TabIndex = 7
        Me.Button2.Text = "testAdd"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(1191, 241)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(192, 74)
        Me.Button3.TabIndex = 8
        Me.Button3.Text = "删除"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'ReoGridControl2
        '
        Me.ReoGridControl2.BackColor = System.Drawing.Color.White
        Me.ReoGridControl2.ColumnHeaderContextMenuStrip = Nothing
        Me.ReoGridControl2.LeadHeaderContextMenuStrip = Nothing
        Me.ReoGridControl2.Location = New System.Drawing.Point(580, 430)
        Me.ReoGridControl2.Name = "ReoGridControl2"
        Me.ReoGridControl2.RowHeaderContextMenuStrip = Nothing
        Me.ReoGridControl2.Script = Nothing
        Me.ReoGridControl2.SheetTabContextMenuStrip = Nothing
        Me.ReoGridControl2.SheetTabNewButtonVisible = True
        Me.ReoGridControl2.SheetTabVisible = True
        Me.ReoGridControl2.SheetTabWidth = 60
        Me.ReoGridControl2.ShowScrollEndSpacing = True
        Me.ReoGridControl2.Size = New System.Drawing.Size(560, 398)
        Me.ReoGridControl2.TabIndex = 10
        Me.ReoGridControl2.Text = "ReoGridControl2"
        '
        'TableLayoutPanel2
        '
        Me.TableLayoutPanel2.ColumnCount = 2
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel2.Location = New System.Drawing.Point(580, 0)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        Me.TableLayoutPanel2.RowCount = 2
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel2.Size = New System.Drawing.Size(426, 383)
        Me.TableLayoutPanel2.TabIndex = 1
        '
        'ReoGridControl3
        '
        Me.ReoGridControl3.BackColor = System.Drawing.Color.White
        Me.ReoGridControl3.ColumnHeaderContextMenuStrip = Nothing
        Me.ReoGridControl3.LeadHeaderContextMenuStrip = Nothing
        Me.ReoGridControl3.Location = New System.Drawing.Point(1156, 430)
        Me.ReoGridControl3.Name = "ReoGridControl3"
        Me.ReoGridControl3.RowHeaderContextMenuStrip = Nothing
        Me.ReoGridControl3.Script = Nothing
        Me.ReoGridControl3.SheetTabContextMenuStrip = Nothing
        Me.ReoGridControl3.SheetTabNewButtonVisible = True
        Me.ReoGridControl3.SheetTabVisible = True
        Me.ReoGridControl3.SheetTabWidth = 60
        Me.ReoGridControl3.ShowScrollEndSpacing = True
        Me.ReoGridControl3.Size = New System.Drawing.Size(560, 398)
        Me.ReoGridControl3.TabIndex = 9
        Me.ReoGridControl3.Text = "ReoGridControl3"
        '
        'ButtonPull
        '
        Me.ButtonPull.Location = New System.Drawing.Point(1182, 35)
        Me.ButtonPull.Name = "ButtonPull"
        Me.ButtonPull.Size = New System.Drawing.Size(201, 65)
        Me.ButtonPull.TabIndex = 11
        Me.ButtonPull.Text = "TestPull"
        Me.ButtonPull.UseVisualStyleBackColor = True
        '
        'ButtonPush
        '
        Me.ButtonPush.Location = New System.Drawing.Point(1191, 136)
        Me.ButtonPush.Name = "ButtonPush"
        Me.ButtonPush.Size = New System.Drawing.Size(192, 75)
        Me.ButtonPush.TabIndex = 12
        Me.ButtonPush.Text = "TestPush"
        Me.ButtonPush.UseVisualStyleBackColor = True
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(1191, 333)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(192, 72)
        Me.Button4.TabIndex = 13
        Me.Button4.Text = "增加"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'Form2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(12.0!, 24.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(2443, 818)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.ButtonPush)
        Me.Controls.Add(Me.ButtonPull)
        Me.Controls.Add(Me.ReoGridControl3)
        Me.Controls.Add(Me.TableLayoutPanel2)
        Me.Controls.Add(Me.ReoGridControl2)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.ReoGridControl1)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "Form2"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Form2"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents ReoGridControl1 As unvell.ReoGrid.ReoGridControl
    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents Button3 As Button
    Friend WithEvents ReoGridControl2 As unvell.ReoGrid.ReoGridControl
    Friend WithEvents TableLayoutPanel2 As TableLayoutPanel
    Friend WithEvents ReoGridControl3 As unvell.ReoGrid.ReoGridControl
    Friend WithEvents ButtonPull As Button
    Friend WithEvents ButtonPush As Button
    Friend WithEvents Button4 As Button
End Class
