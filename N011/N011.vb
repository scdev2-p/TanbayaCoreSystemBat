Namespace N011TableAdapters

    Partial Class T_ポイントTableAdapter

        Public Sub New(timeOut As Int32)

            MyBase.New()

            For Each cmd As SqlClient.SqlCommand In Me.CommandCollection
                cmd.CommandTimeout = timeOut
            Next

        End Sub

    End Class

End Namespace

Partial Public Class N011
End Class
