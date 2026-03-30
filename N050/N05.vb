Imports System.Globalization

Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim updateTime As DateTime = DateTime.Now
        Dim tanadateTA As New N050TableAdapters.M_コードTableAdapter
        Dim tanaoroshidate As String = tanadateTA.SelectTanaoroshiYM.ToString
        Dim zaikoCommon As New common_bat.ZaikoHosei

        Try

            '開始ログ()
            batLogTadap.InsertBatLog(updateTime, "N050", DateTime.Now)

            Dim dt As Date = Convert.ToDateTime(tanaoroshidate.Substring(0, 4) & "/" &
                                                tanaoroshidate.Substring(4, 2) & "/" &
                                                tanaoroshidate.Substring(6, 2))

            Do While (dt <= Now)

                Dim targetDay As String = dt.ToString("yyyyMMdd")

                'Using tran As New Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(1, 0, 0))

                '仕入補正
                '棚卸から棚卸確定までの間の仕入業務はSTOPしてもらう
                'shiireHosei(targetDay)

                '売上補正
                uriageHosei(targetDay)

                '浜町入庫補正
                hamachoNyukoHosei(targetDay)

                '浜町出庫補正
                hamachoShukkoHosei(targetDay)

                dt = dt.AddDays(1)

            Loop

            '終了ログ(d)
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N050")

        Catch ex As Exception

            '終了エラーログ()
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N050")

        End Try


    End Sub

#Region "仕入補正"

    'Private Sub shiireHosei(ByVal shiireDay As String)

    '    Dim shiireAdapter As New N050TableAdapters.T_仕入TableAdapter(common_bat.COMMAND_TIME_OUT)

    '    Dim shiireDt As N050.T_仕入DataTable = shiireAdapter.SelectShiire(shiireDay)
    '    Dim zaikoList As New List(Of ZaikoValueObject)
    '    Dim zaikoCommon As New ZaikoHosei
    '    Dim shohinTA As New N050TableAdapters.M_商品TableAdapter(common_bat.COMMAND_TIME_OUT)

    '    Me.txtResult.Text &= vbTab & "仕入補正　件数=[ " & shiireDt.Count & " ] ..."
    '    Dim debug As String = String.Empty

    '    Try

    '        For i As Integer = 0 To shiireDt.Count - 1

    '            Dim denpyoNo As String = shiireDt(i).仕入伝票番号
    '            debug &= "仕入伝票番号=[ " & denpyoNo & " ] 仕入区分=[ " & shiireDt(i).仕入区分 & " ] " & vbNewLine

    '            '品番変更または原価訂正はスキップ
    '            If shiireDt(i).仕入区分 = TnbDefine.ShiireKubun.HinbanHenko OrElse
    '                shiireDt(i).仕入区分 = TnbDefine.ShiireKubun.GenkaTeisei Then
    '                debug &= vbTab & "品番変更または原価訂正のためスキップします" & vbNewLine
    '                Continue For
    '            End If

    '            '仕入伝票明細を取得
    '            Select Case shiireDt(i).仕入区分
    '                Case TnbDefine.ShiireKubun.Shiire, TnbDefine.ShiireKubun.Henpin

    '                    '仕入 or 返品
    '                    Dim denpyoTA As New N050TableAdapters.C010伝票情報TableAdapter(common_bat.COMMAND_TIME_OUT)
    '                    Dim denpyoDT As N050.C010伝票情報DataTable = Nothing
    '                    If shiireDt(i).仕入区分 = TnbDefine.ShiireKubun.Shiire Then
    '                        '仕入データ
    '                        denpyoDT = denpyoTA.SelectHacchuShiire(Convert.ToString(TnbDefine.ZaikoKubun.Honsha), denpyoNo)
    '                        If denpyoDT.Rows.Count = 0 Then
    '                            denpyoDT = denpyoTA.SelectShiire(denpyoNo)
    '                        End If
    '                    Else
    '                        '返品データ
    '                        denpyoDT = denpyoTA.SelectHenpinShiire(denpyoNo)
    '                        If denpyoDT.Rows.Count = 0 Then
    '                            denpyoDT = denpyoTA.SelectShiire(denpyoNo)
    '                        End If
    '                    End If

    '                    debug &= vbTab & "仕入明細件数=[" & denpyoDT.Rows.Count & "]" & vbNewLine

    '                    '件数チェック
    '                    If denpyoDT.Rows.Count = 0 Then
    '                        debug &= vbTab & "仕入明細取得に失敗したためスキップします" & vbNewLine
    '                        Continue For
    '                    End If

    '                    '在庫補正
    '                    For Each zaikoValue As ZaikoValueObject In Me.getZaikoValue(False, shohinTA, denpyoDT, shiireDt(i))

    '                        debug &= vbTab & "商品管理番号=[" & zaikoValue.ShohinKanriNo & "]、数量=[" & zaikoValue.Suryo & "]" & vbNewLine

    '                        Select Case shiireDt(i).仕入区分

    '                            Case TnbDefine.ShiireKubun.Henpin

    '                                zaikoCommon.ExecuteZaikoHikiateMakerHenpin(zaikoValue.ShohinKanriNo,
    '                                                                           shiireDt(i).引当先在庫区分,
    '                                                                           zaikoValue.Suryo,
    '                                                                           Me._updateUser)
    '                                'shiireDt(i).登録者番号)

    '                            Case Else

    '                                zaikoCommon.ExecuteZaikoKeijoShiire(zaikoValue.ShohinKanriNo,
    '                                                                    denpyoNo,
    '                                                                    shiireDt(i).引当先在庫区分,
    '                                                                    zaikoValue.Suryo,
    '                                                                    Me._updateUser)
    '                                'shiireDt(i).登録者番号)

    '                        End Select

    '                    Next

    '                Case TnbDefine.ShiireKubun.HinbanHurikae
    '                    '品番振替
    '                    Dim denpyoTA As New N050TableAdapters.C020伝票情報TableAdapter(common_bat.COMMAND_TIME_OUT)
    '                    Dim denpyoDT As N050.C020伝票情報DataTable = Nothing
    '                    denpyoDT = denpyoTA.SelectHinbanHenkoShiire(denpyoNo)

    '                    debug &= vbTab & "仕入明細件数=[" & denpyoDT.Rows.Count & "]" & vbNewLine

    '                    '件数チェック
    '                    If denpyoDT.Rows.Count = 0 Then
    '                        debug &= vbTab & "仕入明細取得に失敗したためスキップします" & vbNewLine
    '                        Continue For
    '                    End If

    '                    For Each dtRow As N050.C020伝票情報Row In denpyoDT.Rows

    '                        debug &= vbTab & "商品管理番号=[" & dtRow.商品管理番号 & "]、数量=[" & dtRow.数量 & "]" & vbNewLine

    '                        If dtRow.明細番号 Mod 2 = 0 Then

    '                            zaikoCommon.ExecuteZaikoKeijoShiire(dtRow.商品管理番号,
    '                                                                dtRow.伝票番号,
    '                                                                dtRow.引当先在庫区分,
    '                                                                dtRow.数量,
    '                                                                Me._updateUser,
    '                                                                TnbDefine.AccessKubun.HinbanHenko)
    '                            'shiireDt(i).登録者番号,

    '                        Else

    '                            zaikoCommon.ExecuteZaikoHikiateMakerHenpin(dtRow.商品管理番号,
    '                                                                       dtRow.引当先在庫区分,
    '                                                                       dtRow.数量,
    '                                                                       Me._updateUser,
    '                                                                       TnbDefine.AccessKubun.HinbanHenko)
    '                            'shiireDt(i).登録者番号,

    '                        End If

    '                    Next

    '                Case TnbDefine.ShiireKubun.Haiki
    '                    '廃棄
    '                    Dim denpyoTA As New N050TableAdapters.C010伝票情報TableAdapter(common_bat.COMMAND_TIME_OUT)
    '                    Dim denpyoDT As N050.C010伝票情報DataTable = Nothing
    '                    denpyoDT = denpyoTA.SelectHaikiShiire(denpyoNo)
    '                    If denpyoDT.Rows.Count = 0 Then
    '                        denpyoDT = denpyoTA.SelectShiire(denpyoNo)
    '                    End If

    '                    debug &= vbTab & "仕入明細件数=[" & denpyoDT.Rows.Count & "]" & vbNewLine

    '                    '件数チェック
    '                    If denpyoDT.Rows.Count = 0 Then
    '                        debug &= vbTab & "仕入明細取得に失敗したためスキップします" & vbNewLine
    '                        Continue For
    '                    End If

    '                    '在庫補正
    '                    Dim regSeqNo As String = Me.getUriageSeq(shiireDay)
    '                    zaikoCommon.ExecuteZaikoHikiateUriage(shiireDt(i).仕入伝票日付,
    '                                                          TnbDefine.HAIKI_REGISTER_NO + (Convert.ToInt32(regSeqNo) + 1).ToString().PadLeft(4, "0"c),
    '                                                          Me.getZaikoValue(False, shohinTA, denpyoDT, shiireDt(i)),
    '                                                          denpyoNo)
    '            End Select

    '        Next

    '        Me.txtResult.Text &= vbTab & "OK" & vbNewLine

    '    Catch ex As Exception

    '        Me.txtResult.Text &= vbTab & "NG Exception=[" & ex.Message & "]" & vbNewLine
    '        Me.txtResult.Text &= vbTab & "----- debug message -----" & vbNewLine & debug & "-------------------------" & vbNewLine
    '        Throw New Exception(ex.Message, ex)

    '    End Try

    'End Sub

#End Region

#Region "在庫処理を行うための準備を行う"

    ''' <summary>
    ''' 在庫処理を行うための準備を行う
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    'Private Function getZaikoValue(ByVal isBeforeData As Boolean,
    '                               ByVal shohinTA As N050TableAdapters.M_商品TableAdapter,
    '                               ByRef denpyoDT As N050.C010伝票情報DataTable,
    '                               ByVal shiireDT As N050.T_仕入Row) As List(Of ZaikoValueObject)

    '    Dim shohinDT As N050.M_商品DataTable = Nothing
    '    Dim zaikoList As New List(Of ZaikoValueObject)

    '    Dim query = From source In denpyoDT.AsEnumerable
    '                Group By 商品管理番号 = source.商品管理番号 Into 数量 = Sum(source.数量)
    '                Select 商品管理番号, 数量

    '    If isBeforeData Then

    '        query = From source In denpyoDT.AsEnumerable
    '                Group By 商品管理番号 = source.元商品管理番号 Into 数量 = Sum(source.元数量 * -1)
    '                Select 商品管理番号, 数量

    '    End If


    '    For Each item In query

    '        Dim zaikoValueObject As New ZaikoValueObject

    '        shohinDT = shohinTA.SelectByShohinKanriNo(item.商品管理番号)

    '        zaikoValueObject.ShohinKanriNo = shohinDT(0).商品管理番号

    '        Select Case Convert.ToString(shiireDT.引当先在庫区分)

    '            Case Convert.ToString(TnbDefine.ZaikoKubun.Honsha)

    '                zaikoValueObject.ZaikoKubun = TnbDefine.ZaikoKubun.Honsha

    '            Case Convert.ToString(TnbDefine.ZaikoKubun.Hamacho)

    '                zaikoValueObject.ZaikoKubun = TnbDefine.ZaikoKubun.Hamacho

    '            Case Convert.ToString(TnbDefine.ZaikoKubun.ShagaiSoko)

    '                zaikoValueObject.ZaikoKubun = TnbDefine.ZaikoKubun.ShagaiSoko

    '        End Select

    '        zaikoValueObject.IsSetShohin = shohinDT(0).セット品フラグ
    '        zaikoValueObject.IsShokaNohin = shohinDT(0).消化納品フラグ
    '        zaikoValueObject.isOutOfZaiko = shohinDT(0).在庫対象外フラグ
    '        zaikoValueObject.ShohinName = shohinDT(0).商品名
    '        zaikoValueObject.ShohinCode = shohinDT(0).商品コード
    '        zaikoValueObject.BarCode = shohinDT(0).バーコード
    '        zaikoValueObject.KakerituCode = shohinDT(0).掛率コード
    '        zaikoValueObject.FloorCode = shohinDT(0).フロアコード
    '        zaikoValueObject.ShohinGroupCode = shohinDT(0).商品グループコード
    '        zaikoValueObject.ShiireCode = shohinDT(0).仕入先コード
    '        zaikoValueObject.ShiireRirekiNo = shohinDT(0).仕入先履歴番号

    '        zaikoValueObject.KokyakuKanriNo = String.Empty
    '        zaikoValueObject.KokyakuName = String.Empty
    '        zaikoValueObject.KokyakuCode = String.Empty
    '        zaikoValueObject.DaihyoKokyakuCode = String.Empty

    '        zaikoValueObject.JodaiTanka = 0
    '        zaikoValueObject.GedaiTanka = 0
    '        zaikoValueObject.Suryo = item.数量

    '        If shiireDT.仕入区分 = TnbDefine.ShiireKubun.Haiki Then
    '            zaikoValueObject.Suryo = item.数量 * -1
    '        End If

    '        zaikoValueObject.HanbaiKingaku = 0
    '        zaikoValueObject.TantoShainCode = shiireDT.登録者番号
    '        zaikoValueObject.TantoShainName = String.Empty

    '        If zaikoValueObject.Suryo <> 0 Then

    '            zaikoList.Add(zaikoValueObject)

    '        End If

    '    Next

    '    Return zaikoList

    'End Function

