Module N14_1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim targetYM As String = common_bat.ValueToDate(common_bat.BatDate).ToString("yyyyMM")
        Dim updateTime As DateTime = DateTime.Now

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N141", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope

                Dim jissekiTA As New N141TableAdapters.T_商品月別実績TableAdapter(common_bat.COMMAND_TIME_OUT)

                '[T_商品月別実績]Delete
                Dim result As Int32 = jissekiTA.DeleteShouhinTsukibetsuJisseki(targetYM)

                '[T_商品月別実績]Insert
                result = jissekiTA.InsertShouhinTsukibetsuJisseki(targetYM)

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N141")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N141")

        End Try

    End Sub

End Module
