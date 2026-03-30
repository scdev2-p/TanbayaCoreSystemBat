Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim targetYM As String = common_bat.ValueToDate(common_bat.BatDate).ToString("yyyyMM")
        Dim updateTime As DateTime = DateTime.Now

        Try

            '開始ログ
            batLogTadap.InsertBatLog(updateTime, "N140", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(80000000000))

                Dim jissekiTA As New N140TableAdapters.QueriesTableAdapter(common_bat.COMMAND_TIME_OUT)

                '[T_商品月別実績]Delete
                jissekiTA.DeleteShouhinTsukibetsuJisseki(targetYM)

                '2013/04/20 Del Str t-orii
                ''[T_商品月別実績]Insert 2012/03/21 T-orii T_レジ明細を見るようSQL文を修正
                'Dim result As Int32 = jissekiTA.InsertShouhinTsukibetsuJisseki(targetYM)
                '2013/04/20 Del End t-orii

                '2013/04/20 Add Str t-orii 「V_売上(SQL上はT_売上とT_セット売上から集計)から集計するように変更。 」
                jissekiTA.InsertTsukibetsuJiseki1(targetYM) 'T_売上から集計
                jissekiTA.InsertTsukibetsuJiseki2(targetYM) 'T_セット売上から集計
                '2013/04/20 Add End t-orii

                scope.Complete()

            End Using

            '終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N140")

        Catch ex As Exception

            '終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N140")

        End Try

    End Sub

End Module
