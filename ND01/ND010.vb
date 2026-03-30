Module Module1

    Sub Main(ByVal args As String())

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim targetDate As DateTime = common_bat.ValueToDate(common_bat.BatDate)
        Dim updateTime As DateTime = DateTime.Now

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "ND01", DateTime.Now)

            Dim hacchuYoyakuDate As String = Format(targetDate.AddDays(Convert.ToInt32(args(0)) * -1).ToString("yyyyMMdd"))
            Dim result As Int32 = (New ND01TableAdapters.発注予約TableAdapter(common_bat.COMMAND_TIME_OUT)).DeleteHacchuYoyaku(hacchuYoyakuDate)

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "ND01")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "ND01")

        End Try

    End Sub

End Module
