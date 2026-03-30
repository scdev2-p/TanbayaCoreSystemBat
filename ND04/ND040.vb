Module Module1

    Sub Main(ByVal args() As String)

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim targetDate As DateTime = common_bat.ValueToDate(common_bat.BatDate)
        Dim updateTime As DateTime = DateTime.Now

        Try
            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "ND04", DateTime.Now)

            '削除月数取得
            If args.Length < 1 Then
                Throw New Exception("引数に削除月数が指定されていません。")
            End If
            Dim param As String = args(0)
            If Not IsNumeric(param) Then
                Throw New Exception("引数に指定された削除月数が不正です。")
            End If
            Dim months As Integer = Convert.ToInt32(param)
            Dim deletedDate As String = Format(targetDate.AddMonths(months * -1).ToString("yyyyMM"))

            Dim TA As New ND04TableAdapters.QueriesTableAdapter(common_bat.COMMAND_TIME_OUT)

            'W_カード利用者CFの削除処理
            TA.DeleteTG(deletedDate)

            'W_カード利用者TGの削除処理
            TA.DeleteCF(deletedDate)

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, "削除完了　削除対象年月:" & deletedDate, updateTime, "ND04")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "ND04")

        End Try

    End Sub

End Module
