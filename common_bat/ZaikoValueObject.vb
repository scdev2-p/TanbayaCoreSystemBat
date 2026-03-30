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
    Public Property ZaikoKubun As String
    'common_bat.ZaikoKubun

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
