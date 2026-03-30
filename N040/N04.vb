Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim updateTime As DateTime = DateTime.Now
        Dim tanaoroshidate As String = DateTime.Now.ToString("yyyyMM")

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N040", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope

                Dim shukeiTA As New N040TableAdapters.棚卸事前集計TableAdapter

                '[M_コード.棚卸年月]Update
                shukeiTA.UpdateTanaoroshiDate(tanaoroshidate)

                '[T_棚卸事前集計]Delete
                shukeiTA.DeleteTanaoroshiJizenShukei()

                '[T_棚卸事前集計]Insert
                Dim result As Int32 = shukeiTA.InsertTanaoroshiJizenShukei

                '[T_取置棚卸]Delete
                shukeiTA.DeleteToriokiTanaoroshi()

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N040")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N040")

        End Try

    End Sub

End Module
