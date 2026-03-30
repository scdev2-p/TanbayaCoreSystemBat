Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim batDay As Integer = CInt(common_bat.ValueToDate(common_bat.BatDate).ToString("dd"))

        Dim currentZaikoYM As String = common_bat.ValueToDate(common_bat.BatDate).ToString("yyyyMM")
        Dim beforeZaikoYM As String = common_bat.ValueToDate(common_bat.BatDate).AddMonths(-1).ToString("yyyyMM")
        Dim afterZaikoYM As String = common_bat.ValueToDate(common_bat.BatDate).AddMonths(1).ToString("yyyyMM")

        Dim beforeSireYM As String = common_bat.ValueToDate(common_bat.BatDate).AddMonths(-1).ToString("yyyyMM")
        Dim currentSireYM As String = common_bat.ValueToDate(common_bat.BatDate).ToString("yyyyMM")

        Dim updateTime As DateTime = DateTime.Now
        Dim steps As String = "step0"

        Try

            '開始ログ
            batLogTadap.InsertBatLog(updateTime, "N020", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(80000000000))


                Dim uriageTA As New N020TableAdapters.月初在庫TableAdapter(common_bat.COMMAND_TIME_OUT)

                '実行日が１５日までは月初、１６日以降は月末として扱う
                If batDay < 16 Then

                    '実行日が月初の場合

                    '[T_月初在庫]Delete
                    uriageTA.DeleteGesshoZaiko(currentZaikoYM) : steps = "step1-1"

                    '[T_月初在庫]Insert
                    '2013/04/20 UPD str t-orii
                    Dim result As Int32 = uriageTA.InsertGesshoZaiko(currentZaikoYM, beforeZaikoYM, beforeSireYM)
                    'Dim result As Int32 = uriageTA.InsertGesshoZaiko2(currentZaikoYM, beforeZaikoYM, beforeSireYM)
                    '2013/04/20 UPD End t-orii

                    '[T_月別効率表]Update 在庫の値を０にする
                    uriageTA.UpdateKouritsuhyoZaikoZero(beforeZaikoYM) : steps = "step1-2"

                    '[T_月別効率表]Update 在庫の値をT_月初在庫の値でUpdateする
                    uriageTA.UpdateKouritsuhyo(beforeZaikoYM, currentZaikoYM) : steps = "step1-3"

                Else

                    '実行日が月末の場合

                    '[T_月初在庫]Delete
                    uriageTA.DeleteGesshoZaiko(afterZaikoYM) : steps = "step2-1"

                    '[T_月初在庫]Insert
                    Dim result As Int32 = uriageTA.InsertGesshoZaiko(afterZaikoYM, currentZaikoYM, currentSireYM) : steps = "step2-2"

                End If

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N020")

        Catch ex As Exception

            ' 終了エラーログ
            'batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N020")
            batLogTadap.UpdateBatLog(DateTime.Now, False, "steps=" & steps & " " & ex.Message, updateTime, "N020")

        End Try

    End Sub

End Module
