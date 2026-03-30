Namespace ND01TableAdapters

    Partial Class 発注予約TableAdapter

        Public Sub New(ByVal timeOut As Int32)

            MyBase.New()

            For Each cmd As SqlClient.SqlCommand In Me.CommandCollection
                cmd.CommandTimeout = timeOut
            Next

        End Sub

    End Class

End Namespace

Partial Class ND01
End Class
