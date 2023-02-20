using System.Diagnostics.CodeAnalysis;
using Npgsql;
using OrmTxcSql.Data;

namespace OrmTxcSql.Npgsql.Data
{
    /// <summary>
    /// プロパティの接続文字列を戻すデータソース。
    /// </summary>
    public class NpgsqlConnectionStringBuilderDataSource : DataSource
    {
        /// <summary>
        /// <see cref="NpgsqlConnectionStringBuilder"/>を取得または設定します。
        /// </summary>
#if NET6_0_OR_GREATER
        [AllowNull]
#endif
        public NpgsqlConnectionStringBuilder ConnectionStringBuilder { get; set; }
        /// <summary>
        /// 接続文字列を取得します。
        /// </summary>
        public override string GetConnectionString()
            => this.ConnectionStringBuilder.ToString();
    }
}
