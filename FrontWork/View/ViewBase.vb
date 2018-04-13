Public MustInherit Class ViewBase(Of TPanel)
    Implements IView

    Private _mode As String = "default"
    Private _panel As TPanel
    Private _model As IModel

    Protected Property MetaData As EditPanelMetaData = Nothing Implements IView.MetaData
    Protected Property JsEngine As New Jint.Engine

    Public Sub New()
        JsEngine.SetValue("log", New Action(Of String)(AddressOf Console.WriteLine))
    End Sub

    Public Property Mode As String Implements IView.Mode
        Get
            Return _mode
        End Get
        Set(value As String)
            _mode = value
            Call Me.TryInitEditPanel()
            Call Me.TryBindModel()
        End Set
    End Property

    Public Property Panel As TPanel
        Get
            Return _panel
        End Get
        Set(value As TPanel)
            _panel = value
            Call Me.TryInitEditPanel()
            Call Me.TryBindModel()
        End Set
    End Property

    Public Property Model As IModel Implements IView.Model
        Get
            Return Me._model
        End Get
        Set(value As IModel)
            If value Is Me._model Then
                Return
            Else
                If Me._model IsNot Nothing Then
                    Me.UnbindModel()
                End If
                Me._model = value
                Call Me.TryBindModel()
            End If
        End Set
    End Property

    Protected MustOverride Sub InitEditPanel()
    Protected MustOverride Sub BindModel()
    Protected MustOverride Sub UnbindModel()

    Public Sub SetMethodListener(methodListener As IMethodListener, Optional mode As String = "default")
        If Me.MetaData.ContainsMode(mode) = False Then
            Throw New Exception("SetMethodListener failed because mode """ + mode + """ not found!")
            Exit Sub
        End If
        Me.MetaData.SetMethodListener(methodListener, mode)
    End Sub

    Private Sub TryInitEditPanel()
        If Me.Panel Is Nothing Then Return
        If Me.Mode Is Nothing Then Return
        If Me.MetaData Is Nothing Then Return
        Call Me.InitEditPanel()
    End Sub

    Private Sub TryBindModel()
        If Me.Mode Is Nothing Then Return
        If Me.MetaData Is Nothing Then Return
        Call Me.BindModel()
    End Sub

    Public Function SetMetaDataFromJson(jsonStr As String, modeMethodListener As Dictionary(Of String, IMethodListener)) As Boolean Implements IView.SetMetaDataFromJson
        Dim metaData = EditPanelMetaData.FromJson(Me.JsEngine, jsonStr)
        If metaData Is Nothing Then
            Return False
        Else
            Me.MetaData = metaData
            For Each item In modeMethodListener
                Dim mode = item.Key
                Dim methodListener = item.Value
                metaData.SetMethodListener(methodListener, mode)
            Next
            Call Me.TryInitEditPanel()
            Return True
        End If
    End Function

    Public Function SetMetaDataFromJson(jsonStr As String, methodListenerForAllModes As IMethodListener) As Boolean Implements IView.SetMetaDataFromJson
        Dim metaData = EditPanelMetaData.FromJson(Me.JsEngine, jsonStr)
        If metaData Is Nothing Then
            Return False
        Else
            Me.MetaData = metaData
            metaData.SetMethodListener(methodListenerForAllModes)
            Call Me.TryInitEditPanel()
            Return True
        End If
    End Function
End Class
