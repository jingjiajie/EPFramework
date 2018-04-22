Imports System.Linq
Imports System.Reflection
Imports Jint.Native

''' <summary>
''' HTTPAPI配置信息
''' </summary>
Public Class HTTPAPIConfiguration
    ''' <summary>
    ''' API类型，如pull,add,update,remove等
    ''' </summary>
    ''' <returns></returns>
    Public Property Type As String

    ''' <summary>
    ''' API对应的URL
    ''' </summary>
    ''' <returns></returns>
    Public Property URL As String

    ''' <summary>
    ''' API的HTTP方法,如"GET","POST"等
    ''' </summary>
    ''' <returns></returns>
    Public Property Method As String

    ''' <summary>
    ''' 请求体
    ''' </summary>
    ''' <returns></returns>
    Public Property RequestBody As String

    ''' <summary>
    ''' 响应体
    ''' </summary>
    ''' <returns></returns>
    Public Property ResponseBody As String

    ''' <summary>
    ''' API回调函数。不管调用成功或者失败都会调用
    ''' 函数类型(HTTPWebResponse,WebException):Boolean 失败后是否继续执行下一个API
    ''' </summary>
    ''' <returns></returns>
    Public Property Callback As FieldMethod

    ''' <summary>
    ''' 所属配置模式
    ''' </summary>
    ''' <returns></returns>
    Public Property ModeConfiguration As ModeConfiguration

    ''' <summary>
    ''' 从Jint.JsValue转换
    ''' </summary>
    ''' <param name="modeConfiguration">所属配置模式</param>
    ''' <param name="jsValue">JsValue对象</param>
    ''' <returns></returns>
    Public Shared Function FromJSValue(modeConfiguration As ModeConfiguration, jsValue As JsValue) As HTTPAPIConfiguration()
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        '如果是数组，则遍历解析
        If jsValue.IsArray Then
            Dim newHTTPAPIConfigurationArray(jsValue.AsArray.GetLength - 1) As HTTPAPIConfiguration
            For i As Integer = 0 To jsValue.AsArray.GetLength - 1
                newHTTPAPIConfigurationArray(i) = MakeHTTPApiConfiguration(modeConfiguration, jsValue.AsArray.Get(i))
                If newHTTPAPIConfigurationArray(i) Is Nothing Then Return Nothing
            Next
            Return newHTTPAPIConfigurationArray
        ElseIf jsValue.IsObject Then
            '如果是对象，则直接解析
            Dim newHTTPAPIConfiguration = MakeHTTPApiConfiguration(modeConfiguration, jsValue)
            If newHTTPAPIConfiguration Is Nothing Then Return Nothing
            Return New HTTPAPIConfiguration() {newHTTPAPIConfiguration}
        Else '既不是数组又不是对象，报错返回空
            Throw New Exception("Only js object or array is accepted to generate FieldConfiguration!")
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' 生成一项HTTPApiConfiguration
    ''' </summary>
    ''' <param name="modeConfiguration">所属配置模式</param>
    ''' <param name="jsValue">JsValue对象</param>
    ''' <returns></returns>
    Private Shared Function MakeHTTPApiConfiguration(modeConfiguration As ModeConfiguration, jsValue As JsValue) As HTTPAPIConfiguration
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        If Not jsValue.IsObject Then
            Throw New Exception("Not a valid JsObject!")
            Return Nothing
        End If
        Dim jsEngine = modeConfiguration.JsEngine
        Dim jsObject = jsValue.AsObject

        '新建HTTPAPIConfiguration
        Dim newHTTPAPIConfiguration As New HTTPAPIConfiguration
        newHTTPAPIConfiguration.ModeConfiguration = modeConfiguration

        Dim typeHTTPAPIConfiguration = GetType(HTTPAPIConfiguration)
        '遍历字典，赋值给HTTPAPIConfiguration
        For Each item In jsObject.GetOwnProperties
            Dim key = item.Key
            Dim value = item.Value.Value.ToObject
            '获取js字段对应的HTTPAPIConfiguration属性，找不到就跳过并报错
            Dim prop = typeHTTPAPIConfiguration.GetProperty(key, BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.IgnoreCase)
            If prop Is Nothing Then
                Throw New Exception("can not resolve property:""" + key + """ in json configure")
                Continue For
            ElseIf prop.PropertyType <> GetType(FieldMethod) Then '如果不是函数，则直接赋值
                prop.SetValue(newHTTPAPIConfiguration, value, Nothing)
            Else '如果是函数，特殊处理
                Dim jsProp = item.Value.Value
                If jsProp.IsString Then '如果为字符串，则调用MethodListener的方法
                    Dim methodName = value.ToString
                    '实例化一个绑定MethodListener方法的方法。运行时该方法动态执行MethodListener中的相应方法
                    Dim fieldMethod As New FieldMethod
                    fieldMethod.Func =
                        Sub()
                            If String.IsNullOrWhiteSpace(modeConfiguration.MethodListenerName) Then
                                Throw New Exception("No MethodListener setted!")
                                Return
                            End If
                            Dim methodListener = MethodListenerContainer.Get(modeConfiguration.MethodListenerName)
                            If methodListener Is Nothing Then
                                Throw New Exception($"MethodListener: {modeConfiguration.MethodListenerName} not found in MethodListenerContainer!" & vbCrLf & "Have you register your MethodListener into MethodListenerContainer?")
                            End If
                            Dim method = methodListener.GetType().GetMethod(methodName, BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.IgnoreCase)
                            If method Is Nothing Then
                                Throw New Exception("Method """ + methodName + """ not found in current MethodListener!")
                                Return
                            End If
                            Try
                                fieldMethod.ReturnValue = method.Invoke(methodListener, fieldMethod.Parameters)
                                Return
                            Catch ex As Exception
                                Throw New Exception("Bind method """ + methodName + """ failed: " + ex.Message)
                                Return
                            End Try
                        End Sub
                    prop.SetValue(newHTTPAPIConfiguration, fieldMethod, Nothing)
                Else
                    '否则，认为是js的方法
                    Dim jsMethod = jsProp
                    Dim fieldMethod As New FieldMethod
                    fieldMethod.Func =
                        Sub()
                            Try
                                Dim jsParams As JsValue() = (From p In fieldMethod.Parameters Select JsValue.FromObject(jsEngine, p)).ToArray
                                fieldMethod.ReturnValue = jsMethod.Invoke((From p In fieldMethod.Parameters Select JsValue.FromObject(jsEngine, p)).ToArray).ToObject
                                Exit Sub
                            Catch ex As Exception
                                Throw New Exception("Execute js function failed: " + ex.Message)
                                Exit Sub
                            End Try
                        End Sub
                    prop.SetValue(newHTTPAPIConfiguration, fieldMethod, Nothing)
                End If
            End If
        Next
        Return newHTTPAPIConfiguration
    End Function

End Class
