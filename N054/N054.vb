Namespace N054TableAdapters

    Partial Class T_在庫WORKTableAdapter

        Public Sub New(ByVal timeOut As Int32)

            MyBase.New()

            For Each cmd As SqlClient.SqlCommand In Me.CommandCollection
                cmd.CommandTimeout = timeOut
            Next

        End Sub

    End Class

End Namespace

Partial Class N054
End Class
