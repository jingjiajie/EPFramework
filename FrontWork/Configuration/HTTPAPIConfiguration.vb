﻿Imports System.Linq
Imports System.Reflection
Imports Jint.Native

''' <summary>
''' HTTPAPI配置信息
''' </summary>
Public Class HTTPAPIConfiguration
    Implements ICloneable
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
    ''' 从Jint.JsValue转换
    ''' </summary>
    ''' <param name="methodListenerNames">方法监听器名称</param>
    ''' <param name="jsValue">JsValue对象</param>
    ''' <returns></returns>
    Public Shared Function FromJSValue(methodListenerNames As String(), jsValue As JsValue, refConfigurations As HTTPAPIConfiguration()) As HTTPAPIConfiguration()
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        '如果是数组，则遍历解析
        If jsValue.IsArray Then
            '先把引用的所有字段都拷贝过来，再根据新的配置进行更新
            Dim newHTTPAPIConfigurations As List(Of HTTPAPIConfiguration) = refConfigurations.ToList
            Dim jsArray = jsValue.AsArray
            For i As Integer = 0 To jsValue.AsArray.GetLength - 1
                If Not jsArray.Get(i).AsObject().HasOwnProperty("type") Then
                    Throw New Exception("HTTPAPIConfiguration must contains ""type"" property!")
                End If
                '新配置的type
                Dim type = jsArray.Get(i).AsObject.GetOwnProperty("type").Value.ToString
                '根据名称寻找是否存在引用的相应字段
                Dim foundRef = (From r In refConfigurations Where r.Type = type Select r).FirstOrDefault
                '生成合并后的配置（若无引用，则是全新配置）
                Dim newFieldConfiguration = MakeHTTPApiConfiguration(methodListenerNames, jsArray.Get(i), foundRef)
                '如果字段有引用，则将合并的配置放到引用原来的位置
                If foundRef IsNot Nothing Then
                    '寻找新配置在已经拷贝过来的引用中是第几个
                    Dim pos = newHTTPAPIConfigurations.FindIndex(
                        Function(api)
                            Return api.Type = foundRef.Type
                        End Function)
                    newHTTPAPIConfigurations(pos) = newFieldConfiguration
                Else '否则新增加一条配置
                    newHTTPAPIConfigurations.Add(newFieldConfiguration)
                End If
            Next
            Return newHTTPAPIConfigurations.ToArray
        Else '不是数组，报错
            Throw New Exception("Only js array is accepted to generate HTTPAPIConfiguration!")
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' 生成一项HTTPApiConfiguration
    ''' </summary>
    ''' <param name="methodListenerNames">方法监听器名称</param>
    ''' <param name="jsValue">JsValue对象</param>
    ''' <returns></returns>
    Private Shared Function MakeHTTPApiConfiguration(methodListenerNames As String(), jsValue As JsValue, refConfiguration As HTTPAPIConfiguration) As HTTPAPIConfiguration
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        If Not jsValue.IsObject Then
            Throw New Exception("Not a valid JsObject!")
            Return Nothing
        End If
        Dim jsEngine = ModeConfiguration.JsEngine
        Dim jsObject = jsValue.AsObject

        '新建HTTPAPIConfiguration
        Dim newHTTPAPIConfiguration As New HTTPAPIConfiguration
        If refConfiguration IsNot Nothing Then
            newHTTPAPIConfiguration = refConfiguration.Clone
        Else
            newHTTPAPIConfiguration = New HTTPAPIConfiguration
        End If

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
                    Dim strValue = jsProp.ToString
                    If strValue.StartsWith("$") Then
                        Dim methodName = value.ToString
                        '实例化一个绑定MethodListener方法的方法。运行时该方法动态执行MethodListener中的相应方法
                        Dim newFieldMethod As FieldMethod = FieldMethod.NewInstance(methodName, methodListenerNames, strValue)
                        prop.SetValue(newHTTPAPIConfiguration, newFieldMethod, Nothing)
                    Else
                        prop.SetValue(newHTTPAPIConfiguration, FieldMethod.NewInstance(strValue, strValue), Nothing)
                    End If

                    'Else
                    '    '否则，认为是js的方法
                    '    Dim jsMethod = jsProp
                    '    Dim fieldMethod As New FieldMethod
                    '    fieldMethod.Func =
                    '        Sub()
                    '            Try
                    '                Dim jsParams As JsValue() = (From p In fieldMethod.Parameters Select JsValue.FromObject(jsEngine, p)).ToArray
                    '                fieldMethod.ReturnValue = jsMethod.Invoke((From p In fieldMethod.Parameters Select JsValue.FromObject(jsEngine, p)).ToArray).ToObject
                    '                Exit Sub
                    '            Catch ex As Exception
                    '                Throw New Exception("Execute js function failed: " + ex.Message)
                    '                Exit Sub
                    '            End Try
                    '        End Sub
                    '    prop.SetValue(newHTTPAPIConfiguration, fieldMethod, Nothing)
                End If
            End If
        Next
        Return newHTTPAPIConfiguration
    End Function

    Public Function Clone() As Object Implements ICloneable.Clone
        Return Util.DeepClone(Me)
    End Function

    Public Sub SetMethodListener(methodListenerNames As String())
        Dim myType = Me.GetType
        Dim myFields = myType.GetFields
        For Each myField In myFields
            If myField.GetType.Equals(GetType(FieldMethod)) Then
                Dim fieldValue = CType(myField.GetValue(Me), FieldMethod)
                fieldValue.SetMethodListenerNames(methodListenerNames)
            End If
        Next
    End Sub
End Class
