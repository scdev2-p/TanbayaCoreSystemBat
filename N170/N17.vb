Module N17

    Sub Main()

        Dim updateTime As DateTime = DateTime.Now
        Dim batLogTA As New common_bat.CommonTableAdapters.バッチログTableAdapter()

        Try

            ' 開始ログ
            batLogTA.InsertBatLog(updateTime, "N170", DateTime.Now)

            'トランザクション開始
            'Using scope As New System.Transactions.TransactionScope

            Dim thisYearMonth As String = common_bat.BatDate.Substring(0, 6)
            Dim lastYesrMonth As String = common_bat.ValueToDate(common_bat.BatDate).AddMonths(-1).ToString("yyyyMM")

            Dim koritsuTA As New N170TableAdapters.効率表集計TableAdapter(common_bat.COMMAND_TIME_OUT)
            Dim hinbanKoritsuTA As New N170TableAdapters.品番別効率表TableAdapter(common_bat.COMMAND_TIME_OUT)
            Dim hinbanKoritsuDT As New N170.品番別効率表DataTable

            Dim koritsu99TA As New N170TableAdapters.品番別効率表99以外TableAdapter(common_bat.COMMAND_TIME_OUT)
            Dim hinbanKoritsuDT2 As N170.品番別効率表DataTable = hinbanKoritsuTA.SelectKourituList(lastYesrMonth,
                                                                                            thisYearMonth,
                                                                                            common_bat.BatDate)

            Dim targetFloorCD As String = String.Empty
            Dim targetFloorName As String = String.Empty
            Dim targetStartFloorCD As String = String.Empty
            Dim is99First As Boolean = True

            For Each kourituRow As N170.品番別効率表Row In hinbanKoritsuDT2.Rows

                If targetFloorCD = String.Empty Then

                    targetStartFloorCD = kourituRow.フロアコード
                    targetFloorCD = kourituRow.フロアコード
                    targetFloorName = kourituRow.フロア名

                End If

                If targetFloorCD <> kourituRow.フロアコード Then

                    If kourituRow.フロアコード = "99*" AndAlso is99First Then

                        hinbanKoritsuDT.ImportRow(koritsu99TA.SelectKourituList(lastYesrMonth, thisYearMonth, common_bat.BatDate).Rows(0))
                        hinbanKoritsuDT(hinbanKoritsuDT.Rows.Count - 1).フロアコード = "9**"

                        is99First = False

                    Else

                        hinbanKoritsuDT.ImportRow(koritsuTA.SelectFloorBetweenSummary(targetFloorCD,
                                                                                        targetFloorName,
                                                                                        lastYesrMonth,
                                                                                        thisYearMonth,
                                                                                        targetFloorCD,
                                                                                        targetFloorCD,
                                                                                        common_bat.BatDate).Rows(0))

                    End If

                    targetFloorCD = kourituRow.フロアコード
                    targetFloorName = kourituRow.フロア名

                End If

                hinbanKoritsuDT.ImportRow(kourituRow)

            Next

            ' 最後のフロアの計を設定
            hinbanKoritsuDT.ImportRow(koritsuTA.SelectFloorBetweenSummary(targetFloorCD,
                                                                            targetFloorName,
                                                                            lastYesrMonth,
                                                                            thisYearMonth,
                                                                            targetFloorCD,
                                                                            targetFloorCD,
                                                                            common_bat.BatDate).Rows(0))

            'hinbanKourituListDT(hinbanKourituListDT.Rows.Count - 1).フロアコード = "9**"
            hinbanKoritsuDT(hinbanKoritsuDT.Rows.Count - 1).品番 = "9**"

            ' 全ての総合計を設定
            hinbanKoritsuDT.ImportRow(koritsuTA.SelectFloorBetweenSummary("***",
                                                                            "全社 総合計",
                                                                            lastYesrMonth,
                                                                            thisYearMonth,
                                                                            targetFloorCD,
                                                                            targetFloorCD,
                                                                            common_bat.BatDate).Rows(0))


            'T_部門別効率表の削除
            Dim koritsuhyoTA As New N170TableAdapters.T_部門別効率表TableAdapter
            koritsuhyoTA.DeleteKoritsuhyo(lastYesrMonth)

            'T_部門別効率表の登録
            For i As Integer = 0 To hinbanKoritsuDT.Rows.Count - 1

                Try

                    koritsuhyoTA.InsertKoritsuhyo(lastYesrMonth,
                      hinbanKoritsuDT.Rows(i).Item("品番"),
                      hinbanKoritsuDT.Rows(i).Item("商品グループ名カナ"),
                      hinbanKoritsuDT.Rows(i).Item("フロアコード"),
                      hinbanKoritsuDT.Rows(i).Item("フロア名"),
                      hinbanKoritsuDT.Rows(i).Item("月首原価"),
                      hinbanKoritsuDT.Rows(i).Item("月首上代"),
                      hinbanKoritsuDT.Rows(i).Item("仕入原価"),
                      hinbanKoritsuDT.Rows(i).Item("仕入上代"),
                      hinbanKoritsuDT.Rows(i).Item("原価合計"),
                      hinbanKoritsuDT.Rows(i).Item("上代合計"),
                      hinbanKoritsuDT.Rows(i).Item("月末原価"),
                      hinbanKoritsuDT.Rows(i).Item("月末上代"),
                      hinbanKoritsuDT.Rows(i).Item("月末原価_棚卸"),
                      hinbanKoritsuDT.Rows(i).Item("月末上代_棚卸"),
                      hinbanKoritsuDT.Rows(i).Item("月末個数"),
                      hinbanKoritsuDT.Rows(i).Item("原価売上"),
                      hinbanKoritsuDT.Rows(i).Item("上代売上"),
                      hinbanKoritsuDT.Rows(i).Item("粗利金額"),
                      hinbanKoritsuDT.Rows(i).Item("売価売上"),
                      hinbanKoritsuDT.Rows(i).Item("売上構成比"),
                      hinbanKoritsuDT.Rows(i).Item("粗利構成比"),
                      hinbanKoritsuDT.Rows(i).Item("粗利率"),
                      hinbanKoritsuDT.Rows(i).Item("廃棄上代"),
                      hinbanKoritsuDT.Rows(i).Item("回転率"),
                      hinbanKoritsuDT.Rows(i).Item("在庫日数"),
                      hinbanKoritsuDT.Rows(i).Item("交差率"),
                      hinbanKoritsuDT.Rows(i).Item("交差日数"),
                      hinbanKoritsuDT.Rows(i).Item("実質原価"),
                      hinbanKoritsuDT.Rows(i).Item("原価差額"),
                      hinbanKoritsuDT.Rows(i).Item("実質上代"),
                      hinbanKoritsuDT.Rows(i).Item("上代差額"),
                      hinbanKoritsuDT.Rows(i).Item("実質個数"),
                      hinbanKoritsuDT.Rows(i).Item("個数差"),
                      hinbanKoritsuDT.Rows(i).Item("営業日数"))

                Catch ex As Exception

                    koritsuhyoTA.InsertKoritsuhyoZero(lastYesrMonth,
                      hinbanKoritsuDT.Rows(i).Item("品番"),
                      hinbanKoritsuDT.Rows(i).Item("商品グループ名カナ"),
                      hinbanKoritsuDT.Rows(i).Item("フロアコード"),
                      hinbanKoritsuDT.Rows(i).Item("フロア名"),
                      hinbanKoritsuDT.Rows(i).Item("営業日数"))

                End Try

            Next

            'トランザクションコミット
            'scope.Complete()

            'End Using

            ' 終了ログ
            batLogTA.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N170")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTA.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N170")

        End Try

    End Sub

End Module
