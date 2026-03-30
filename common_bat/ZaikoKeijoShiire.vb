Partial Public Class ZaikoHosei

#Region "【メソッド】"

#Region "仕入の在庫計上を実行する"

    ''' <summary>
    ''' 仕入の在庫計上を実行する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ExecuteZaikoKeijoShiire(ByVal shohinKanriNo As String,
                                            ByVal shiireDenpyoNo As String,
                                            ByVal zaikoKubun As String,
                                            ByVal suryo As Int32,
                                            ByVal loginUser As String,
                                            Optional accessKubun As String = "") As Boolean

        ' ---------------------------------------------------------------------------------------------------

        ' 商品マスタデータを取得する
        Dim shohinTableAdapter As New CommonTableAdapters.M_商品TableAdapter
        Dim shohinDt As Common.M_商品DataTable = shohinTableAdapter.SelectByShohinKanriNo(shohinKanriNo)

        If shohinDt.Rows.Count > 0 Then

            If DirectCast(shohinDt.Rows(0), Common.M_商品Row).在庫対象外フラグ = True Then

                Return True

            End If

        End If

        ' ---------------------------------------------------------------------------------------------------

        If accessKubun = String.Empty Then
            accessKubun = common_bat.AccessKubun.Shiire
        End If

        '商品アクセスへ登録
        Dim accessAdpter As New CommonTableAdapters.T_商品アクセスTableAdapter
        Dim updateCount As Integer = accessAdpter.UpdateShohinAccess(
                                        accessKubun,
                                        loginUser,
                                        shohinKanriNo)
        If updateCount = 0 Then

            accessAdpter.InsertShohinAccess(
                                        shohinKanriNo,
                                        accessKubun,
                                        loginUser)

        End If

        ' ---------------------------------------------------------------------------------------------------

        ' 在庫データを取得する

        Dim zaikoTableAdapter As New CommonTableAdapters.T_在庫WORKTableAdapter
        Dim zaikoDt As Common.T_在庫WORKDataTable = zaikoTableAdapter.SelectZaikoKeijoByShohinKanriZaikoKubun(shohinKanriNo,
                                                                                                          Convert.ToString(zaikoKubun))
        Dim result As Int32 = 0

        ' ---------------------------------------------------------------------------------------------------

        ' 在庫データ作成
        If zaikoDt.Rows.Count = 0 Then

            ' 在庫データ作成
            result = zaikoTableAdapter.InsertZaikoData(shohinKanriNo,
                                                       Convert.ToString(IIf(String.IsNullOrEmpty(shiireDenpyoNo) = True, common_bat.Constant.ZAIKO_SHIIRE_DENPYONO_ALL_ZERO, shiireDenpyoNo)),
                                                       Convert.ToString(zaikoKubun),
                                                       suryo,
                                                       loginUser)

            Return True

        Else

            ' 在庫データ作成ループ

            For Each zaikoDtRow As Common.T_在庫WORKRow In zaikoDt.Rows

                ' 在庫引当

                If zaikoDtRow.数量 < 0 Then

                    'マイナス在庫時存在時


                    ' 在庫データを計算する

                    suryo += zaikoDtRow.数量


                    If suryo > 0 Then

                        ' 在庫数0のデータは削除する
                        result = zaikoTableAdapter.DeleteZeroZaiko(zaikoDtRow.商品管理番号,
                                                                   zaikoDtRow.仕入伝票番号,
                                                                   zaikoDtRow.在庫区分)


                        ' 在庫データ作成
                        result = zaikoTableAdapter.InsertZaikoData(shohinKanriNo,
                                                                   Convert.ToString(IIf(String.IsNullOrEmpty(shiireDenpyoNo) = True, common_bat.Constant.ZAIKO_SHIIRE_DENPYONO_ALL_ZERO, shiireDenpyoNo)),
                                                                   Convert.ToString(zaikoKubun),
                                                                   suryo,
                                                                   loginUser)

                        Return True

                    ElseIf suryo < 0 Then

                        ' 在庫データ更新
                        result = zaikoTableAdapter.UpdateZaikoData(suryo,
                                                                   loginUser,
                                                                   shohinKanriNo,
                                                                   zaikoDtRow.仕入伝票番号,
                                                                   Convert.ToString(zaikoKubun))

                        Return True

                    Else

                        ' 在庫数0のデータは削除する
                        result = zaikoTableAdapter.DeleteZeroZaiko(zaikoDtRow.商品管理番号,
                                                                   zaikoDtRow.仕入伝票番号,
                                                                   zaikoDtRow.在庫区分)

                        Return True

                    End If

                Else

                    'プラス在庫存在時
                    If zaikoDtRow.仕入伝票番号 = common_bat.Constant.ZAIKO_SHIIRE_DENPYONO_ALL_ZERO Then

                        ' 在庫データを計算する
                        suryo += zaikoDtRow.数量

                        ' 対象の行を削除する
                        result = zaikoTableAdapter.DeleteZeroZaiko(zaikoDtRow.商品管理番号,
                                                                   zaikoDtRow.仕入伝票番号,
                                                                   zaikoDtRow.在庫区分)

                        ' 在庫データ作成
                        result = zaikoTableAdapter.InsertZaikoData(shohinKanriNo,
                                                                   Convert.ToString(IIf(String.IsNullOrEmpty(shiireDenpyoNo) = True, common_bat.Constant.ZAIKO_SHIIRE_DENPYONO_ALL_ZERO, shiireDenpyoNo)),
                                                                   Convert.ToString(zaikoKubun),
                                                                   suryo,
                                                                   loginUser)

                        Return True

                    ElseIf zaikoDtRow.仕入伝票番号 = shiireDenpyoNo Then

                        ' 在庫データを計算する
                        suryo += zaikoDtRow.数量

                        If suryo > 0 Then

                            ' 在庫数0のデータは削除する
                            result = zaikoTableAdapter.DeleteZeroZaiko(zaikoDtRow.商品管理番号,
                                                                       zaikoDtRow.仕入伝票番号,
                                                                       zaikoDtRow.在庫区分)


                            ' 在庫データ作成
                            result = zaikoTableAdapter.InsertZaikoData(shohinKanriNo,
                                                                       Convert.ToString(IIf(String.IsNullOrEmpty(shiireDenpyoNo) = True, common_bat.Constant.ZAIKO_SHIIRE_DENPYONO_ALL_ZERO, shiireDenpyoNo)),
                                                                       Convert.ToString(zaikoKubun),
                                                                       suryo,
                                                                       loginUser)

                            Return True

                        ElseIf suryo < 0 Then

                            ' 在庫データ更新
                            result = zaikoTableAdapter.UpdateZaikoData(suryo,
                                                                       loginUser,
                                                                       shohinKanriNo,
                                                                       zaikoDtRow.仕入伝票番号,
                                                                       Convert.ToString(zaikoKubun))

                            Return True

                        Else

                            ' 在庫数0のデータは削除する
                            result = zaikoTableAdapter.DeleteZeroZaiko(zaikoDtRow.商品管理番号,
                                                                       zaikoDtRow.仕入伝票番号,
                                                                       zaikoDtRow.在庫区分)

                            Return True

                        End If


                    End If

                End If

            Next

        End If

        ' 在庫データ作成
        If suryo <> 0 Then

            result = zaikoTableAdapter.InsertZaikoData(shohinKanriNo,
                                                       Convert.ToString(IIf(String.IsNullOrEmpty(shiireDenpyoNo) = True, common_bat.Constant.ZAIKO_SHIIRE_DENPYONO_ALL_ZERO, shiireDenpyoNo)),
                                                       Convert.ToString(zaikoKubun),
                                                       suryo,
                                                       loginUser)

        End If

        Return True

    End Function

#End Region

#End Region

End Class

