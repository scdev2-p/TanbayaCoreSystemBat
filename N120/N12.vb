Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim beforeMonth As String = common_bat.ValueToDate(common_bat.BatDate).ToString("yyyyMM")
        Dim updateTime As DateTime = DateTime.Now

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N120", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope

                Dim uriageTA As New N120TableAdapters.QueriesTableAdapter(common_bat.COMMAND_TIME_OUT)

                '[T_顧客月別実績]Delete
                uriageTA.DeleteKokyakuTukibetuJisseki(beforeMonth)

                '[T_顧客月別実績]Insert
                Dim i As Int32 = uriageTA.InsertKokyakuTukibetuJisseki(beforeMonth)

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N120")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N120")

        End Try

    End Sub

End Module
