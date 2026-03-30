Module Module1

    Sub Main(ByVal args As String())

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim targetDate As DateTime = common_bat.ValueToDate(common_bat.BatDate)
        Dim updateTime As DateTime = DateTime.Now

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "ND02", DateTime.Now)

            Dim henpinYoyakuDate As String = Format(targetDate.AddDays(Convert.ToInt32(args(0)) * -1).ToString("yyyyMMdd"))
            Dim result As Int32 = (New ND02TableAdapters.返品予約TableAdapter(common_bat.COMMAND_TIME_OUT)).DeleteHenpinYoyaku(henpinYoyakuDate)

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "ND02")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "ND02")

        End Try

    End Sub

End Module
