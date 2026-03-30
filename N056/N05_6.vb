Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim updateTime As DateTime = DateTime.Now
        Dim zaikoYM As String = common_bat.ValueToDate(common_bat.BatDate).ToString("yyyyMM")
        Dim tanaWorkTA As New N056TableAdapters.QueriesTableAdapter()
        Dim tanaoroshiWorkTA As New N056TableAdapters.T_棚卸明細WORKTableAdapter
        Dim tanaoroshiWorkDT As N056.T_棚卸明細WORKDataTable
        Dim moneyList As New List(Of Decimal)
        Dim result As Int32 = 0
        Dim tanadateTA As New N056TableAdapters.M_コードTableAdapter
        Dim tanaoroshiYM As String = tanadateTA.SelectTanaoroshiYM

        Try

            '開始ログ()
            batLogTadap.InsertBatLog(updateTime, "N056", DateTime.Now)

            'Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(80000000000))

            'T_棚卸明細WORKへのDELETE,INSERT処理(データはT_棚卸明細から取得)
            tanaoroshiWorkTA.DeleteTanaWork()
            tanaWorkTA.InsertSelectTanaWork(tanaoroshiYM)

            tanaoroshiWorkDT = tanaoroshiWorkTA.GetData

            'T_棚卸明細WORKの更新
            For Each tanaoroshiRow As N056.T_棚卸明細WORKRow In tanaoroshiWorkDT

                moneyList = getMoneyList(tanaoroshiRow.商品管理番号, tanaoroshiRow.数量)

                result = tanaoroshiWorkTA.UpdateTanaWork(moneyList(0), moneyList(1), tanaoroshiRow.商品管理番号)

            Next

            'T_月初在庫へのINSERT処理
            tanaWorkTA.DeleteGessyo(zaikoYM)
            tanaWorkTA.InsertSelectGesyoZaiko(common_bat.Constant.ZAIKO_SHIIRE_DENPYONO_ALL_ZERO, zaikoYM)

            'scope.Complete()

            'End Using

            '終了ログ(d)
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N056")

        Catch ex As Exception

            '終了エラーログ()
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N056")

        End Try

    End Sub

    Private Function getMoneyList(shohinKanriNo As String, suryo As Decimal) As List(Of Decimal)

        Dim mShohintTa As New N056TableAdapters.M_商品TableAdapter
        Dim mShiireTa As New N056TableAdapters.T_仕入明細TableAdapter

        Dim returnList As New List(Of Decimal)

        Dim genkalist As New List(Of Decimal)
        Dim jyodailist As New List(Of Decimal)

        Try

            For Each dtRowShiire As N056.T_仕入明細Row In mShiireTa.GetData(shohinKanriNo)

                Dim nowSuryo As Decimal

                If suryo > dtRowShiire.数量 Then
                    nowSuryo = dtRowShiire.数量

                Else
                    nowSuryo = suryo
                End If

                suryo -= dtRowShiire.数量

                genkalist.Add(dtRowShiire.原価単価 * nowSuryo)
                jyodailist.Add(dtRowShiire.上代単価 * nowSuryo)

                If suryo <= 0 Then
                    Exit For
                End If

            Next

            If suryo > 0 Then

                Try
                    Dim dtRow As N056.M_商品Row = mShohintTa.GetData(shohinKanriNo)(0)

                    genkalist.Add(dtRow.原価単価 * suryo)
                    jyodailist.Add(dtRow.上代単価 * suryo)

                Catch ex As Exception

                    genkalist.Add(0)
                    jyodailist.Add(0)

                End Try

            End If

            returnList.Add(genkalist.Sum)
            returnList.Add(jyodailist.Sum)

            Return returnList

        Catch ex As Exception

            Throw (ex)

        End Try

    End Function

End Module
