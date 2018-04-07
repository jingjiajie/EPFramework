Imports System.Linq
Imports System.Reflection
Imports Jint.Native

Public Class FieldMetaData
    Private _name As String = Nothing
    Public Property DisplayName As String = Nothing
    Public Property Visible As Boolean = True
    Public Property Editable As Boolean = True
    Public Property Values As FieldMethod 'No Params Returns (in Object)()
    Public Property ForwardMapper As FieldMethod 'Params Object Returns String
    Public Property BackwordMapper As FieldMethod 'Params String Returns Object
    Public Property Association As FieldMethod 'Params String Returns AssociationItem()

    Public Property ContentChanged As FieldMethod 'Params String No Returns
    Public Property EditEnded As FieldMethod 'Params String No Returns
    Public Property Name As String
        Get
            Return Me._name
        End Get
        Set(value As String)
            If DisplayName Is Nothing Then DisplayName = value
            _name = value
        End Set
    End Property

    Public Property MethodListener As IMethodListener

    Public Shared Function FromJsValue(jsEngine As Jint.Engine, jsValue As JsValue) As FieldMetaData()
        Logger.SetMode(LogMode.PARSING_METADATA)
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        '如果是数组，则遍历解析
        If jsValue.IsArray Then
            Dim newFieldMetaDataArray(jsValue.AsArray.GetLength - 1) As FieldMetaData
            For i As Integer = 0 To jsValue.AsArray.GetLength - 1
                newFieldMetaDataArray(i) = MakeFieldMetaDataFromJsValue(jsEngine, jsValue.AsArray.Get(i))
                If newFieldMetaDataArray(i) Is Nothing Then Return Nothing
            Next
            Return newFieldMetaDataArray
        ElseIf jsValue.IsObject Then
            '如果是对象，则直接解析
            Dim newFieldMetaData = MakeFieldMetaDataFromJsValue(jsEngine, jsValue)
            If newFieldMetaData Is Nothing Then Return Nothing
            Return New FieldMetaData() {newFieldMetaData}
        Else '既不是数组又不是对象，报错返回空
            Logger.PutMessage("Only js object or array is accepted to generate FieldMetaData!")
            Return Nothing
        End If
    End Function

    Private Shared Function MakeFieldMetaDataFromJsValue(jsEngine As Jint.Engine, jsValue As JsValue) As FieldMetaData
        Logger.SetMode(LogMode.PARSING_METADATA)
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        If Not jsValue.IsObject Then
            Logger.PutMessage("Not a valid JsObject!")
            Return Nothing
        End If

        Dim jsObject = jsValue.AsObject

        '新建FieldMetaData
        Dim newFieldMetaData As New FieldMetaData

        Dim typeFieldMetaData = GetType(FieldMetaData)
        '遍历字典，赋值给FieldMetaData
        For Each item In jsObject.GetOwnProperties
            Dim key = item.Key
            Dim value = item.Value.Value.ToObject
            '获取js字段对应的FieldMetaData属性，找不到就跳过并报错
            Dim prop = typeFieldMetaData.GetProperty(key, BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.IgnoreCase)
            If prop Is Nothing Then
                Logger.PutMessage("can not resolve property:""" + key + """ in json configure")
                Continue For
            ElseIf prop.PropertyType <> GetType(FieldMethod) Then '如果不是函数，则直接赋值
                prop.SetValue(newFieldMetaData, value)
            Else '如果是函数，特殊处理
                Dim jsProp = item.Value.Value
                If jsProp.IsString Then '如果为字符串，则调用Controller的方法
                    Dim methodName = value.ToString
                    '实例化一个绑定MethodListener方法的方法。运行时该方法动态执行MethodListener中的相应方法
                    Dim fieldMethod As New FieldMethod
                    fieldMethod.Func =
                        Sub()
                            Logger.SetMode(LogMode.INIT_VIEW)
                            If newFieldMetaData.MethodListener Is Nothing Then
                                Logger.PutMessage("No MethodListener setted!")
                                Return
                            End If
                            Dim method = newFieldMetaData.MethodListener.GetType().GetMethod(methodName, BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.IgnoreCase)
                            If method Is Nothing Then
                                Logger.PutMessage("Method """ + methodName + """ not found in current MethodListener!")
                                Return
                            End If
                            Try
                                fieldMethod.ReturnValue = method.Invoke(newFieldMetaData.MethodListener, fieldMethod.Parameters)
                                Return
                            Catch ex As Exception
                                Logger.PutMessage("Bind method """ + methodName + """ failed: " + ex.Message)
                                Return
                            End Try
                        End Sub
                    prop.SetValue(newFieldMetaData, fieldMethod)
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
                    prop.SetValue(newFieldMetaData, fieldMethod)
                End If
            End If
        Next
        Return newFieldMetaData
    End Function

End Class
