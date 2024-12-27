#if NET6_0_OR_GREATER
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Diagnostics.CodeAnalysis;
using OrmTxcSql.Data;

namespace OrmTxcSql.SqlClient.Data
{
    /// <summary>
    /// プロパティの接続文字列を戻すデータソース。
    /// </summary>
    public class SqlConnectionStringBuilderDataSource : DataSource
    {
        /// <summary>
        /// <see cref="SqlConnectionStringBuilder"/>を取得または設定します。
        /// </summary>
#if NET6_0_OR_GREATER
        [AllowNull]
#endif
        public SqlConnectionStringBuilder ConnectionStringBuilder { get; set; }
        /// <summary>
        /// 接続文字列を取得します。
        /// </summary>
        public override string GetConnectionString()
            => this.ConnectionStringBuilder.ToString();
    }
}
