Imports System.Collections.Generic

Partial Public Class ZaikoHosei

#Region "【メソッド】"

#Region "売上の在庫引当を実行する"

    ''' <summary>
    ''' 棚卸の時の在庫引当を実行する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ExecuteZaikoTanaoroshi(ByVal zaikoValue As ZaikoValueObject)


        Try

            'レジ伝票番号（伝票年月日(8桁)＋出力レジ番号(3桁)＋レジ伝票連番(4桁)）
            Dim zaikoAdapter As New CommonTableAdapters.T_在庫TableAdapter(COMMAND_TIME_OUT)

            '更新者はレジ担当者
            Dim updateUser As String = zaikoValue.TantoShainCode

            '数量が0の場合はスキップ
            If zaikoValue.Suryo = 0 Then
                Exit Sub
            End If

            'セット商品の場合は内訳商品の在庫引当を行う
            Dim hikiateZaikos() As ZaikoValueObject
            If zaikoValue.IsSetShohin Then

                'セット商品の場合
                hikiateZaikos = Me.getSetShohinList(zaikoValue)

            Else

                'セット商品ではない場合は在庫引当処理ループを１回行うよう設定
                ReDim hikiateZaikos(0)
                hikiateZaikos(0) = zaikoValue.CloneCopy()

            End If

            '在庫引当処理
            For i As Integer = 0 To hikiateZaikos.Length - 1

                Dim hikiateZaiko As ZaikoValueObject = hikiateZaikos(i)

                '在庫処理対象外の場合はT売上にのみ登録
                If hikiateZaiko.isOutOfZaiko Then

                    Continue For

                End If


                If Not hikiateZaiko.IsShokaNohin Then

                    '通常商品の場合

                    '通常商品の在庫引き当て処理
                    Me.hikiateShohin2(zaikoAdapter,
                                      updateUser,
                                      hikiateZaiko)


                End If

            Next i

        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Sub

#End Region

