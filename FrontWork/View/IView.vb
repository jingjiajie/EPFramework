Public Interface IView
    Property MetaData As EditPanelMetaData
    Property Mode As String
    Property Model As IModel

    Function SetMetaDataFromJson(jsonStr As String, modeMethodListener As Dictionary(Of String, IMethodListener)) As Boolean
    Function SetMetaDataFromJson(jsonStr As String, methodListenerForAllModes As IMethodListener) As Boolean

End Interface
