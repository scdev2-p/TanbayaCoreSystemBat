Public Module Module1

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

#Region "アクセス区分"

    ''' <summary>
    ''' アクセス区分
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure AccessKubun

        ''' <summary>
        ''' 発注
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Hacchu As String = "1"

        ''' <summary>
        ''' 仕入
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Shiire As String = "2"

        ''' <summary>
        ''' 売上
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Uriage As String = "3"

        ''' <summary>
        ''' 返品
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Henpin As String = "4"

        ''' <summary>
        ''' 廃棄
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Haiki As String = "5"

        ''' <summary>
        ''' 廃棄
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared HinbanHenko As String = "6"

    End Structure

#End Region

#Region "【定数】"

    Public Structure Constant
        ''' <summary>
        ''' マイナス在庫時に使用する伝票番号
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared ReadOnly ZAIKO_SHIIRE_DENPYONO_ALL_ZERO As String = "0000000000"

    End Structure

#End Region

    ''' <summary>
    ''' 接続文字列を取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns>バッチ実行日</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property DBConnectionString As String

        Get

            Return My.Settings.tnbConnectionString

        End Get

    End Property

    ''' <summary>
    ''' M_コードからバッチ実行日を取得する
    ''' </summary>
    ''' <value></value>
    ''' <returns>バッチ実行日</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property BatDate As String
        Get
            Static targetDay As String = String.Empty
            If targetDay = String.Empty Then

                Dim batDateDT As Common.バッチ実行日DataTable = (New CommonTableAdapters.バッチ実行日TableAdapter).SelectBatDate()
                If batDateDT.Rows.Count = 0 Then
                    Dim updateTime As DateTime = DateTime.Now
                    Dim batLogTadap As New common_bat.CommonTableAdapters.バッチログTableAdapter()
                    batLogTadap.InsertBatLog(DateTime.Now, "common", updateTime)
                    batLogTadap.UpdateBatLog(DateTime.Now, False, "「バッチ実行日」がM_コードから取得できませんでした。", updateTime, "common")

                    Throw New Exception("「バッチ実行日」がM_コードから取得できませんでした。")
                End If
                targetDay = batDateDT(0).バッチ実行日

            End If
            Return targetDay

        End Get

    End Property

    ''' <summary>
    ''' 日付変換を行う(yyyyMMdd→yyyy/MM/dd)
    ''' </summary>
    ''' <param name="baseDate">"baseDate">変換する日付(YYYYMMDD)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ValueToDateFormat(ByVal baseDate As String) As String

        If baseDate.Length <> 8 Then

            Return baseDate

        End If

        Return baseDate.Substring(0, 4) & "/" & baseDate.Substring(4, 2) & "/" & baseDate.Substring(6, 2)

    End Function

    ''' <summary>
    ''' 日付変換を行う(文字列：yyyyMMdd→Date型)
    ''' </summary>
    ''' <param name="baseDate">"baseDate">変換する日付(YYYYMMDD)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ValueToDate(ByVal baseDate As String) As Date

        Dim dt As Date = Nothing
        If Not Date.TryParse(ValueToDateFormat(baseDate), dt) Then
            Return Nothing
        End If

        Return dt

    End Function

    ''' <summary>
    ''' コマンドタイムアウト
    ''' </summary>
    ''' <remarks></remarks>
    Public COMMAND_TIME_OUT As Int32 = 3600

End Module
