Imports System.Text

Public Class Configurator
    Private Sub ButtonAddField_Click(sender As Object, e As EventArgs) Handles ButtonAddField.Click
        Me.ModelFields.AddRow(New Dictionary(Of String, Object) From {
         {"visible", True}, {"editable", True}
        })
    End Sub

    Private Sub ButtonRemoveField_Click(sender As Object, e As EventArgs) Handles ButtonRemoveField.Click
        Call Me.ModelFields.RemoveSelectedRows()
    End Sub

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint

    End Sub

    Private Sub ButtonGenerateJson_Click(sender As Object, e As EventArgs) Handles ButtonGenerateJson.Click
        If Me.ModelFields.RowCount = 0 Then
            Call MessageBox.Show("请添加字段！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Dim dataTable = Me.ModelFields.GetDataTable
        Dim sbJson As New StringBuilder
        sbJson.AppendLine("[")
        sbJson.AppendLine("{""mode"":""default"",")
        sbJson.AppendLine(vbTab & """fields"":[")
        For i = 0 To dataTable.Rows.Count - 1
            Dim row = dataTable.Rows(i)
            If IsDBNull(row("name")) Then Continue For
            sbJson.Append(vbTab & vbTab)
            sbJson.Append("{""name"":""" & row("name") & """,")
            If Not IsDBNull(row("displayName")) Then
                sbJson.Append("""displayName"":""" & row("displayName") & """,")
            End If
            If Not IsDBNull(row("visible")) Then
                sbJson.Append("""visible"":" & If(row("visible") = True, "true", "false") & ",")
            End If
            If Not IsDBNull(row("editable")) Then
                sbJson.Append("""editable"":" & If(row("editable") = True, "true", "false") & ",")
            End If
            If Not IsDBNull(row("placeHolder")) Then
                sbJson.Append("""placeHolder"":""" & row("placeHolder") & """,")
            End If
            If Not IsDBNull(row("values")) Then
                If CheckFunction(row("values")) Then
                    sbJson.Append("""values"":" & row("values") & ",")
                Else
                    sbJson.Append("""values"":""" & row("values") & """,")
                End If
            End If
            If Not IsDBNull(row("association")) Then
                If CheckFunction(row("association")) Then
                    sbJson.Append("""association"":" & row("association") & ",")
                Else
                    sbJson.Append("""association"":""" & row("association") & """,")
                End If
            End If
            If Not IsDBNull(row("forwardMapper")) Then
                Dim str As String = row("forwardMapper")
                If CheckFunction(str) Then
                    sbJson.Append("""forwardMapper"":" & str & ",")
                Else
                    sbJson.Append("""forwardMapper"":""" & str & """,")
                End If
            End If
            If Not IsDBNull(row("backwardMapper")) Then
                Dim str As String = row("backwardMapper")
                If CheckFunction(str) Then
                    sbJson.Append("""backwardMapper"":" & str & ",")
                Else
                    sbJson.Append("""backwardMapper"":""" & str & """,")
                End If
            End If
            If Not IsDBNull(row("contentChanged")) Then
                Dim str As String = row("contentChanged")
                If CheckFunction(str) Then
                    sbJson.Append("""contentChanged"":" & str & ",")
                Else
                    sbJson.Append("""contentChanged"":""" & str & """,")
                End If
            End If
            If Not IsDBNull(row("editEnded")) Then
                Dim str As String = row("editEnded")
                If CheckFunction(str) Then
                    sbJson.Append("""editEnded"":" & str & ",")
                Else
                    sbJson.Append("""editEnded"":""" & str & """,")
                End If
            End If
            sbJson.Length = sbJson.Length - 1
            sbJson.Append("}")
            If i <> dataTable.Rows.Count - 1 Then
                sbJson.Append(",")
            End If
            sbJson.AppendLine()
        Next
        sbJson.Append(vbTab)
        sbJson.AppendLine("]")
        sbJson.AppendLine("}")
        sbJson.Append("]")
        Me.TextBoxResult.Text = sbJson.ToString
        Me.TabControlBottom.SelectedIndex = 1
    End Sub

    Private Function CheckFunction(str As String) As Boolean
        Call str.Trim()
        Return str.StartsWith("function")
    End Function

    Private Sub ButtonLoad_Click(sender As Object, e As EventArgs) Handles ButtonLoad.Click
        Dim cfg As New Configuration
        Try
            cfg.Configurate(Me.TextBoxResult.Text)
        Catch ex As Exception
            Call MessageBox.Show("输入的Json不合法，错误信息：" & vbCrLf & ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try
        Dim dataTable = Me.ModelFields.GetDataTable
        Call dataTable.Rows.Clear()
        For Each curField In cfg.GetFieldConfigurations
            Dim newRow = dataTable.NewRow
            dataTable.Rows.Add(newRow)
            newRow("name") = curField.Name
            newRow("displayName") = curField.DisplayName
            newRow("visible") = curField.Visible
            newRow("editable") = curField.Editable
            newRow("placeHolder") = curField.PlaceHolder
            newRow("values") = curField.Values?.DeclareString
            newRow("association") = curField.Association?.DeclareString
            newRow("forwardMapper") = curField.ForwardMapper?.DeclareString
            newRow("backwardMapper") = curField.BackwardMapper?.DeclareString
            newRow("contentChanged") = curField.ContentChanged?.DeclareString
            newRow("editEnded") = curField.EditEnded?.DeclareString
        Next
        Call Me.ModelFields.Refresh(dataTable, Nothing, Nothing)
    End Sub
End Class