#Region "通常の商品の在庫引き当て処理を行う"

    ''' <summary>
    ''' 通常の商品の在庫引き当て処理を行う
    ''' </summary>
    ''' <param name="zaikoAdapter">T_在庫TableAdapter</param>
    ''' <param name="updateUser">更新者</param>
    ''' <param name="zaiko">在庫ValueObject</param>
    ''' <remarks></remarks>
    Private Sub hikiateShohin2(ByRef zaikoAdapter As CommonTableAdapters.T_在庫TableAdapter,
                               ByVal updateUser As String,
                               ByVal zaiko As ZaikoValueObject)

        Try

            Dim suryo As Integer = zaiko.Suryo

            'T在庫取得
            Dim zaikoDt As Common.T_在庫DataTable = zaikoAdapter.SelectZaikoKeijoByShohinKanriZaikoKubun( _
                                                                zaiko.ShohinKanriNo, Convert.ToString(zaiko.ZaikoKubun))

            'T在庫に当該在庫が全くない場合
            '売上の場合はダミー仕入伝票番号で在庫へマイナス計上
            '返品の場合は最新仕入伝票番号（取得できない場合はダミー）で在庫へプラス計上
            If zaikoDt.Rows.Count = 0 Then

                'T在庫へダミー仕入伝票番号で登録
                Dim shiireDenpyoNo As String = "0000000000"
                zaikoAdapter.InsertZaikoData(zaiko.ShohinKanriNo,
                                             shiireDenpyoNo,
                                             Convert.ToString(zaiko.ZaikoKubun),
                                             suryo * (-1),
                                             updateUser)

                Exit Sub

            End If


            '在庫データから引き落とし（売上・廃棄分を減算）
            For Each zaikoDtRow As Common.T_在庫Row In zaikoDt.Rows

                '数量0は除外
                If zaikoDtRow.数量 = 0 Then
                    Continue For
                End If

                Dim hikiatoshiSuryo As Integer = 0
                If zaikoDtRow.仕入伝票番号 = "0000000000" Then

                    'ダミー仕入在庫の場合
                    'ダミー仕入在庫の数量はマイナス、プラスを許容するため、数量全てを引き落としor計上する

                    ' 在庫データを計算する
                    If suryo = zaikoDtRow.数量 Then

                        '在庫数が０になったデータは削除する
                        hikiatoshiSuryo = zaikoDtRow.数量

                        zaikoAdapter.DeleteZeroZaiko(zaikoDtRow.商品管理番号,
                                                     zaikoDtRow.仕入伝票番号,
                                                     zaikoDtRow.在庫区分)

                    Else

                        '在庫データ更新
                        hikiatoshiSuryo = suryo
                        zaikoAdapter.UpdateZaikoData((zaikoDtRow.数量 - hikiatoshiSuryo),
                                                     updateUser,
                                                     zaiko.ShohinKanriNo,
                                                     zaikoDtRow.仕入伝票番号,
                                                     Convert.ToString(zaiko.ZaikoKubun))

                    End If

                    '残数量
                    suryo = 0

                Else

                    '通常仕入在庫の場合
                    '通常仕入在庫の数量はマイナス値を許容しない

                    ' 在庫データを計算する
                    If (zaikoDtRow.数量 - suryo) <= 0 Then

                        '在庫数が０になったデータは削除する
                        hikiatoshiSuryo = zaikoDtRow.数量

                        zaikoAdapter.DeleteZeroZaiko(zaikoDtRow.商品管理番号,
                                                     zaikoDtRow.仕入伝票番号,
                                                     zaikoDtRow.在庫区分)
                        '残数量＝引き当てできた分を減算
                        suryo -= hikiatoshiSuryo

                    Else

                        ' 在庫データ更新
                        hikiatoshiSuryo = suryo
                        zaikoAdapter.UpdateZaikoData((zaikoDtRow.数量 - hikiatoshiSuryo),
                                                     updateUser,
                                                     zaiko.ShohinKanriNo,
                                                     zaikoDtRow.仕入伝票番号,
                                                     Convert.ToString(zaiko.ZaikoKubun))
                        '残数量
                        suryo = 0

                    End If

                End If

                '売上個数が全て反映できたらExit
                If suryo = 0 Then '返品の場合は<=0となることに注意
                    Exit For
                End If

            Next


            '在庫データから売上分を減算しきれなかった場合はダミー仕入在庫を作成
            If suryo <> 0 Then '返品の場合は<=0となることに注意

                'ダミー仕入在庫または仕入在庫を作成して計上
                Dim shiireDenpyoNo As String = "0000000000"
                Dim dummySuryo As Integer = suryo * (-1)
                Dim updateCount As Integer = zaikoAdapter.UpdateAddZaikoSuryo(dummySuryo,
                                                                              updateUser,
                                                                              zaiko.ShohinKanriNo,
                                                                              shiireDenpyoNo,
                                                                              Convert.ToString(zaiko.ZaikoKubun))
                If updateCount = 0 Then
                    zaikoAdapter.InsertZaikoData(zaiko.ShohinKanriNo,
                                                 shiireDenpyoNo,
                                                 Convert.ToString(zaiko.ZaikoKubun),
                                                 dummySuryo,
                                                 updateUser)
                End If

            End If

        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Sub

#End Region

#Region "セット商品を一元化する"

    ''' <summary>
    ''' セット商品を一元化する
    ''' ※上代・下代・販売金額は子商品の比率で算出する
    ''' </summary>
    ''' <param name="parent">セット商品のValueObject</param>
    ''' <remarks></remarks>
    ''' <returns>セット商品（子商品）</returns>
    Private Function getSetShohinList(ByVal parent As ZaikoValueObject) As ZaikoValueObject()

        Try

            'セット商品の場合
            '下代、上代、販売金額を比率で算出する
            Dim shohinAdapter As New CommonTableAdapters.M_商品TableAdapter
            Dim setShohinAdapter As New CommonTableAdapters.M_セット商品TableAdapter
            Dim setShohinDt As Common.M_セット商品DataTable = setShohinAdapter.SelectBySetShohinKanriNo(parent.ShohinKanriNo)
            If setShohinDt.Rows.Count = 0 Then

                Throw New Exception("M_セット商品の情報取得に失敗しました。 " & _
                                    "商品管理番号=" & parent.ShohinKanriNo & " はセット商品として登録されていません。")

            End If

            '子商品の上代販売合計
            Dim totalJodai As Decimal = 0

            '子商品を一元化
            Dim shohins(setShohinDt.Rows.Count - 1) As ZaikoValueObject
            For i As Integer = 0 To shohins.Length - 1

                '親商品の情報をコピーする
                'Me.copyZaikoValueObject(parent, shohins(0))
                shohins(i) = parent.CloneCopy()

                '子商品の商品管理番号、数量をM_セットから取得
                Dim shohinDtRow As Common.M_セット商品Row = DirectCast(setShohinDt.Rows(i), Common.M_セット商品Row)
                shohins(i).ShohinKanriNo = shohinDtRow.商品管理番号
                'shohins(i).Suryo = shohinDtRow.数量 * parent.Suryo
                shohins(i).Suryo = shohinDtRow.数量

                '子商品の情報をM_商品から取得
                Dim shohinDt As Common.M_商品DataTable = shohinAdapter.SelectByShohinKanriNo(shohins(i).ShohinKanriNo)
                If shohinDt.Rows.Count = 0 Then

                    Throw New Exception("セット商品の子商品の上代取得に失敗しました。 " & _
                                        "親商品管理番号=" & parent.ShohinKanriNo & ", 商品管理番号=" & shohins(i).ShohinKanriNo)

                End If

                '上代・下代・販売を記憶
                shohins(i).JodaiTanka = shohinDt(0).上代単価
                shohins(i).GedaiTanka = 0
                shohins(i).HanbaiKingaku = 0

                '子商品情報で上書き
                shohins(i).BarCode = shohinDt(0).バーコード
                shohins(i).FloorCode = shohinDt(0).フロアコード
                shohins(i).isOutOfZaiko = shohinDt(0).在庫対象外フラグ
                shohins(i).IsSetShohin = shohinDt(0).セット品フラグ
                shohins(i).IsShokaNohin = shohinDt(0).消化納品フラグ
                shohins(i).KakerituCode = shohinDt(0).掛率コード
                shohins(i).ShiireCode = shohinDt(0).仕入先コード
                shohins(i).ShiireRirekiNo = shohinDt(0).仕入先履歴番号
                shohins(i).ShohinName = shohinDt(0).商品名
                shohins(i).ShohinCode = shohinDt(0).商品コード
                shohins(i).ShohinGroupCode = shohinDt(0).商品グループコード

                '合計上代へ加算
                totalJodai += shohins(i).JodaiTanka * shohins(i).Suryo

            Next

            '子商品の上代・下代・販売合計
            '※最後の子商品で誤差を吸収するため、１～(ｎ-1)個目までの合計金額を取得する
            Dim subtotalJodai As Decimal = 0
            Dim subtotalGedai As Decimal = 0
            Dim subotalHanbaiKingaku As Decimal = 0
            For i As Integer = 0 To shohins.Length - 2

                '上代・下代・販売を算出
                Dim rate As Decimal = shohins(i).JodaiTanka * shohins(i).Suryo / totalJodai
                shohins(i).JodaiTanka = Fix(parent.JodaiTanka * rate)
                shohins(i).GedaiTanka = Fix(parent.GedaiTanka * rate)
                shohins(i).HanbaiKingaku = Fix(parent.HanbaiKingaku * rate)

                '数量を算出
                shohins(i).Suryo = shohins(i).Suryo * parent.Suryo

                '合計上代・下代・販売へ加算
                subtotalJodai += shohins(i).JodaiTanka
                subtotalGedai += shohins(i).GedaiTanka
                subotalHanbaiKingaku += shohins(i).HanbaiKingaku

            Next

            '最後の子商品で誤差を吸収する
            Dim lastShohin As ZaikoValueObject = shohins(shohins.Length - 1)
            lastShohin.JodaiTanka = Fix((parent.JodaiTanka - subtotalJodai) / lastShohin.Suryo)
            lastShohin.GedaiTanka = Fix((parent.GedaiTanka - subtotalGedai) / lastShohin.Suryo)
            lastShohin.HanbaiKingaku = Fix((parent.HanbaiKingaku - subotalHanbaiKingaku))
            lastShohin.Suryo = lastShohin.Suryo * parent.Suryo

            Return shohins

        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Function

#End Region

#End Region

End Class
