Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim uriageYM As String = common_bat.ValueToDate(common_bat.BatDate).ToString("yyyyMM")
        Dim updateTime As DateTime = DateTime.Now

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N060", DateTime.Now)

            'Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(80000000000))

            Dim uriageTA As New N060TableAdapters.顧客月別売上TableAdapter(common_bat.COMMAND_TIME_OUT)

            '[M_月別顧客別売上]Delete
            uriageTA.DeleteKokyakuTukibetuUriage(uriageYM)

            '[M_月別顧客別売上]Insert
            Dim result As Int32 = uriageTA.InsertKokyakuTukibetuUriage(uriageYM)

            'scope.Complete()

            'End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N060")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N060")

        End Try

    End Sub

End Module