#End Region

#Region "売上連番を取得する"

    ''' <summary>
    ''' 売上連番を取得する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getUriageSeq(ByVal denpyoDate As String) As String

        Dim uriageTA As New N050TableAdapters.T_売上TableAdapter(common_bat.COMMAND_TIME_OUT)
        Dim regSeq As Decimal

        regSeq = Convert.ToDecimal(uriageTA.SelectRegRenban(denpyoDate & "999"))

        Return (regSeq + 1).ToString.PadLeft(15, "0"c).Substring((regSeq + 1).ToString.PadLeft(15, "0"c).ToString.Length - 4, 4)

    End Function

#End Region


#Region "売上補正"

    Private Sub uriageHosei(ByVal uriageDay As String)

        Dim uriageAdapter As New N050TableAdapters.T_売上TableAdapter(common_bat.COMMAND_TIME_OUT)
        Dim uriageDt As N050.T_売上DataTable = uriageAdapter.SelectUriage(uriageDay)
        Dim debug As String = String.Empty

        Try

            For i As Integer = 0 To uriageDt.Count - 1

                Dim zaiko As New ZaikoValueObject

                '在庫区分は本社固定
                zaiko.ZaikoKubun = ZaikoKubun.Honsha

                '顧客管理番号、代表顧客コード、顧客コード、顧客名
                zaiko.KokyakuKanriNo = uriageDt(i).顧客管理番号
                zaiko.DaihyoKokyakuCode = uriageDt(i).代表顧客コード
                zaiko.KokyakuName = uriageDt(i).顧客名
                zaiko.KokyakuCode = uriageDt(i).顧客コード

                '商品管理番号、商品名、商品コード、商品グループコード、フロアコード
                zaiko.ShohinKanriNo = uriageDt(i).商品管理番号
                zaiko.ShohinName = uriageDt(i).商品名
                zaiko.ShohinCode = uriageDt(i).商品コード
                zaiko.ShohinGroupCode = uriageDt(i).商品グループコード
                zaiko.FloorCode = uriageDt(i).フロアコード

                '仕入先コード、仕入先履歴番号
                zaiko.ShiireCode = uriageDt(i).仕入先コード
                zaiko.ShiireRirekiNo = uriageDt(i).仕入先履歴番号

                'セット商品フラグ、消化納品フラグ、在庫対象外フラグ
                zaiko.IsSetShohin = uriageDt(i).セット品フラグ
                zaiko.IsShokaNohin = uriageDt(i).消化納品フラグ
                zaiko.isOutOfZaiko = uriageDt(i).在庫対象外フラグ

                'バーコード、掛率コード
                zaiko.BarCode = uriageDt(i).バーコード
                zaiko.KakerituCode = uriageDt(i).掛率コード

                '上代単価、下代単価、数量、販売金額
                zaiko.GedaiTanka = uriageDt(i).下代単価
                zaiko.JodaiTanka = uriageDt(i).上代単価
                zaiko.Suryo = uriageDt(i).数量
                zaiko.HanbaiKingaku = uriageDt(i).販売金額

                '売上担当者番号、売上担当者名
                zaiko.TantoShainCode = uriageDt(i).売上担当者番号
                zaiko.TantoShainName = uriageDt(i).売上担当者名

                Dim zaikoList As New List(Of ZaikoValueObject)
                zaikoList.Add(zaiko)

                Dim zaikoHikiate As New ZaikoHosei
                zaikoHikiate.ExecuteZaikoHikiateUriage(uriageDay,
                                                       uriageDt(i).売上伝票番号.Substring(8),
                                                       zaikoList)

            Next


        Catch ex As Exception


        End Try

    End Sub

#End Region


#Region "浜町入庫補正"

    Private Sub hamachoNyukoHosei(ByVal nyukoDay As String)

        Dim nyukoAdapter As New N050TableAdapters.T_浜町入庫TableAdapter(common_bat.COMMAND_TIME_OUT)
        Dim zaikoAdapter As New N050TableAdapters.T_在庫TableAdapter(common_bat.COMMAND_TIME_OUT)
        Dim nyukoDt As N050.T_浜町入庫DataTable = nyukoAdapter.SelectHamachoNyuko(nyukoDay)

        Dim debug As String = String.Empty

        Try

            Dim count As Integer = 1
            For i As Integer = 0 To nyukoDt.Count - 1

                Dim zaiko As New ZaikoValueObject

                debug &= "商品管理番号=[" & nyukoDt(i).商品管理番号 & "], 入庫元在庫区分=[" & nyukoDt(i).入庫元在庫区分 & "], 入庫数量=[" & nyukoDt(i).入庫数量 & "]" & vbNewLine

                '在庫の移動
                hamachoZaikoMove(zaikoAdapter,
                                    nyukoDt(i).商品管理番号,
                                    nyukoDt(i).入庫元在庫区分,
                                    "2",
                                    nyukoDt(i).入庫数量)

            Next

        Catch ex As Exception



        End Try

    End Sub

#End Region

#Region "浜町出庫補正"

    Private Sub hamachoShukkoHosei(ByVal shukkoDay As String)

        Dim shukkoAdapter As New N050TableAdapters.T_浜町出庫TableAdapter(common_bat.COMMAND_TIME_OUT)
        Dim zaikoAdapter As New N050TableAdapters.T_在庫TableAdapter(common_bat.COMMAND_TIME_OUT)
        Dim shukkoDt As N050.T_浜町出庫DataTable = shukkoAdapter.SelectHamachoShukko(shukkoDay)

        Dim debug As String = String.Empty

        Try

            Dim count As Integer = 1
            For i As Integer = 0 To shukkoDt.Count - 1

                Dim zaiko As New ZaikoValueObject

                debug &= "商品管理番号=[" & shukkoDt(i).商品管理番号 & "], 出庫先在庫区分=[" & shukkoDt(i).出庫先在庫区分 & "], 出庫数量=[" & shukkoDt(i).出庫数量 & "]" & vbNewLine

                '在庫の移動
                hamachoZaikoMove(zaikoAdapter,
                                    shukkoDt(i).商品管理番号,
                                    "2",
                                    shukkoDt(i).出庫先在庫区分,
                                    shukkoDt(i).出庫数量)

            Next

        Catch ex As Exception


        End Try

    End Sub

#End Region

#Region "浜町在庫移動"

    Private Sub hamachoZaikoMove(ByRef zaikoAdapter As N050TableAdapters.T_在庫TableAdapter,
                                 ByVal shohinKanriNo As String,
                                 ByVal fromZaikokubun As String,
                                 ByVal toZaikokubun As String,
                                 ByVal moveSuryo As Integer)

        '移動元T在庫取得
        Dim zaikoDt As N050.T_在庫DataTable = zaikoAdapter.SelectZaikoKeijoByShohinKanriZaikoKubun(shohinKanriNo, fromZaikokubun)

        Dim suryo As Integer = moveSuryo
        If zaikoDt.Rows.Count = 0 Then

            'T在庫に当該在庫区分の在庫が全くない場合
            'ダミー仕入伝票番号で在庫へマイナス計上

            Dim shiireDenpyoNo As String = "0000000000"
            zaikoAdapter.InsertZaikoData(shohinKanriNo,
                                         shiireDenpyoNo,
                                         fromZaikokubun,
                                         suryo * (-1),
                                         "N050")


        Else

            '在庫データから引き落とし（移動分を減算）
            For Each zaikoDtRow As N050.T_在庫Row In zaikoDt.Rows

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
                                                     "N050",
                                                     zaikoDtRow.商品管理番号,
                                                     zaikoDtRow.仕入伝票番号,
                                                     zaikoDtRow.在庫区分)

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
                                                     "N050",
                                                     zaikoDtRow.商品管理番号,
                                                     zaikoDtRow.仕入伝票番号,
                                                     zaikoDtRow.在庫区分)
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
                                                                              "N050",
                                                                              shohinKanriNo,
                                                                              shiireDenpyoNo,
                                                                              fromZaikokubun)
                If updateCount = 0 Then
                    zaikoAdapter.InsertZaikoData(shohinKanriNo,
                                                 shiireDenpyoNo,
                                                 fromZaikokubun,
                                                 dummySuryo,
                                                 "N050")
                End If

            End If

        End If


        '移動先T在庫取得
        suryo = moveSuryo
        zaikoDt = zaikoAdapter.SelectZaikoKeijoByShohinKanriZaikoKubun(shohinKanriNo, toZaikokubun)

        If zaikoDt.Rows.Count = 0 Then

            'T在庫に当該在庫区分の在庫が全くない場合
            'ダミー仕入伝票番号で在庫へ計上

            Dim shiireDenpyoNo As String = "0000000000"
            zaikoAdapter.InsertZaikoData(shohinKanriNo,
                                         shiireDenpyoNo,
                                         toZaikokubun,
                                         suryo,
                                         "N050")


        Else

            '在庫データへ加算
            For Each zaikoDtRow As N050.T_在庫Row In zaikoDt.Rows

                ' 在庫データを計算する
                If suryo + zaikoDtRow.数量 = 0 Then

                    '加算して在庫数が０になる場合はデータを削除する
                    zaikoAdapter.DeleteZeroZaiko(zaikoDtRow.商品管理番号,
                                                 zaikoDtRow.仕入伝票番号,
                                                 zaikoDtRow.在庫区分)

                Else

                    '在庫データ更新
                    zaikoAdapter.UpdateZaikoData((zaikoDtRow.数量 + suryo),
                                                 "N050",
                                                 zaikoDtRow.商品管理番号,
                                                 zaikoDtRow.仕入伝票番号,
                                                 zaikoDtRow.在庫区分)

                End If

                '加算できたらExit
                Exit For

            Next

        End If

    End Sub

#End Region

End Module

Public Class ZaikoValueObject
    Implements System.ICloneable

#Region "【プロパティ】"

#Region "商品管理番号"

    ''' <summary>
    ''' 商品管理番号を設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ShohinKanriNo As String

#End Region

#Region "在庫区分"

    ''' <summary>
    ''' 在庫区分を設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ZaikoKubun As ZaikoKubun

#End Region

#Region "セット商品フラグ"

    ''' <summary>
    ''' セット商品フラグを設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsSetShohin As Boolean

#End Region

#Region "消化納品フラグ"

    ''' <summary>
    ''' 消化納品フラグを設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsShokaNohin As Boolean

#End Region

#Region "在庫対象外フラグ"

    ''' <summary>
    ''' 在庫対象外フラグを設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property isOutOfZaiko As Boolean

#End Region

#Region "商品名"

    ''' <summary>
    ''' 商品名を設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ShohinName As String

#End Region

#Region "商品コード"

    ''' <summary>
    ''' 商品コードを設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ShohinCode As String

#End Region

#Region "バーコード"

    ''' <summary>
    ''' バーコードを設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property BarCode As String

#End Region

#Region "掛率コード"

    ''' <summary>
    ''' 掛率コードを設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property KakerituCode As String

#End Region

#Region "フロアコード"

    ''' <summary>
    ''' フロアコードを設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FloorCode As String

#End Region

#Region "商品グループコード"

    ''' <summary>
    ''' 商品グループコードを設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ShohinGroupCode As String

#End Region

#Region "仕入先コード"

    ''' <summary>
    ''' 仕入先コードを設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ShiireCode As String

#End Region

#Region "仕入先履歴番号"

    ''' <summary>
    ''' 仕入先履歴番号を設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ShiireRirekiNo As Integer

#End Region

#Region "顧客管理番号"

    ''' <summary>
    ''' 顧客管理番号を設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property KokyakuKanriNo As String

#End Region

#Region "顧客名"

    ''' <summary>
    ''' 顧客名を設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property KokyakuName As String

#End Region

#Region "顧客コード"

    ''' <summary>
    ''' 顧客コードを設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property KokyakuCode As String

#End Region

#Region "代表顧客コード"

    ''' <summary>
    ''' 代表顧客コードを設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DaihyoKokyakuCode As String

#End Region

#Region "上代単価"

    ''' <summary>
    ''' 上代単価を設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property JodaiTanka As Decimal

