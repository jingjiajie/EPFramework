''' <summary>
''' 模式配置，一个配置中心包含若干模式配置。一个模式配置中包含若干字段，API配置等信息
''' </summary>
Public Class ModeConfiguration
    Private _methodListener As String

    ''' <summary>
    ''' 模式名称
    ''' </summary>
    ''' <returns></returns>
    Public Property Mode As String

    ''' <summary>
    ''' 字段配置信息
    ''' </summary>
    ''' <returns></returns>
    Public Property Fields As FieldConfiguration() = {}

    ''' <summary>
    ''' HTTPAPI配置信息
    ''' </summary>
    ''' <returns></returns>
    Public Property HTTPAPIs As HTTPAPIConfiguration() = {}

    ''' <summary>
    ''' 方法监听器
    ''' </summary>
    Public Property MethodListenerName As String
        Get
            Return Me._methodListener
        End Get
        Set(value As String)
            Me._methodListener = value
        End Set
    End Property

    ''' <summary>
    ''' 所属配置中心
    ''' </summary>
    ''' <returns></returns>
    Public Property Configuration As Configuration

    ''' <summary>
    ''' Js引擎
    ''' </summary>
    ''' <returns></returns>
    Public Property JsEngine As New Jint.Engine

    ''' <summary>
    ''' 从JsValue转换
    ''' </summary>
    ''' <param name="configuration">所属配置中心</param>
    ''' <param name="jsValue">JsValue对象</param>
    ''' <returns></returns>
    Public Shared Function FromJsValue(configuration As Configuration, jsValue As Jint.Native.JsValue) As ModeConfiguration()
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        '如果是数组，则遍历解析
        If jsValue.IsArray Then
            Dim newModeConfigurationArray(jsValue.AsArray.GetLength - 1) As ModeConfiguration
            For i As Integer = 0 To jsValue.AsArray.GetLength - 1
                newModeConfigurationArray(i) = MakeModeConfigurationFromJsValue(configuration, jsValue.AsArray.Get(i))

                If newModeConfigurationArray(i) Is Nothing Then Return Nothing
            Next
            Return newModeConfigurationArray
        ElseIf jsValue.IsObject Then
            '如果是对象，则直接解析
            Dim newModeConfiguration = MakeModeConfigurationFromJsValue(configuration, jsValue)
            If newModeConfiguration Is Nothing Then Return Nothing
            Return New ModeConfiguration() {newModeConfiguration}
        Else '既不是数组又不是对象，报错返回空
            Throw New Exception("Only js object or array is accepted to generate EditPanelModeConfiguration!")
        End If
    End Function

    ''' <summary>
    ''' 生成一个ModeConfiguration
    ''' </summary>
    ''' <param name="configuration">所属配置中心</param>
    ''' <param name="jsValue">JsValue对象</param>
    ''' <returns></returns>
    Private Shared Function MakeModeConfigurationFromJsValue(configuration As Configuration, jsValue As Jint.Native.JsValue) As ModeConfiguration
        If jsValue Is Nothing Then Throw New Exception("JsValue can not be null!")
        If Not jsValue.IsObject Then
            Throw New Exception("Not a valid JsObject!")
            Return Nothing
        End If

        Dim jsObject = jsValue.AsObject

        '把字段，赋值给EditPanelModeConfiguration
        Dim newModeConfiguration As New ModeConfiguration
        newModeConfiguration.Configuration = configuration
        If jsObject.HasOwnProperty("mode") Then
            newModeConfiguration.Mode = jsObject.GetOwnProperty("mode").Value.ToObject
        Else
            newModeConfiguration.Mode = "default"
            Throw New Exception("""mode"" property not found, set as ""default"" automatically")
        End If

        If jsObject.HasOwnProperty("fields") Then
            newModeConfiguration.Fields = FieldConfiguration.FromJsValue(newModeConfiguration, jsObject.GetOwnProperty("fields").Value)
        Else
            Throw New Exception("""fields"" property is necessary!")
        End If

        If jsObject.HasOwnProperty("httpAPIs") Then
            newModeConfiguration.HTTPAPIs = HTTPAPIConfiguration.FromJSValue(newModeConfiguration, jsObject.GetOwnProperty("httpAPIs").Value)
        End If

        Return newModeConfiguration
    End Function
End Class
