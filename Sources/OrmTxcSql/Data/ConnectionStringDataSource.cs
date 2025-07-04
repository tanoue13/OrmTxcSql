using System.Diagnostics.CodeAnalysis;

namespace OrmTxcSql.Data
{
    /// <summary>
    /// プロパティの接続文字列を戻すデータソース。
    /// </summary>
    public class ConnectionStringDataSource : DataSource, IDataSource
    {
        /// <summary>
        /// 接続文字列を取得または設定します。
        /// </summary>
#if NET6_0_OR_GREATER
        [AllowNull]
#endif
        public string ConnectionString { get; set; }
        /// <summary>
        /// 接続文字列を戻す。
        /// </summary>
        /// <returns></returns>
        public override string GetConnectionString() => this.ConnectionString;
    }
}