#End Region

#Region "下代単価"

    ''' <summary>
    ''' 下代単価を設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GedaiTanka As Decimal

#End Region

#Region "数量"

    ''' <summary>
    ''' 数量を設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Suryo As Int32

#End Region

#Region "販売金額"

    ''' <summary>
    ''' 販売金額を設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property HanbaiKingaku As Decimal

#End Region

#Region "売上担当者番号"

    ''' <summary>
    ''' 売上担当者番号を設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TantoShainCode As String

#End Region

#Region "売上担当者名"

    ''' <summary>
    ''' 売上担当者名を設定、取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TantoShainName As String

#End Region

#End Region

#Region "【プロパティ】"

#Region "インスタンスコピーして返す"

    ' System.ICloneable.Clone メソッド (非公開メンバとする)
    Private Function Clone() As Object Implements System.ICloneable.Clone
        Return Me.MemberwiseClone()
    End Function

    ' 同じクラスのインスタンスを返すクローン コピーメソッド (上のメソッドを型変換して返す)
    Public Function CloneCopy() As ZaikoValueObject
        Return DirectCast(Me.Clone(), ZaikoValueObject)
    End Function

#End Region

#End Region

End Class

Partial Public Class ZaikoHosei

#Region "【メソッド】"

#Region "売上の在庫引当を実行する"

    ''' <summary>
    ''' 売上と廃棄時の在庫引当を実行する
    ''' 
    ''' ・T在庫への更新
    ''' ・T売上への登録
    ''' 
    ''' ※在庫対象外の場合は、T売上のみ登録を行う
    ''' ※T在庫に当該商品レコードがなかった場合の処理
    ''' 　売上の場合：ダミー仕入在庫で計上
    ''' 　返品の場合：最新の仕入伝票番号で計上
    ''' </summary>
    ''' <param name="denpyoDate">レジ伝票日付</param>
    ''' <param name="registerDenpyoNo">レジ伝票番号（出力レジ番号(3桁)＋レジ伝票連番(4桁)）</param>
    ''' <param name="regMeisaiList">在庫ValueObject（レジ伝票明細ベース）</param>
    ''' <param name="nebikiRate">値引率</param>
    ''' <param name="haikiDenpyoNo">廃棄伝票番号（廃棄処理の場合にのみ指定）</param>
    ''' <remarks></remarks>
    Public Sub ExecuteZaikoHikiateUriage(ByVal denpyoDate As String,
                                         ByVal registerDenpyoNo As String,
                                         ByVal regMeisaiList As List(Of ZaikoValueObject),
                                         Optional ByVal haikiDenpyoNo As String = "",
                                         Optional ByVal nebikiRate As Decimal = -1)
        '2012/01/17 #1 UPD 引数にnebikiRate（値引率）を追加

        Try

            'レジ伝票番号（伝票年月日(8桁)＋出力レジ番号(3桁)＋レジ伝票連番(4桁)）
            Dim denpyoNo As String = denpyoDate & registerDenpyoNo

            Dim zaikoAdapter As New N050TableAdapters.T_在庫TableAdapter(common_bat.COMMAND_TIME_OUT)
            Dim uriageAdapter As New N050TableAdapters.T_売上TableAdapter(common_bat.COMMAND_TIME_OUT)
            Dim accessAdapter As New N050TableAdapters.T_商品アクセスTableAdapter(common_bat.COMMAND_TIME_OUT)
            Dim shiireAdapter As New N050TableAdapters.T_仕入TableAdapter(common_bat.COMMAND_TIME_OUT)
            Dim shiireMeisaiAdapter As New N050TableAdapters.T_仕入明細TableAdapter(common_bat.COMMAND_TIME_OUT)
            '2012/01/17 #1 ADD START
            Dim setUriageAdapter As New N050TableAdapters.T_セット売上TableAdapter(common_bat.COMMAND_TIME_OUT)
            '2012/01/17 #1 ADD END

            '2012/01/25 #2 ADD STAR
            '2012/01/27 #3 DEL START
            'Dim setUriageMeisaiAdapter As New N050TableAdapters.T_セット売上明細TableAdapter
            '2012/01/27 #3 DEL END
            Dim setShohinKanriNo As String = String.Empty
            'Dim renban As Integer = 1 '2012/01/30 ADD DEL
            '2012/01/25 #2 ADD END

            For Each meisai As ZaikoValueObject In regMeisaiList

                '更新者はレジ担当者
                'Dim updateUser As String = meisai.TantoShainCode
                Dim updateUser As String = "BAT"

                '数量が0の場合はスキップ
                If meisai.Suryo = 0 Then
                    Continue For
                End If

                'セット商品の場合は内訳商品の在庫引当を行う
                Dim hikiateZaikos() As ZaikoValueObject
                If meisai.IsSetShohin Then

                    'セット商品の場合
                    '2012/01/25 #2 ADD START
                    setShohinKanriNo = meisai.ShohinKanriNo
                    '2012/01/25 #2 ADD END

                    '2012/01/17 #1 UPD START
                    'hikiateZaikos = Me.getSetShohinList(meisai)
                    hikiateZaikos = Me.getSetShohinList(meisai, nebikiRate)
                    '2012/01/17 #1 UPD END

                Else

                    'セット商品ではない場合は在庫引当処理ループを１回行うよう設定
                    ReDim hikiateZaikos(0)
                    hikiateZaikos(0) = meisai.CloneCopy()

                End If

                '2012/01/17 #1 ADD START
                'セット商品の場合は、T_セット商品.原価の登録に、子商品の原価単価合計を使用する
                Dim totalGenka As Decimal = 0
                '2012/01/17 #1 ADD END

                '2012/01/27 #3 ADD START
                'セット商品の場合は、T_セット商品.上代と下代の登録に、子商品の上代単価合計と下代単価合計を使用する
                Dim totalJodai As Decimal = 0
                Dim totalGedai As Decimal = 0
                Dim renban As Integer = 1 '2012/01/30 ADD
                '2012/01/27 #3 ADD END

                '在庫引当処理
                For i As Integer = 0 To hikiateZaikos.Length - 1
                    '2012/01/25 #2 ADD START
                    renban = i + 1
                    '2012/01/25 #2 ADD END

                    Dim hikiateZaiko As ZaikoValueObject = hikiateZaikos(i)
                    Dim akakuroEdaban As Integer = 0

                    '在庫処理対象外の場合はT売上にのみ登録
                    If hikiateZaiko.isOutOfZaiko Then

                        'T売上へ登録
                        Me.insertUriage(uriageAdapter,
                                        accessAdapter,
                                        denpyoDate,
                                        denpyoNo,
                                        "0000000000",
                                        haikiDenpyoNo,
                                        hikiateZaiko.Suryo,
                                        hikiateZaiko)

                        Continue For

                    End If


                    If hikiateZaiko.IsShokaNohin Then

                        '消化納品の場合

                        '消化納品の在庫引き当て処理
                        '消化納品の場合は (T仕入 or T_仕入先返品) とT売上に登録してスキップ
                        Me.hikiateShokanohin(uriageAdapter,
                                             accessAdapter,
                                             shiireAdapter,
                                             shiireMeisaiAdapter,
                                             denpyoDate,
                                             denpyoNo,
                                             updateUser,
                                             hikiateZaiko,
                                             haikiDenpyoNo)


                    Else

                        '通常商品の場合

                        '通常商品の在庫引き当て処理
                        '2012/01/17 #1 UPD START
                        'Me.hikiateShohin(uriageAdapter,
                        '                 accessAdapter,
                        '                 zaikoAdapter,
                        '                 shiireMeisaiAdapter,
                        '                 denpyoDate,
                        '                 denpyoNo,
                        '                 updateUser,
                        '                 hikiateZaiko,
                        '                 haikiDenpyoNo)
                        Me.hikiateShohin(uriageAdapter,
                                         accessAdapter,
                                         zaikoAdapter,
                                         shiireMeisaiAdapter,
                                         denpyoDate,
                                         denpyoNo,
                                         updateUser,
                                         hikiateZaiko,
                                         haikiDenpyoNo,
                                         totalGenka,
                                         totalJodai,
                                         totalGedai,
                                         meisai.IsSetShohin,
                                         setShohinKanriNo,
                                         renban)
                        '2012/01/17 #1 UPD END


                    End If

                Next i

                '2012/01/17 #1 ADD START
                'セット商品の場合は、親商品でT_セット売上にINSERT
                If meisai.IsSetShohin Then

                    'Tセット売上へ登録
                    Me.insertSetUriage(setUriageAdapter,
                                       accessAdapter,
                                       denpyoDate,
                                       denpyoNo,
                                       "0000000000",
                                       haikiDenpyoNo,
                                       meisai,
                                       renban,
                                       totalGenka,
                                       totalJodai,
                                       totalGedai)

                End If
                '2012/01/17 #1 ADD END

            Next


        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Sub

#End Region

#Region "通常の商品の在庫引き当て処理を行う"

    ''' <summary>
    ''' 通常の商品の在庫引き当て処理を行う
    ''' </summary>
    ''' <param name="uriageAdapter">T_売上TableAdapter</param>
    ''' <param name="accessAdapter">T_商品アクセスTableAdapter</param>
    ''' <param name="zaikoAdapter">T_在庫TableAdapter</param>
    ''' <param name="shiireMeisaiAdapter">T_仕入明細TableAdapter</param>
    ''' <param name="denpyoDate"></param>
    ''' <param name="denpyoNo">レジ伝票番号</param>
    ''' <param name="updateUser">更新者</param>
    ''' <param name="zaiko">在庫ValueObject</param>
    ''' <param name="haikiDenpyo">廃棄伝票番号</param>
    ''' <param name="totalGenka">原価金額合計</param>
    ''' <remarks></remarks>
    Private Sub hikiateShohin(ByRef uriageAdapter As N050TableAdapters.T_売上TableAdapter,
                                  ByRef accessAdapter As N050TableAdapters.T_商品アクセスTableAdapter,
                                  ByRef zaikoAdapter As N050TableAdapters.T_在庫TableAdapter,
                                  ByRef shiireMeisaiAdapter As N050TableAdapters.T_仕入明細TableAdapter,
                                  ByVal denpyoDate As String,
                                  ByVal denpyoNo As String,
                                  ByVal updateUser As String,
                                  ByVal zaiko As ZaikoValueObject,
                                  ByVal haikiDenpyo As String,
                                  ByRef totalGenka As Decimal,
                                  ByRef totalJodai As Decimal,
                                  ByRef totalGedai As Decimal,
                                  ByVal isSetShohin As Boolean,
                                  ByVal setShohinKanriNo As String,
                                  ByVal renban As Integer)
        '2012/01/17 #1 UPD 引数にtotalGenka（原価金額合計）を追加

        '2012/01/25 #2 UPD 引数にsetUriageMeisaiを追加
        '2012/01/25 #2 UPD 引数にisSetShohinを追加
        '2012/01/25 #2 UPD 引数にsetShohinKanriNoを追加
        '2012/01/25 #2 UPD 引数にrenbanを追加

        '2012/01/27 #3 UPD 引数のsetUriageMeisaiを削除
        '2012/01/27 #3 UPD 引数にtotalJodaiを追加
        '2012/01/27 #3 UPD 引数にtotalGedaiを追加

        Try

            '2012/01/17 #1 ADD START
            '原価金額
            Dim genka As Decimal = 0
            '2012/01/17 #1 ADD END

            Dim suryo As Integer = zaiko.Suryo

            'T在庫取得
            Dim zaikoDt As N050.T_在庫DataTable = zaikoAdapter.SelectZaikoKeijoByShohinKanriZaikoKubun( _
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

                'T売上へ登録
                '2012/01/17 #1 UPD START
                Me.insertUriage(uriageAdapter,
                                accessAdapter,
                                denpyoDate,
                                denpyoNo,
                                shiireDenpyoNo,
                                haikiDenpyo,
                                suryo,
                                zaiko,
                                isSetShohin,
                                setShohinKanriNo,
                                renban,
                                genka,
                                totalJodai,
                                totalGedai)
                totalGenka += genka
                '2012/01/17 #1 UPD END

                Exit Sub

            End If


            '在庫データから引き落とし（売上・廃棄分を減算）
            For Each zaikoDtRow As N050.T_在庫Row In zaikoDt.Rows

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

                'T売上へ登録
                Dim isLast As Boolean = If(suryo = 0, True, False)
                '2012/01/17 #1 UPD START
                Me.insertUriage(uriageAdapter,
                                accessAdapter,
                                denpyoDate,
                                denpyoNo,
                                zaikoDtRow.仕入伝票番号,
                                haikiDenpyo,
                                hikiatoshiSuryo,
                                zaiko,
                                isSetShohin,
                                setShohinKanriNo,
                                renban,
                                genka,
                                totalJodai,
                                totalGedai,
                                isLast)
                totalGenka += genka
                '2012/01/17 #1 UPD END

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

                'T売上へ登録
                '2012/01/17 #1 UPD START
                Me.insertUriage(uriageAdapter,
                                accessAdapter,
                                denpyoDate,
                                denpyoNo,
                                shiireDenpyoNo,
                                haikiDenpyo,
                                suryo,
                                zaiko,
                                False,
                                setShohinKanriNo,
                                renban,
                                genka,
                                totalJodai,
                                totalGedai)
                totalGenka += genka
                '2012/01/17 #1 UPD END

            End If

        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Sub

#End Region

#Region "消化納品の在庫引き当て処理を行う"

    ''' <summary>
    ''' 消化納品の在庫引き当て処理を行う
    ''' </summary>
    ''' <param name="uriageAdapter">T_売上TableAdapter</param>
    ''' <param name="accessAdapter">T_商品アクセスTableAdapter</param>
    ''' <param name="shiireAdapter">T_仕入TableAdapter</param>
    ''' <param name="shiireMeisaiAdapter">T_仕入明細TableAdapter</param>
    ''' <param name="denpyoDate"></param>
    ''' <param name="denpyoNo">レジ伝票番号</param>
    ''' <param name="updateUser">更新者</param>
    ''' <param name="zaiko">在庫ValueObject</param>
    ''' <param name="haikiDenpyo">廃棄伝票番号</param>
    ''' <remarks></remarks>
    Private Sub hikiateShokanohin(ByRef uriageAdapter As N050TableAdapters.T_売上TableAdapter,
                                  ByRef accessAdapter As N050TableAdapters.T_商品アクセスTableAdapter,
                                  ByRef shiireAdapter As N050TableAdapters.T_仕入TableAdapter,
                                  ByRef shiireMeisaiAdapter As N050TableAdapters.T_仕入明細TableAdapter,
                                  ByVal denpyoDate As String,
                                  ByVal denpyoNo As String,
                                  ByVal updateUser As String,
                                  ByVal zaiko As ZaikoValueObject,
                                  ByVal haikiDenpyo As String)


        Try

            ' 仕入伝票番号採番
            Dim shiireDenpyoNo As String = (New SaibanTran).GetHacchuDenpyoNo()

            '仕入区分確定
            Dim shiireKubun As String
            If zaiko.Suryo > 0 Then

                shiireKubun = "1"

            ElseIf zaiko.Suryo < 0 Then

                shiireKubun = "8"

            Else

                Throw New Exception("数量０で仕入計上しようとしました。")

            End If

            '原価単価を取得
            Dim genkaTanka As Decimal = getGenkaTanka("0000000000", zaiko.ShohinKanriNo)

            '集計年月を取得
            Dim shukeiYM As String = Converter.GetShiireInputDateToShukeiYM(denpyoDate)

            'T_仕入へ登録
            shiireAdapter.InsertShiire(shiireDenpyoNo,
                                       0,
                                       CStr(ZaikoKubun.Honsha),
                                       shiireKubun, _
                                       zaiko.ShiireCode,
                                       zaiko.ShiireRirekiNo,
                                       denpyoDate,
                                       denpyoDate,
                                       zaiko.TantoShainCode,
                                       denpyoDate,
                                       updateUser,
                                       shukeiYM)

            'T_仕入明細へ登録
            Dim meisaiNo As Integer = 1 '明細１行のみの仕入を作成する
            shiireMeisaiAdapter.InsertShiireMeisai(shiireDenpyoNo,
                                       0,
                                       meisaiNo,
                                       zaiko.ShohinKanriNo,
                                       zaiko.ShohinCode,
                                       genkaTanka,
                                       zaiko.Suryo,
                                       zaiko.JodaiTanka,
                                       zaiko.ShohinName,
                                       updateUser)

            'T売上へ登録
            Me.insertUriage(uriageAdapter,
                            accessAdapter,
                            denpyoDate,
                            denpyoNo,
                            shiireDenpyoNo,
                            haikiDenpyo,
                            zaiko.Suryo,
                            zaiko)

        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Sub

