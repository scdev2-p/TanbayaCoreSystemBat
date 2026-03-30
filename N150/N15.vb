Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim targetYMD As String = common_bat.BatDate
        Dim updateTime As DateTime = DateTime.Now

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N150", DateTime.Now)

            Dim result As Int32 = (New N150TableAdapters.在庫推移TableAdapter(common_bat.COMMAND_TIME_OUT)).InsertZaikoSuii(targetYMD)

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N150")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N150")

        End Try

    End Sub

End Module
