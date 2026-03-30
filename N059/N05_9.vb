Module N05_9

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim updateTime As DateTime = DateTime.Now
        Dim tanadateTA As New N059TableAdapters.M_コードTableAdapter
        Dim tanaoroshiYM As String = tanadateTA.SelectTanaoroshiYM

        Dim result As Int32

        Dim N059TA As New N059TableAdapters.T_在庫TableAdapter

        Try

            '開始ログ()
            batLogTadap.InsertBatLog(updateTime, "N059", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(80000000000))

                result = N059TA.DeleteZaiko()

                result = N059TA.InsertZaiko(tanaoroshiYM)

                scope.Complete()

            End Using

            '終了ログ()
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N059")

        Catch ex As Exception

            '終了エラーログ()
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N059")

        End Try

    End Sub

End Module