#End Region

#Region "T売上を登録する"

    ''' <summary>
    ''' T売上登録
    ''' </summary>
    ''' <param name="uriageAdapter">T_売上TableAdapter</param>
    ''' <param name="accessAdapter">T_商品アクセスTableAdapter</param>
    ''' <param name="denpyoDate"></param>
    ''' <param name="denpyoNo">レジ伝票番号</param>
    ''' <param name="shiireDenpyoNo">仕入伝票番号</param>
    ''' <param name="haikiDenpyoNo">廃棄伝票番号</param>
    ''' <param name="uriageSuryo">売上数量</param>
    ''' <param name="zaiko">在庫ValueObject</param>
    ''' <param name="genka">原価金額（単価ではない）</param>
    ''' <remarks></remarks>
    Private Sub insertUriage(ByRef uriageAdapter As N050TableAdapters.T_売上TableAdapter,
                             ByRef accessAdapter As N050TableAdapters.T_商品アクセスTableAdapter,
                             ByVal denpyoDate As String,
                             ByVal denpyoNo As String,
                             ByVal shiireDenpyoNo As String,
                             ByVal haikiDenpyoNo As String,
                             ByVal uriageSuryo As Integer,
                             ByVal zaiko As ZaikoValueObject,
                             Optional ByVal isSetShohin As Boolean = False,
                             Optional ByVal setShohinKanriNo As String = "",
                             Optional ByVal renban As Integer = 0,
                             Optional ByRef genka As Decimal = 0,
                             Optional ByRef jodai As Decimal = 0,
                             Optional ByRef gedai As Decimal = 0,
                             Optional ByVal isLast As Boolean = True)
        '2012/01/17 #1 UPD 引数にgenka（原価金額）を追加

        '2012/01/25 #2 UPD 引数にsetUriageMeisaiを追加
        '2012/01/25 #2 UPD 引数にsetShohinKanriNoを追加
        '2012/01/25 #2 UPD 引数にrenbanを追加
        '2012/01/25 #2 UPD 引数にisSetShohinを追加

        '2012/01/27 #3 UPD 引数のsetShohinKanriNoを削除
        '2012/01/27 #3 UPD 引数にjodaiを追加
        '2012/01/27 #3 UPD 引数にgedaiを追加

        Try

            Dim updateUser As String = zaiko.TantoShainCode

            '商品アクセスへ登録
            Dim accessKubun As String = "3"
            If haikiDenpyoNo <> String.Empty Then
                accessKubun = "5"
            End If
            Dim updateCount As Integer = accessAdapter.UpdateShohinAccess(
                                            accessKubun,
                                            updateUser,
                                            zaiko.ShohinKanriNo)
            If updateCount = 0 Then
                accessAdapter.InsertShohinAccess(
                                            zaiko.ShohinKanriNo,
                                            accessKubun,
                                            updateUser)
            End If


            '廃棄の場合は、T売上に登録しない
            If haikiDenpyoNo <> String.Empty Then
                Exit Sub
            End If

            ''原価単価を取得
            'Dim genkaTanka As Decimal = 0
            'If zaiko.IsShokaNohin Then
            '    '消化納品の場合は、原価単価をM商品から取得する
            '    genkaTanka = Me.getGenkaTanka("0000000000", zaiko.ShohinKanriNo)
            'Else
            '    genkaTanka = Me.getGenkaTanka(shiireDenpyoNo, zaiko.ShohinKanriNo)
            'End If

            ''2012/01/17 #1 DEL START
            ''原価金額を引数OUT
            'genka = genkaTanka * zaiko.Suryo
            ''2012/01/17 #1 DEL END

            ''販売金額を算出
            'Dim hanbaiKingaku As Decimal = Fix(zaiko.HanbaiKingaku / zaiko.Suryo * uriageSuryo)
            'If isLast Then
            '    hanbaiKingaku = zaiko.HanbaiKingaku - Fix((zaiko.Suryo - uriageSuryo) * zaiko.HanbaiKingaku / zaiko.Suryo)
            'End If

            ''T_売上登録
            'Dim uriageCount As Integer = uriageAdapter.UpdateUriageSuryo(uriageSuryo,
            '                                                             hanbaiKingaku,
            '                                                             updateUser,
            '                                                             denpyoNo,
            '                                                             shiireDenpyoNo,
            '                                                             zaiko.ShohinKanriNo)
            'If uriageCount = 0 Then
            '    Dim hinban3 As String = zaiko.FloorCode & zaiko.ShohinGroupCode
            '    Dim hinban5 As String = hinban3 & zaiko.KakerituCode
            '    Dim hinban8 As String = zaiko.ShiireCode & hinban5
            '    uriageCount = uriageAdapter.InsertUriageByShohin(denpyoNo,
            '                                                     shiireDenpyoNo,
            '                                                     zaiko.ShohinKanriNo,
            '                                                     denpyoDate,
            '                                                     zaiko.KokyakuKanriNo,
            '                                                     zaiko.KokyakuName,
            '                                                     zaiko.KokyakuCode,
            '                                                     zaiko.DaihyoKokyakuCode,
            '                                                     zaiko.ShohinName,
            '                                                     zaiko.ShohinCode,
            '                                                     zaiko.BarCode,
            '                                                     zaiko.FloorCode,
            '                                                     zaiko.ShohinGroupCode,
            '                                                     zaiko.ShiireCode,
            '                                                     zaiko.ShiireRirekiNo,
            '                                                     genkaTanka,
            '                                                     zaiko.JodaiTanka,
            '                                                     zaiko.GedaiTanka,
            '                                                     uriageSuryo,
            '                                                     hanbaiKingaku,
            '                                                     zaiko.TantoShainCode,
            '                                                     zaiko.TantoShainName,
            '                                                     hinban3,
            '                                                     hinban5,
            '                                                     hinban8,
            '                                                     updateUser)
            'End If

            'If uriageCount = 0 Then

            '    Dim msg As String = "T_売上の登録に失敗しました。"
            '    Try
            '        msg &= "  伝票番号=" & denpyoNo
            '        msg &= ", 仕入伝票番号=" & shiireDenpyoNo
            '        msg &= ", 廃棄伝票番号=" & haikiDenpyoNo
            '        msg &= ", 商品管理番号=" & zaiko.ShohinKanriNo
            '        msg &= ", 数量=" & zaiko.Suryo
            '        msg &= ", 顧客管理番号=" & zaiko.KokyakuKanriNo
            '        msg &= ", 顧客コード=" & zaiko.KokyakuCode
            '    Catch ex As Exception
            '        '処理を継続
            '    End Try

            '    Throw New Exception(msg)

            'End If

            ''2012/01/25 #2 UPD START
            'If isSetShohin = True Then

            '    'セット商品の場合

            '    jodai += zaiko.JodaiTanka * zaiko.Suryo
            '    gedai += zaiko.GedaiTanka * zaiko.Suryo

            '    Me.insertSetUriageMeisai(accessAdapter,
            '                                 denpyoDate,
            '                                 denpyoNo,
            '                                 "0000000000",
            '                                 haikiDenpyoNo,
            '                                 zaiko,
            '                                 setShohinKanriNo,
            '                                 renban,
            '                                 genkaTanka,
            '                                 hanbaiKingaku)


            'End If
            ''2012/01/25 #2 UPD END

        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Sub
#End Region

    '2012/01/17 #1 ADD START
#Region "Tセット売上を登録する"

    ''' <summary>
    ''' Tセット売上登録
    ''' </summary>
    ''' <param name="uriageAdapter">T_セット売上TableAdapter</param>
    ''' <param name="accessAdapter">T_商品アクセスTableAdapter</param>
    ''' <param name="denpyoDate"></param>
    ''' <param name="denpyoNo">レジ伝票番号</param>
    ''' <param name="shiireDenpyoNo">仕入伝票番号</param>
    ''' <param name="haikiDenpyoNo">廃棄伝票番号</param>
    ''' <param name="zaiko">在庫ValueObject</param>
    ''' <param name="totalGenka">子商品の原価合計合計</param>
    ''' <remarks></remarks>
    Private Sub insertSetUriage(ByRef uriageAdapter As N050TableAdapters.T_セット売上TableAdapter,
                             ByRef accessAdapter As N050TableAdapters.T_商品アクセスTableAdapter,
                             ByVal denpyoDate As String,
                             ByVal denpyoNo As String,
                             ByVal shiireDenpyoNo As String,
                             ByVal haikiDenpyoNo As String,
                             ByVal zaiko As ZaikoValueObject,
                             ByVal renban As Integer,
                             ByVal totalGenka As Decimal,
                             ByVal totalJodai As Decimal,
                             ByVal totalGedai As Decimal)
        '2012/01/27 #3 UPD 引数にtotalJodaiを追加
        '2012/01/27 #3 UPD 引数にtotalGedaiを追加

        Try

            Dim updateUser As String = zaiko.TantoShainCode

            ''商品アクセスへ登録
            'Dim accessKubun As String = "3"
            'If haikiDenpyoNo <> String.Empty Then
            '    accessKubun = "5"
            'End If
            'Dim updateCount As Integer = accessAdapter.UpdateShohinAccess(
            '                                accessKubun,
            '                                updateUser,
            '                                zaiko.ShohinKanriNo)
            'If updateCount = 0 Then
            '    accessAdapter.InsertShohinAccess(
            '                                zaiko.ShohinKanriNo,
            '                                accessKubun,
            '                                updateUser)
            'End If


            ''廃棄の場合は、Tセット売上に登録しない
            'If haikiDenpyoNo <> String.Empty Then
            '    Exit Sub
            'End If

            ''上代と下代は「０」固定
            'Dim jodaiTanka As Decimal = 0
            'Dim gedaiTanka As Decimal = 0

            '2012/01/27 #3 ADD START
            Dim genkaTanka As Decimal = 0
            Dim jodaiTanka As Decimal = 0
            Dim gedaiTanka As Decimal = 0

            '親商品の販売金額 - 子明細の単価合計
            '2012/01/30 #4 UPD START 親の上代単価に 親の売上数量を掛ける
            jodaiTanka = (zaiko.JodaiTanka * zaiko.Suryo) - totalJodai
            gedaiTanka = zaiko.HanbaiKingaku - totalGedai
            '2012/01/30 #4 UPD END
            '2012/01/27 #3 UPD START
            'jodaiTanka = zaiko.JodaiTanka - totalJodai
            'gedaiTanka = zaiko.HanbaiKingaku - totalGedai
            '2012/01/27 #3 UPD END
            '2012/01/27 #3 DEL START
            'jodaiTanka = zaiko.HanbaiKingaku - totalJodai
            'gedaiTanka = zaiko.HanbaiKingaku - totalGedai
            '2012/01/27 #3 DEL END

            Dim hanbaiKingaku As Decimal = gedaiTanka
            '2012/01/27 #3 ADD END



            '2011/1/17要チェック
            '原価は｛親商品の販売金額 - 子明細の原価合計｝
            'Dim gedaiTanka As Decimal = zaiko.HanbaiKingaku - totalGenka
            'Dim hanbaiKingaku As Decimal = gedaiTanka

            ''上代と下代は「０」固定
            'Dim jodaiTanka As Decimal = 0
            'Dim gedaiTanka As Decimal = 0
            'Dim hanbaiKingaku As Decimal = 0

            ''2011/1/17要チェック
            ''原価は｛子明細の原価合計 - 親商品の販売金額｝
            'Dim genkaTanka As Decimal = totalGenka - zaiko.HanbaiKingaku

            ''T_セット売上登録
            'Dim uriageCount As Integer = uriageAdapter.UpdateUriageSuryo(zaiko.Suryo,
            '                                                             hanbaiKingaku,
            '                                                             updateUser,
            '                                                             denpyoNo,
            '                                                             shiireDenpyoNo,
            '                                                             zaiko.ShohinKanriNo)
            'If uriageCount = 0 Then
            '    Dim hinban3 As String = zaiko.FloorCode & zaiko.ShohinGroupCode
            '    Dim hinban5 As String = hinban3 & zaiko.KakerituCode
            '    Dim hinban8 As String = zaiko.ShiireCode & hinban5
            '    uriageCount = uriageAdapter.InsertSetUriage(denpyoNo,
            '                                                shiireDenpyoNo,
            '                                                zaiko.ShohinKanriNo,
            '                                                denpyoDate,
            '                                                zaiko.KokyakuKanriNo,
            '                                                zaiko.KokyakuName,
            '                                                zaiko.KokyakuCode,
            '                                                zaiko.DaihyoKokyakuCode,
            '                                                zaiko.ShohinName,
            '                                                zaiko.ShohinCode,
            '                                                zaiko.BarCode,
            '                                                zaiko.FloorCode,
            '                                                zaiko.ShohinGroupCode,
            '                                                zaiko.ShiireCode,
            '                                                zaiko.ShiireRirekiNo,
            '                                                genkaTanka,
            '                                                jodaiTanka,
            '                                                gedaiTanka,
            '                                                zaiko.Suryo,
            '                                                hanbaiKingaku,
            '                                                zaiko.TantoShainCode,
            '                                                zaiko.TantoShainName,
            '                                                hinban3,
            '                                                hinban5,
            '                                                hinban8,
            '                                                updateUser)
            'End If
            'If uriageCount = 0 Then

            '    Dim msg As String = "T_セット売上の登録に失敗しました。"
            '    Try
            '        msg &= "  伝票番号=" & denpyoNo
            '        msg &= ", 仕入伝票番号=" & shiireDenpyoNo
            '        msg &= ", 廃棄伝票番号=" & haikiDenpyoNo
            '        msg &= ", 商品管理番号=" & zaiko.ShohinKanriNo
            '        msg &= ", 数量=" & zaiko.Suryo
            '        msg &= ", 顧客管理番号=" & zaiko.KokyakuKanriNo
            '        msg &= ", 顧客コード=" & zaiko.KokyakuCode
            '    Catch ex As Exception
            '        '処理を継続
            '    End Try

            '    Throw New Exception(msg)

            'End If

        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Sub
#End Region
    '2012/01/17 #1 ADD END

    '2012/01/25 #2 ADD START
#Region "Tセット売上明細を登録する"

    ''' <summary>
    ''' Tセット売上明細登録
    ''' </summary>
    ''' <param name="accessAdapter">T_商品アクセスTableAdapter</param>
    ''' <param name="denpyoDate"></param>
    ''' <param name="denpyoNo">レジ伝票番号</param>
    ''' <param name="shiireDenpyoNo">仕入伝票番号</param>
    ''' <param name="haikiDenpyoNo">廃棄伝票番号</param>
    ''' <param name="zaiko">在庫ValueObject</param>
    ''' <param name="genkaTanka">子商品の原価単価</param>
    ''' <remarks></remarks>
    Private Sub insertSetUriageMeisai(ByRef accessAdapter As N050TableAdapters.T_商品アクセスTableAdapter,
                             ByVal denpyoDate As String,
                             ByVal denpyoNo As String,
                             ByVal shiireDenpyoNo As String,
                             ByVal haikiDenpyoNo As String,
                             ByVal zaiko As ZaikoValueObject,
                             ByVal setShohinKanriNo As String,
                             ByVal renban As Integer,
                             ByVal genkaTanka As Decimal,
                             ByVal hanbaiKingaku As Decimal)

        '2012/01/27 #3 UPD 引数のsetShohinKanriNoを削除

        Try

            Dim updateUser As String = zaiko.TantoShainCode

            '廃棄の場合は、Tセット売上明細に登録しない
            If haikiDenpyoNo <> String.Empty Then
                Exit Sub
            End If

            ''T_セット売上明細登録
            ''2012/01/27 #3 UPD START
            'Dim uriageAdapter As New N050TableAdapters.T_セット売上明細TableAdapter
            ''2012/01/27 #3 UPD START
            'Dim uriageCount As Integer = 0

            'If uriageCount = 0 Then
            '    Dim hinban3 As String = zaiko.FloorCode & zaiko.ShohinGroupCode
            '    Dim hinban5 As String = hinban3 & zaiko.KakerituCode
            '    Dim hinban8 As String = zaiko.ShiireCode & hinban5

            '    '2012/02/01 #5 ADD START
            '    Try
            '        'INSERTでエラー（KEY重複違反）時は販売金額をUPDATEする
            '        uriageCount = uriageAdapter.InsertSetUriageMeisai(denpyoNo,
            '                                    shiireDenpyoNo,
            '                                    setShohinKanriNo,
            '                                    renban,
            '                                    zaiko.ShohinKanriNo,
            '                                    denpyoDate,
            '                                    zaiko.KokyakuKanriNo,
            '                                    zaiko.KokyakuName,
            '                                    zaiko.KokyakuCode,
            '                                    zaiko.DaihyoKokyakuCode,
            '                                    zaiko.ShohinName,
            '                                    zaiko.ShohinCode,
            '                                    zaiko.BarCode,
            '                                    zaiko.FloorCode,
            '                                    zaiko.ShohinGroupCode,
            '                                    zaiko.ShiireCode,
            '                                    zaiko.ShiireRirekiNo,
            '                                    genkaTanka,
            '                                    zaiko.JodaiTanka,
            '                                    zaiko.GedaiTanka,
            '                                    zaiko.Suryo,
            '                                    hanbaiKingaku,
            '                                    zaiko.TantoShainCode,
            '                                    zaiko.TantoShainName,
            '                                    hinban3,
            '                                    hinban5,
            '                                    hinban8,
            '                                    updateUser)

            '    Catch ex As Exception

            '        If DirectCast(ex.GetBaseException(), SqlClient.SqlException).Number = 2627 Then

            '            uriageCount = uriageAdapter.UpdateUriageSuryo(hanbaiKingaku, denpyoNo, shiireDenpyoNo, setShohinKanriNo, renban)

            '        End If

            '    End Try
            '    '2012/02/01 #5 ADD END

            'End If
            'If uriageCount = 0 Then

            '    Dim msg As String = "T_セット売上明細の登録に失敗しました。"
            '    Try
            '        msg &= "  伝票番号=" & denpyoNo
            '        msg &= ", 仕入伝票番号=" & shiireDenpyoNo
            '        msg &= ", 廃棄伝票番号=" & haikiDenpyoNo
            '        msg &= ", セット商品管理番号=" & setShohinKanriNo
            '        msg &= ", 商品管理番号=" & zaiko.ShohinKanriNo
            '        msg &= ", 数量=" & zaiko.Suryo
            '        msg &= ", 顧客管理番号=" & zaiko.KokyakuKanriNo
            '        msg &= ", 顧客コード=" & zaiko.KokyakuCode
            '    Catch ex As Exception
            '        '処理を継続
            '    End Try

            '    Throw New Exception(msg)

            'End If

        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Sub
