Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim startday As DateTime = DateTime.Now
        Dim updateDT As New N210.M_コードDataTable
        Dim updateTA As New N210TableAdapters.M_コードTableAdapter(common_bat.COMMAND_TIME_OUT)

        Dim time As DateTime = common_bat.ValueToDate(common_bat.BatDate).AddMonths(1)
        Dim month As String = time.ToString("MM")

        Try
            ' 開始ログ
            batLogTadap.InsertBatLog(startday, "N210", DateTime.Now)

            ' データ取得
            updateDT = updateTA.GetData

            ' トランザクション開始
            Using scope As New System.Transactions.TransactionScope

                For i As Integer = 0 To updateDT.Rows.Count - 1

                    Dim codename As String = updateDT.Rows(i).Item("コード名称").ToString
                    Dim daikubun As String = updateDT.Rows(i).Item("大区分").ToString
                    Dim chuukubun As String = updateDT.Rows(i).Item("中区分").ToString
                    Dim koushinbi As String = String.Empty
                    If Convert.ToString(updateDT.Rows(i).Item("更新日")) <> String.Empty Then
                        koushinbi = Convert.ToDateTime(updateDT.Rows(i).Item("更新日")).ToString("yyyyMM")
                    End If

                    Select Case daikubun

                        Case "900"

                            Select Case chuukubun

                                Case "001"
                                    codename = time.ToString("yyyy")

                                Case "002"
                                    codename = time.ToString("MM")

                                Case "003"
                                    codename = time.AddYears(-1).ToString("yyyy")

                                Case "004"
                                    codename = time.AddMonths(-1).ToString("MM")

                            End Select

                            updateTA.UpdateCode(codename, daikubun, chuukubun)

                    End Select

                Next

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, startday, "N210")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, startday, "N210")

        End Try

    End Sub

End Module
