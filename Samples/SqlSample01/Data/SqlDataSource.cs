using System.Data.SqlClient;
using OrmTxcSql.Data;

namespace SqlSample01.Data
{
    public class SqlDataSource : DataSource
    {   /// <summary>
        /// 接続文字列ビルダ
        /// </summary>
        private readonly SqlConnectionStringBuilder _connectionStringBuilder;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public SqlDataSource()
        {
            // 接続文字列ビルダを生成する。
            _connectionStringBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = "localhost",
                InitialCatalog = "sample01",
                IntegratedSecurity = true,
                ApplicationName = this.GetType().FullName,
            };
        }

        public override string GetConnectionString()
        {
            string connectionString = _connectionStringBuilder.ToString();
            return connectionString;
        }
    }
}
