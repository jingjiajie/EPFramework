﻿Imports System.Linq
Imports System.Reflection
Imports Jint.Native

Public Class HTTPAPIConfiguration
    Public Property APIType As String
    Public Property URL As String
    Public Property Method As String
    Public Property RequestBody As String
    Public Property ResponseBody As String
    Public Property Callback As FieldMethod

    Public Property MethodListener As IMethodListener

    Public Shared Function FromJSValue(jsEngine As Jint.Engine, jsValue As JsValue) As HTTPAPIConfiguration()
        Logger.SetMode(LogMode.PARSING_Configuration)
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        '如果是数组，则遍历解析
        If jsValue.IsArray Then
            Dim newHTTPAPIConfigurationArray(jsValue.AsArray.GetLength - 1) As HTTPAPIConfiguration
            For i As Integer = 0 To jsValue.AsArray.GetLength - 1
                newHTTPAPIConfigurationArray(i) = MakeHTTPApiConfiguration(jsEngine, jsValue.AsArray.Get(i))
                If newHTTPAPIConfigurationArray(i) Is Nothing Then Return Nothing
            Next
            Return newHTTPAPIConfigurationArray
        ElseIf jsValue.IsObject Then
            '如果是对象，则直接解析
            Dim newFieldConfiguration = MakeHTTPApiConfiguration(jsEngine, jsValue)
            If newFieldConfiguration Is Nothing Then Return Nothing
            Return New HTTPAPIConfiguration() {newFieldConfiguration}
        Else '既不是数组又不是对象，报错返回空
            Logger.PutMessage("Only js object or array is accepted to generate FieldConfiguration!")
            Return Nothing
        End If
    End Function

    Private Shared Function MakeHTTPApiConfiguration(jsEngine As Jint.Engine, jsValue As JsValue) As HTTPAPIConfiguration
        Logger.SetMode(LogMode.PARSING_Configuration)
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        If Not jsValue.IsObject Then
            Logger.PutMessage("Not a valid JsObject!")
            Return Nothing
        End If

        Dim jsObject = jsValue.AsObject

        '新建HTTPAPIConfiguration
        Dim newHTTPAPIConfiguration As New HTTPAPIConfiguration

        Dim typeHTTPAPIConfiguration = GetType(HTTPAPIConfiguration)
        '遍历字典，赋值给HTTPAPIConfiguration
        For Each item In jsObject.GetOwnProperties
            Dim key = item.Key
            Dim value = item.Value.Value.ToObject
            '获取js字段对应的HTTPAPIConfiguration属性，找不到就跳过并报错
            Dim prop = typeHTTPAPIConfiguration.GetProperty(key, BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.IgnoreCase)
            If prop Is Nothing Then
                Logger.PutMessage("can not resolve property:""" + key + """ in json configure")
                Continue For
            ElseIf prop.PropertyType <> GetType(FieldMethod) Then '如果不是函数，则直接赋值
                prop.SetValue(newHTTPAPIConfiguration, value, Nothing)
            Else '如果是函数，特殊处理
                Dim jsProp = item.Value.Value
                If jsProp.IsString Then '如果为字符串，则调用Controller的方法
                    Dim methodName = value.ToString
                    '实例化一个绑定MethodListener方法的方法。运行时该方法动态执行MethodListener中的相应方法
                    Dim fieldMethod As New FieldMethod
                    fieldMethod.Func =
                        Sub()
                            Logger.SetMode(LogMode.REFRESH_VIEW)
                            If newHTTPAPIConfiguration.MethodListener Is Nothing Then
                                Logger.PutMessage("No MethodListener setted!")
                                Return
                            End If
                            Dim method = newHTTPAPIConfiguration.MethodListener.GetType().GetMethod(methodName, BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.IgnoreCase)
                            If method Is Nothing Then
                                Logger.PutMessage("Method """ + methodName + """ not found in current MethodListener!")
                                Return
                            End If
                            Try
                                fieldMethod.ReturnValue = method.Invoke(newHTTPAPIConfiguration.MethodListener, fieldMethod.Parameters)
                                Return
                            Catch ex As Exception
                                Logger.PutMessage("Bind method """ + methodName + """ failed: " + ex.Message)
                                Return
                            End Try
                        End Sub
                    prop.SetValue(newHTTPAPIConfiguration, fieldMethod, Nothing)
                Else '否则，认为是js的方法
                    Dim jsMethod = jsProp
                    Dim fieldMethod As New FieldMethod
                    fieldMethod.Func =
                        Sub()
                            Logger.SetMode(LogMode.INIT_VIEW)
                            Try
                                Dim jsParams As JsValue() = (From p In fieldMethod.Parameters Select JsValue.FromObject(jsEngine, p)).ToArray
                                fieldMethod.ReturnValue = jsMethod.Invoke((From p In fieldMethod.Parameters Select JsValue.FromObject(jsEngine, p)).ToArray).ToObject
                                Exit Sub
                            Catch ex As Exception
                                Logger.PutMessage("Execute js function failed: " + ex.Message)
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