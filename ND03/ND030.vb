Module Module1

    Sub Main(ByVal args() As String)

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim targetDate As DateTime = common_bat.ValueToDate(common_bat.BatDate)
        Dim updateTime As DateTime = DateTime.Now

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "ND03", DateTime.Now)

            '削除日数取得
            If args.Length < 1 Then
                Throw New Exception("引数に削除日数が指定されていません。")
            End If
            Dim param As String = args(0)
            If Not IsNumeric(param) Then
                Throw New Exception("引数に指定された削除日数が不正です。")
            End If
            Dim days As Integer = Convert.ToInt32(param)
            Dim deletedDate As String = Format(targetDate.AddDays(days * -1).ToString("yyyyMMdd"))

            Dim deleteTA As New ND03TableAdapters.レジワークTableAdapter(common_bat.COMMAND_TIME_OUT)

            'W_レジ伝票明細削除
            deleteTA.DeleteRegisterKanri(deletedDate)

            'W_レジ伝票明細削除
            deleteTA.DeleteRegisterMeisai(deletedDate)

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, "削除完了　削除対象日:" & deletedDate, updateTime, "ND03")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "ND03")

        End Try

    End Sub

End Module
