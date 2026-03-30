Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()

        Dim fromYMD As String = common_bat.ValueToDate(common_bat.BatDate).AddDays(-14).ToString("yyyyMMdd")
        Dim toYMD As String = common_bat.BatDate
        Dim startDate As DateTime = DateTime.Now
        Dim openYMD As String
        Dim updateTime As DateTime = DateTime.Now

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N070", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope

                ' 直近の月曜日を求める
                While True

                    If Weekday(startDate) = 2 Then

                        Exit While

                    End If

                    startDate = startDate.AddDays(1)

                End While

                ' 月曜日の日付を変換する
                openYMD = startDate.ToString("yyyyMMdd")

                ' 全フロアのループ
                For Each dtRowFloor As N070.M_フロアRow In (New N070TableAdapters.M_フロアTableAdapter(common_bat.COMMAND_TIME_OUT)).SelectFloor()

                    Dim numbering As Int32 = 1

                    For Each dtRowUriage As N070.T_売上Row In (New N070TableAdapters.T_売上TableAdapter(common_bat.COMMAND_TIME_OUT)).SelectFloorUriageBest10(fromYMD, toYMD, dtRowFloor.フロアコード)

                        Dim result As Int32 = (New N070TableAdapters.売上ベスト10TableAdapter(common_bat.COMMAND_TIME_OUT)).InsertUriageBest10(dtRowFloor.フロアコード,
                                                                                                                    openYMD,
                                                                                                                    numbering,
                                                                                                                    dtRowUriage.商品コード,
                                                                                                                    dtRowUriage.商品名,
                                                                                                                    dtRowUriage.上代単価,
                                                                                                                    dtRowUriage.ゾーン,
                                                                                                                    dtRowUriage.数量,
                                                                                                                    dtRowUriage.売上金額,
                                                                                                                    "BAT")
                        numbering += 1

                    Next

                Next

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N070")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N070")

        End Try

    End Sub

End Module
