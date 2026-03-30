Namespace N130TableAdapters

    Partial Class QueriesTableAdapter

        Public Sub New(ByVal timeOut As Int32)

            MyBase.New()

            For Each cmd As SqlClient.SqlCommand In Me.CommandCollection
                cmd.CommandTimeout = timeOut
            Next

        End Sub

    End Class

End Namespace


Partial Class N130
End Class
