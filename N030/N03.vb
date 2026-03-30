Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim zaikoYMD As String = common_bat.BatDate
        Dim updateTime As DateTime = DateTime.Now

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N030", DateTime.Now)

            Dim result As Int32 = (New N030TableAdapters.日別在庫TableAdapter(common_bat.COMMAND_TIME_OUT)).InsertHibetuZaiko(zaikoYMD)
            result = (New N030TableAdapters.日別在庫TableAdapter(common_bat.COMMAND_TIME_OUT)).InsertUriageSummaryNoZaiko(zaikoYMD)

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N030")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N030")

        End Try

    End Sub

End Module