#End Region
    '2012/01/25 #2 ADD END

#Region "原価単価を取得する"

    ''' <summary>
    ''' 原価単価を取得する
    ''' </summary>
    ''' <param name="shiireDenpyoNo">仕入伝票番号</param>
    ''' <param name="shohinKanriNo">商品管理番号</param>
    ''' <remarks></remarks>
    Private Function getGenkaTanka(ByVal shiireDenpyoNo As String, ByVal shohinKanriNo As String) As Decimal

        Try

            Static shiireMeisaiAdapter As New N050TableAdapters.T_仕入明細TableAdapter(common_bat.COMMAND_TIME_OUT)
            Static shohinAdapter As New N050TableAdapters.M_商品TableAdapter

            'T仕入明細またはM商品から原価単価を取得
            Dim genkaTanka As Decimal = 0
            If shiireDenpyoNo <> "0000000000" Then
                Dim shiireDt As N050.T_仕入明細DataTable = shiireMeisaiAdapter.SelectByShiireNoAndShohinKanriNo(shiireDenpyoNo, shohinKanriNo)
                If shiireDt.Rows.Count = 0 Then

                    Throw New Exception("T_仕入明細の取得に失敗しました。原価単価が取得できません。仕入伝票番号=" & shiireDenpyoNo)

                End If
                genkaTanka = shiireDt(0).原価単価
            Else
                'ダミー仕入伝票番号の場合はM商品から単価を取得する
                Dim shohinDt As N050.M_商品DataTable = shohinAdapter.SelectByShohinKanriNo(shohinKanriNo)
                If shohinDt.Rows.Count = 0 Then

                    Throw New Exception("M_商品の取得に失敗しました。原価単価が取得できません。仕入伝票番号=" & shiireDenpyoNo)

                End If
                genkaTanka = shohinDt(0).原価単価

            End If

            Return genkaTanka

        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Function

#End Region

