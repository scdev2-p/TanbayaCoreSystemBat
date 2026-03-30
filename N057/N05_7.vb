Module Module1

    'N057 T_棚卸明細データ補正バッチ
    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()        
        Dim updateTime As DateTime = DateTime.Now
        Dim zaikoCommon As New common_bat.ZaikoHosei

        Try

            '開始ログ出力
            batLogTadap.InsertBatLog(updateTime, "N057", DateTime.Now)

            Dim tanadateTA As New N057TableAdapters.M_コードTableAdapter
            Dim tanaoroshiYM As String = tanadateTA.SelectTanaoroshiYM
            Dim tanaMeisaiTA As New N057TableAdapters.T_棚卸明細TableAdapter
            Dim tanaMeisaiDT As N057.T_棚卸明細DataTable = tanaMeisaiTA.SelectTanaMeisai(tanaoroshiYM)
            Dim list As List(Of Decimal)
            Dim denpyoNo As String = String.Empty

            For Each tanaMeisaiRow As N057.T_棚卸明細Row In tanaMeisaiDT

                '仕入明細から原価・上代を引き直し
                list = getMoneyList(tanaMeisaiRow.商品管理番号, tanaMeisaiRow.数量, denpyoNo)

                'T_棚卸明細を引き直した原価・上代で更新
                Dim i As Integer = tanaMeisaiTA.UpdateTanaMeisai(list(0),
                                                                 list(1),
                                                                 denpyoNo,
                                                                 tanaoroshiYM,
                                                                 tanaMeisaiRow.棚番号,
                                                                 tanaMeisaiRow.行番号)

            Next

            '終了ログ出力
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N057")

        Catch ex As Exception

            '終了エラーログ出力
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N057")

        End Try

    End Sub

    Public Function getMoneyList(shohinKanriNo As String, suryo As Decimal, ByRef denpyoNo As String) As List(Of Decimal)

        Dim mShohintTa As New N057TableAdapters.M_商品TableAdapter
        Dim mShiireTa As New N057TableAdapters.T_仕入明細TableAdapter

        Dim returnList As New List(Of Decimal)
        Dim genkalist As New List(Of Decimal)
        Dim jyodailist As New List(Of Decimal)
        Dim denpyoNoList As New List(Of String)

        Try

            For Each dtRowShiire As N057.T_仕入明細Row In mShiireTa.GetData(shohinKanriNo)

                Dim nowSuryo As Decimal

                If suryo > dtRowShiire.数量 Then
                    nowSuryo = dtRowShiire.数量
                Else
                    nowSuryo = suryo
                End If

                suryo -= dtRowShiire.数量

                genkalist.Add(dtRowShiire.原価単価 * nowSuryo)
                jyodailist.Add(dtRowShiire.上代単価 * nowSuryo)
                denpyoNoList.Add(dtRowShiire.仕入伝票番号)

                If suryo <= 0 Then
                    Exit For
                End If

            Next

            If suryo > 0 Then

                Try

                    Dim dtRow As N057.M_商品Row = mShohintTa.GetData(shohinKanriNo)(0)

                    genkalist.Add(dtRow.原価単価 * suryo)
                    jyodailist.Add(dtRow.上代単価 * suryo)
                    'M_商品の場合は仕入伝票番号ALL0にする
                    denpyoNoList.Add(common_bat.Constant.ZAIKO_SHIIRE_DENPYONO_ALL_ZERO)

                Catch ex As Exception

                    genkalist.Add(0)
                    jyodailist.Add(0)
                    denpyoNoList.Add(common_bat.Constant.ZAIKO_SHIIRE_DENPYONO_ALL_ZERO)

                End Try

            Else

                If denpyoNoList.Count = 0 Then

                    denpyoNoList.Add(common_bat.Constant.ZAIKO_SHIIRE_DENPYONO_ALL_ZERO)

                End If

            End If

            returnList.Add(genkalist.Sum)
            returnList.Add(jyodailist.Sum)
            denpyoNo = denpyoNoList(0)

            Return returnList

        Catch ex As Exception

            Throw (ex)

        End Try

    End Function

End Module
