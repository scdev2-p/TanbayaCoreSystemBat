Namespace N210TableAdapters

    Partial Class M_コードTableAdapter

        Public Sub New(ByVal timeOut As Int32)

            MyBase.New()

            For Each cmd As SqlClient.SqlCommand In Me.CommandCollection
                cmd.CommandTimeout = timeOut
            Next

        End Sub

    End Class

End Namespace

Partial Public Class N210
End Class


Partial Public Class N210
End Class
