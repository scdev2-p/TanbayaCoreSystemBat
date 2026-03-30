Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim updateTime As DateTime = DateTime.Now

        Dim result As Int32

        Dim n052TA As New N052TableAdapters.QueriesTableAdapter(common_bat.COMMAND_TIME_OUT)

        Try

            '開始ログ()
            batLogTadap.InsertBatLog(updateTime, "N052", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(80000000000))


                'T_取置をDELETEして、T_取置棚卸のデータをT_取置へINSERTする
                n052TA.DeleteTorioki()
                result = n052TA.InsertSelectToriokitanaoroshiToTanaoroshi

                scope.Complete()

            End Using

            '終了ログ(d)
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N052")

        Catch ex As Exception

            '終了エラーログ()
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N052")

        End Try

    End Sub

End Module