#Region "セット商品を一元化する"

    ''' <summary>
    ''' セット商品を一元化する
    ''' ※上代・下代・販売金額は子商品の比率で算出する
    ''' </summary>
    ''' <param name="parent">セット商品のValueObject</param>
    ''' <param name="nebikiRate">値引率</param>
    ''' <remarks></remarks>
    ''' <returns>セット商品（子商品）</returns>
    Private Function getSetShohinList(ByVal parent As ZaikoValueObject,
                                      Optional ByVal nebikiRate As Decimal = -1) As ZaikoValueObject()
        '2012/01/17 #1 UPD 引数にnebikiRate（値引率）を追加

        Try

            '2012/01/17 #1 ADD START
            '親商品の明細からランクを元に値引き率を取得する
            If nebikiRate = -1 Then
                Throw New Exception("値引率が指定されていません。")
            End If
            '2012/01/17 #1 ADD END

            'セット商品の場合
            '下代、上代、販売金額を比率で算出する
            Dim shohinAdapter As New N050TableAdapters.M_商品TableAdapter
            Dim setShohinAdapter As New N050TableAdapters.M_セット商品TableAdapter
            Dim setShohinDt As N050.M_セット商品DataTable = setShohinAdapter.SelectBySetShohinKanriNo(parent.ShohinKanriNo)
            If setShohinDt.Rows.Count = 0 Then

                Throw New Exception("M_セット商品の情報取得に失敗しました。 " & _
                                    "商品管理番号=" & parent.ShohinKanriNo & " はセット商品として登録されていません。")

            End If

            '2012/01/17 #1 ADD START
            Dim kakerituAdapter As New N050TableAdapters.M_掛率TableAdapter
            '2012/01/17 #1 ADD END

            '2012/01/17 #1 DEL START
            ''子商品の上代販売合計
            'Dim totalJodai As Decimal = 0
            '2012/01/17 #1 DEL END

            '子商品を一元化
            Dim shohins(setShohinDt.Rows.Count - 1) As ZaikoValueObject
            For i As Integer = 0 To shohins.Length - 1

                '親商品の情報をコピーする
                'Me.copyZaikoValueObject(parent, shohins(0))
                shohins(i) = parent.CloneCopy()

                '子商品の商品管理番号、数量をM_セットから取得
                Dim shohinDtRow As N050.M_セット商品Row = DirectCast(setShohinDt.Rows(i), N050.M_セット商品Row)
                shohins(i).ShohinKanriNo = shohinDtRow.商品管理番号

                '2012/01/17 #1 UPD START
                shohins(i).Suryo = shohinDtRow.数量 * parent.Suryo
                'shohins(i).Suryo = shohinDtRow.数量
                '2012/01/17 #1 UPD END

                '子商品の情報をM_商品から取得
                Dim shohinDt As N050.M_商品DataTable = shohinAdapter.SelectByShohinKanriNo(shohins(i).ShohinKanriNo)
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

                '2012/01/17 #1 ADD START
                '子明細の上代、下代、販売金額はM_商品と顧客ランク（値引率）から算出する
                '原価に関しては、T_売上INSERT時に設定しているためここでは行わない
                shohins(i).JodaiTanka = shohinDt(0).上代単価
                Dim kakerituDT As N050.M_掛率DataTable = kakerituAdapter.SelectKakeritu(shohinDt(0).掛率コード, "1")
                shohins(i).GedaiTanka = TnbKingaku.GetGedai(shohinDt(0).上代単価,
                                                            kakerituDT(0).掛率,
                                                            kakerituDT(0).ランク適用フラグ,
                                                            nebikiRate)
                shohins(i).HanbaiKingaku = shohins(i).GedaiTanka * shohins(i).Suryo
                '2012/01/17 #1 ADD END


                '2012/01/17 #1 DEL START
                ''合計上代へ加算
                'totalJodai += shohins(i).JodaiTanka * shohins(i).Suryo
                '2012/01/17 #1 DEL END

            Next

            '2012/01/17 #1 DEL STAR
            ''子商品の上代・下代・販売合計
            ''※最後の子商品で誤差を吸収するため、１～(ｎ-1)個目までの合計金額を取得する
            'Dim subtotalJodai As Decimal = 0
            'Dim subtotalGedai As Decimal = 0
            'Dim subotalHanbaiKingaku As Decimal = 0
            'For i As Integer = 0 To shohins.Length - 2

            '    '上代・下代・販売を算出
            '    Dim rate As Decimal = shohins(i).JodaiTanka * shohins(i).Suryo / totalJodai
            '    shohins(i).JodaiTanka = Fix(parent.JodaiTanka * rate)
            '    shohins(i).GedaiTanka = Fix(parent.GedaiTanka * rate)
            '    shohins(i).HanbaiKingaku = Fix(parent.HanbaiKingaku * rate)

            '    '数量を算出
            '    shohins(i).Suryo = shohins(i).Suryo * parent.Suryo

            '    '合計上代・下代・販売へ加算
            '    subtotalJodai += shohins(i).JodaiTanka
            '    subtotalGedai += shohins(i).GedaiTanka
            '    subotalHanbaiKingaku += shohins(i).HanbaiKingaku

            'Next

            ''最後の子商品で誤差を吸収する
            'Dim lastShohin As ZaikoValueObject = shohins(shohins.Length - 1)
            'lastShohin.JodaiTanka = Fix((parent.JodaiTanka - subtotalJodai) / lastShohin.Suryo)
            'lastShohin.GedaiTanka = Fix((parent.GedaiTanka - subtotalGedai) / lastShohin.Suryo)
            'lastShohin.HanbaiKingaku = Fix((parent.HanbaiKingaku - subotalHanbaiKingaku))
            'lastShohin.Suryo = lastShohin.Suryo * parent.Suryo
            '2012/01/17 #1 DEL END

            Return shohins

        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Function

#End Region

#Region "メーカー返品の在庫引当を実行する"

    ''' <summary>
    ''' メーカー返品の在庫引当を実行する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ExecuteZaikoHikiateMakerHenpin(ByVal shohinKanriNo As String,
                                                   ByVal zaikoKubun As String,
                                                   ByVal suryo As Int32,
                                                   ByVal loginUser As String,
                                                   Optional accessKubun As String = "") As Boolean

        ' 在庫データを取得する
        Dim zaikoTableAdapter As New N050TableAdapters.T_在庫TableAdapter(common_bat.COMMAND_TIME_OUT)
        Dim zaikoDt As N050.T_在庫DataTable = zaikoTableAdapter.SelectZaikoKeijoByShohinKanriZaikoKubun(shohinKanriNo,
                                                                                                          Convert.ToString(zaikoKubun))
        Dim result As Int32 = 0

        ' ---------------------------------------------------------------------------------------------------

        If accessKubun = String.Empty Then
            accessKubun = "4"
        End If

        '商品アクセスへ登録
        Dim accessAdpter As New N050TableAdapters.T_商品アクセスTableAdapter(common_bat.COMMAND_TIME_OUT)
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

        If zaikoDt.Rows.Count = 0 Then

            ' 在庫データ作成(マイナス)
            result = zaikoTableAdapter.InsertZaikoData(shohinKanriNo,
                                                       "0000000000",
                                                       Convert.ToString(zaikoKubun),
                                                       suryo,
                                                       loginUser)

            Return True

        Else

            ' 在庫データ作成ループ

            For Each zaikoDtRow As N050.T_在庫Row In zaikoDt.Rows

                ' 在庫引当

                If zaikoDtRow.数量 < 0 Then

                    'マイナス在庫時存在時


                    ' 在庫データを計算する

                    suryo += zaikoDtRow.数量


                    If suryo < 0 Then

                        ' 在庫データ更新
                        result = zaikoTableAdapter.UpdateZaikoData(suryo,
                                                                   loginUser,
                                                                   shohinKanriNo,
                                                                   zaikoDtRow.仕入伝票番号,
                                                                   Convert.ToString(zaikoKubun))

                        Return True

                    ElseIf suryo > 0 Then

                        ' 在庫データ更新
                        result = zaikoTableAdapter.UpdateZaikoData(suryo,
                                                                   loginUser,
                                                                   zaikoDtRow.商品管理番号,
                                                                   zaikoDtRow.仕入伝票番号,
                                                                   zaikoDtRow.在庫区分)
                    Else

                        ' 在庫数0のデータは削除する
                        result = zaikoTableAdapter.DeleteZeroZaiko(zaikoDtRow.商品管理番号,
                                                                   zaikoDtRow.仕入伝票番号,
                                                                   zaikoDtRow.在庫区分)

                        Return True

                    End If

                Else

                    'プラス在庫存在時


                    ' 在庫データを計算する

                    suryo += zaikoDtRow.数量


                    If suryo <= 0 Then

                        ' 対象の行を削除する
                        result = zaikoTableAdapter.DeleteZeroZaiko(zaikoDtRow.商品管理番号,
                                                                   zaikoDtRow.仕入伝票番号,
                                                                   zaikoDtRow.在庫区分)

                        If suryo = 0 Then

                            Return True

                        End If

                    Else

                        ' 在庫データ更新
                        result = zaikoTableAdapter.UpdateZaikoData(suryo,
                                                                   loginUser,
                                                                   shohinKanriNo,
                                                                   zaikoDtRow.仕入伝票番号,
                                                                   Convert.ToString(zaikoKubun))
                        Return True

                    End If

                End If

            Next

        End If

        If suryo < 0 Then

            ' 在庫データ作成
            result = zaikoTableAdapter.InsertZaikoData(shohinKanriNo,
                                                       "0000000000",
                                                       Convert.ToString(zaikoKubun),
                                                       suryo,
                                                       loginUser)


        End If

        Return True

    End Function

#End Region

#End Region

End Class

#Region "在庫区分"

''' <summary>
''' 在庫区分
''' </summary>
''' <remarks></remarks>
Public Enum ZaikoKubun

    ''' <summary>
    ''' 本社在庫
    ''' </summary>
    ''' <remarks></remarks>
    Honsha = 1

    ''' <summary>
    ''' 浜町在庫
    ''' </summary>
    ''' <remarks></remarks>
    Hamacho

    ''' <summary>
    ''' 社外倉庫
    ''' </summary>
    ''' <remarks></remarks>
    ShagaiSoko

End Enum

#End Region

Public Class SaibanTran

#Region "【メソッド】"

#Region "発注伝票番号"

    ''' <summary>
    ''' 発注伝票番号を取得する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetHacchuDenpyoNo() As String

        Return Me.saibanDenpyoExecute("0")

    End Function

#End Region

#Region "返品伝票番号"

    ''' <summary>
    ''' 返品伝票番号を取得する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetHenpinDenpyoNo() As String

        Return Me.saibanDenpyoExecute("8")

    End Function

#End Region

#Region "品番変更伝票番号"

    ''' <summary>
    ''' 品番変更伝票番号を取得する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetHinbanHenkoDenpyoNo() As String

        Return Me.saibanDenpyoExecute("7")

    End Function

#End Region

#Region "廃棄伝票番号"

    ''' <summary>
    ''' 廃棄伝票番号を取得する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetHaikiDenpyoNo() As String

        Return Me.saibanDenpyoExecute("9")

    End Function

#End Region

#Region "売上伝票番号"

    ''' <summary>
    ''' 売上伝票番号を取得する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetUriageDenpyoNo() As String

        Return Me.saibanDenpyoExecute("6")

    End Function

#End Region

#Region "顧客管理番号"

    ''' <summary>
    ''' 顧客管理番号を取得する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetKokyakuKanriNo() As String

        Return Me.saibanKanriNoExecute("K")

    End Function

#End Region

#Region "特約店管理番号"

    ''' <summary>
    ''' 特約店管理番号を取得する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetTokuyakutenKanriNo() As String

        Return Me.saibanKanriNoExecute("T")

    End Function

#End Region

#Region "商品管理番号"

    ''' <summary>
    ''' 顧客管理番号を取得する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetShohinKanriNo() As String

        Return Me.saibanKanriNoExecute("S")

    End Function

#End Region

#Region "伝票番号採番処理実行"

    ''' <summary>
    ''' 伝票番号採番処理実行
    ''' </summary>
    ''' <param name="saibanKubun"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function saibanDenpyoExecute(ByVal saibanKubun As String) As String

        Dim saibanAtapter As New N050TableAdapters.T_採番TableAdapter
        Dim saibanResult As String = String.Empty
        Dim result As Int32 = 0
        Dim increment As Int32 = 1
        Dim sysDateDT As N050.SystemDateDataTable = (New N050TableAdapters.SystemDateTableAdapter).SelectSysDate
        Dim saibanDate As String = DirectCast(sysDateDT.Rows(0), N050.SystemDateRow).SysDate.ToString("yyyyMMdd")

        ' 採番データを更新する
        If Me.saibanDataUpdateInsert(saibanKubun, saibanDate) = False Then

            Return String.Empty

        End If

        For Each saibanRow As N050.T_採番Row In saibanAtapter.SelectBySaibanKubun(saibanKubun, saibanDate)

            If saibanKubun = "0" Then

                saibanKubun = String.Empty
                increment = 0

            End If

            saibanResult = saibanRow.採番年月日.Substring(2, 6) + saibanKubun + saibanRow.採番値.ToString().PadLeft(10 - (saibanRow.採番年月日.Substring(2, 6).Length + increment), "0"c)

        Next

        Return saibanResult

    End Function

#End Region

