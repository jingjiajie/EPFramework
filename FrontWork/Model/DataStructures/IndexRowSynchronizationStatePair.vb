Public Class IndexRowSynchronizationStatePair
    Public Property Index As Long
    Public Property RowID As Guid
    Public Property SynchronizationState As SynchronizationState

    Public Sub New(index As Long, rowID As Guid, synchronizationState As SynchronizationState)
        Me.Index = index
        Me.RowID = rowID
        Me.SynchronizationState = synchronizationState
    End Sub

    Public Sub New()

    End Sub
End Class
