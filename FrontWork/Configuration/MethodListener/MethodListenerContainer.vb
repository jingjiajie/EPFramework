﻿Imports System.Linq
Imports System.Reflection
''' <summary>
''' 方法监听器容器，用来存储所有注册的方法监听器
''' </summary>
Public Class MethodListenerContainer
    Private Class NameMethodListenerPair
        Public Property Name As String
        Public Property MethodListener As Object

        Public Sub New(name As String, methodListener As Object)
            Me.Name = name
            Me.MethodListener = methodListener
        End Sub
    End Class

    Private Shared Property MethodListeners As New List(Of NameMethodListenerPair)

    Private Sub New()

    End Sub

    Shared Sub New()
        Dim asm = Assembly.GetEntryAssembly()
        Dim types = asm.GetTypes
        For Each curType In types
            If curType.BaseType IsNot GetType(MethodListenerBase) Then Continue For
            Dim instance As MethodListenerBase = Activator.CreateInstance(curType)
            Dim name = instance.MethodListenerName

            '判断如果目标方法监听器中又包含Configuration类型的对象，则不允许设置。否则会发生无限递归初始化方法监听器
            Dim properties = curType.GetProperties(BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic)
            For Each prop In properties
                If prop.PropertyType = GetType(Configuration) Then
                    Throw New Exception($"MethodListener: {name} cannot contain Configuration property!")
                End If
            Next
            Dim fields = curType.GetFields(BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic)
            For Each field In fields
                If field.Name = "configuration" Then
                    Console.Write("")
                End If
                If field.FieldType = GetType(Configuration) Then
                    Throw New Exception($"MethodListener: {name} cannot contain Configuration field!")
                End If
            Next
            Call MethodListenerContainer.Register(name, instance)
        Next
    End Sub

    ''' <summary>
    ''' 注册一个方法监听器
    ''' </summary>
    ''' <param name="name">方法监听器名称。如果存在重名方法监听器，则覆盖</param>
    ''' <param name="methodListener">方法监听器对象</param>
    Public Shared Sub Register(name As String, methodListener As Object)
        Dim found = (From m In MethodListeners
                     Where m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                     Select m).FirstOrDefault
        If found Is Nothing Then
            MethodListeners.Add(New NameMethodListenerPair(name, methodListener))
        Else
            found.MethodListener = methodListener
        End If
    End Sub

    ''' <summary>
    ''' 注册一个方法监听器
    ''' </summary>
    ''' <param name="methodListener">方法监听器对象。名称默认为方法监听器对象的类名。如果要指定其它名称，请使用Register函数的其他重载</param>
    Public Shared Sub Register(methodListener As Object)
        Dim found = (From m In MethodListeners
                     Where m.Name.Equals(methodListener.GetType.Name, StringComparison.OrdinalIgnoreCase)
                     Select m).FirstOrDefault
        If found Is Nothing Then
            MethodListeners.Add(New NameMethodListenerPair(methodListener.GetType.Name, methodListener))
        Else
            found.MethodListener = methodListener
        End If
    End Sub

    ''' <summary>
    ''' 是否注册过方法监听器
    ''' </summary>
    ''' <param name="name">方法监听器名称</param>
    ''' <returns>是否注册过方法监听器</returns>
    Public Shared Function Contains(name As String) As Boolean
        If (From m In MethodListeners
            Where m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            Select m).Count <> 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' 获取方法监听器
    ''' </summary>
    ''' <param name="name">方法监听器名称</param>
    ''' <returns>方法监听器。如果找不到方法监听器，返回null</returns>
    Public Shared Function [Get](name As String) As Object
        Return (From m In MethodListeners
                Where m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                Select m).FirstOrDefault?.MethodListener
    End Function
End Class