Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim targetYMD As String = common_bat.BatDate
        Dim updateTime As DateTime = DateTime.Now

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N100", DateTime.Now)

            Dim hurikomiTA As New N100TableAdapters.T_振込TableAdapter(common_bat.COMMAND_TIME_OUT)
            Dim nyukinDT As N100.T_振込DataTable
            Dim hurikomiDT As N100.T_振込DataTable

            Using scope As New System.Transactions.TransactionScope

                '当日の入金レコードを取得
                nyukinDT = hurikomiTA.SelectNyukin(targetYMD)

                For row As Integer = 0 To nyukinDT.Rows.Count - 1

                    Dim kokyakuCD As String = nyukinDT(row).顧客コード
                    Dim nyukinPrice As Decimal = nyukinDT(row).入金金額
                    Dim kokyakuKanriNo As String = nyukinDT(row).顧客管理番号
                    Dim shuturyokuRejiNoOfNyukin As String = nyukinDT(row).出力レジ番号
                    Dim rejiDenpyoNoOfNyukin As String = nyukinDT(row).レジ伝票連番
                    Dim meisaiNoOfNyukin As String = nyukinDT(row).明細番号.ToString

                    '顧客管理番号が有る場合は顧客のレコードを取得する、空の場合は顧客以外のレコードを取得する
                    If Not kokyakuKanriNo.Trim() = String.Empty Then

                        '顧客の振込レコードを取得
                        hurikomiDT = hurikomiTA.SelectKokyakuHurikomi(kokyakuKanriNo, nyukinPrice)

                    Else

                        '顧客以外の振込レコードを取得
                        hurikomiDT = hurikomiTA.SelectHurikomi(kokyakuCD, nyukinPrice)

                    End If

                    '振込レコードが存在する場合の処理
                    If hurikomiDT.Rows.Count > 0 Then

                        Dim hurikomiDenpyoDate As String = hurikomiDT(0).伝票年月日
                        Dim shuturyokuRejiNo As String = hurikomiDT(0).出力レジ番号
                        Dim rejiDenpyoNo As String = hurikomiDT(0).レジ伝票連番
                        Dim meisaiNo As String = hurikomiDT(0).明細番号.ToString
                        Dim totalPrice As Decimal = hurikomiDT(0).合計金額
                        Dim nyukinDey As String = nyukinDT(row).入金日付

                        '桁数の修正処理
                        Dim rejiDenpyoNoOfDigitChang As String = rejiDenpyoNo.PadLeft(4, "0")
                        Dim meisaiOfDigitChang As String = meisaiNo.PadLeft(3, "0")

                        '自動消込伝票の値を生成（伝票年月日 + 出力レジ伝票 + レジ伝票連番 + 明細番号）
                        Dim keshikomiDenpyo As String = hurikomiDenpyoDate & shuturyokuRejiNo & rejiDenpyoNoOfDigitChang & meisaiOfDigitChang

                        Dim count As Integer = 0

                        '入金レコードの更新
                        count = hurikomiTA.UpdateNyukin(keshikomiDenpyo, targetYMD, shuturyokuRejiNoOfNyukin, rejiDenpyoNoOfNyukin, meisaiNoOfNyukin)
                        If count = 0 Then
                            Dim msg As String = "入金レコードの更新に失敗しました。" &
                                                "伝票年月日=" & targetYMD &
                                                ",出力レジ番号=" & shuturyokuRejiNoOfNyukin &
                                                ",レジ伝票連番=" & rejiDenpyoNoOfNyukin &
                                                ",明細番号=" & meisaiNoOfNyukin &
                                                ",自動消込伝票=" & keshikomiDenpyo
                            Throw New Exception("入金レコードの更新に失敗しました。")
                        End If

                        '振込レコードの更新
                        count = hurikomiTA.UpdateHurikomi(nyukinPrice, nyukinDey, hurikomiDenpyoDate, shuturyokuRejiNo, rejiDenpyoNo, meisaiNo)
                        If count = 0 Then
                            Dim msg As String = "振込レコードの更新に失敗しました。" &
                                                "伝票年月日=" & hurikomiDenpyoDate &
                                                ",出力レジ番号=" & shuturyokuRejiNo &
                                                ",レジ伝票連番=" & rejiDenpyoNo &
                                                ",明細番号=" & meisaiNo &
                                                ",入金金額=" & nyukinPrice &
                                                ",入金日付=" & nyukinDey
                            Throw New Exception("入金レコードの更新に失敗しました。")
                        End If

                    End If

                Next

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N100")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, updateTime, "N100")

        End Try

    End Sub

End Module
