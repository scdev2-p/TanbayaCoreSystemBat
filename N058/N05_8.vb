Module N05_8

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim updateTime As DateTime = DateTime.Now
        Dim tanadateTA As New N058TableAdapters.M_コードTableAdapter
        Dim tanaoroshiYM As String = tanadateTA.SelectTanaoroshiYM

        Dim result As Int32
        Dim resultDelete As Int32

        Dim N058TA As New N058TableAdapters.QueriesTableAdapter(common_bat.COMMAND_TIME_OUT)
        Dim tanaSethinTA As New N058TableAdapters.棚卸明細セット品TableAdapter
        Dim tanaSethinDT As N058.棚卸明細セット品DataTable
        Dim tanaMeisaiTA As New N058TableAdapters.T_棚卸明細TableAdapter
        Dim tanaCountTA As New N058TableAdapters.棚卸明細CountTableAdapter
        Dim tanaCountDT As N058.棚卸明細CountDataTable

        Try

            '開始ログ()
            batLogTadap.InsertBatLog(updateTime, "N058", DateTime.Now)

            Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(80000000000))

                '棚卸明細セット子商品データの保持
                tanaSethinDT = tanaSethinTA.SelectTanaSethin(tanaoroshiYM)

                tanaCountDT = tanaCountTA.SelectCount

                'ＩＮＳＥＲＴ
                For Each tanaCount As N058.棚卸明細CountRow In tanaCountDT

                    For Each sethinRow As N058.棚卸明細セット品Row In tanaSethinDT.Select("セット商品管理番号 = " & tanaCount.商品管理番号)


                        Dim i As Integer = tanaMeisaiTA.FillBy(sethinRow.棚番号)
                        i += 1
                        result += tanaMeisaiTA.InsertTana(tanaoroshiYM,
                                            sethinRow.棚番号,
                                            i,
                                            sethinRow.商品管理番号,
                                            sethinRow.仕入先コード,
                                            sethinRow.仕入先履歴番号,
                                            sethinRow.担当者番号,
                                            sethinRow.数量,
                                            sethinRow.原価単価合計,
                                            sethinRow.商品管理番号,
                                             sethinRow.入力端末番号,
                                            sethinRow.フロアコード,
                                            sethinRow.処理済みフラグ)

                    Next

                Next

                '棚卸明細から親のセット品を削除
                resultDelete = N058TA.DeleteTanaSethin()

                scope.Complete()

            End Using

            '終了ログ(d)
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N058")

        Catch ex As Exception

            '終了エラーログ()
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N058")

        End Try


    End Sub

End Module
