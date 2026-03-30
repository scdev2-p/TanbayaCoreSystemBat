Module N04_2

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim updateTime As DateTime = DateTime.Now
        Dim tanaoroshiYM As String = DateTime.Now.ToString("yyyyMM")

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N042", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope

                Dim shukeiTA As New N042TableAdapters.棚卸事前集計TableAdapter

                '[T_棚卸事前集計]Delete
                shukeiTA.DeleteTanaoroshiJizenShukei()

                '[T_棚卸事前集計]Insert
                Dim result As Int32 = shukeiTA.InsertTanaoroshiJizenShukei

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N042")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N042")

        End Try

    End Sub

End Module
