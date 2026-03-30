Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim startday As DateTime = DateTime.Now
        Dim updateDT As New N220.M_コードDataTable
        Dim updateTA As New N220TableAdapters.M_コードTableAdapter(common_bat.COMMAND_TIME_OUT)

        Try
            ' 開始ログ
            batLogTadap.InsertBatLog(startday, "N220", DateTime.Now)

            ' データ取得
            updateDT = updateTA.SelectData

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

                    codename = updateDT.Rows(i).Item("コード名称") + 1

                    updateTA.UpdateQuery(codename, daikubun, chuukubun)

                Next

                scope.Complete()

            End Using

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, startday, "N220")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, ex.Message, startday, "N220")

        End Try

    End Sub

End Module
