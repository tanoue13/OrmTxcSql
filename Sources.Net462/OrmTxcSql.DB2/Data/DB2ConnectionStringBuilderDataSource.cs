using IBM.Data.DB2.iSeries;
using OrmTxcSql.Data;

namespace OrmTxcSql.DB2.Data
{

    /// <summary>
    /// プロパティの接続文字列を戻すデータソース。
    /// </summary>
    public class DB2ConnectionStringBuilderDataSource : DataSource
    {

        public iDB2ConnectionStringBuilder ConnectionStringBuilder { get; set; }

        public override string GetConnectionString()
              => this.ConnectionStringBuilder.ToString();

    }

}
