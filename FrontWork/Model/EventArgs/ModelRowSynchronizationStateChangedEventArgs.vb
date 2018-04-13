Public Class ModelRowSynchronizationStateChangedEventArgs
    Inherits EventArgs
    Public Property SynchronizationStateUpdatedRows As IndexRowSynchronizationStatePair()

    Public Sub New(synchronizationStateUpdatedRows As IndexRowSynchronizationStatePair())
        Me.SynchronizationStateUpdatedRows = synchronizationStateUpdatedRows
    End Sub
End Class
