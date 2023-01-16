using IBM.Data.DB2.iSeries;
using OrmTxcSql.Data;

namespace OrmTxcSql.DB2.Data
{
    /// <summary>
    /// プロパティの接続文字列を戻すデータソース。
    /// </summary>
    public class DB2ConnectionStringBuilderDataSource : DataSource
    {
        /// <summary>
        /// <see cref="iDB2ConnectionStringBuilder"/>を取得または設定します。
        /// </summary>
        public iDB2ConnectionStringBuilder ConnectionStringBuilder { get; set; }
        /// <summary>
        /// 接続文字列を取得します。
        /// </summary>
        public override string GetConnectionString()
              => this.ConnectionStringBuilder.ToString();
    }
}
