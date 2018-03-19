Imports System.Linq
Imports System.Reflection
Imports System.Text
Imports System.Web.Script.Serialization

Friend Class Util
    Private Shared jsEngine As New Jint.Engine

    Public Shared Sub PrintDataTable(table As DataTable)
        '打印表头
        For Each column As DataColumn In table.Columns
            Call Console.Write(column.ColumnName + vbTab)
        Next
        Call Console.WriteLine()
        '遍历打印行
        For Each row As DataRow In table.Rows
            For Each column As DataColumn In table.Columns
                Call Console.Write(If(row(column) Is Nothing, "", row(column).ToString + vbTab))
            Next
            Call Console.WriteLine()
        Next
    End Sub

    Public Shared Function ToArray(Of TElem)(obj As Object) As TElem()
        If obj Is Nothing Then
            Return New TElem() {}
        End If
        Dim objType = obj.GetType

        If Not objType.IsArray Then '源类型不是数组,则把obj作为新数组的唯一一项
            Return New TElem() {System.Convert.ChangeType(obj, GetType(TElem))}
        Else '源类型是数组，则直接遍历转型
            Dim objPropertyLength = objType.GetProperty("Length")
            Dim objMethodGetValue = objType.GetMethod("GetValue", New Type() {GetType(Integer)})
            Dim objLength = CType(objPropertyLength.GetValue(obj), Integer)
            Dim result(objLength - 1) As TElem
            For i As Integer = 0 To objLength - 1
                Dim srcValue = objMethodGetValue.Invoke(obj, New Object() {i})
                result(i) = System.Convert.ChangeType(srcValue, GetType(TElem))
            Next
            Return result
        End If
    End Function

    'Private Shared Function DictionaryToJson(Of TKey, TValue)(dic As Dictionary(Of TKey, TValue)) As String
    '    '将Data数组转换成json字符串
    '    Return New JavaScriptSerializer().Serialize(dic)
    '    'Dim sbDataJsonStr As New StringBuilder
    '    'sbDataJsonStr.Append("{")
    '    'For Each field In dic
    '    '    Dim valueStr = Nothing
    '    '    Dim tmp = Nothing
    '    '    '如果是数值类型，不加引号。否则加引号

    '    '    If Double.TryParse(CStr(field.Value), tmp) Then
    '    '        valueStr = Str(field.Value)
    '    '    Else
    '    '        valueStr = """" & CStr(field.Value) & """"
    '    '    End If
    '    '    sbDataJsonStr.AppendFormat("""" & field.Key & """:" & valueStr & ",")
    '    'Next
    '    'sbDataJsonStr.Length -= 1
    '    'sbDataJsonStr.Append("},")

    '    'sbDataJsonStr.Length -= 1

    '    'jsEngine.Execute(String.Format("$data = {0};", sbDataJsonStr.ToString))
    '    'Dim result = jsEngine.Execute(String.Format("JSON.stringify({0})", Me.JsonTemplate)).GetCompletionValue.AsString
    '    'Return result
    'End Function

End Class
