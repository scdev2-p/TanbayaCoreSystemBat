
Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim updateTime As DateTime = DateTime.Now
        Dim tanaoroshiYM As String = DateTime.Now.ToString("yyyyMM")

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N041", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope

                Dim tanaoroshiTA As New N041TableAdapters.取置棚卸TableAdapter

                '[M_コード.棚卸年月]Update
                tanaoroshiTA.UpdateTanaoroshiDate(tanaoroshiYM)

                '[T_取置棚卸]Delete
                tanaoroshiTA.DeleteToriokiTanaoroshi()

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N041")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N041")

        End Try

    End Sub

End Module
