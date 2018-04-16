Public Interface ISynchronizer
    Property Configuration As Configuration

    Function PullFromServer() As Boolean
    Function PushToServer() As Boolean
End Interface
