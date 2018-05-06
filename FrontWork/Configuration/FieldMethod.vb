''' <summary>
''' 字段方法，对于函数类型的字段属性，字段属性的实际类型为此类型
''' </summary>
Public Class FieldMethod
    ''' <summary>
    ''' 配置中声明该方法的字符串
    ''' </summary>
    ''' <returns></returns>
    Public Property DeclareString As String

    ''' <summary>
    ''' 参数列表
    ''' </summary>
    ''' <returns></returns>
    Public Property Parameters As Object() = New Object() {}

    ''' <summary>
    ''' 返回值，Invoke后返回此值
    ''' </summary>
    ''' <returns></returns>
    Public Property ReturnValue As Object

    ''' <summary>
    ''' 要执行的函数
    ''' </summary>
    ''' <returns></returns>
    Public Property Func As Action

    ''' <summary>
    ''' 执行函数
    ''' </summary>
    ''' <param name="params">参数列表，执行函数前自动赋值到Parameters</param>
    ''' <returns>返回值，执行函数后，返回ReturnValue</returns>
    Public Function Invoke(ParamArray params As Object()) As Object
        If params IsNot Nothing Then
            Me.Parameters = params
        End If
        Me.ReturnValue = Nothing
        Me.Func?.Invoke()
        Return ReturnValue
    End Function
End Class
