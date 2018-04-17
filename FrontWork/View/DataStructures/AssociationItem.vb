''' <summary>
''' 联想提示中的一项
''' </summary>
Public Class AssociationItem
    ''' <summary>
    ''' 联想的单词，用来补全到编辑框上
    ''' </summary>
    Public Word As String

    ''' <summary>
    ''' 单词的附加提示，附在Word后面，用户可以看到，Hint不会随Word补全到编辑框上
    ''' </summary>
    Public Hint As String

    Public Overrides Function ToString() As String
        Dim str As String = String.Format("{0,-15}", Word)
        If String.IsNullOrEmpty(Hint) = False Then
            str += " -" & Hint
        End If

        Return str
    End Function
End Class
