Imports System.Linq
Imports System.Reflection
Imports System.Text
Imports Jint.Native

''' <summary>
''' 字段配置信息
''' </summary>
Public Class FieldConfiguration
    Private _name As String = Nothing
    ''' <summary>
    ''' 显示名称
    ''' </summary>
    ''' <returns></returns>
    Public Property DisplayName As String = Nothing

    ''' <summary>
    ''' 占位符
    ''' </summary>
    ''' <returns></returns>
    Public Property PlaceHolder As String = Nothing

    ''' <summary>
    ''' 是否可视
    ''' </summary>
    ''' <returns></returns>
    Public Property Visible As Boolean = True

    ''' <summary>
    ''' 是否可编辑
    ''' </summary>
    ''' <returns></returns>
    Public Property Editable As Boolean = True

    ''' <summary>
    ''' 字段可以选择的几种值，函数类型（）: Object数组
    ''' </summary>
    ''' <returns></returns>
    Public Property Values As FieldMethod 'No Params Returns (in Object)()

    ''' <summary>
    ''' 前向映射，从模型显示到视图时的转换。函数类型(Object) : String
    ''' </summary>
    ''' <returns></returns>
    Public Property ForwardMapper As FieldMethod 'Params Object Returns String

    ''' <summary>
    ''' 反向映射，从视图映射到模型时的转换。函数类型(String) : Object
    ''' </summary>
    ''' <returns></returns>
    Public Property BackwordMapper As FieldMethod 'Params String Returns Object

    ''' <summary>
    ''' 联想提示，回调传入用户已经输入的内容，返回联想提示内容。函数类型(String) : AssociationItem()
    ''' </summary>
    ''' <returns></returns>
    Public Property Association As FieldMethod 'Params String Returns AssociationItem()

    ''' <summary>
    ''' 内容改变事件，传入该字段用户已经输入的文本，函数类型(String)
    ''' </summary>
    ''' <returns></returns>
    Public Property ContentChanged As FieldMethod 'Params String No Returns

    ''' <summary>
    ''' 编辑结束事件，传入该字段用户已经输入的文本，函数类型(String)
    ''' </summary>
    ''' <returns></returns>
    Public Property EditEnded As FieldMethod 'Params String No Returns

    ''' <summary>
    ''' 字段名
    ''' </summary>
    ''' <returns></returns>
    Public Property Name As String
        Get
            Return Me._name
        End Get
        Set(value As String)
            If DisplayName Is Nothing Then DisplayName = value
            _name = value
        End Set
    End Property

    ''' <summary>
    ''' 字段所属的配置模式
    ''' </summary>
    ''' <returns></returns>
    Public Property ModeConfiguration As ModeConfiguration

    ''' <summary>
    ''' 从Jint.JsValue转换
    ''' </summary>
    ''' <param name="modeConfiguration">所属配置模式</param>
    ''' <param name="jsValue">要转换的JsValue</param>
    ''' <returns></returns>
    Public Shared Function FromJsValue(modeConfiguration As ModeConfiguration, jsValue As JsValue) As FieldConfiguration()
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        '如果是数组，则遍历解析
        If jsValue.IsArray Then
            Dim newFieldConfigurationArray(jsValue.AsArray.GetLength - 1) As FieldConfiguration
            For i As Integer = 0 To jsValue.AsArray.GetLength - 1
                newFieldConfigurationArray(i) = MakeFieldConfigurationFromJsValue(modeConfiguration, jsValue.AsArray.Get(i))
                If newFieldConfigurationArray(i) Is Nothing Then Return Nothing
            Next
            Return newFieldConfigurationArray
        ElseIf jsValue.IsObject Then
            '如果是对象，则直接解析
            Dim newFieldConfiguration = MakeFieldConfigurationFromJsValue(modeConfiguration, jsValue)
            If newFieldConfiguration Is Nothing Then Return Nothing
            Return New FieldConfiguration() {newFieldConfiguration}
        Else '既不是数组又不是对象，报错返回空
            Throw New Exception("Only js object or array is accepted to generate FieldConfiguration!")
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' 从JsValue转换一项字段配置
    ''' </summary>
    ''' <param name="modeConfiguration">配置模式</param>
    ''' <param name="jsValue">JsValue</param>
    ''' <returns></returns>
    Private Shared Function MakeFieldConfigurationFromJsValue(modeConfiguration As ModeConfiguration, jsValue As JsValue) As FieldConfiguration
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        If Not jsValue.IsObject Then
            Throw New Exception("Not a valid JsObject!")
            Return Nothing
        End If

        Dim jsObject = jsValue.AsObject

        '新建FieldConfiguration
        Dim newFieldConfiguration As New FieldConfiguration
        newFieldConfiguration.ModeConfiguration = modeConfiguration
        Dim jsEngine = modeConfiguration.JsEngine

        Dim typeFieldConfiguration = GetType(FieldConfiguration)
        '遍历字典，赋值给FieldConfiguration
        For Each item In jsObject.GetOwnProperties
            Dim key = item.Key
            Dim value = item.Value.Value.ToObject
            '获取js字段对应的FieldConfiguration属性，找不到就跳过并报错
            Dim prop = typeFieldConfiguration.GetProperty(key, BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.IgnoreCase)
            If prop Is Nothing Then
                Throw New Exception("can not resolve property:""" + key + """ in json configure")
                Continue For
            ElseIf prop.PropertyType <> GetType(FieldMethod) Then '如果不是函数，则直接赋值
                prop.SetValue(newFieldConfiguration, value, Nothing)
            Else '如果是函数，特殊处理
                Dim jsProp = item.Value.Value
                If jsProp.IsString Then '如果为字符串，则调用MethodListener的方法
                    Dim methodName = value.ToString
                    '实例化一个绑定MethodListener方法的方法。运行时该方法动态执行MethodListener中的相应方法
                    Dim fieldMethod As New FieldMethod
                    fieldMethod.Func =
                        Sub()
                            If String.IsNullOrWhiteSpace(modeConfiguration.MethodListenerName) Then
                                Throw New Exception("No MethodListener set!")
                                Return
                            End If
                            Dim methodListener = MethodListenerContainer.Get(modeConfiguration.MethodListenerName)
                            If methodListener Is Nothing Then
                                Throw New Exception(vbCrLf & $"  MethodListener: ""{modeConfiguration.MethodListenerName}"" not found!" &
                                                    vbCrLf &
                                                    "  Have you register your MethodListener into MethodListenerContainer before it should be called?")
                            End If
                            Dim method = methodListener.GetType().GetMethod(methodName, BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.IgnoreCase)
                            If method Is Nothing Then
                                Throw New Exception($"Method: ""{methodName}"" not found in MethodListener: {methodListener.GetType.Name}!")
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
                    prop.SetValue(newFieldConfiguration, fieldMethod, Nothing)
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
                    prop.SetValue(newFieldConfiguration, fieldMethod, Nothing)
                End If
            End If
        Next
        Return newFieldConfiguration
    End Function

    'Private Sub CopyMethodListenerFunctionsToJsEngine()
    '    If Me.ModeConfiguration.MethodListener Is Nothing Then Return
    '    Dim jsEngine = Me.ModeConfiguration.JsEngine
    '    Dim methodListener = Me.ModeConfiguration.MethodListener
    '    Dim listenerType = methodListener.GetType
    '    Dim methodInfos = listenerType.GetMethods()
    '    '将MethodListener对象设置到JsEngine里
    '    jsEngine.SetValue(listenerType.Name, methodListener)
    '    For Each methodInfo In methodInfos
    '        '将MethodInfo按照<methodListener对象名>_<methodInfo方法名>的格式设置到js引擎中
    '        Dim methodInfoJSName = listenerType.Name & "_" & methodInfo.Name
    '        jsEngine.SetValue(methodInfoJSName, methodInfo)
    '        Dim methodNameCamelCase = Char.ToLower(methodInfo.Name(0)) + methodInfo.Name.Substring(1)
    '        Dim sbJsMethod As StringBuilder = New StringBuilder($"function {methodNameCamelCase}(){{")
    '        sbJsMethod.AppendFormat(<string>
    '                                    return {0}.Invoke({1},arguments);
    '                                </string>.Value, methodInfoJSName, listenerType.Name)
    '        sbJsMethod.Append("}")
    '        jsEngine.Execute(sbJsMethod.ToString)
    '    Next
    'End Sub
End Class
