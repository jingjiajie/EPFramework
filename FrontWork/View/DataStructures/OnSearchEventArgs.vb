Public Class OnSearchEventArgs
    Inherits EventArgs

    Public Property Conditions As SearchConditionItem() = New SearchConditionItem() {}
    Public Property Orders As OrderConditionItem() = New OrderConditionItem() {}

    Public Sub New(conditions As SearchConditionItem(), orders As OrderConditionItem())
        Me.Conditions = conditions
        Me.Orders = orders
    End Sub

    Public Sub New()

    End Sub


    Public Class OrderConditionItem
        Public Property Key As String
        Public Property Order As Order = [Order].ASC

        Public Sub New(key As String, order As Order)
            Me.Key = key
            Me.Order = order
        End Sub
    End Class

    Public Class SearchConditionItem
        Public Property Key As String
        Public Property Relation As Relation = Relation.EQUAL
        Public Property Values As Object()

        Public Sub New(key As String, relation As Relation, values As Object())
            Me.Key = key
            Me.Relation = relation
            Me.Values = values
        End Sub
    End Class

    Public Enum Order
        ASC
        DESC
    End Enum

    Public Enum Relation
        EQUAL
        GREATER_THAN
        LESS_THAN
    End Enum
End Class
