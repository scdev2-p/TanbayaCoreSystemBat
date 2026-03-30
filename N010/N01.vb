Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim queryTadap As New N010TableAdapters.QueriesTableAdapter(common_bat.COMMAND_TIME_OUT)
        Dim targetDate As DateTime = common_bat.ValueToDate(common_bat.BatDate)
        Dim targetDateUpdate As DateTime = targetDate.AddMonths(-1).AddYears(1)
        Dim updateTime As DateTime = DateTime.Now
        Dim result As Int32 = 0
        Dim isJyogaiDate As Boolean = False

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N010", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope

                isJyogaiDate = Convert.ToBoolean(queryTadap.SelectPointJyogaiDate(targetDate.ToString("yyyyMMdd")) = 1)

                For Each dtRow As N010.来店ポイントRow In (New N010TableAdapters.来店ポイントTableAdapter(common_bat.COMMAND_TIME_OUT)).SelectRaitenPoint(targetDate.ToString("yyyyMMdd"))

                    result = queryTadap.DeletePointData(dtRow.顧客管理番号, dtRow.使用ポイント数)

                    If isJyogaiDate = False Then

                        If Convert.ToInt32(dtRow.付与ポイント * dtRow.ポイント掛率) + dtRow.加算ポイント <> 0 Then

                            If queryTadap.UpdateRaitenPoint(Convert.ToInt32(dtRow.付与ポイント * dtRow.ポイント掛率) + dtRow.加算ポイント, "BAT", dtRow.顧客管理番号) = 0 Then

                                queryTadap.InsertRaitenPoint(dtRow.顧客管理番号,
                                                             Convert.ToInt32(dtRow.付与ポイント * dtRow.ポイント掛率) + dtRow.加算ポイント,
                                                             targetDateUpdate.ToString("yyyyMM") + Convert.ToString(DateTime.DaysInMonth(targetDateUpdate.Year, targetDateUpdate.Month)).PadLeft(2, "0"c),
                                                             "BAT")

                            End If

                        End If

                        If dtRow.Is加算ポイントNull = False AndAlso dtRow.加算ポイント <> 0 Then

                            ' 来店ポイント履歴
                            If queryTadap.UpdatePointHistory(dtRow.加算ポイント,
                                                             dtRow.顧客管理番号,
                                                             dtRow.年月日,
                                                             "1") = 0 Then

                                queryTadap.InsertPointHistory(dtRow.顧客管理番号,
                                                              dtRow.年月日,
                                                              "1",
                                                              dtRow.加算ポイント)


                            End If

                        End If

                        If dtRow.Is付与ポイントNull = False AndAlso dtRow.付与ポイント <> 0 Then

                            ' お買上ポイント履歴
                            If queryTadap.UpdatePointHistory(Convert.ToInt32(dtRow.付与ポイント * dtRow.ポイント掛率),
                                                             dtRow.顧客管理番号,
                                                             dtRow.年月日,
                                                             "2") = 0 Then

                                queryTadap.InsertPointHistory(dtRow.顧客管理番号,
                                                              dtRow.年月日,
                                                              "2",
                                                              Convert.ToInt32(dtRow.付与ポイント * dtRow.ポイント掛率))

                            End If

                        End If

                    End If

                    If dtRow.Is使用ポイント数Null = False AndAlso dtRow.使用ポイント数 <> 0 Then

                        ' 使用ポイント履歴
                        If queryTadap.UpdatePointHistory(dtRow.使用ポイント数,
                                                         dtRow.顧客管理番号,
                                                         dtRow.年月日,
                                                         "3") = 0 Then

                            queryTadap.InsertPointHistory(dtRow.顧客管理番号,
                                                          dtRow.年月日,
                                                          "3",
                                                          dtRow.使用ポイント数)

                        End If

                    End If

                Next

                '' 失効ポイント履歴
                'For Each dtRow As N010.T_ポイントRow In (New N010TableAdapters.T_ポイントTableAdapter(common_bat.COMMAND_TIME_OUT)).SelectShikkouPoint(targetDate.ToString("yyyyMMdd"))

                '    queryTadap.DeletePointShikkou(dtRow.顧客管理番号)

                '    ' 失効ポイント履歴
                '    If queryTadap.UpdatePointHistory(dtRow.ポイント,
                '                                     dtRow.顧客管理番号,
                '                                     dtRow.有効年月日,
                '                                     "4") = 0 Then

                '        queryTadap.InsertPointHistory(dtRow.顧客管理番号,
                '                                      dtRow.有効年月日,
                '                                      "4",
                '                                      dtRow.ポイント)

                '    End If

                'Next

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N010")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N010")

        End Try

    End Sub

End Module
