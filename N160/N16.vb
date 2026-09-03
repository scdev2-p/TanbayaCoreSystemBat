Module Module1

    Sub Main()

        Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
        Dim targetYMD As String = common_bat.BatDate
        Dim targetYM As String = targetYMD.Substring(0, 6)
        Dim updateTime As DateTime = DateTime.Now
        Dim result As Int32

        Dim steps As String = "step0"

        Try

            ' 開始ログ
            batLogTadap.InsertBatLog(updateTime, "N160", DateTime.Now)

            'Dim koritsuTA As New N160TableAdapters.T_月別効率表TableAdapter(common_bat.COMMAND_TIME_OUT)
            'koritsuTA.DeleteKoritsuHyo(targetYM)
            'result = koritsuTA.InsertKoritsuhyo1(targetYM)

            Dim koritsuTA As New N160TableAdapters.T_月別効率表TableAdapter(common_bat.COMMAND_TIME_OUT)

            '2012/03/09 ADD START t-orii
            Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(1, 0, 0))

                'Dim koritsuTA As New N160TableAdapters.T_月別効率表TableAdapter(common_bat.COMMAND_TIME_OUT)

                koritsuTA.DeleteKoritsuHyo(targetYM)
                result = koritsuTA.InsertKoritsuhyo(targetYM)

                scope.Complete()

            End Using

            ' InsertをCommitしてからインデックスを再構築する 2026/09/03 Takagi@SC
            koritsuTA.UpdateIndex()

            Using scope As New System.Transactions.TransactionScope(Transactions.TransactionScopeOption.Required, New TimeSpan(1, 0, 0))

                'Dim koritsuTA As New N160TableAdapters.T_月別効率表TableAdapter(common_bat.COMMAND_TIME_OUT)

                koritsuTA.UpdateInGessoZaiko(targetYM) : steps = "step1"
                koritsuTA.UpdateInShiire(targetYM) : steps = "step2"
                koritsuTA.UpdateInZaiko(targetYM) : steps = "step3"
                koritsuTA.UpdateInUriage(targetYM) : steps = "step4"
                koritsuTA.UpdateInHaiki(targetYM) : steps = "step5"
                koritsuTA.DeleteAllZero(targetYM) : steps = "step6"

                scope.Complete()

            End Using
            '2012/03/09 ADD END t-orii

            koritsuTA.Dispose()

            ' 終了ログ
            batLogTadap.UpdateBatLog(DateTime.Now, True, String.Empty, updateTime, "N160")

        Catch ex As Exception

            ' 終了エラーログ
            batLogTadap.UpdateBatLog(DateTime.Now, False, "steps=" & steps & " " & ex.Message, updateTime, "N160")

        End Try

    End Sub

End Module
