Module Module1

#Region "修正履歴"

    '2014/03/27 '7月は前期で処理する Kanai

#End Region

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim targetYM As String = common_bat.ValueToDate(common_bat.BatDate).AddMonths(-1).ToString("yyyyMM") '前月を取得する
        Dim updateTime As DateTime = DateTime.Now

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N130", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope

                Dim jissekiTA As New N130TableAdapters.QueriesTableAdapter(common_bat.COMMAND_TIME_OUT)

                '[T_仕入先月別実績]Delete
                jissekiTA.DeleteShiiresakiTsukibetsuJisseki(targetYM)

                '[T_仕入先月別実績]Insert
                Dim result As Int32 = 0

                '7月は前期で処理する 2014/03/27 Update Kanai
                If targetYM.Substring(4, 2) = "07" Then
                    result = jissekiTA.InsertShiiresakiTsukibetsuJissekiZenki(targetYM)
                Else
                    result = jissekiTA.InsertShiiresakiTsukibetsuJisseki(targetYM)
                End If

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N130")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N130")

        End Try

    End Sub

End Module
