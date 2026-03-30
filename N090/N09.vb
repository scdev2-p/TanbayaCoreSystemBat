Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim beforemonth As String = common_bat.ValueToDate(common_bat.BatDate).ToString("yyyyMM")
        Dim bunruiTA As New N090TableAdapters.V_売上TableAdapter(common_bat.COMMAND_TIME_OUT)
        Dim bunruiDT As N090.V_売上DataTable
        Dim kokyakubunruiCD As String = String.Empty
        Dim kokyaku As String = String.Empty
        Dim uriage As Integer
        Dim startday As DateTime = DateTime.Now

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(startday, "N090", DateTime.Now)

            ' データ取得
            bunruiDT = bunruiTA.SelectKokyakuBunrui(beforemonth)

            ' トランザクション開始
            Using scope As New System.Transactions.TransactionScope

                For i As Integer = 0 To bunruiDT.Rows.Count - 1

                    '前の顧客管理Noと今の顧客管理Noが同じ時
                    If kokyaku = bunruiDT.Rows(i).Item("顧客管理番号").ToString Then

                        '顧客分類の変数が3文字以下の時
                        If kokyakubunruiCD.Length < 2 Then

                            '次の売上金額が前の売上金額の20%以上の時
                            If uriage <= Convert.ToInt32(bunruiDT.Rows(i).Item("売上金額")) Then

                                kokyakubunruiCD += bunruiDT.Rows(i).Item("フロアコード").ToString()

                            End If

                        End If

                    Else

                        '更新作業
                        bunruiTA.UpdateKokyakuBunrui(kokyakubunruiCD, kokyaku)

                        '変数に値をセットする
                        kokyakubunruiCD = bunruiDT.Rows(i).Item("フロアコード").ToString
                        uriage = Convert.ToInt32(Convert.ToInt32(bunruiDT.Rows(i).Item("売上金額")) * 0.2)
                        kokyaku = Convert.ToString(bunruiDT.Rows(i).Item("顧客管理番号"))

                    End If

                Next

                '更新作業
                bunruiTA.UpdateKokyakuBunrui(kokyakubunruiCD, kokyaku)

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, startday, "N090")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, startday, "N090")

        End Try

    End Sub

End Module