#Region "管理番号採番処理実行"

    ''' <summary>
    ''' 管理番号採番処理実行
    ''' </summary>
    ''' <param name="saibanKubun"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function saibanKanriNoExecute(ByVal saibanKubun As String) As String

        Dim saibanAtapter As New N050TableAdapters.T_採番TableAdapter
        Dim saibanResult As String = String.Empty
        Dim result As Int32 = 0
        Dim increment As Int32 = 1
        Dim saibanDate As String = "99999999"

        ' 採番データを更新する
        If Me.saibanDataUpdateInsert(saibanKubun, saibanDate) = False Then

            Return String.Empty

        End If

        For Each saibanRow As N050.T_採番Row In saibanAtapter.SelectBySaibanKubun(saibanKubun, saibanDate)

            Dim firstChar As String = "0"

            If saibanKubun = "T" Then
                firstChar = "9"
            End If

            saibanResult = firstChar + saibanRow.採番値.ToString().PadLeft(7, "0"c)

        Next

        Return saibanResult

    End Function

#End Region

#Region "採番データ更新作成処理"

    ''' <summary>
    ''' 採番データ更新作成処理
    ''' </summary>
    ''' <param name="saibanKubun"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function saibanDataUpdateInsert(ByVal saibanKubun As String, ByVal saibanDate As String) As Boolean

        Dim saibanAtapter As New N050TableAdapters.T_採番TableAdapter
        Dim result As Int32 = 0

        ' 採番値をインクリメントする
        result = saibanAtapter.UpdateSaibanData("Sys", saibanKubun, saibanDate)

        ' 結果が0件の場合は採番データを作成する
        If result = 0 Then

            saibanAtapter.InsertSaibanData(saibanKubun, saibanDate, 1, "Sys")

        End If

        Return True

    End Function

#End Region

#End Region

End Class

Public Class Converter

#Region "【メソッド】"

#Region "引数が空白の場合はNothingにして返却する（String）"

    ''' <summary>
    ''' 引数が空白の場合はNothingにして返却する
    ''' </summary>
    ''' <param name="value">対象文字列</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function BlankToNull(ByVal value As String) As String

        If String.IsNullOrEmpty(value) = True Then

            Return Nothing

        Else

            Return value

        End If

    End Function

#End Region

#Region "引数がnothingの場合は文字列にして返却する（String）"

    ''' <summary>
    ''' 引数が空白の場合は文字列にして返却する
    ''' </summary>
    ''' <param name="value">対象文字列</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function NullToString(ByVal value As Object) As String

        If String.IsNullOrEmpty(Convert.ToString(value)) = False Then

            Return Convert.ToString(value)

        Else

            Return String.Empty

        End If

    End Function

#End Region

#Region "引数がnothingの場合はゼロにして返却する（Decimal）"

    ''' <summary>
    ''' 引数が空白の場合はゼロにして返却する
    ''' </summary>
    ''' <param name="value">対象文字列</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function NullToDecimalZero(ByVal value As Object) As Decimal

        If String.IsNullOrEmpty(Convert.ToString(value)) = False Then

            Return Convert.ToDecimal(value)

        Else

            Return 0

        End If

    End Function

#End Region

#Region "引数がnothingの場合はゼロにして返却する（Int32）"

    ''' <summary>
    ''' 引数が空白の場合はゼロにして返却する
    ''' </summary>
    ''' <param name="value">対象文字列</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function NullToInt32Zero(ByVal value As Object) As Int32

        If String.IsNullOrEmpty(Convert.ToString(value)) = False Then

            Return Convert.ToInt32(value)

        Else

            Return 0

        End If

    End Function

#End Region

#Region "引数がnothingの場合はNothingにして返却する（Nullable(Of Decimal)）"

    ''' <summary>
    ''' 引数が空白の場合はNothingにして返却する
    ''' </summary>
    ''' <param name="value">対象文字列</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function NullToNullableDecimal(ByVal value As Object) As Nullable(Of Decimal)

        If value IsNot Nothing Then

            Return Convert.ToDecimal(value)

        Else

            Return Nothing

        End If

    End Function

#End Region

#Region "引数がnothingの場合はNothingにして返却する（Nullable(Of Int32)）"

    ''' <summary>
    ''' 引数が空白の場合はNothingにして返却する
    ''' </summary>
    ''' <param name="value">対象文字列</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function NullToNullableInt32(ByVal value As Object) As Nullable(Of Int32)

        If value IsNot Nothing Then

            Return Convert.ToInt32(value)

        Else

            Return Nothing

        End If

    End Function

#End Region

#Region "日付変換を行う(yyyyMMdd→yyyy/MM/dd)"

    ''' <summary>
    ''' 日付変換を行う(yyyyMMdd→yyyy/MM/dd)
    ''' </summary>
    ''' <param name="baseDate">変換する日付(YYYYMMDD)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ValueToDateFormat(ByVal baseDate As String) As String

        If baseDate.Length <> 8 Then

            Return baseDate

        End If

        Return baseDate.Substring(0, 4) & "/" & baseDate.Substring(4, 2) & "/" & baseDate.Substring(6, 2)

    End Function

#End Region

#Region "日付変換を行う(yyyyMMdd→指定書式の日付文字列)"

    ''' <summary>
    ''' 日付変換を行う(yyyyMMdd→指定書式の日付文字列)
    ''' </summary>
    ''' <param name="baseDate">変換する日付</param>
    ''' <param name="format">書式</param>
    ''' <returns>指定した書式の日付文字列</returns>
    ''' <remarks></remarks>
    Public Shared Function ValueToDateFormat(ByVal baseDate As String, ByVal format As String) As String

        If baseDate.Length <> 8 Then

            Return baseDate

        End If

        Dim dt As Date = ValueToDate(baseDate)

        Return dt.ToString(format)

    End Function

#End Region

#Region "日付変換を行う(文字列：yyyyMMdd→Date型)"

    ''' <summary>
    ''' 日付変換を行う(文字列：yyyyMMdd→Date型)
    ''' </summary>
    ''' <param name="baseDate">変換する日付(YYYYMMDD)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ValueToDate(ByVal baseDate As String) As Date

        Dim dt As Date = Nothing
        If Not Date.TryParse(ValueToDateFormat(baseDate), dt) Then
            Return Nothing
        End If

        Return dt

    End Function

#End Region

#Region "引数の値をカンマ編集して返却する"

    ''' <summary>
    ''' 引数の値をカンマ編集して返却する
    ''' </summary>
    ''' <param name="item">変換する文字列</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ToCommaValue(ByVal item As String) As String

        Dim result As Decimal

        If String.IsNullOrEmpty(item) = True Then

            Return String.Empty

        End If

        If Decimal.TryParse(item, result) = True Then

            Return result.ToString("#,##0")

        Else

            Return item

        End If

    End Function

#End Region

#Region "引数を元に曜日を取得する"

    ''' <summary>
    ''' 引数を元に曜日を取得する
    ''' </summary>
    ''' <param name="item">曜日変換する日付(YYYY/MM/DD or YYYYMMDD)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ToDayOfWeek(ByVal item As String) As String

        If String.IsNullOrEmpty(item) = True Then

            Return String.Empty

        Else

            If item.Length = 8 Then

                item = ValueToDateFormat(item)

                Return DateAndTime.WeekdayName(DateAndTime.Weekday(Convert.ToDateTime(item), FirstDayOfWeek.System), False, FirstDayOfWeek.System)

            ElseIf item.Length = 10 Then

                Return DateAndTime.WeekdayName(DateAndTime.Weekday(Convert.ToDateTime(item), FirstDayOfWeek.System), False, FirstDayOfWeek.System)

            Else

                Return item

            End If

        End If

    End Function

#End Region

#Region "引数を元に和暦に変換して返却する"

    ''' <summary>
    ''' 引数を元に和暦に変換して返却する
    ''' </summary>
    ''' <param name="item">和暦変換する日付(YYYY/MM/DD or YYYYMMDD)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ToWareki(ByVal item As String) As String

        Dim culture As New CultureInfo("ja-JP", True)

        If String.IsNullOrEmpty(item) = True Then

            Return String.Empty

        End If

        If item.Length = 8 Then

            item = ValueToDateFormat(item)

        End If

        culture.DateTimeFormat.Calendar = New JapaneseCalendar()

        Return Convert.ToDateTime(item).ToString("gggyy年M月d日", culture)

    End Function

#End Region

#Region "引数を元に和暦に変換してコレクションで返却する"

    ''' <summary>
    ''' 引数を元に和暦に変換して返却する
    ''' 返却結果はコレクションで返却する
    ''' コレクションインデックス０：元号
    ''' コレクションインデックス１：年
    ''' コレクションインデックス２：月
    ''' コレクションインデックス３：日
    ''' </summary>
    ''' <param name="item">和暦変換する日付(YYYY/MM/DD or YYYYMMDD)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ToWarekiCollect(ByVal item As String) As List(Of String)

        Dim culture As New CultureInfo("ja-JP", True)
        Dim warekiList As New List(Of String)

        If String.IsNullOrEmpty(item) = True Then

            Return Nothing

        End If

        If item.Length = 8 Then

            item = ValueToDateFormat(item)

        End If

        culture.DateTimeFormat.Calendar = New JapaneseCalendar()

        warekiList.Add(Convert.ToDateTime(item).ToString("ggg", culture))
        warekiList.Add(Convert.ToDateTime(item).ToString("yy", culture))
        warekiList.Add(Convert.ToDateTime(item).ToString("MM", culture))
        warekiList.Add(Convert.ToDateTime(item).ToString("dd", culture))

        Return warekiList

    End Function

#End Region

#Region "引数をもとに期の開始年月日、終了年月日を返却する"

    ''' <summary>
    ''' 引数をもとに期の開始年月日、終了年月日を返却する
    ''' </summary>
    ''' <param name="targetDate">YYYYMMDD形式</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetKiStartDateEndDate(ByVal targetDate As DateTime) As List(Of String)

        Dim kiStartEndYMD As New List(Of String)
        Dim convertDate As DateTime = targetDate
        Dim startDate As String
        Dim endDate As String

        ' 8月より大きい場合
        If Convert.ToInt32(convertDate.ToString("MM")) >= 8 Then

            startDate = convertDate.Year.ToString() + "0801"
            endDate = convertDate.AddYears(1).Year.ToString() + "0731"

        Else

            startDate = convertDate.AddYears(-1).Year.ToString() + "0801"
            endDate = convertDate.Year.ToString() + "0731"

        End If

        kiStartEndYMD.Add(startDate)
        kiStartEndYMD.Add(endDate)

        Return kiStartEndYMD

    End Function

#End Region

#Region "引数をもとに期（前半）の開始年月日、終了年月日を返却する"

    ''' <summary>
    ''' 引数をもとに期（前半）の開始年月日、終了年月日を返却する
    ''' </summary>
    ''' <param name="targetDate">YYYYMMDD形式</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetKamihanKiStartDateEndDate(ByVal targetDate As DateTime) As List(Of String)

        '期の開始年月日、終了年月日を取得する
        Dim kiStartEndYMD As List(Of String) = GetKiStartDateEndDate(targetDate)

        '終了年月日を６カ月前にする
        Dim endDate As Date = ValueToDate(kiStartEndYMD(1))
        endDate = endDate.AddMonths(-6)
        kiStartEndYMD(1) = endDate.ToString("yyyyMM") & Date.DaysInMonth(endDate.Year, endDate.Month).ToString()

        Return kiStartEndYMD

    End Function

#End Region

#Region "引数をもとに期（後半）の開始年月日、終了年月日を返却する"

    ''' <summary>
    ''' 引数をもとに期（後半）の開始年月日、終了年月日を返却する
    ''' </summary>
    ''' <param name="targetDate">YYYYMMDD形式</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetShimohanKiStartDateEndDate(ByVal targetDate As DateTime) As List(Of String)

        '期の開始年月日、終了年月日を取得する
        Dim kiStartEndYMD As List(Of String) = GetKiStartDateEndDate(targetDate)

        '開始年月日を６カ月あとにする
        Dim startDate As Date = ValueToDate(kiStartEndYMD(0))
        startDate = startDate.AddMonths(6)
        kiStartEndYMD(0) = startDate.ToString("yyyyMM") & "01"

        Return kiStartEndYMD

    End Function

#End Region

