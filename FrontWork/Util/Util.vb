﻿Imports System.Linq
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
            Dim objLength = CType(objPropertyLength.GetValue(obj, Nothing), Integer)
            Dim result(objLength - 1) As TElem
            For i As Integer = 0 To objLength - 1
                Dim srcValue = objMethodGetValue.Invoke(obj, New Object() {i})
                result(i) = System.Convert.ChangeType(srcValue, GetType(TElem))
            Next
            Return result
        End If
    End Function

    Public Shared Function Range(start As Long, [end] As Long) As Long()
        Dim length = [end] - start
        Dim result(length - 1) As Long
        For i = 0 To length - 1
            result(i) = start + i
        Next
        Return result
    End Function

    Public Shared Function Times(Of T)(data As T, repeatTimes As Long) As T()
        Dim result(repeatTimes - 1) As T
        For i = 0 To repeatTimes - 1
            result(i) = data
        Next
        Return result
    End Function
End Class
