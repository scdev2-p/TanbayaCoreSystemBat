

Partial Public Class Common
End Class

Namespace CommonTableAdapters

    Partial Class T_在庫TableAdapter

        Public Sub New(ByVal timeOut As Int32)

            MyBase.New()

            For Each cmd As SqlClient.SqlCommand In Me.CommandCollection
                cmd.CommandTimeout = timeOut
            Next

        End Sub

    End Class

End Namespace
