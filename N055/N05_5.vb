Module Module1

#Region "修正履歴"

    '2014/03/25 在庫がマイナスになった場合はマスタの単価で処理 Kanai

    '2025/11/11 デグレを元に戻す　Takagi@sc

#End Region

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim targetYM As String = common_bat.ValueToDate(common_bat.BatDate).ToString("yyyyMM")
        Dim tanadateTA As New N055TableAdapters.M_コードTableAdapter
        Dim tanaoroshiYM As String = tanadateTA.SelectTanaoroshiYM
        Dim ShireDenpyo As String = targetYM.Substring(2, 4) & "000000"
        
        Dim updateTime As DateTime = DateTime.Now

        Dim shiireTA As New N055TableAdapters.仕入TableAdapter
        Dim tanaJizenTA As New N055TableAdapters.T_棚卸事前集計TableAdapter
        Dim zaikoWorkTA As New N055TableAdapters.T_在庫WORKTableAdapter
        Dim shiireDT As N055.仕入DataTable
        Dim zaikoCommon As New common_bat.ZaikoHosei

        Try

            'Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(80000000000))

            '開始ログ()
            batLogTadap.InsertBatLog(updateTime, "N055", DateTime.Now)

            shiireDT = shiireTA.SelectShiire(ShireDenpyo, tanaoroshiYM & "00", tanaoroshiYM & "99")

            '①在庫ワークに2月入力の1月仕入を反映
            For Each shiireRow As N055.仕入Row In shiireDT

                zaikoCommon.ExecuteZaikoKeijoShiire(shiireRow.商品管理番号,
                                                    shiireRow.仕入伝票番号,
                                                    shiireRow.引当先在庫区分,
                                                    shiireRow.数量,
                                                    "N055")

            Next

            '②棚卸事前集計を削除
            tanaJizenTA.DeleteTanaJizen()

            '③在庫ワークから棚卸事前集計を生成
            Dim zaikoWorkDT As N055.T_在庫WORKDataTable = zaikoWorkTA.GetData
            Dim list As List(Of Decimal)

            For Each zaikoWorkRow As N055.T_在庫WORKRow In zaikoWorkDT

                list = getMoneyList(zaikoWorkRow.商品管理番号, zaikoWorkRow.数量)
                Dim i As Integer = tanaJizenTA.InsertTanaJizen(tanaoroshiYM,
                                            zaikoWorkRow.商品管理番号,
                                            zaikoWorkRow.在庫区分,
                                            zaikoWorkRow.数量,
                                            list(0),
                                            list(1))

            Next

            'scope.Complete()

            'End Using

            '終了ログ()
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N055")

        Catch ex As Exception

            '終了エラーログ()
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N055")

        End Try

    End Sub

    Public Function getMoneyList(ByVal shohinKanriNo As String, ByVal suryo As Decimal) As List(Of Decimal)

        Dim mShohintTa As New N055TableAdapters.M_商品TableAdapter
        Dim mShiireTa As New N055TableAdapters.T_仕入明細TableAdapter

        Dim returnList As New List(Of Decimal)
        Dim genkalist As New List(Of Decimal)
        Dim jyodailist As New List(Of Decimal)

        Try

            For Each dtRowShiire As N055.T_仕入明細Row In mShiireTa.GetData(shohinKanriNo)

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

                    Dim dtRow As N055.M_商品Row = mShohintTa.GetData(shohinKanriNo)(0)

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
