Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim updateTime As DateTime = DateTime.Now
        Dim tanadateTA As New N051TableAdapters.M_コードTableAdapter
        Dim tanaoroshiYM As String = tanadateTA.SelectTanaoroshiYM

        Dim result As Int32
        Dim rowNo As Integer = 0

        Dim n051TA As New N051TableAdapters.QueriesTableAdapter(common_bat.COMMAND_TIME_OUT)
        Dim toriokiTanaoroshiTA As New N051TableAdapters.T_取置棚卸TableAdapter

        Dim toriokiTanaoroshiDT As N051.T_取置棚卸DataTable

        Try

            '開始ログ()
            batLogTadap.InsertBatLog(updateTime, "N051", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(80000000000))

                'T_取置棚卸のデータをT_棚卸とT_棚卸明細へINSERTする
                toriokiTanaoroshiDT = toriokiTanaoroshiTA.SelectToriokiTanaoroshi(tanaoroshiYM)

                '棚卸と棚卸明細のDELTE
                result = n051TA.DeleteTanaoroshi(tanaoroshiYM, "2999")
                result = n051TA.DeleteTanaoroshiMeisai(tanaoroshiYM, "2999")

                'T_棚卸へINSERTする
                result = n051TA.InsertTanaoroshi(tanaoroshiYM,
                                                "2999",
                                                0,
                                                "0",
                                                "0",
                                                "",
                                                "",
                                                "N051",
                                                updateTime)

                For Each DtRow As N051.T_取置棚卸Row In toriokiTanaoroshiDT

                    rowNo += 1

                    'T_棚卸明細へINSERTする
                    result = n051TA.InsertTanaoroshiMeisai(tanaoroshiYM,
                                                                  "2999",
                                                                  rowNo,
                                                                  DtRow.商品管理番号,
                                                                  DtRow.仕入先コード,
                                                                  DtRow.仕入先履歴番号,
                                                                  DtRow.担当者番号,
                                                                  DtRow.数量,
                                                                  DtRow.原価単価合計,
                                                                  DtRow.上代単価合計,
                                                                  "",
                                                                  DtRow.フロアコード,
                                                                  DtRow.処理済みフラグ,
                                                                  "N051",
                                                                  updateTime)

                Next

                scope.Complete()

            End Using

            '終了ログ(d)
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N051")

        Catch ex As Exception

            '終了エラーログ()
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N051")

        End Try

    End Sub

End Module
