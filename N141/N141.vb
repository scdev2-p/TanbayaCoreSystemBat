Namespace N141TableAdapters

    Partial Class T_商品月別実績TableAdapter

        Public Sub New(ByVal timeOut As Int32)

            MyBase.New()

            For Each cmd As SqlClient.SqlCommand In Me.CommandCollection
                cmd.CommandTimeout = timeOut
            Next

        End Sub

    End Class


End Namespace

Partial Class N141
End Class
