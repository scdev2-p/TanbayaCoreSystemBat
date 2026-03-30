Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim targetDate As DateTime = common_bat.ValueToDate(common_bat.BatDate)
        Dim updateTime As DateTime = DateTime.Now
        Dim result As Int32 = 0

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N011", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope

                Dim pointTA As New N011TableAdapters.T_ポイントTableAdapter(common_bat.COMMAND_TIME_OUT)
                Dim calendarTA As New N011TableAdapters.M_カレンダTableAdapter
                Dim calendarDT As N011.M_カレンダDataTable


                calendarDT = calendarTA.SelectMatsuJitsu(targetDate.ToString("yyyyMM"))

                '実行月の最終営業日の場合は失効ポイントの処理を行う
                If calendarDT(0).年月日 = targetDate.ToString("yyyyMMdd") Then
                    result = 0
                    ' 失効ポイント履歴
                    For Each dtRow As N011.T_ポイントRow In pointTA.SelectShikkouPointByMatsuJitsu(targetDate.ToString("yyyyMM"))

                        pointTA.DeletePointShikkou(dtRow.顧客管理番号)

                        ' 失効ポイント履歴
                        If pointTA.UpdatePointHistory(dtRow.ポイント,
                                                         dtRow.顧客管理番号,
                                                         dtRow.有効年月日,
                                                         "4") = 0 Then

                            result += pointTA.InsertPointHistory(dtRow.顧客管理番号,
                                                          dtRow.有効年月日,
                                                          "4",
                                                          dtRow.ポイント)

                        Else
                            result += 1
                        End If

                    Next

                End If

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N011")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N011")

        End Try

    End Sub

End Module