#Region "最終棚卸日を返却する"

    ''' <summary>
    ''' M_カレンダから棚卸フラグを参照し、最終棚卸日を返却する
    ''' 該当レコードがない場合はT_日別在庫の最小年月日を取得する
    ''' </summary>
    ''' <returns>最終棚卸日</returns>
    ''' <remarks></remarks>
    'Public Shared Function GetLastTanaoroshiDate() As String

    '    '最終棚卸日を取得
    '    Dim lastTanaoroshiDay As String = String.Empty
    '    Dim tanaoroshiAdapter As New N050TableAdapters.最終棚卸日取得TableAdapter
    '    Dim tanaoroshiDT As N050.最終棚卸日取得DataTable = tanaoroshiAdapter.SelectTanaoroshiDay()
    '    If tanaoroshiDT.Rows.Count > 0 AndAlso Not tanaoroshiDT(0).Is最終棚卸日Null Then
    '        lastTanaoroshiDay = tanaoroshiDT(0).最終棚卸日
    '    Else
    '        tanaoroshiDT = tanaoroshiAdapter.SelectFirstZaikoDay()
    '        If tanaoroshiDT.Rows.Count > 0 AndAlso Not tanaoroshiDT(0).Is最終棚卸日Null Then
    '            lastTanaoroshiDay = tanaoroshiDT(0).最終棚卸日
    '        Else
    '            lastTanaoroshiDay = Now.AddDays(-1).ToString("yyyyMMdd")
    '        End If
    '    End If

    '    Return lastTanaoroshiDay

    'End Function

#End Region

#Region "顧客カードのカード番号の変換処理"

    ''' <summary>
    ''' カード番号の変換処理
    ''' カード番号が正しくない場合はエラーを返す
    ''' 
    ''' カード番号が13桁の場合は変換処理を行う。
    ''' ・13桁目はチェックデジットなので無視
    ''' ・1桁目が「0」、2～4桁目が「000」～「255」の場合は、M顧客用のカード番号
    ''' ・1～5桁目が「99999」の場合は、M特約店用のカード番号    
    ''' ＜M顧客＞
    ''' 　2～4桁目をJISコード変換
    ''' 　10～13桁目をJISコード変換。ただし「000」、「255」以上の場合は無視
    ''' ＜M特約店＞
    ''' 　6～12桁目をカード番号とする
    ''' </summary>
    ''' <param name="cardNo">in変換するカード番号</param>
    ''' <param name="formatedCardNo">out変換後のカード番号</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ConvertCardNo(ByVal cardNo As String, ByRef formatedCardNo As String) As Boolean

        formatedCardNo = cardNo.ToUpper()

        If formatedCardNo = String.Empty Then
            Return False
        End If

        'カード番号が2桁以下の場合はエラーとする
        Dim length As Integer = formatedCardNo.Length
        If length < 2 Then
            Return False
        End If

        '先頭文字の変換
        Dim c As String = String.Empty
        c = formatedCardNo.Substring(0, 1)

        '[\]→[ﾜ]変換
        If c = "\" Then
            formatedCardNo = "ﾜ" & formatedCardNo.Substring(1, length - 1)
        End If

        '[-]→[=]変換
        If c = "-" Then
            formatedCardNo = "=" & formatedCardNo.Substring(1, length - 1)
        End If

        '"["→"]"変換
        If c = "[" Then
            formatedCardNo = "]" & formatedCardNo.Substring(1, length - 1)
        End If

        '桁数調節
        If formatedCardNo.Substring(0, 1) = "@" Then

            '臨時会員の場合は「@」以降が数値でなければエラーとし、数値部分が5桁に満たない場合は0埋めをする
            Dim tempNo As String = formatedCardNo.Replace("@", String.Empty)
            If Not IsNumeric(tempNo) Then
                Return False
            End If

            '0埋め処理
            If tempNo.Length < 5 Then
                tempNo = tempNo.PadLeft(5, "0"c)
                formatedCardNo = formatedCardNo.Substring(0, 1) & tempNo
            End If

        ElseIf formatedCardNo.Substring(0, 1) = "5" Then

            '特約店会員の場合は７桁に合わせて0埋め処理
            Dim tempNo As String = formatedCardNo.Substring(1)
            If tempNo.Length < 6 Then
                tempNo = tempNo.PadLeft(6, "0"c)
                formatedCardNo = formatedCardNo.Substring(0, 1) & tempNo
            End If

        ElseIf formatedCardNo.Substring(0, 1) = "]" OrElse
               formatedCardNo.Substring(0, 1) = "=" Then

            '仕入先、社員の場合は４桁に合わせて0埋め処理
            Dim tempNo As String = formatedCardNo.Substring(1)
            If tempNo.Length < 3 Then
                tempNo = tempNo.PadLeft(3, "0"c)
                formatedCardNo = formatedCardNo.Substring(0, 1) & tempNo
            End If

        Else

            '通常顧客の場合は６桁に合わせて0埋め処理
            Dim tempNo As String = formatedCardNo.Substring(1)
            If tempNo.Length < 5 Then
                tempNo = tempNo.PadLeft(5, "0"c)
                formatedCardNo = formatedCardNo.Substring(0, 1) & tempNo
            End If

        End If

        '13桁の場合はカード番号を変換する
        If length = 13 Then

            Dim c1 As String = cardNo.Substring(0, 1)
            Dim c24 As String = cardNo.Substring(1, 3)
            Dim c15 As String = cardNo.Substring(0, 5)
            If c1 = "0" AndAlso IsNumeric(c24) AndAlso CInt(c24) <= 255 Then

                '1桁目が「0」、2～4桁目が「000」～「255」の場合は、M顧客用のカード番号
                Dim c10 As String = cardNo.Substring(9, 3)
                If IsNumeric(c10) Then
                    Dim num10 As Integer = CInt(c10)
                    If num10 = 0 Then
                        '10～13桁目が「000」の場合は無視
                        c10 = String.Empty
                    ElseIf num10 <= 255 Then
                        '10～13桁目が「255」以下の場合は変換
                        c10 = Chr(num10).ToString()
                    Else
                        'それ以外は変換しない

                    End If
                End If

                '2～4桁目、10～13桁目をJISコード変換
                formatedCardNo = Chr(CInt(c24)).ToString() & cardNo.Substring(4, 5) & c10

            ElseIf c15 = "99999" Then

                '1～5桁目が「99999」の場合は、M特約店用のカード番号

                '6～12桁目をカード番号とする
                formatedCardNo = cardNo.Substring(5, 7)

            End If

        End If

        '[@00000][@000000]の場合はエラーとする
        If formatedCardNo = "@00000" OrElse formatedCardNo = "@000000" Then
            Return False
        End If

        'カード番号の桁数チェック
        If formatedCardNo.Length > 9 Then
            Return False
        End If

        Return True
    End Function

#End Region

#Region "文字列を指定した桁数でカットする"

    ''' <summary>
    ''' 文字列を指定した桁数でカットする
    ''' 指定文字数よりも短い場合はカットしない
    ''' </summary>
    ''' <param name="item">カットする文字列</param>
    ''' <param name="length">桁数</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function SubString(ByVal item As String, ByVal length As Integer) As String

        If String.IsNullOrEmpty(item) = True Then

            Return String.Empty

        Else

            If item.Length <= length Then

                Return item

            Else

                Return item.Substring(0, length)

            End If

        End If

    End Function

#End Region

#Region "上代を指数に変換する"

    ''' <summary>
    ''' 上代を指数に変換する
    ''' </summary>
    ''' <param name="jyodai">上代</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function JyodaiToShisuJyodai(ByVal jyodai As String) As String

        '2011/11/21 本番障害No13対応
        '4桁指数を生成
        If jyodai.Length >= 4 Then
            Dim wk4keta As String = jyodai.Substring(0, 3) & jyodai.Length - 3

            '生成した4桁指数の復元値が引数に一致しない場合は生成エラー
            If Convert.ToInt32(wk4keta.Substring(0, 3)) * 10 ^ Convert.ToInt32(wk4keta.Substring(3, 1)) =
                Convert.ToInt32(jyodai) Then
                Return wk4keta
            End If
        Else
            '3桁以下は末尾0埋めしてリターン
            Return jyodai.PadLeft(3, CChar("0")).PadRight(4, CChar("0"))
        End If

        Return "-1"

        '3桁より大きい場合はチェック
        'If jyodai.Length > 3 Then

        '    Dim jodai As Decimal = Convert.ToDecimal(jyodai)

        '    If jodai Mod 10 ^ (jyodai.Length - 3) <> 0 Then

        '        Return "-1"

        '    Else

        '        Return jyodai.Substring(0, 3) + (jyodai.Length - 3).ToString

        '    End If

        'Else
        '    Return "-1"
        'End If

    End Function

#End Region

#Region "指数上代を上代に変換する"

    ''' <summary>
    ''' 上代を指数に変換する
    ''' </summary>
    ''' <param name="shisuJyoodai">上代</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ShisuJyodaiToJyodai(ByVal shisuJyoodai As String) As String

        If shisuJyoodai.Length = 4 Then
            Return shisuJyoodai.Substring(0, 3) + "0".PadRight(Convert.ToInt32(shisuJyoodai.Substring(3)), "0"c)
        Else
            Return Convert.ToInt32(shisuJyoodai).ToString()
        End If

    End Function

#End Region

#Region "チェックディジットを算出する"

    ''' <summary>
    ''' チェックディジットを算出する
    ''' </summary>
    ''' <param name="item"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetCheckDigit(ByVal item As String) As String

        ' 偶数
        Dim evenNumbers As New List(Of Int32)

        ' 奇数
        Dim oddNumbers As New List(Of Int32)

        ' 偶数、奇数インデックス
        Dim i As Int32 = 0

        For index As Int32 = item.Length To 1 Step -1

            If i Mod 2 = 0 Then
                evenNumbers.Add(Convert.ToInt32(item.Substring(index - 1, 1)))
            Else
                oddNumbers.Add(Convert.ToInt32(item.Substring(index - 1, 1)))
            End If

            i += 1

        Next

        Dim summary As String = Convert.ToString((evenNumbers.Sum() * 3) + oddNumbers.Sum())

        If Convert.ToInt32(summary.Substring(summary.Length - 1)) = 0 Then
            Return "0"
        Else
            Return Convert.ToString(10 - Convert.ToInt32(summary.Substring(summary.Length - 1)))
        End If

    End Function

#End Region

#Region "仕入入力日から集計年月を取得する"

    ''' <summary>
    ''' 仕入入力日から集計年月を取得する
    ''' </summary>
    ''' <param name="shiireDate">仕入入力日(年月日)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetShiireInputDateToShukeiYM(ByVal shiireDate As String) As String

        Dim calDt As N050.M_カレンダDataTable = (New N050TableAdapters.M_カレンダTableAdapter).SelectYearMonthFirstDay(shiireDate.Substring(0, 6))

        If shiireDate = calDt(0).年月日 Then

            Return Converter.ValueToDate(shiireDate).AddMonths(-1).ToString("yyyyMM")

        Else

            Return shiireDate.Substring(0, 6)

        End If

    End Function

#End Region

#End Region

End Class

Public Class TnbKingaku

#Region "上代・掛率・ランク掛率から、下代を算出する"

    ''' <summary>
    ''' 上代・掛率・ランク掛率から、下代を算出する
    ''' クラス割引＝購入者により変動する「値引率」
    ''' ※購入者の指定がなく、割引した金額を算出する必要がない場合には、第3,4引数は指定しなくてよい
    ''' </summary>
    ''' <param name="jodai">商品の上代金額</param>
    ''' <param name="kakeritu">商品の掛率（掛率コードではなく、M_掛率から取得した掛率を指定）</param>
    ''' <param name="rankFlg">ランク適用フラグ（購入者のランク割引を適用するかどうか）</param>
    ''' <param name="rankWaribiki">ランクの割引率（購入者のランクにより決定する割引率）</param>
    ''' <returns>算出した下代を返す</returns>
    ''' <remarks></remarks>
    Public Shared Function GetGedai(ByVal jodai As Decimal, _
                                    ByVal kakeritu As Decimal, _
                                    Optional ByVal rankFlg As Boolean = False, _
                                    Optional ByVal rankWaribiki As Decimal = 0) As Integer

        Try

            Dim gedai As Integer = 0

            If rankFlg Then

                '計算式１（クラス割引あり）

                If jodai >= 90 Then

                    gedai = ToRoundUp(jodai * kakeritu * rankWaribiki / 5) * 5

                Else

                    gedai = ToRoundUp(jodai * kakeritu * rankWaribiki)

                End If

            Else

                '計算式２（クラス割引なし）

                If jodai >= 90 Then

                    gedai = ToRoundUp(jodai * kakeritu / 5) * 5

                Else

                    gedai = ToRoundUp(jodai * kakeritu)

                End If

            End If


            Return gedai

        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Function

#End Region

#Region "RoundUp"

    ''' <summary>
    ''' 小数点以下の切り上げを行う
    ''' </summary>
    ''' <param name="value">切り上げを行う値</param>
    ''' <remarks></remarks>
    Public Shared Function ToRoundUp(ByVal value As Decimal) As Integer

        Try

            If value > 0 Then
                Return CInt(Math.Ceiling(value))
            Else
                Return CInt(Math.Floor(value))
            End If

        Catch ex As Exception

            Throw New Exception(ex.Message, ex)

        End Try

    End Function

#End Region

End Class