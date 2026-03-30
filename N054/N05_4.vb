Module N05_4

    'N054 在庫WORK再作成バッチ
    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim updateTime As DateTime = DateTime.Now
        Dim zaikoWorkTA As New N054TableAdapters.T_在庫WORKTableAdapter

        Try

            '開始ログ出力
            batLogTadap.InsertBatLog(updateTime, "N054", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(80000000000))

                'T_在庫WORKをクリア
                zaikoWorkTA.DeleteZaikoWork()

                'T_在庫からT_在庫WORKへデータコピー
                zaikoWorkTA.InsertZaikoWork()

                scope.Complete()

            End Using

            '終了ログ出力
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N054")

        Catch ex As Exception

            '終了エラーログ()
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N054")

        End Try


    End Sub

End Module
