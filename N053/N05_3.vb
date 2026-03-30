Module N05_3

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim updateTime As DateTime = DateTime.Now
        Dim tanadateTA As New N053TableAdapters.M_コードTableAdapter
        Dim tanaoroshiYM As String = tanadateTA.SelectTanaoroshiYM

        Dim result As Int32

        Dim N053TA As New N053TableAdapters.QueriesTableAdapter(common_bat.COMMAND_TIME_OUT)

        Try

            '開始ログ()
            batLogTadap.InsertBatLog(updateTime, "N053", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(80000000000))

                '先に今回分のT_棚卸差異のデータを削除する
                result = N053TA.DeleteTanaoroshiSai(tanaoroshiYM)

                '在庫差異がある商品を抽出して、T_棚卸差異へINSERTする
                result = N053TA.InsertSelectSaiSuryo(tanaoroshiYM)

                scope.Complete()

            End Using

            '終了ログ(d)
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N053")

        Catch ex As Exception

            '終了エラーログ()
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N053")

        End Try


    End Sub

End Module
