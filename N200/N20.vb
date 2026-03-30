Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim startday As DateTime = DateTime.Now
        Dim update As New N200TableAdapters.QueriesTableAdapter(common_bat.COMMAND_TIME_OUT)
        Dim updatetime As String = DateTime.Now.ToString("HHmm")

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(startday, "N200", DateTime.Now)

            '開始時間が12時前だったら前日の日付で更新する
            If updatetime > "1200" Then

                update.UpdateCodeName(DateTime.Now.ToString("yyyyMMdd"))

            Else

                update.UpdateCodeName(DateTime.Now.AddDays(-1).ToString("yyyyMMdd"))

            End If

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, startday, "N200")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, startday, "N200")

        End Try

    End Sub

End Module
